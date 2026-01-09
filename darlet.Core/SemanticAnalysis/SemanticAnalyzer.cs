using darlet.Core.Errors;
using darlet.Core.SyntaxAnalysis.AST;
using System;

namespace darlet.Core.SemanticAnalysis
{
    /// <summary>
    /// Семантичний аналізатор.
    /// Перевіряє AST дерево на логічні помилки, які не може виявити парсер:
    /// використання неоголошених змінних, перевірка типів (спрощено) та керування таблицею символів.
    /// </summary>
    public class SemanticAnalyzer : IVisitor
    {
        private readonly SymbolTable _symbolTable;

        /// <summary>
        /// Ініціалізує аналізатор та створює глобальну таблицю символів.
        /// </summary>
        public SemanticAnalyzer()
        {
            _symbolTable = new SymbolTable();
            // Реєструємо системні змінні/константи, якщо потрібно
            _symbolTable.Declare("STDOUT");
        }

        /// <summary>
        /// Точка входу для аналізу всього дерева.
        /// </summary>
        /// <param name="root">Кореневий вузол AST.</param>
        public void Analyze(AstNode root)
        {
            root.Accept(this);
        }

        /// <summary>
        /// Відвідує блок коду. Тут повинна відбуватися робота з областями видимості (Scopes).
        /// </summary>
        public void Visit(BlockNode node)
        {
            // У повноцінній мові тут викликався б _symbolTable.EnterScope();

            foreach (var child in node.GetChildren())
            {
                child.Accept(this);
            }

            // _symbolTable.ExitScope();
        }

        /// <summary>
        /// Відвідує числовий вузол. Числа семантично завжди валідні.
        /// </summary>
        public void Visit(NumberNode node)
        {
            // Нічого не робимо
        }

        /// <summary>
        /// Перевіряє використання змінної (читання).
        /// </summary>
        public void Visit(VariableNode node)
        {
            // Примітка: Тут ми перевіряємо лише наявність змінної у таблиці.
            // Контекст (чи це читання, чи запис) має контролювати батьківський вузол (наприклад, BinOpNode).
            if (!_symbolTable.Lookup(node.Token.Lexeme))
            {
                // Помилку тут не викидаємо, залишаючи це на розсуд батьківського вузла (через спрощену архітектуру),
                // або ж тут можна було б викинути "Variable not defined", якщо ми впевнені, що це R-Value.
            }
        }

        /// <summary>
        /// Обробляє бінарні операції. Це найважливіший метод для перевірки декларацій та типів.
        /// </summary>
        public void Visit(BinOpNode node)
        {
            // Логіка для присвоєння (наприклад: x = 5)
            if (node.Token.Type == LexicalAnalysis.TokenType.OP_ASSIGN)
            {
                var varName = node.Left.Token.Lexeme;

                // 1. Спочатку перевіряємо праву частину (вираз, який присвоюємо)
                node.Right.Accept(this);

                // 2. Тепер перевіряємо ліву частину (куди присвоюємо)
                // Якщо змінної немає в таблиці - ми її створюємо (Implicit Declaration).
                // У строгих мовах тут була б помилка, якщо немає ключового слова 'var'.
                if (!_symbolTable.Lookup(varName))
                {
                    _symbolTable.Declare(varName);
                    Console.WriteLine($"[Semantic] Declared variable: {varName}");
                }
            }
            // Логіка для арифметики та порівнянь (+, -, *, /, ==, <, >)
            else
            {
                // 1. Перевірка лівого операнда
                if (node.Left is VariableNode varNode)
                {
                    // Якщо зліва змінна — вона зобов'язана бути оголошеною раніше
                    if (!_symbolTable.Lookup(varNode.Token.Lexeme))
                    {
                        throw new CompilerException(
                            $"Semantic Error: Variable '{varNode.Token.Lexeme}' used before declaration.",
                            varNode.Token.Line, varNode.Token.Column);
                    }
                }
                else
                {
                    // Якщо це вираз — рекурсивно перевіряємо його
                    node.Left.Accept(this);
                }

                // 2. Перевірка правого операнда
                if (node.Right is VariableNode varRight)
                {
                    if (!_symbolTable.Lookup(varRight.Token.Lexeme))
                    {
                        throw new CompilerException(
                            $"Semantic Error: Variable '{varRight.Token.Lexeme}' used before declaration.",
                            varRight.Token.Line, varRight.Token.Column);
                    }
                }
                else
                {
                    node.Right.Accept(this);
                }
            }
        }

        /// <summary>
        /// Перевіряє семантику умовної конструкції IF.
        /// </summary>
        public void Visit(IfNode node)
        {
            node.Condition.Accept(this); // Перевірка умови
            node.ThenBody.Accept(this);  // Перевірка тіла True

            if (node.ElseBody != null)
            {
                node.ElseBody.Accept(this); // Перевірка тіла Else
            }
        }

        /// <summary>
        /// Перевіряє семантику циклу WHILE.
        /// </summary>
        public void Visit(WhileNode node)
        {
            node.Condition.Accept(this);
            node.Body.Accept(this);
        }

        public void Visit(StringNode node)
        {
            // Семантичний аналізатор поки що може ігнорувати рядки,
            // або просто сказати "Ок, це рядок".
            // Порожнього методу достатньо, щоб зняти помилку.
        }
    }
}