using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GW_1
{
    public static class FormulaTransformator
    {
        public static Func<Dictionary<string, double>, double> Transform(string formula)
        {
            formula = formula.Replace(" ", "");

            var variables = Expression.Parameter(typeof(Dictionary<string, double>), "variables");
            var tokens = formula.Split(new char[] { '*', '+', '-', '/' }, StringSplitOptions.RemoveEmptyEntries);

            var expressions = new List<Expression>();
            var operators = new List<char>();

            int index = 0;
            foreach (char c in formula)
            {
                if ("*+-/".Contains(c))
                {
                    operators.Add(c);
                }
                else
                {
                    string token = tokens[index++];
                    expressions.Add(double.TryParse(token, out double value)
                        ? (Expression)Expression.Constant(value)
                        : Expression.Call(variables, typeof(Dictionary<string, double>).GetMethod("get_Item"), Expression.Constant(token)));
                }
            }

            ProcessOperators(expressions, operators, '*', Expression.Multiply, Expression.Divide);
            ProcessOperators(expressions, operators, '+', Expression.Add, Expression.Subtract);

            var body = expressions.First();
            var lambda = Expression.Lambda<Func<Dictionary<string, double>, double>>(body, variables);
            return lambda.Compile();
        }

        private static void ProcessOperators(List<Expression> expressions, List<char> operators, char targetOperator, Func<Expression, Expression, BinaryExpression> binaryExpression1, Func<Expression, Expression, BinaryExpression> binaryExpression2)
        {
            for (int i = 0; i < operators.Count; i++)
            {
                if (operators[i] == targetOperator)
                {
                    var left = expressions[i];
                    var right = expressions[i + 1];
                    var operation = targetOperator == operators[i] ? binaryExpression1(left, right) : binaryExpression2(left, right);

                    expressions[i] = operation;
                    expressions.RemoveAt(i + 1);
                    operators.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
