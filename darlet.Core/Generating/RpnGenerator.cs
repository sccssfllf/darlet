using darlet.Core.LexicalAnalysis;
using darlet.Core.SemanticAnalysis;
using darlet.Core.SyntaxAnalysis.AST;
using System.Text;

namespace darlet.Core.Generating
{
    public class RpnGenerator : IVisitor
    {
        private readonly StringBuilder _output = new StringBuilder();
        private int _labelCounter = 0;

        public string GetOutput() => _output.ToString().Trim();

        public void Visit(BlockNode node)
        {
            foreach (var child in node.GetChildren())
            {
                child.Accept(this);
            }
        }

        public void Visit(NumberNode node)
        {
            _output.Append($"{node.Token.Lexeme} ");
        }

        public void Visit(VariableNode node)
        {
            _output.Append($"{node.Token.Lexeme} ");
        }

        public void Visit(BinOpNode node)
        {
            if (node.Token.Type == TokenType.KW_PRINT)
            {
                node.Right.Accept(this);
                _output.Append("PRINT "); 
                return;
            }

            if (node.Token.Type == TokenType.OP_ASSIGN)
            {
                node.Left.Accept(this);

                node.Right.Accept(this);

                _output.Append("= ");
            }
            else
            {
                node.Left.Accept(this);
                node.Right.Accept(this);
                _output.Append($"{node.Token.Lexeme} ");
            }
        }

        public void Visit(IfNode node)
        {
            var labelElse = NewLabel();
            var labelEnd= NewLabel();

            node.Condition.Accept(this);
            _output.Append($"{labelElse} !F ");

            node.ThenBody.Accept(this);

            _output.Append($"{labelEnd} ! ");

            _output.Append($"{labelElse}: !");

            if(node.ElseBody != null)
            {
                node.ElseBody.Accept(this);
            }

            _output.Append($"{labelEnd}: ");
        }

        public void Visit(WhileNode node)
        {
            var labelStart = NewLabel();
            var labelEnd = NewLabel();

            _output.Append($"{labelStart}: ");

            node.Condition.Accept(this);

            _output.Append($"{labelEnd} !F ");

            node.Body.Accept(this);

            _output.Append($"{labelStart} ! ");

            _output.Append($"{labelEnd}: ");
        }

        private string NewLabel() => $"m{_labelCounter++}";
    }
}