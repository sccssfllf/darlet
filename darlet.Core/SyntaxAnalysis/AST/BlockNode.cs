using darlet.Core.LexicalAnalysis;
using darlet.Core.SemanticAnalysis;
using System.Collections.Generic;

namespace darlet.Core.SyntaxAnalysis.AST
{
    /// <summary>
    /// Вузол блоку коду.
    /// Представляє послідовність інструкцій (statements), які виконуються одна за одною.
    /// Зазвичай це код, взятий у фігурні дужки { ... }, або весь файл програми.
    /// </summary>
    public class BlockNode : AstNode
    {
        // Список інструкцій, що входять до цього блоку
        private readonly List<AstNode> _statements;

        /// <summary>
        /// Створює вузол блоку.
        /// </summary>
        /// <param name="token">Токен, що позначає початок блоку (зазвичай '{').</param>
        /// <param name="statements">Список розпарсених інструкцій всередині блоку.</param>
        public BlockNode(Token token, List<AstNode> statements) : base(token)
        {
            _statements = statements;
        }

        /// <summary>
        /// Повертає всі інструкції блоку як дочірні вузли.
        /// Це дозволяє візитору (SemanticAnalyzer або CodeGenerator) пройтись по них по черзі.
        /// </summary>
        public override IEnumerable<AstNode> GetChildren() => _statements;

        public override string ToString() => "Block";

        /// <summary>
        /// Приймає відвідувача. 
        /// У SemanticAnalyzer це призведе до обходу всіх інструкцій всередині цього методу,
        /// і, можливо, до створення нової області видимості (Scope).
        /// </summary>
        public override void Accept(IVisitor visitor) => visitor.Visit(this);
    }
}