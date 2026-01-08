using darlet.Core.LexicalAnalysis;
using darlet.Core.SemanticAnalysis;
using System.Collections.Generic;

namespace darlet.Core.SyntaxAnalysis.AST
{
    /// <summary>
    /// Вузол, що представляє змінну (ідентифікатор).
    /// Це "листок" (Leaf Node) дерева AST, оскільки змінна не містить вкладених інструкцій.
    /// </summary>
    public class VariableNode : AstNode
    {
        public VariableNode(Token token) : base(token)
        {
        }

        /// <summary>
        /// Повертає порожній список дітей.
        /// Рекурсія обходу дерева зупиняється на змінних.
        /// </summary>
        public override IEnumerable<AstNode> GetChildren()
        {
            yield break;
        }

        /// <summary>
        /// Повертає назву змінної для зручності відлагодження.
        /// Наприклад: "Var (counter)"
        /// </summary>
        public override string ToString() => $"Var ({Token.Lexeme})";

        /// <summary>
        /// Приймає відвідувача.
        /// У SemanticAnalyzer тут відбувається перевірка:
        /// 1. Чи оголошена ця змінна? (Lookup в SymbolTable)
        /// 2. Чи правильно вона використовується?
        /// </summary>
        public override void Accept(IVisitor visitor) => visitor.Visit(this);
    }
}