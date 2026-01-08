using darlet.Core.LexicalAnalysis;
using darlet.Core.SemanticAnalysis;

namespace darlet.Core.SyntaxAnalysis.AST
{
    public class IfNode : AstNode
    {
        public AstNode Condition { get; }
        public AstNode ThenBody { get; }
        public AstNode ElseBody { get; }

        public IfNode(Token token, AstNode condition, AstNode thenBody, AstNode elseBody) : base(token)
        {
            Condition = condition;
            ThenBody = thenBody;
            ElseBody = elseBody;
        }

        public override IEnumerable<AstNode> GetChildren()
        {
            yield return Condition;
            yield return ThenBody;  
            if (ElseBody != null) yield return ElseBody;
        }

        public override void Accept(IVisitor visitor) => visitor.Visit(this);
        public override string ToString() => "If Statement";
    }
}
