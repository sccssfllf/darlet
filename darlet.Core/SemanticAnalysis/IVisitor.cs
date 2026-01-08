using darlet.Core.SyntaxAnalysis.AST;

namespace darlet.Core.SemanticAnalysis 
{
    public interface IVisitor
    {
        void Visit(BlockNode node);
        void Visit(BinOpNode node);
        void Visit(NumberNode node);
        void Visit(VariableNode node);
        void Visit(IfNode node);
        void Visit(WhileNode node);
    }
}