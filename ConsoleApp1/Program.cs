using System;
using System.Collections.Generic;
using System.Linq;

namespace MathSyntaxTree
{
    public interface INode
    {
        double Evaluate();
    }

    public interface IOperandNode : INode
    {
        double Value { get; }
    }

    public interface IOperatorNode : INode
    {
        OperatorType OperatorType { get; }
        List<INode> Children { get; }
    }
    public interface INodeBuilder
    {
        INode BuildNode(IToken token, params INode[] children);
    }

    public class MathOperandNode : IOperandNode
    {
        public double Value { get; }

        public MathOperandNode(double value)
        {
            Value = value;
        }

        public double Evaluate()
        {
            return Value;
        }
    }

    public class MathOperatorNode : IOperatorNode
    {
        public OperatorType OperatorType { get; }
        public List<INode> Children { get; }

        public MathOperatorNode(OperatorType operatorType, params INode[] children)
        {
            OperatorType = operatorType;
            Children = children.ToList();
        }

        public double Evaluate()
        {
            double result = Children[0].Evaluate();

            for (int i = 1; i < Children.Count; i += 2)
            {
                double nextValue = Children[i + 1].Evaluate();

                switch (OperatorType)
                {
                    case OperatorType.Addition:
                        result += nextValue;
                        break;
                    case OperatorType.Subtraction:
                        result -= nextValue;
                        break;
                    case OperatorType.Multiplication:
                        result *= nextValue;
                        break;
                    case OperatorType.Division:
                        if (Math.Abs(nextValue) < double.Epsilon)
                            throw new DivideByZeroException("Division by zero");
                        result /= nextValue;
                        break;
                    default:
                        throw new InvalidOperationException("Invalid operator type");
                }
            }

            return result;
        }
    }

    public class MathNodeBuilder : INodeBuilder
    {
        public INode BuildNode(IToken token, params INode[] children)
        {
            if (token is OperandToken operandToken)
            {
                return new MathOperandNode(operandToken.Value);
            }
            else if (token is OperatorToken operatorToken)
            {
                return new MathOperatorNode(operatorToken.OperatorType, children);
            }
            throw new InvalidOperationException("Invalid token type");
        }
    }

    public class MathASTBuilder
    {
        private readonly INodeBuilder _nodeBuilder;

        public MathASTBuilder(INodeBuilder nodeBuilder)
        {
            _nodeBuilder = nodeBuilder;
        }

        public INode BuildSyntaxTree(IEnumerable<IToken> infixNotationTokens)
        {
            var stack = new Stack<INode>();
            var operatorStack = new Stack<OperatorToken>();

            foreach (var token in infixNotationTokens)
            {
                if (token is OperandToken operandToken)
                {
                    stack.Push(_nodeBuilder.BuildNode(operandToken));
                }
                else if (token is OperatorToken operatorToken)
                {
                    while (operatorStack.Count > 0 &&
                           GetOperatorPriority(operatorStack.Peek().OperatorType) >= GetOperatorPriority(operatorToken.OperatorType))
                    {
                        INode rightChild = stack.Pop();
                        INode leftChild = stack.Pop();
                        stack.Push(_nodeBuilder.BuildNode(operatorStack.Pop(), leftChild, rightChild));
                    }
                    operatorStack.Push(operatorToken);
                }
            }

            while (operatorStack.Count > 0)
            {
                INode rightChild = stack.Pop();
                INode leftChild = stack.Pop();
                stack.Push(_nodeBuilder.BuildNode(operatorStack.Pop(), leftChild, rightChild));
            }

            if (stack.Count != 1 || operatorStack.Count != 0)
            {
                throw new InvalidOperationException("Invalid infix notation");
            }

            return stack.Pop();
        }

        private int GetOperatorPriority(OperatorType operatorType)
        {
            switch (operatorType)
            {
                case OperatorType.Addition:
                case OperatorType.Subtraction:
                    return 1;
                case OperatorType.Multiplication:
                case OperatorType.Division:
                    return 2;
                case OperatorType.OpeningBracket:
                case OperatorType.ClosingBracket:
                    return 3;
                default:
                    throw new InvalidOperationException("Invalid operator type");
            }
        }
    }



    public static class StackExtensions
    {
        public static INode[] PopChildren(this Stack<INode> stack, int count)
        {
            var children = new INode[count];
            for (int i = count - 1; i >= 0; i--)
            {
                children[i] = stack.Pop();
            }
            return children;
        }
    }

    class Program
    {
        static void Main()
        {
            string mathExpression = "3 + 5 * (2 - 8)";
            Tokenizer tokenizer = new Tokenizer();
            IEnumerable<IToken> infixNotationTokens = tokenizer.Parse(mathExpression);

            MathNodeBuilder nodeBuilder = new MathNodeBuilder();
            MathASTBuilder astBuilder = new MathASTBuilder(nodeBuilder);
            INode rootNode = astBuilder.BuildSyntaxTree(infixNotationTokens);

            //PrintTreeStructure(rootNode, 0);

            Console.WriteLine($"Syntax Tree Result: {rootNode.Evaluate()}");

        }
        /*        static void PrintTreeStructure(INode node, int indentLevel)
                {
                    if (node is MathOperandNode operandNode)
                    {
                        PrintIndentedLine($"OperandNode: {operandNode.Value}", indentLevel);
                    }
                    else if (node is MathOperatorNode operatorNode)
                    {
                        PrintIndentedLine($"OperatorNode: {operatorNode.OperatorType}", indentLevel);
                        PrintTreeStructure(operatorNode.Left, indentLevel + 1);
                        PrintTreeStructure(operatorNode.Right, indentLevel + 1);
                    }
                }

                static void PrintIndentedLine(string text, int indentLevel)
                {
                    Console.WriteLine($"{new string(' ', indentLevel * 2)}{text}");
                }*/
    }
}
