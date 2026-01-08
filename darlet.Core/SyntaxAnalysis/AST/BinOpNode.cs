using darlet.Core.LexicalAnalysis;
using darlet.Core.SemanticAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace darlet.Core.SyntaxAnalysis.AST
{
    public class BinOpNode : AstNode
    {
        public AstNode Left { get; }
        public AstNode Right { get; }

        public BinOpNode(AstNode left, Token op, AstNode right) : base(op)
        {
            Left = left;
            Right = right;
        }

        public override IEnumerable<AstNode> GetChildren()
        {
            yield return Left;
            yield return Right;
        }

        public override string ToString() => $"BinOpNode ({Token.Lexeme})";
        public override void Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
