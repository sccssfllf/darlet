using System;
using System.Collections.Generic;
using darlet.Core.Errors;
using darlet.Core.LexicalAnalysis;
using darlet.Core.SyntaxAnalysis.AST;

namespace darlet.Core.SyntaxAnalysis
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _position;
        
        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _position = 0;
        }

        private Token Current => _position < _tokens.Count ? _tokens[_position] : new Token(TokenType.EOF, "", 0, 0);

        private Token Consume(TokenType type)
        {
            if (Current.Type == type) return _tokens[_position++];

            throw new CompilerException($"Expected {type}, but {Current.Type} found.", Current.Line, Current.Column);
        }
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
        // РІВЕНЬ 1: STATEMENTS
        // ==========================================

        // 1.1 Program ::= (Statement)*
        public AstNode ParseProgram()
        {
            var statements = new List<AstNode>();

            while (Current.Type != TokenType.EOF)
            {
                statements.Add(ParseStatement());
            }

            // Створимо фіктивний токен для кореня
            return new BlockNode(new Token(TokenType.LBRACE, "{", 0, 0), statements);
        }

        // 1.2 Statement ::= Print | Declaration | ...
        public AstNode ParseStatement()
        {
            if (Current.Type == TokenType.KW_PRINT) return ParsePrintStatement();
            if (Current.Type == TokenType.KW_VAR) return ParseDeclaration();
            if (Current.Type == TokenType.KW_IF) return ParseIfStatement();       // <-- Додано
            if (Current.Type == TokenType.KW_WHILE) return ParseWhileStatement(); // <-- Додано
            if (Current.Type == TokenType.LBRACE) return ParseBlock();

            var expr = ParseExpression();
            Consume(TokenType.SEMICOLON);
            return expr;
        }

        public AstNode ParseBlock()
        {
            var startToken = Consume(TokenType.LBRACE); // {
            var statements = new List<AstNode>();
            while (Current.Type != TokenType.RBRACE && Current.Type != TokenType.EOF)
            {
                statements.Add(ParseStatement());
            }
            Consume(TokenType.RBRACE);
            return new BlockNode(startToken, statements);
        }

        public AstNode ParseIfStatement()
        {
            var ifToken = Consume(TokenType.KW_IF);
            Consume(TokenType.LPAREN);
            var condition = ParseExpression();
            Consume(TokenType.RPAREN);

            var thenStmt = ParseStatement();
            AstNode elseStmt = null;

            if (Match(TokenType.KW_ELSE))
            {
                elseStmt = ParseStatement();
            }

            return new IfNode(ifToken, condition, thenStmt, elseStmt);
        }

        public AstNode ParseWhileStatement()
        {
            var whileToken = Consume(TokenType.KW_WHILE);
            Consume(TokenType.LPAREN);
            var condition = ParseExpression();
            Consume(TokenType.RPAREN);

            var body = ParseStatement();

            return new WhileNode(whileToken, condition, body);

        }

        // 1.3 PrintStatement ::= 'print' '(' Expression ')' ';'
        public AstNode ParsePrintStatement()
        {
            var printToken = Consume(TokenType.KW_PRINT);
            Consume(TokenType.LPAREN);
            var expression = ParseExpression();
            Consume(TokenType.RPAREN);
            Consume(TokenType.SEMICOLON);

            return new BinOpNode(new VariableNode(new Token(TokenType.IDENTIFIER, "STDOUT", 0, 0)), printToken, expression);
        }

        // 1.4 Declaration ::= 'var' IDENTIFIER '=' Expression ';'
        private AstNode ParseDeclaration()
        {
            var varToken = Consume(TokenType.KW_VAR);
            var idToken = Consume(TokenType.IDENTIFIER);
            Consume(TokenType.OP_ASSIGN); // =
            var expr = ParseExpression();
            Consume(TokenType.SEMICOLON);

            // Повертаємо вузол присвоєння (або декларації)
            // Використаємо BinOpNode з оператором "=" (varToken тут просто маркер)
            return new BinOpNode(new VariableNode(idToken), new Token(TokenType.OP_ASSIGN, "=", 0, 0), expr);
        }

        // ==========================================
        // РІВЕНЬ 2: EXPRESSIONS
        // ==========================================

        // 2.1 Expression ::= Equality
        public AstNode ParseExpression()
        {
            return ParseEquality();
        }

        // 2.2 Equality ::= Comparison (('=='|'!=') Comparison)*
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

        // 2.3 Comparison ::= Term (('<'|'>'|'<='|'>=') Term)*
        private AstNode ParseComparison()
        {
            var left = ParseTerm();

            // Перевіряємо, чи є оператори порівняння
            while (Current.Type == TokenType.OP_LESS || Current.Type == TokenType.OP_GREATER ||
                   Current.Type == TokenType.OP_LESS_EQUAL || Current.Type == TokenType.OP_GREATER_EQUAL)
            {
                var op = Consume(Current.Type);
                var right = ParseTerm();
                left = new BinOpNode(left, op, right);
            }

            return left;
        }

        // 2.4 Term ::= Factor (('+'|'-') Factor)*
        private AstNode ParseTerm()
        {
            var left = ParseFactor();

            while (Current.Type == TokenType.OP_PLUS || Current.Type == TokenType.OP_MINUS)
            {
                var op = Consume(Current.Type);
                var right = ParseFactor();
                left = new BinOpNode(left, op, right);
            }

            return left;
        }

        // 2.5 Factor ::= Power (('*'|'/') Power)*
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

        // 2.6. Power ::= Primary ('^' Power)? 
        // УВАГА: Це право-асоціативна операція (2^3^2 = 2^(3^2)), тому тут рекурсія замість циклу
        private AstNode ParsePower()
        {
            var left = ParsePrimary();

            if (Current.Type == TokenType.OP_POWER) // Якщо зустріли '^'
            {
                var op = Consume(TokenType.OP_POWER);
                var right = ParsePower(); // <--- Рекурсивний виклик самого себе
                return new BinOpNode(left, op, right);
            }

            return left;
        }

        // 2.7 Primary ::= Number | Ident | '(' Expression ')' ...
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
                var node = ParseExpression(); // Всередині дужок може бути цілий вираз
                Consume(TokenType.RPAREN);    // ')'
                return node;
            }

            throw new CompilerException($"Not expected Token: {Current.Type}", Current.Line, Current.Column);
        }
    }
}