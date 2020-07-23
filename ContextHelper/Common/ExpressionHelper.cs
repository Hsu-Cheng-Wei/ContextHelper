using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ContextHelper.Common
{
    public class ExpressionHelper
    {
        public static MemberInfo FindProperty(LambdaExpression lambdaExpression)
        {
            var expr = (Expression)lambdaExpression;
            while (expr != null)
            {
                switch (expr.NodeType)
                {
                    case ExpressionType.Convert:
                        expr = ((UnaryExpression)expr).Operand;
                        break;
                    case ExpressionType.Lambda:
                        expr = ((LambdaExpression)expr).Body;
                        break;
                    case ExpressionType.MemberAccess:
                        return ((MemberExpression)expr).Member;
                    case ExpressionType.Call:
                        return ((MethodCallExpression)expr).Method;
                }
            }
            return null;
        }

        public static T FindProperty<T>(LambdaExpression lambdaExpression) where T : MemberInfo
        {
            var result = FindProperty(lambdaExpression);
            if (result is T)
                return (T)result;
            throw new ArgumentException($"Can't not convert {nameof(MemberInfo)} to  {typeof(T)}");
        }

        public static MethodCallExpression FindExpressionCall(Expression expression)
        {
            var expr = expression;
            while (expr.NodeType != ExpressionType.Call)
            {
                switch (expr.NodeType)
                {
                    case ExpressionType.Convert:
                        expr = ((UnaryExpression)expr).Operand;
                        break;
                    case ExpressionType.Lambda:
                        expr = ((LambdaExpression)expr).Body;
                        break;
                    default:
                        throw new ArgumentException($"Exception {((LambdaExpression)expression).Name} not MethodCallExpression");
                }
            }
            return (MethodCallExpression)expr;
        }
    }
}
