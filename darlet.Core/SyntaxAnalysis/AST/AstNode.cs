using darlet.Core.LexicalAnalysis;
using darlet.Core.SemanticAnalysis;
using System;
using System.Collections.Generic;

namespace darlet.Core.SyntaxAnalysis.AST
{
    public abstract class AstNode
    {
        public Token Token {  get; }

        protected AstNode(Token token) 
        {
            Token = token;
        }

        public abstract IEnumerable<AstNode> GetChildren();

        public override string ToString()
        {
            return GetType().Name;
        }

        public abstract void Accept(IVisitor visitor);

        public void PrintTree(string indent = "", bool isLast = true)
        {
            // Використовуємо прості символи замість графічних
            var marker = isLast ? "+-- " : "+-- ";

            Console.Write(indent);
            Console.Write(marker);

            // Виводимо інформацію про сам вузол
            Console.Write(this.ToString());

            // Якщо є токен, дописуємо його значення для ясності
            if (Token != null && !(this is BinOpNode) && !(this is NumberNode))
            {
                Console.Write($" [{Token.Lexeme}]");
            }
            Console.WriteLine();

            // Підготовка відступу для дітей
            indent += isLast ? "    " : "|   ";

            var children = GetChildren().ToList(); // ToList потрібен для доступу за індексом
            for (int i = 0; i < children.Count; i++)
            {
                children[i].PrintTree(indent, i == children.Count - 1);
            }
        }
    }
}