using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MathSyntaxTree
{
    public interface INode<T>
    {
        T Evaluate();
    }

    public interface IOperandNode<T> : INode<T>
    {
        T Value { get; }
    }

    public interface IOperatorNode<T> : INode<T>
    {
        OperatorType OperatorType { get; }
        List<INode<T>> Children { get; }
    }


    public interface IFunctionNode<T> : INode<T>
    {
        FunctionType FunctionType { get; }
        List<INode<T>> Children { get; }
    }

    public class MathOperandNode<T> : IOperandNode<T>
    {
        public T Value { get; }

        public MathOperandNode(T value)
        {
            Value = value;
        }

        public T Evaluate()
        {
            return Value;
        }
    }

    public class MathFunctionNode<T> : IFunctionNode<T>
    {
        public FunctionType FunctionType { get; }
        public List<INode<T>> Children { get; } = new List<INode<T>>();

        public MathFunctionNode(FunctionType functionType, List<INode<T>> children)
        {
            FunctionType = functionType;
            Children.AddRange(children);
        }

        public T Evaluate()
        {
            switch (FunctionType)
            {
                case FunctionType.Str:
                    if (typeof(T) == typeof(string))
                    {
                        string strResult = $"String result: {Children[0].Evaluate()}";
                        Console.WriteLine(strResult);
                        return (T)(object)strResult;
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid type for Str function");
                    }
                case FunctionType.Sin:
                    return (T)(object)Math.Sin(Convert.ToDouble(Children[0].Evaluate()));
                case FunctionType.Cos:
                    return (T)(object)Math.Cos(Convert.ToDouble(Children[0].Evaluate()));
                case FunctionType.Tan:
                    return (T)(object)Math.Tan(Convert.ToDouble(Children[0].Evaluate()));
                case FunctionType.Sqrt:
                    return (T)(object)Math.Sqrt(Convert.ToDouble(Children[0].Evaluate()));
                case FunctionType.Ln:
                    return (T)(object)Math.Log(Convert.ToDouble(Children[0].Evaluate()));
                case FunctionType.Pow:
                    return (T)(object)Math.Pow(Convert.ToDouble(Children[0].Evaluate()), Convert.ToDouble(Children[1].Evaluate()));
                case FunctionType.Abs:
                    return (T)(object)Math.Abs(Convert.ToDouble(Children[0].Evaluate()));
                default:
                    throw new InvalidOperationException("Invalid function type");
            }
        }

    }

    public class MathOperatorNode<T> : IOperatorNode<T>
    {
        public OperatorType OperatorType { get; }
        public List<INode<T>> Children { get; }

        public MathOperatorNode(OperatorType operatorType, params INode<T>[] children)
        {
            OperatorType = operatorType;
            Children = children.ToList();
        }

        public T Evaluate()
        {
            switch (OperatorType)
            {
                case OperatorType.Addition:
                    return Addition();
                case OperatorType.Subtraction:
                    return Subtract();
                case OperatorType.Multiplication:
                    return Multiply();
                case OperatorType.Division:
                    return Divide();
                default:
                    throw new InvalidOperationException("Invalid operator type");
            }
        }

        private T Addition()
        {
            return OperatorHelper<T>.Addition(Children[0].Evaluate(), Children[1].Evaluate());
        }

        private T Subtract()
        {
            return OperatorHelper<T>.Subtract(Children[0].Evaluate(), Children[1].Evaluate());
        }

        private T Multiply()
        {
            return OperatorHelper<T>.Multiply(Children[0].Evaluate(), Children[1].Evaluate());
        }

        private T Divide()
        {
            T divisor = Children[0].Evaluate();
            if (OperatorHelper<T>.IsZero(divisor))
                throw new DivideByZeroException("Division by zero");

            return OperatorHelper<T>.Divide(Children[1].Evaluate(), divisor);
        }
    }

    public static class OperatorHelper<T>
    {
        public static T Addition(T a, T b)
        {
            if (a == null || b == null)
            {
                throw new ArgumentNullException("Cannot add null values.");
            }

            return (dynamic)a + (dynamic)b;
        }


        public static T Subtract(T a, T b)
        {
            if (a == null || b == null)
            {
                throw new ArgumentNullException("Cannot add null values.");
            }
            return (dynamic)a - (dynamic)b;
        }

        public static T Multiply(T a, T b)
        {
            if (a == null || b == null)
            {
                throw new ArgumentNullException("Cannot add null values.");
            }
            return (dynamic)a * (dynamic)b;
        }

        public static T Divide(T a, T b)
        {
            if (a == null || b == null)
            {
                throw new ArgumentNullException("Cannot add null values.");
            }
            return (dynamic)a / (dynamic)b;
        }

        public static bool IsZero(T value)
        {
            return EqualityComparer<T>.Default.Equals(value, default(T));
        }
    }


    public class MathASTBuilder
    {
        public List<INode<T>> BuildSyntaxTree<T>(List<IToken> infixNotationTokens)
        {
            List<INode<T>> tree = new List<INode<T>>();
            Stack<OperatorToken> operatorStack = new Stack<OperatorToken>();
            Stack<INode<T>> operandStack = new Stack<INode<T>>();

            foreach (var token in infixNotationTokens)
            {
                if (token is OperandToken<T> operandToken)
                {
                    operandStack.Push(new MathOperandNode<T>(operandToken.Value));
                }
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
                else if (token is FunctionToken)
                {
                    // Функция - обрабатываем аргументы
                    operandStack.Push(BuildFunctionNode<T>((FunctionToken)token));
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

        private INode<T> BuildFunctionNode<T>(FunctionToken functionToken)
        {
            if (functionToken.FunctionType is not FunctionType.Str)
            {
                List<INode<T>> arguments = BuildSyntaxTree<T>(functionToken.Arguments).ToList();

                var functionNode = new MathFunctionNode<T>(functionToken.FunctionType, arguments);

                return functionNode;
            }
            else
            {
                List<INode<T>> arguments = BuildSyntaxTree<T>(functionToken.Arguments).ToList();

                var functionNode = new MathFunctionNode<T>(functionToken.FunctionType, arguments);

                return functionNode;
            }
        }


        private void PopAndAddOperatorToTree<T>(Stack<OperatorToken> operatorStack, Stack<INode<T>> operandStack)
        {
            if (operandStack.Count < 2)
            {
                throw new InvalidOperationException("Not enough operands for the remaining operators.");
            }

            OperatorToken poppedOperator = operatorStack.Pop();
            operandStack.Push(new MathOperatorNode<T>(poppedOperator.OperatorType, operandStack.Pop(), operandStack.Pop()));
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
            string mathExpression = "Ln(10)";
            Tokenizer tokenizer = new Tokenizer();
            List<IToken> infixNotationTokens = tokenizer.Parse(mathExpression);

            MathASTBuilder astBuilder = new MathASTBuilder();
            var rootNode = astBuilder.BuildSyntaxTree<double>(infixNotationTokens);

            PrintTreeStructure(rootNode[0], 0);

            Console.WriteLine($"Syntax Tree Result: {rootNode[0].Evaluate()}");

        }

        static void PrintTreeStructure<T>(INode<T> node, int indentLevel)
        {
            if (node is MathOperandNode<T> operandNode)
            {
                PrintIndentedLine($"OperandNode: {operandNode.Value}", indentLevel);
            }
            else if (node is MathOperatorNode<T> operatorNode)
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
