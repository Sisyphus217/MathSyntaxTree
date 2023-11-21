using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace MathSyntaxTree
{
    class Program
    {

        private readonly Tokenizer _tokenizer;
        private readonly ShuntingYardAlgorithm _algorithm;
        private readonly PostfixNotationCalculator _calculator;

        public Program()
        {
            _tokenizer = new Tokenizer();
            _algorithm = new ShuntingYardAlgorithm();
            _calculator = new PostfixNotationCalculator();
        }

        static void Main()
        {
            Program program = new();
            string expression = "2*(3+4)+5";
            Console.WriteLine(program.Calculate(expression));
        }
        private double Calculate(string expression)
        {
            var infixNotationTokens = _tokenizer.Parse(expression);
            var postfixNotationTokens = _algorithm.Apply(infixNotationTokens);
            return _calculator.Calculate(postfixNotationTokens).Value;
        }
    }
}
