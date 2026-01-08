using darlet.Core.LexicalAnalysis;
using darlet.Core.SemanticAnalysis;
using System.Collections.Generic;

namespace darlet.Core.SyntaxAnalysis.AST
{
    /// <summary>
    /// Вузол умовної конструкції.
    /// Представляє оператор розгалуження: if (Condition) { ThenBody } else { ElseBody }.
    /// Це одна з ключових структур для керування потоком виконання програми.
    /// </summary>
    public class IfNode : AstNode
    {
        /// <summary>
        /// Вираз умови.
        /// Під час семантичного аналізу треба перевірити, що цей вираз повертає Boolean,
        /// або може бути приведений до нього.
        /// </summary>
        public AstNode Condition { get; }

        /// <summary>
        /// Основне тіло (гілка "True").
        /// Виконується, якщо умова істинна.
        /// </summary>
        public AstNode ThenBody { get; }

        /// <summary>
        /// Альтернативне тіло (гілка "False").
        /// Виконується, якщо умова хибна.
        /// Може бути null, якщо блок 'else' у коді відсутній.
        /// </summary>
        public AstNode ElseBody { get; }

        /// <summary>
        /// Створює вузол IF.
        /// </summary>
        /// <param name="token">Ключове слово 'if'.</param>
        /// <param name="condition">Вузол умови.</param>
        /// <param name="thenBody">Вузол блоку виконання.</param>
        /// <param name="elseBody">Вузол блоку else (опціонально).</param>
        public IfNode(Token token, AstNode condition, AstNode thenBody, AstNode elseBody) : base(token)
        {
            Condition = condition;
            ThenBody = thenBody;
            ElseBody = elseBody;
        }

        /// <summary>
        /// Повертає компоненти конструкції для обходу.
        /// Важливо: ElseBody повертається тільки якщо він фізично існує (не null).
        /// </summary>
        public override IEnumerable<AstNode> GetChildren()
        {
            yield return Condition;
            yield return ThenBody;
            if (ElseBody != null) yield return ElseBody;
        }

        /// <summary>
        /// Приймає візитора для обробки логіки розгалуження.
        /// </summary>
        public override void Accept(IVisitor visitor) => visitor.Visit(this);

        public override string ToString() => "If Statement";
    }
}