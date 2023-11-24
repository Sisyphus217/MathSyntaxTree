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

            // Adjust the sign for subtraction
            return rightValue - leftValue;
        }
    }
    public class SignedMathOperandNode : IOperandNode
    {
        private readonly IOperandNode _operand;
        private readonly bool _isNegative;

        public double Value => _isNegative ? -_operand.Value : _operand.Value;

        public SignedMathOperandNode(IOperandNode operand, bool isNegative = false)
        {
            _operand = operand ?? throw new ArgumentNullException(nameof(operand));
            _isNegative = isNegative;
        }

        public double Evaluate()
        {
            return Value;
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
                else if (token is OperatorToken)
                {
                    OperatorToken currentOperator = (OperatorToken)token;

                    if (currentOperator.OperatorType == OperatorType.OpeningBracket)
                    {
                        // Открывающая скобка - помещаем ее в стек
                        operatorStack.Push(currentOperator);
                    }
                    else if (currentOperator.OperatorType == OperatorType.ClosingBracket)
                    {
                        // Закрывающая скобка - обрабатываем выражение в скобках
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

            // Теперь tree содержит список узлов синтаксического дерева, представленных объектами MathOperandNode и MathOperatorNode
            // Вам может потребоваться дополнительно обработать этот список с учетом ваших классов
            tree.Add(operandStack.Pop());

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
            // Реализуйте логику для возвращения приоритета оператора
            // Например, умножение и деление имеют более высокий приоритет, чем сложение и вычитание
            switch (operatorType)
            {
                case OperatorType.Multiplication:
                case OperatorType.Division:
                    return 2;
                case OperatorType.Addition:
                case OperatorType.Subtraction:
                    return 1;
                default:
                    return 0; // По умолчанию
            }
        }

    }

    class Program
    {
        static void Main()
        {
            string mathExpression = "(2+6)/(8*(8/3))*(2-6)*2";
            Tokenizer tokenizer = new Tokenizer();
            List<IToken> infixNotationTokens = tokenizer.Parse(mathExpression);

            MathNodeBuilder nodeBuilder = new MathNodeBuilder();
            MathASTBuilder astBuilder = new MathASTBuilder(nodeBuilder);
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
