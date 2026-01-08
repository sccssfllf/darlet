using darlet.Core.LexicalAnalysis;
using darlet.Core.SemanticAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace darlet.Core.SyntaxAnalysis.AST
{
    public class WhileNode : AstNode
    {
        public AstNode Condition { get; }

        public AstNode Body { get; }

        public WhileNode(Token token, AstNode condition, AstNode body) : base(token)
        {
            Condition = condition;  
            Body = body;
        }


        public override void Accept(IVisitor visitor) => visitor.Visit(this);

        public override IEnumerable<AstNode> GetChildren()
        {
            yield return Condition;
            yield return Body;
        }

        public override string ToString() => "While Loop";
    }
}
