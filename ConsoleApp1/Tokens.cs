namespace MathSyntaxTree
{

    public interface IToken { }

    public class OperandToken<T> : IToken
    {
        public T Value { get; }

        public OperandToken(T value)
        {
            Value = value;
        }
    }

    public enum OperatorType
    {
        Addition,
        Subtraction,
        Multiplication,
        Division,
        OpeningBracket,
        ClosingBracket, 
    }

    public class OperatorToken : IToken
    {
        public OperatorType OperatorType { get; }
        public List<IToken> Children { get; set; } = new List<IToken>();

        public OperatorToken(OperatorType operatorType)
        {
            OperatorType = operatorType;
        }
    }
    public enum FunctionType
    {
        Str,
        Sin,
        Cos,
        Tan,
        Sqrt,
        Ln,
        Pow,
        Abs,
        Area,
        Perimeter,
        X,
        Y,
        Z,
    }

    public class FunctionToken : IToken
    {
        public FunctionType FunctionType { get; }
        public List<IToken> Arguments { get; set; } = new List<IToken>();
        public int OpenParenthesesCount { get; set; }
        public int CloseParenthesesCount { get; set; }

        public FunctionToken(FunctionType functionType)
        {
            FunctionType = functionType;
        }
    }
}