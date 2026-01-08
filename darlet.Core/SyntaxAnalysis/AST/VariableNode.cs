using darlet.Core.LexicalAnalysis;
using darlet.Core.SemanticAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace darlet.Core.SyntaxAnalysis.AST
{
    public class VariableNode : AstNode
    {
        public VariableNode(Token token) : base(token)
        {
        }

        public override IEnumerable<AstNode> GetChildren()
        {
            yield break;
        }

        public override string ToString() => $"Var ({Token.Lexeme})";
        public override void Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
