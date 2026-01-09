using darlet.Core.SyntaxAnalysis.AST;

namespace darlet.Core.SemanticAnalysis
{
    /// <summary>
    /// Інтерфейс для реалізації патерну Visitor (Відвідувач).
    /// Дозволяє відокремити алгоритми обробки дерева (семантичний аналіз, генерація коду)
    /// від самих класів вузлів AST.
    /// </summary>
    public interface IVisitor
    {
        /// <summary>
        /// Обробка блоку коду (списку інструкцій у фігурних дужках).
        /// </summary>
        void Visit(BlockNode node);

        /// <summary>
        /// Обробка бінарних операцій (арифметика, присвоєння, порівняння).
        /// </summary>
        void Visit(BinOpNode node);

        /// <summary>
        /// Обробка числових літералів (листків дерева).
        /// </summary>
        void Visit(NumberNode node);

        /// <summary>
        /// Обробка змінних (ідентифікаторів).
        /// </summary>
        void Visit(VariableNode node);

        /// <summary>
        /// Обробка умовної конструкції IF/ELSE.
        /// </summary>
        void Visit(IfNode node);

        /// <summary>
        /// Обробка циклу WHILE.
        /// </summary>
        void Visit(WhileNode node);
        /// <summary>
        /// Обробка рядка String.
        /// </summary>
        void Visit(StringNode node);
    }
}