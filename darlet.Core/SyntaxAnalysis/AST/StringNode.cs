using darlet.Core.LexicalAnalysis;
using darlet.Core.SemanticAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace darlet.Core.SyntaxAnalysis.AST
{
    public class StringNode : AstNode
    {
        public StringNode(Token token) : base(token) { }

        public override IEnumerable<AstNode> GetChildren()
        {
            yield break;
        }

        public override string ToString()
        {
            return $"String (\"{Token.Lexeme}\")";
        }

        // Це для Візитора
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
