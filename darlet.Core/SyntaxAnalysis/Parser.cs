using System;
using System.Collections.Generic;
using darlet.Core.Errors;
using darlet.Core.LexicalAnalysis;
using darlet.Core.SyntaxAnalysis.AST;

namespace darlet.Core.SyntaxAnalysis
{
    /// <summary>
    /// Парсер (Синтаксичний аналізатор).
    /// Перетворює плоский список токенів у ієрархічну структуру - Абстрактне Синтаксичне Дерево (AST).
    /// Використовує алгоритм рекурсивного спуску.
    /// </summary>
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _position;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _position = 0;
        }

        /// <summary>
        /// Повертає поточний токен. Якщо список закінчився, повертає токен EOF.
        /// </summary>
        private Token Current => _position < _tokens.Count ? _tokens[_position] : new Token(TokenType.EOF, "", 0, 0);

        /// <summary>
        /// Очікує певний тип токена. Якщо він є — повертає його і рухається далі.
        /// Якщо ні — викидає синтаксичну помилку.
        /// </summary>
        private Token Consume(TokenType type)
        {
            if (Current.Type == type) return _tokens[_position++];

            throw new CompilerException($"Expected {type}, but {Current.Type} found.", Current.Line, Current.Column);
        }

        /// <summary>
        /// Перевіряє тип поточного токена. Якщо співпадає — "з'їдає" його і повертає true.
        /// </summary>
        private bool Match(TokenType type)
        {
            if (Current.Type == type)
            {
                _position++;
                return true;
            }
            return false;
        }

        // ==========================================
        // РІВЕНЬ 1: ІНСТРУКЦІЇ (STATEMENTS)
        // ==========================================

        // Граматика: Program ::= (Statement)*
        public AstNode ParseProgram()
        {
            var statements = new List<AstNode>();

            while (Current.Type != TokenType.EOF)
            {
                statements.Add(ParseStatement());
            }

            // Весь код загортаємо в один глобальний BlockNode
            return new BlockNode(new Token(TokenType.LBRACE, "{", 0, 0), statements);
        }

        // Граматика: Statement ::= Print | Declaration | If | While | Block | ExpressionStatement
        public AstNode ParseStatement()
        {
            if (Current.Type == TokenType.KW_PRINT) return ParsePrintStatement();
            if (Current.Type == TokenType.KW_VAR) return ParseDeclaration();
            if (Current.Type == TokenType.KW_IF) return ParseIfStatement();
            if (Current.Type == TokenType.KW_WHILE) return ParseWhileStatement();
            if (Current.Type == TokenType.LBRACE) return ParseBlock();

            // Якщо це не ключове слово, це може бути вираз (наприклад, x = 5 + 2;)
            var expr = ParseExpression();
            Consume(TokenType.SEMICOLON); // Очікуємо крапку з комою
            return expr;
        }

        /// <summary>
        /// Парсинг блоку коду { ... }.
        /// </summary>
        public AstNode ParseBlock()
        {
            var startToken = Consume(TokenType.LBRACE); // {
            var statements = new List<AstNode>();

            // Читаємо інструкції поки не зустрінемо закриваючу дужку або кінець файлу
            while (Current.Type != TokenType.RBRACE && Current.Type != TokenType.EOF)
            {
                statements.Add(ParseStatement());
            }
            Consume(TokenType.RBRACE); // }
            return new BlockNode(startToken, statements);
        }

        /// <summary>
        /// Парсинг IF-ELSE.
        /// </summary>
        public AstNode ParseIfStatement()
        {
            var ifToken = Consume(TokenType.KW_IF);
            Consume(TokenType.LPAREN);
            var condition = ParseExpression();
            Consume(TokenType.RPAREN);

            var thenStmt = ParseStatement();
            AstNode elseStmt = null;

            // Перевіряємо наявність опціонального блоку ELSE
            if (Match(TokenType.KW_ELSE))
            {
                elseStmt = ParseStatement();
            }

            return new IfNode(ifToken, condition, thenStmt, elseStmt);
        }

        /// <summary>
        /// Парсинг циклу WHILE.
        /// </summary>
        public AstNode ParseWhileStatement()
        {
            var whileToken = Consume(TokenType.KW_WHILE);
            Consume(TokenType.LPAREN);
            var condition = ParseExpression();
            Consume(TokenType.RPAREN);

            var body = ParseStatement();

            return new WhileNode(whileToken, condition, body);
        }

        // Граматика: PrintStatement ::= 'print' '(' Expression ')' ';'
        public AstNode ParsePrintStatement()
        {
            var printToken = Consume(TokenType.KW_PRINT);
            Consume(TokenType.LPAREN);
            var expression = ParseExpression();
            Consume(TokenType.RPAREN);
            Consume(TokenType.SEMICOLON);

            // Хак: Представляємо print як псевдо-присвоєння у змінну STDOUT. 
            // Це спрощує генерацію коду пізніше.
            return new BinOpNode(new VariableNode(new Token(TokenType.IDENTIFIER, "STDOUT", 0, 0)), printToken, expression);
        }

        // Граматика: Declaration ::= 'var' IDENTIFIER '=' Expression ';'
        private AstNode ParseDeclaration()
        {
            var varToken = Consume(TokenType.KW_VAR);
            var idToken = Consume(TokenType.IDENTIFIER);
            Consume(TokenType.OP_ASSIGN); // =
            var expr = ParseExpression();
            Consume(TokenType.SEMICOLON);

            // Повертаємо це як операцію присвоєння.
            return new BinOpNode(new VariableNode(idToken), new Token(TokenType.OP_ASSIGN, "=", 0, 0), expr);
        }

        // ==========================================
        // РІВЕНЬ 2: ВИРАЗИ (EXPRESSIONS)
        // Тут визначається пріоритет операцій (Operator Precedence)
        // Чим глибше метод, тим вищий пріоритет.
        // ==========================================

        public AstNode ParseExpression()
        {
            return ParseAssignment();
        }

        private AstNode ParseAssignment()
        {
            // Спочатку читаємо ліву частину (це може бути просто змінна 'x')
            var expr = ParseEquality();

            // Якщо після неї йде знак '=', значить це присвоєння
            if (Current.Type == TokenType.OP_ASSIGN)
            {
                var equals = Consume(TokenType.OP_ASSIGN);

                // Рекурсивно парсимо праву частину (дозволяє a = b = 5)
                var value = ParseAssignment();

                // Перевіряємо, чи зліва стоїть змінна (не можна написати 5 = 10)
                if (expr is VariableNode)
                {
                    return new BinOpNode(expr, equals, value);
                }

                throw new CompilerException("Invalid assignment target.", equals.Line, equals.Column);
            }

            return expr;
        }

        // Пріоритет: ==, !=
        private AstNode ParseEquality()
        {
            var left = ParseComparison();

            while (Current.Type == TokenType.OP_EQUAL || Current.Type == TokenType.OP_NOT_EQUAL)
            {
                var op = Consume(Current.Type);
                var right = ParseComparison();
                left = new BinOpNode(left, op, right);
            }
            return left;
        }

        // Пріоритет: <, >, <=, >=
        private AstNode ParseComparison()
        {
            var left = ParseTerm();

            while (Current.Type == TokenType.OP_LESS || Current.Type == TokenType.OP_GREATER ||
                   Current.Type == TokenType.OP_LESS_EQUAL || Current.Type == TokenType.OP_GREATER_EQUAL)
            {
                var op = Consume(Current.Type);
                var right = ParseTerm();
                left = new BinOpNode(left, op, right);
            }

            return left;
        }

        // Пріоритет: +, - (Ліва асоціативність)
        private AstNode ParseTerm()
        {
            var left = ParseFactor();

            // Використовуємо WHILE для лівої асоціативності: a - b - c => (a - b) - c
            while (Current.Type == TokenType.OP_PLUS || Current.Type == TokenType.OP_MINUS)
            {
                var op = Consume(Current.Type);
                var right = ParseFactor();
                left = new BinOpNode(left, op, right);
            }

            return left;
        }

        // Пріоритет: *, / (Ліва асоціативність)
        private AstNode ParseFactor()
        {
            var left = ParsePower();

            while (Current.Type == TokenType.OP_MULTIPLY || Current.Type == TokenType.OP_DIVIDE)
            {
                var op = Consume(Current.Type);
                var right = ParsePower();
                left = new BinOpNode(left, op, right);
            }

            return left;
        }

        // Пріоритет: ^ (Степінь, Права асоціативність)
        // a ^ b ^ c => a ^ (b ^ c)
        private AstNode ParsePower()
        {
            var left = ParsePrimary();

            if (Current.Type == TokenType.OP_POWER)
            {
                var op = Consume(TokenType.OP_POWER);

                // РЕКУРСІЯ! Тут ми викликаємо ParsePower, а не ParsePrimary.
                // Це створює вкладеність справа наліво.
                var right = ParsePower();
                return new BinOpNode(left, op, right);
            }

            return left;
        }

        // Найвищий пріоритет: Числа, Змінні, Дужки
        private AstNode ParsePrimary()
        {
            if (Current.Type == TokenType.INTEGER_LITERAL)
            {
                return new NumberNode(Consume(TokenType.INTEGER_LITERAL));
            }

            if (Current.Type == TokenType.IDENTIFIER)
            {
                return new VariableNode(Consume(TokenType.IDENTIFIER));
            }

            if (Match(TokenType.LPAREN)) // '('
            {
                var node = ParseExpression(); // Рекурсивно парсимо вираз всередині
                Consume(TokenType.RPAREN);    // ')'
                return node;
            }

            throw new CompilerException($"Not expected Token: {Current.Type}", Current.Line, Current.Column);
        }
    }
}
