using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

    public interface IFunctionNode : INode
    {
        FunctionType FunctionType { get; }
        List<INode> Children { get; }
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

    public class MathFunctionNode : IFunctionNode
    {
        public FunctionType FunctionType { get; }
        public List<INode> Children { get; }

        public MathFunctionNode(FunctionType functionType, List<INode> children)
        {
            FunctionType = functionType;
            Children = new List<INode>();

        }
        public double Evaluate()
        {
            switch (FunctionType)
            {
                case FunctionType.Sin:
                    return Math.Sin(Children[0].Evaluate());
                case FunctionType.Cos:
                    return Math.Cos(Children[0].Evaluate());
                default:
                    throw new InvalidOperationException("Invalid function type");
            }
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
            switch (OperatorType)
            {
                case OperatorType.Addition:
                    return Children[0].Evaluate() + Children[1].Evaluate();
                case OperatorType.Subtraction:
                    return Subtract();
                case OperatorType.Multiplication:
                    return Children[0].Evaluate() * Children[1].Evaluate();
                case OperatorType.Division:
                    double divisor = Children[0].Evaluate();
                    if (Math.Abs(divisor) < double.Epsilon)
                        throw new DivideByZeroException("Division by zero");
                    return Children[1].Evaluate() / divisor;
                default:
                    throw new InvalidOperationException("Invalid operator type");
            }
        }

        private double Subtract()
        {
            if (Children.Count < 2)
            {
                throw new InvalidOperationException("Not enough operands for subtraction.");
            }

            double leftValue = Children[0].Evaluate();
            double rightValue = Children[1].Evaluate();

            return rightValue - leftValue;
        }
    }

    public class MathASTBuilder
    {
        public List<INode> BuildSyntaxTree(List<IToken> infixNotationTokens)
        {
            List<INode> tree = new List<INode>();
            Stack<OperatorToken> operatorStack = new Stack<OperatorToken>();
            Stack<INode> operandStack = new Stack<INode>();

            foreach (var token in infixNotationTokens)
            {
                if (token is OperandToken)
                {
                    operandStack.Push(new MathOperandNode(((OperandToken)token).Value));
                }
/*                else if (token is FunctionToken)
                {
                    FunctionToken functionToken = (FunctionToken)token;

                    List<INode> argumentNodes = new List<INode>();
                    foreach (var argument in functionToken.Arguments)
                    {
                        if (argument is INode argumentNode)
                        {
                            argumentNodes.Add(argumentNode);
                        }
                        else
                        {
                            throw new InvalidOperationException("Invalid argument type for function node");
                        }
                    }
                    operandStack.Push(new MathFunctionNode(functionToken.FunctionType, argumentNodes));
                }*/

                else if (token is OperatorToken)
                {
                    OperatorToken currentOperator = (OperatorToken)token;

                    if (currentOperator.OperatorType == OperatorType.OpeningBracket)
                    {
                        // Открывающая скобка  помещаем ее в стек
                        operatorStack.Push(currentOperator);
                    }
                    else if (currentOperator.OperatorType == OperatorType.ClosingBracket)
                    {
                        // Закрывающая скобка  обрабатываем выражение в скобках
                        while (operatorStack.Count > 0 && operatorStack.Peek().OperatorType != OperatorType.OpeningBracket)
                        {
                            PopAndAddOperatorToTree(operatorStack, operandStack);
                        }

                        // Удаляем открывающую скобку из стека
                        if (operatorStack.Count > 0 && operatorStack.Peek().OperatorType == OperatorType.OpeningBracket)
                        {
                            operatorStack.Pop();
                        }
                    }
                    else
                    {
                        // Оператор - обрабатываем приоритеты
                        while (operatorStack.Count > 0 &&
                               GetOperatorPriority(operatorStack.Peek().OperatorType) >= GetOperatorPriority(currentOperator.OperatorType))
                        {
                            PopAndAddOperatorToTree(operatorStack, operandStack);
                        }

                        // Помещаем текущий оператор в стек
                        operatorStack.Push(currentOperator);
                    }
                }
            }

            // Очищаем оставшиеся операторы из стека
            while (operatorStack.Count > 0)
            {
                PopAndAddOperatorToTree(operatorStack, operandStack);
            }

            tree.AddRange(operandStack.Reverse());

            return tree;
        }

        private void PopAndAddOperatorToTree(Stack<OperatorToken> operatorStack, Stack<INode> operandStack)
        {
            if (operandStack.Count < 2)
            {
                throw new InvalidOperationException("Not enough operands for the remaining operators.");
            }

            OperatorToken poppedOperator = operatorStack.Pop();
            operandStack.Push(new MathOperatorNode(poppedOperator.OperatorType, operandStack.Pop(), operandStack.Pop()));
        }

        private int GetOperatorPriority(OperatorType operatorType)
        {
            switch (operatorType)
            {
                case OperatorType.Multiplication:
                case OperatorType.Division:
                    return 2;
                case OperatorType.Addition:
                case OperatorType.Subtraction:
                    return 1;
                default:
                    return 0; 
            }
        }
    }

    class Program
    {
        static void Main()
        {
            string mathExpression = "Sin()";
            Tokenizer tokenizer = new Tokenizer();
            List<IToken> infixNotationTokens = tokenizer.Parse(mathExpression);

            MathASTBuilder astBuilder = new MathASTBuilder();
            var rootNode = astBuilder.BuildSyntaxTree(infixNotationTokens);

            PrintTreeStructure(rootNode[0], 0);

            Console.WriteLine($"Syntax Tree Result: {rootNode[0].Evaluate()}");

        }
        static void PrintTreeStructure(INode node, int indentLevel)
        {
            if (node is MathOperandNode operandNode)
            {
                PrintIndentedLine($"OperandNode: {operandNode.Value}", indentLevel);
            }
            else if (node is MathOperatorNode operatorNode)
            {
                PrintIndentedLine($"OperatorNode: {operatorNode.OperatorType}", indentLevel);
                foreach (var child in operatorNode.Children)
                {
                    PrintTreeStructure(child, indentLevel + 1);
                }
            }
        }

        static void PrintIndentedLine(string text, int indentLevel)
        {
            Console.WriteLine($"{new string(' ', indentLevel * 2)}{text}");
        }

    }
}
