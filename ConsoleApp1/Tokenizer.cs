using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MathSyntaxTree
{

    public class Tokenizer
    {
        private static readonly List<IToken> _infixNotationTokens = new List<IToken>();
        private readonly StringBuilder _valueTokenBuilder;
        private static bool _insideFunctionArgs;
        private static FunctionToken _currentFunctionToken;


        public Tokenizer()
        {
            _valueTokenBuilder = new StringBuilder();
        }

        public List<IToken> Parse(string expression)
        {
            Reset();
            foreach (char next in expression)
            {
                FeedCharacter(next);
            }
            return GetResult();
        }

        private void Reset()
        {
            _valueTokenBuilder.Clear();
            _infixNotationTokens.Clear();
        }

        private void FeedCharacter(char next)
        {
            if (IsSpacingCharacter(next))
            {
                if (_valueTokenBuilder.Length > 0)
                {
                    var variable = _valueTokenBuilder.ToString();
                    if (!IsFunctionName(variable))
                    {
                        var token = CreateOperandToken(variable);
                        _infixNotationTokens.Add(token);
                        _valueTokenBuilder.Clear();
                    }
                    else
                    {
                        var token = CreateFunctionToken(variable);
                        _infixNotationTokens.Add(token);
                        _valueTokenBuilder.Clear();
                    }

                }
            }
            else if (IsOperatorCharacter(next))
            {
                if (_valueTokenBuilder.Length > 0)
                {
                    var variable = _valueTokenBuilder.ToString();

                    if (_insideFunctionArgs && _currentFunctionToken != null)
                    {
                        var operandToken = CreateOperandToken(variable);
                        _currentFunctionToken.Arguments.Add(operandToken);
                        _valueTokenBuilder.Clear();
                    }
                    else if (!IsFunctionName(variable))
                    {
                        // Создание обычного операнда
                        var token = CreateOperandToken(variable);
                        _infixNotationTokens.Add(token);
                        _valueTokenBuilder.Clear();
                    }
                    else
                    {
                        // Создание функционального токена и вход в режим аргументов функции
                        var token = CreateFunctionToken(variable);
                        _infixNotationTokens.Add(token);
                        _valueTokenBuilder.Clear();
                        _insideFunctionArgs = true;
                        _currentFunctionToken = token as FunctionToken;
                    }
                }

                var operatorToken = CreateOperatorToken(next);

                if (_insideFunctionArgs && _currentFunctionToken != null)
                {
                    // Если внутри аргументов функции, добавляем оператор в аргументы
                    _currentFunctionToken.Arguments.Add(operatorToken);
                }
                else
                {
                    _infixNotationTokens.Add(operatorToken);
                }

                if (next == ')' && _insideFunctionArgs)
                {
                    // Если это закрывающая скобка и мы внутри аргументов функции, выходим из режима аргументов
                    _insideFunctionArgs = false;
                }
            }
            else
            {
                _valueTokenBuilder.Append(next);
            }
        }

        private static bool IsOperatorCharacter(char c) => c switch
        {
            var x when new char[] { '(', ')', '+', '-', '*', '/' }.Contains(x) => true,
            _ => false
        };

        private static bool IsSpacingCharacter(char c)
        {
            return c switch
            {
                ' ' => true,
                _ => false,
            };
        }
        private static bool IsFunctionName(string raw)
        {
            return Enum.GetNames(typeof(FunctionType)).Contains(raw, StringComparer.OrdinalIgnoreCase);
        }

        private static IToken CreateOperandToken(string raw)
        {
            if (double.TryParse(
                raw,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out double result))
            {
                return new OperandToken(result);
            }
            throw new SyntaxException($"The operand {raw} has an invalid format.");
        }

        private static IToken CreateFunctionToken(string raw)
        {
            if (IsFunctionName(raw))
            {
                return new FunctionToken(Enum.Parse<FunctionType>(raw, ignoreCase: true)); ;
            }

            throw new SyntaxException($"The function {raw} has an invalid format.");
        }

        private static OperatorToken CreateOperatorToken(char c)
        {
            return c switch
            {
                '(' => new OperatorToken(OperatorType.OpeningBracket),
                ')' => new OperatorToken(OperatorType.ClosingBracket),
                '+' => new OperatorToken(OperatorType.Addition),
                '-' => new OperatorToken(OperatorType.Subtraction),
                '*' => new OperatorToken(OperatorType.Multiplication),
                '/' => new OperatorToken(OperatorType.Division),
                _ => throw new SyntaxException($"There's no a suitable operator for the char {c}"),
            };
        }
/*        private static IToken AddFunctionArgs(string functionName, List<IToken> currentTokens)
        {
            FunctionToken functionToken = new FunctionToken(Enum.Parse<FunctionType>(functionName, ignoreCase: true));

            // Collect function arguments
            while (currentTokens.Count > 0 && currentTokens.Last() is not OperatorToken opToken)
            {
                functionToken.Arguments.Add(currentTokens.Last());
                currentTokens.RemoveAt(currentTokens.Count - 1);
            }

            return functionToken;
        }*/

        private List<IToken> GetResult()
        {
            if (_valueTokenBuilder.Length > 0)
            {
                var token = CreateOperandToken(_valueTokenBuilder.ToString());
                _valueTokenBuilder.Clear();
                _infixNotationTokens.Add(token);
            }

            return _infixNotationTokens;
        }

    }

}