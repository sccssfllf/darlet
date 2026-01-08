using darlet.Core.LexicalAnalysis;
using darlet.Core.SemanticAnalysis;
using System.Collections.Generic;

namespace darlet.Core.SyntaxAnalysis.AST
{
    public class NumberNode : AstNode
    {
        public NumberNode(Token token) : base(token)
        {
        }

        // У числа немає дітей, це кінець гілки
        public override IEnumerable<AstNode> GetChildren()
        {
            yield break;
        }

        public override string ToString() => $"Number ({Token.Lexeme})";

        public override void Accept(IVisitor visitor) => visitor.Visit(this);
    }
}