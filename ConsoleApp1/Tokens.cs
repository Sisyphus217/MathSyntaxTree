namespace MathSyntaxTree
{

    public interface IToken { }

    public class OperandToken : IToken
    {
        public double Value { get; }

        public OperandToken(double value)
        {
            Value = value;
        }
    }

    public enum OperatorType
    {
        Addition = 1,
        Subtraction = 2,
        Multiplication = 3,
        Division = 4,
        OpeningBracket = 5,
        ClosingBracket =6 
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
}