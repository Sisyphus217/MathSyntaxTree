using System;
using System.Collections.Generic;
using System.Linq;

namespace MathSyntaxTree
{
    public interface INode
    {
        double Evaluate();
    }

    public class OperandNode : INode
    {
        public double Value { get; }

        public OperandNode(double value)
        {
            Value = value;
        }

        public double Evaluate()
        {
            return Value;
        }
    }

    public class OperatorNode : INode
    {
        public OperatorType OperatorType { get; }
        public INode Left { get; }
        public INode Right { get; }

        public OperatorNode(OperatorType operatorType, INode left, INode right)
        {
            OperatorType = operatorType;
            Left = left;
            Right = right;
        }

        public double Evaluate()
        {
            double leftValue = Left.Evaluate();
            double rightValue = Right.Evaluate();

            switch (OperatorType)
            {
                case OperatorType.Addition:
                    return leftValue + rightValue;
                case OperatorType.Subtraction:
                    return leftValue - rightValue;
                case OperatorType.Multiplication:
                    return leftValue * rightValue;
                case OperatorType.Division:
                    if (Math.Abs(rightValue) < double.Epsilon)
                        throw new DivideByZeroException("Division by zero");
                    return leftValue / rightValue;
                default:
                    throw new InvalidOperationException("Invalid operator type");
            }
        }
    }

    public class SyntaxTreeBuilder
    {
        public INode BuildSyntaxTree(IEnumerable<IToken> postfixNotationTokens)
        {
            var stack = new Stack<INode>();

            foreach (var token in postfixNotationTokens)
            {
                if (token is OperandToken operandToken)
                {
                    stack.Push(new OperandNode(operandToken.Value));
                }
                else if (token is OperatorToken operatorToken)
                {
                    INode right = stack.Pop();
                    INode left = stack.Pop();
                    stack.Push(new OperatorNode(operatorToken.OperatorType, left, right));
                }
            }

            if (stack.Count != 1)
            {
                throw new InvalidOperationException("Invalid postfix notation");
            }

            return stack.Pop();
        }
    }

    class Program
    {
        static void Main()
        {
            string mathExpression = "3 + 5 * (2 - 8)";
            Tokenizer tokenizer = new Tokenizer();
            IEnumerable<IToken> infixNotationTokens = tokenizer.Parse(mathExpression);

            ShuntingYardAlgorithm shuntingYard = new ShuntingYardAlgorithm();
            IEnumerable<IToken> postfixNotationTokens = shuntingYard.Apply(infixNotationTokens);

            SyntaxTreeBuilder treeBuilder = new SyntaxTreeBuilder();
            INode rootNode = treeBuilder.BuildSyntaxTree(postfixNotationTokens);

            PrintTreeStructure(rootNode, 0);

            Console.WriteLine($"Syntax Tree Result: {rootNode.Evaluate()}");
        }
        static void PrintTreeStructure(INode node, int indentLevel)
        {
            if (node is OperandNode operandNode)
            {
                PrintIndentedLine($"OperandNode: {operandNode.Value}", indentLevel);
            }
            else if (node is OperatorNode operatorNode)
            {
                PrintIndentedLine($"OperatorNode: {operatorNode.OperatorType}", indentLevel);
                PrintTreeStructure(operatorNode.Left, indentLevel + 1);
                PrintTreeStructure(operatorNode.Right, indentLevel + 1);
            }
        }

        static void PrintIndentedLine(string text, int indentLevel)
        {
            Console.WriteLine($"{new string(' ', indentLevel * 2)}{text}");
        }
    }
}
