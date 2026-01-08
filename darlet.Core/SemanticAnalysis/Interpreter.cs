using darlet.Core.LexicalAnalysis;
using darlet.Core.SyntaxAnalysis.AST;

namespace darlet.Core.SemanticAnalysis
{
    public class Interpreter : IVisitor
    {
        private readonly Dictionary<string, object> _variables = new Dictionary<string, object>();

        private object _lastResult;


        public void Visit(BlockNode node)
        {
            foreach (var stmt in node.GetChildren())
            {
                stmt.Accept(this);
            }
        }

        public void Visit(BinOpNode node)
        {
            if (node.Token.Type == TokenType.OP_ASSIGN)
            {
                var varName = ((VariableNode)node.Left).Token.Lexeme;
                node.Right.Accept(this);
                var value = _lastResult;

                if (_variables.ContainsKey(varName))
                {
                    _variables[varName] = value;
                } else
                {
                    _variables.Add(varName, value);
                }
                return;
            }

            if (node.Left is VariableNode v && v.Token.Lexeme == "STDOUT")
            {
                node.Right.Accept(this);
                Console.WriteLine(_lastResult);
                return;
            }

            node.Left.Accept(this);
            var left = Convert.ToInt32(_lastResult);
            
            node.Right.Accept(this);
            var right = Convert.ToInt32(_lastResult);

            switch(node.Token.Type)
            {
                case TokenType.OP_PLUS: _lastResult = left + right; break;
                case TokenType.OP_MINUS: _lastResult = left - right; break;
                case TokenType.OP_MULTIPLY: _lastResult = left * right; break;
                case TokenType.OP_DIVIDE: _lastResult = left / right; break;

                case TokenType.OP_GREATER: _lastResult = left > right ? 1 : 0; break;
                case TokenType.OP_LESS: _lastResult = left < right ? 1 : 0; break;
                case TokenType.OP_EQUAL: _lastResult = left == right ? 1 : 0; break;
            }
        }

        public void Visit(NumberNode node)
        {
            _lastResult = int.Parse(node.Token.Lexeme);
        }

        public void Visit(VariableNode node)
        {
            if (_variables.ContainsKey(node.Token.Lexeme))
            {
                _lastResult = _variables[node.Token.Lexeme];
            } else
            {
                throw new Exception($"Variable '{node.Token.Lexeme}' not defined.");
            }
        }

        public void Visit(IfNode node)
        {
            node.Condition.Accept(this);
            var result = Convert.ToInt32(_lastResult);

            if (result != 0)
            {
                node.ThenBody.Accept(this);
            } else if (node.ElseBody != null)
            {
                node.ElseBody.Accept(this);
            }
        }

        public void Visit(WhileNode node)
        {
            while (true)
            {
                node.Condition.Accept(this);
                if (Convert.ToInt32(_lastResult) == 0) break;

                node.Body.Accept(this);
            }
        }
    }
}
