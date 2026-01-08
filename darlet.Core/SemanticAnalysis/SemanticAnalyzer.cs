using darlet.Core.Errors;
using darlet.Core.SyntaxAnalysis.AST;
using System;

namespace darlet.Core.SemanticAnalysis
{
    public class SemanticAnalyzer : IVisitor
    {
        private readonly SymbolTable _symbolTable;
        public SemanticAnalyzer()
        {
            _symbolTable = new SymbolTable();
            _symbolTable.Declare("STDOUT");
        }

        public void Analyze(AstNode root)
        {
            root.Accept(this);
        }

        public void Visit(BlockNode node)
        {
            // Для блоку коду (наприклад, у if або while) ми б створювали нову область видимості
             //_symbolTable.EnterScope();
            foreach (var child in node.GetChildren())
            {
                child.Accept(this);
            }
             //_symbolTable.ExitScope();
        }

        public void Visit(NumberNode node)
        {
            // Числа завжди валідні, нічого не робимо
        }

        public void Visit(VariableNode node)
        {
            // Перевірка: чи змінна була оголошена?
            // УВАГА: VariableNode використовується і при оголошенні, і при використанні.
            // Тут ми перевіряємо ВИКОРИСТАННЯ (читання).
            // Але оскільки у нас дерево трохи спрощене, нам треба контекст.

            // Давайте поки просто перевіримо наявність, 
            // але нам треба розрізняти ліву частину присвоєння (декларацію) і праву (використання).
            // Це зазвичай робиться передачею параметрів або розділенням типів вузлів.

            // ДЛЯ СПРОЩЕННЯ: тут ми лише перевіряємо, чи існує змінна.
            // Якщо це частина декларації - її обробить Visit(BinOpNode)
            if (!_symbolTable.Lookup(node.Token.Lexeme))
            {
                // Цю помилку ми викинемо, якщо це не декларація. 
                // Але оскільки у нас Visitor тупий, він не знає контексту.
                // Тому перевірку "IsDeclared" краще робити у батьківському вузлі (Assignment).
            }
        }
        public void Visit(BinOpNode node)
        {
            if (node.Token.Type == LexicalAnalysis.TokenType.OP_ASSIGN)
            {
                // Це присвоєння: x = 5;
                // Або декларація, якщо ми так вирішили. 
                // Щоб розрізнити var x = 5 і x = 5, нам треба було в парсері це зберегти.


                var varName = node.Left.Token.Lexeme;

                // 1. Спочатку аналізуємо праву частину (Expression)
                node.Right.Accept(this);

                // 2. Тепер ліва частина. 
                // Якщо це просто =, то змінна має вже існувати.
                // Якщо у нас була б окрема нода DeclarationNode, ми б робили Declare.

                if (!_symbolTable.Lookup(varName))
                {
                    // Якщо мова вимагає var, тут має бути помилка.
                    // Але оскільки у нас в дереві немає ознаки "var", 
                    // ми додаємо змінну в таблицю тут.
                    _symbolTable.Declare(varName);
                    Console.WriteLine($"[Semantic] Declared variable: {varName}");
                }
            }
            else
            {
                // Звичайна операція (+, -, *, /)
                // Спочатку перевіряємо ліву частину
                // Якщо зліва змінна - вона має існувати.
                if (node.Left is VariableNode varNode)
                {
                    if (!_symbolTable.Lookup(varNode.Token.Lexeme))
                    {
                        throw new CompilerException($"Semantic Error: Variable '{varNode.Token.Lexeme}' used before declaration.",
                            varNode.Token.Line, varNode.Token.Column);
                    }
                }
                else
                {
                    node.Left.Accept(this);
                }

                // Перевіряємо праву частину
                if (node.Right is VariableNode varRight)
                {
                    if (!_symbolTable.Lookup(varRight.Token.Lexeme))
                    {
                        throw new CompilerException($"Semantic Error: Variable '{varRight.Token.Lexeme}' used before declaration.",
                            varRight.Token.Line, varRight.Token.Column);
                    }
                }
                else
                {
                    node.Right.Accept(this);
                }
            }
        }

        public void Visit(IfNode node)
        {
            node.Condition.Accept(this);
            node.ThenBody.Accept(this);
            if (node.ElseBody != null)
            {
                node.ElseBody.Accept(this);
            }
        }

        public void Visit(WhileNode node)
        {
            node.Condition.Accept(this);
            node.Body.Accept(this);
        }
    }
}
