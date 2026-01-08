using darlet.Core.LexicalAnalysis;
using darlet.Core.SemanticAnalysis;
using System.Collections.Generic;

namespace darlet.Core.SyntaxAnalysis.AST
{
    /// <summary>
    /// Вузол циклу WHILE.
    /// Представляє конструкцію повторення: while (Condition) { Body }.
    /// Це вузол керування потоком виконання (Control Flow), який виконує тіло циклу доти,
    /// доки умова залишається істинною.
    /// </summary>
    public class WhileNode : AstNode
    {
        /// <summary>
        /// Вираз умови.
        /// Оцінюється перед кожною ітерацією.
        /// Семантичний аналізатор повинен перевірити, чи повертає цей вузол булеве значення.
        /// </summary>
        public AstNode Condition { get; }

        /// <summary>
        /// Тіло циклу.
        /// Код, який буде виконуватися багаторазово.
        /// Зазвичай це BlockNode (набір інструкцій у фігурних дужках).
        /// </summary>
        public AstNode Body { get; }

        /// <summary>
        /// Створює вузол циклу.
        /// </summary>
        /// <param name="token">Ключове слово 'while'.</param>
        /// <param name="condition">Вузол умови.</param>
        /// <param name="body">Вузол тіла циклу.</param>
        public WhileNode(Token token, AstNode condition, AstNode body) : base(token)
        {
            Condition = condition;
            Body = body;
        }

        /// <summary>
        /// Приймає відвідувача.
        /// SemanticAnalyzer перевірить валідність умови та тіла.
        /// Interpreter/CodeGenerator згенерує інструкції переходу (JUMP/BRANCH) для реалізації циклу.
        /// </summary>
        public override void Accept(IVisitor visitor) => visitor.Visit(this);

        /// <summary>
        /// Повертає дочірні вузли для обходу дерева.
        /// Спочатку умова, потім тіло.
        /// </summary>
        public override IEnumerable<AstNode> GetChildren()
        {
            yield return Condition;
            yield return Body;
        }

        public override string ToString() => "While Loop";
    }
}