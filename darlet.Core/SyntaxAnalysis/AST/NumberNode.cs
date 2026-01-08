using darlet.Core.LexicalAnalysis;
using darlet.Core.SemanticAnalysis;
using System.Collections.Generic;

namespace darlet.Core.SyntaxAnalysis.AST
{
    /// <summary>
    /// Вузол числового літерала.
    /// Це "листок" (Leaf Node) дерева AST. Він не містить вкладених вузлів.
    /// Представляє конкретні дані: цілі (INTEGER) або дійсні (REAL) числа.
    /// </summary>
    public class NumberNode : AstNode
    {
        public NumberNode(Token token) : base(token)
        {
        }

        /// <summary>
        /// Повертає порожній перелічувач.
        /// Оскільки це листок дерева, у нього немає нащадків.
        /// Тут зупиняється рекурсивний обхід дерева (наприклад, у PrintTree).
        /// </summary>
        public override IEnumerable<AstNode> GetChildren()
        {
            yield break;
        }

        /// <summary>
        /// Виводить тип вузла та саме значення числа для зручності відлагодження.
        /// </summary>
        public override string ToString() => $"Number ({Token.Lexeme})";

        /// <summary>
        /// Приймає відвідувача.
        /// SemanticAnalyzer зазвичай ігнорує цей вузол (числа завжди семантично валідні),
        /// а CodeGenerator генерує інструкцію завантаження константи (наприклад, push або ldc).
        /// </summary>
        public override void Accept(IVisitor visitor) => visitor.Visit(this);
    }
}