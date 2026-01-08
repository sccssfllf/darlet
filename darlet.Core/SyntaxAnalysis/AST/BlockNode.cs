using darlet.Core.LexicalAnalysis;
using darlet.Core.SemanticAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace darlet.Core.SyntaxAnalysis.AST
{
    public class BlockNode : AstNode
    {
        private readonly List<AstNode> _statements;

        public BlockNode(Token token, List<AstNode> statements) : base(token)
        {
            _statements = statements;
        }

        public override IEnumerable<AstNode> GetChildren() => _statements;

        public override string ToString() => "Block";
        public override void Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
