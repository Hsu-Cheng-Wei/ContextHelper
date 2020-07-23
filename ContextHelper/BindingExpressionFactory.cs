using ContextHelper.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ContextHelper
{
    public class BindingParameter
    {
        public int Count() => Parameters.Count() + ArrayParameters.Count();

        public List<object> Parameters = new List<object>();

        public List<BindingParameter> ArrayParameters = new List<BindingParameter>();

        public BindingParameter(IEnumerable<object> _params)
        {
            foreach (var _param in _params)
                if (_param != null && typeof(IEnumerable).IsAssignableFrom(_param.GetType()) && _param.GetType() != typeof(string))
                    ArrayParameters.Add(new BindingParameter((IEnumerable<object>)_param));
                else
                    Parameters.Add(_param);
        }
    }

    public class BindingExpressionFactory
    {
        private static bool IsEnumerable(Type type) => typeof(IEnumerable).IsAssignableFrom(type);

        public static IBindingExpression CreatePropertyBinding(PropertyInfo propertyInfo, object constant = null)
        {
            if (!IsEnumerable(propertyInfo.PropertyType) || propertyInfo.PropertyType == typeof(string))
                return new PropertyBindingExpression(propertyInfo, constant);
            else
                return new PropertyArrayBindingExpression(propertyInfo, constant);
        }

        public static IBindingExpression CreateConvertBinding(PropertyInfo propertyInfo, Expression convert)
        {
            if (!IsEnumerable(propertyInfo.PropertyType))
                return new ConvertBindingExpression(propertyInfo, convert);
            else
                return new ConvertArrayBindingExpression(propertyInfo, convert);
        }

        public static IBindingExpression CreateObjectBinding(PropertyInfo propertyInfo, ObjectConfigure configure)
        {
            if (!IsEnumerable(propertyInfo.PropertyType))
                return new ObjectBindingExpression(propertyInfo, configure);
            else
                return new ObjectArrayBindingExpression(propertyInfo, configure);
        }

        public static IBindingExpression CreateMethodBinding(PropertyInfo propertyInfo, Expression call)
        {
            return new CallMethodBindingExpression(propertyInfo, call);
        }
    }

    #region Call
    public class CallMethodBindingExpression : PropertyBindingExpressionBase
    {
        protected Expression CallExpression { get; }

        public CallMethodBindingExpression(PropertyInfo propertyInfo, Expression call) : base(propertyInfo) { CallExpression = call; }

        public override MemberBinding CreateExpression(ref BindingParameter _params)
        {
            var call = ExpressionHelper.FindExpressionCall(CallExpression);
            return Expression.Bind(PropertyInfo, call);
        }
    }
    #endregion

    #region Convert
    public static class ConvertEpxressionHelper
    {
        public static object AnalyzeParameter(object param)
        {
            if (param is string)
                return string.IsNullOrEmpty((string)param) ? null : param;

            return param;
        }

        public static Expression AnalyzeConvertExpression(this Expression _expr, object param)
        {
            param = AnalyzeParameter(param);
            if (param == null)
                return Expression.Constant(null, ((LambdaExpression)_expr).Body.Type);

            var lambda = ((LambdaExpression)_expr).Body;
            var _call = ExpressionHelper.FindExpressionCall(_expr);
            return (lambda.NodeType == ExpressionType.Convert) ? Expression.Convert(_(_call), lambda.Type) :
                _(_call); Expression _(MethodCallExpression call)
            {
                if (call.Object == null && param != null)
                    return call.Update(null, new Expression[] { Expression.Constant(param) });

                return Expression.Call(_((MethodCallExpression)call.Object), call.Method);
            }
        }
    }

    public class ConvertArrayBindingExpression : ArrayBindingExpressionBase
    {
        protected Expression ConvertExpression { get; }

        public ConvertArrayBindingExpression(PropertyInfo propertyInfo, Expression convert) : base(propertyInfo) { ConvertExpression = convert; }

        public override MemberBinding CreateExpression(ref BindingParameter _params)
        => Expression.Bind(PropertyInfo,
                Expression.NewArrayInit(GetGenericType(),
                    PopParameters(ref _params).Select(x => ConvertExpression.AnalyzeConvertExpression(x))));
    }

    public class ConvertBindingExpression : PropertyBindingExpressionBase
    {
        protected Expression ConvertExpression { get; }

        public ConvertBindingExpression(PropertyInfo propertyInfo, Expression expr) : base(propertyInfo)
        {
            ConvertExpression = expr;
        }

        public override MemberBinding CreateExpression(ref BindingParameter _params)
        => Expression.Bind(PropertyInfo, ConvertExpression.AnalyzeConvertExpression(PopParameter(ref _params)));
    }
    #endregion

    #region ObjectNew
    public class ObjectArrayBindingExpression : ArrayBindingExpressionBase
    {
        protected ObjectConfigure Configure { get; set; }

        public ObjectArrayBindingExpression(PropertyInfo propertyInfo, ObjectConfigure configure) : base(propertyInfo) { Configure = configure; }

        public override MemberBinding CreateExpression(ref BindingParameter _params)
        {
            var genericType = GetGenericType();
            var elementType = GetElementType();

            var cp = PopConfigureParameter(ref _params);
            if (cp == null)
                return Expression.Bind(PropertyInfo, Expression.NewArrayInit(genericType ?? elementType, new Expression[] { }));

            return Expression.Bind(PropertyInfo, Expression.NewArrayInit(genericType ?? elementType, _(cp)));
            IEnumerable<Expression> _(BindingParameter __params)
            {
                foreach (var param in __params.ArrayParameters)
                    yield return Configure.CreateObjectExpression(param);
            }
        }
    }

    public class ObjectBindingExpression : PropertyBindingExpressionBase
    {
        protected ObjectConfigure Configure { get; set; }

        public ObjectBindingExpression(PropertyInfo propertyInfo, ObjectConfigure configure) : base(propertyInfo)
        {
            Configure = configure;
        }

        public override MemberBinding CreateExpression(ref BindingParameter _params)
        {
            var configureParameter = PopConfigureParameter(ref _params);

            return configureParameter == null ? null :
                Expression.Bind(PropertyInfo, Configure.CreateObjectExpression(configureParameter));
        }
    }
    #endregion

    #region Property
    public class PropertyArrayBindingExpression : ArrayBindingExpressionBase
    {
        private object _constant { get; set; }
        public PropertyArrayBindingExpression(PropertyInfo propertyInfo, object constant) : base(propertyInfo) { _constant = constant; }

        public override MemberBinding CreateExpression(ref BindingParameter _params)
        {
            var elemntType = GetElementType();
            var genericType = GetGenericType();
            IEnumerable<object> __params;
            if (_constant == null || typeof(IEnumerable).IsAssignableFrom(_constant.GetType()) || _constant.GetType() == typeof(string))
                __params = PopParameters(ref _params);
            else
                __params = (IEnumerable<object>)_constant;

            if (__params == null)
                return Expression.Bind(PropertyInfo, Expression.NewArrayInit(genericType ?? elemntType, new Expression[] { }));

            return Expression.Bind(PropertyInfo, Expression.NewArrayInit(genericType ?? elemntType, __params.Select(x =>
               ConvertTypeConstant(x, genericType ?? elemntType))));
        }

    }

    public class PropertyBindingExpression : PropertyBindingExpressionBase
    {
        private object _constant { get; set; }
        public PropertyBindingExpression(PropertyInfo propertyInfo, object constant) : base(propertyInfo) { _constant = constant; }

        public override MemberBinding CreateExpression(ref BindingParameter _params)
        {
            object _param = null;
            if (_constant == null)
                _param = PopParameter(ref _params);
            else
                _param = _constant;

            if (_param == null)
                return null;
            return Expression.Bind(PropertyInfo, ConvertTypeConstant(_param, PropertyInfo.PropertyType));
        }
    }
    #endregion

    #region Base
    public abstract class ArrayBindingExpressionBase : PropertyBindingExpressionBase, IArrayBindingExpression
    {
        public ArrayBindingExpressionBase(PropertyInfo propertyInfo) : base(propertyInfo) { }

        public Type GetGenericType() => this.PropertyInfo.PropertyType.GetGenericArguments().FirstOrDefault();

        public Type GetElementType() => this.PropertyInfo.PropertyType.GetElementType();

        public Type GetBaseType() => PropertyInfo.PropertyType;

        public override abstract MemberBinding CreateExpression(ref BindingParameter _params);
    }

    public abstract class PropertyBindingExpressionBase : IBindingExpression
    {
        protected PropertyInfo PropertyInfo { get; }

        protected Func<object, Expression> Constant = (o) => Expression.Constant(o);

        protected Func<object, Type, Expression> ConvertTypeConstant = (o, t) => Expression.Convert(Expression.Constant(o), t);

        public PropertyBindingExpressionBase(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
        }

        public BindingParameter PopConfigureParameter(ref BindingParameter _params)
        {
            var cp = _params.ArrayParameters.FirstOrDefault();
            if (cp != null)
                _params.ArrayParameters.RemoveAt(0);
            return cp;
        }

        public List<object> PopParameters(ref BindingParameter _params)
        {
            var cp = _params.ArrayParameters.FirstOrDefault();
            if (cp == null)
                return null;

            _params.ArrayParameters.RemoveAt(0);
            return cp.Parameters;
        }

        public object PopParameter(ref BindingParameter _params)
        {
            var p = _params.Parameters.FirstOrDefault();
            _params.Parameters.RemoveAt(0);
            return p;
        }

        public abstract MemberBinding CreateExpression(ref BindingParameter _params);
    }

    public interface IBindingExpression
    {
        MemberBinding CreateExpression(ref BindingParameter _params);
        BindingParameter PopConfigureParameter(ref BindingParameter _params);
        List<object> PopParameters(ref BindingParameter _params);
        object PopParameter(ref BindingParameter _params);
    }

    public interface IArrayBindingExpression
    {
        Type GetBaseType();
        Type GetGenericType();
    }
    #endregion
}
