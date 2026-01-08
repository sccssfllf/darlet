using darlet.Core.LexicalAnalysis;
using darlet.Core.SemanticAnalysis;
using System;
using System.Collections.Generic;

namespace darlet.Core.SyntaxAnalysis.AST
{
    /// <summary>
    /// Вузол бінарної операції.
    /// Представляє будь-яку конструкцію, що має два операнди (лівий і правий) та оператор між ними.
    /// Приклади:
    /// 1. Арифметика: a + b
    /// 2. Порівняння: a > b
    /// 3. Присвоєння: a = b (де 'a' - ліва частина, 'b' - права частина)
    /// </summary>
    public class BinOpNode : AstNode
    {
        /// <summary>
        /// Лівий операнд (Left Hand Side - LHS).
        /// Наприклад, у виразі "x + 5" це вузол змінної "x".
        /// </summary>
        public AstNode Left { get; }

        /// <summary>
        /// Правий операнд (Right Hand Side - RHS).
        /// Наприклад, у виразі "x + 5" це вузол числа "5".
        /// </summary>
        public AstNode Right { get; }

        /// <summary>
        /// Створює вузол.
        /// </summary>
        /// <param name="left">Вузол зліва.</param>
        /// <param name="op">Токен самого оператора (+, -, *, /, =, == тощо).</param>
        /// <param name="right">Вузол справа.</param>
        public BinOpNode(AstNode left, Token op, AstNode right) : base(op)
        {
            Left = left;
            Right = right;
        }

        /// <summary>
        /// Повертає список дітей для обходу дерева.
        /// Важливий порядок: спочатку обробляється ліва частина, потім права (зазвичай).
        /// </summary>
        public override IEnumerable<AstNode> GetChildren()
        {
            yield return Left;
            yield return Right;
        }

        /// <summary>
        /// Для зручності налагодження виводимо сам знак операції.
        /// </summary>
        public override string ToString() => $"BinOpNode ({Token.Lexeme})";

        /// <summary>
        /// Реалізація Visitor: направляє потік виконання у метод Visit(BinOpNode).
        /// Там буде вирішуватися, що робити: додавати, віднімати чи присвоювати.
        /// </summary>
        public override void Accept(IVisitor visitor) => visitor.Visit(this);
    }
}