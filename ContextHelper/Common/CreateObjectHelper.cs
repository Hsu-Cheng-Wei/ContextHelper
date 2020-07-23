using ContextHelper.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ContextHelper.Common
{
    public static class CreateObjectHelper
    {
        public static object CreateObject(this ObjectConfigure objectConfigure, IEnumerable<object> _params)
        {
            Expression expr;
            if (!_params.Any())
                expr = Expression.New(objectConfigure.Type);
            else
            {
                var p = new BindingParameter(_params);
                expr = objectConfigure.CreateObjectExpression(p);
            }

            var lambda = Expression.Lambda(expr);
            return lambda.Compile().DynamicInvoke();
        }

        public static Expression CreateObjectExpression(this ObjectConfigure objectConfigure, BindingParameter _params)
        {
            if (_params == null)
                return Expression.New(objectConfigure.Type);

            return Expression.MemberInit(Expression.New(objectConfigure.Type),
                objectConfigure.Bindings.Select(x => x.CreateExpression(ref _params)).Where(x => x != null));
        }

        public static IEnumerable<ReturnObject> CreateObject(this Type type)
        {
            var profiles = new ClassProfile(type);

            foreach (var profile in profiles.GetMembers())
                if (profile.GetConfigureAttribute().Any(x => typeof(ConfigureAttribute).IsAssignableFrom(x.GetType())))
                    yield return profile.CreateObject();

        }

        public static IEnumerable<object> CreateObject<T>(string name)
        {
            var type = typeof(T);
            var member = type.GetMember(name).FirstOrDefault();
            if (member == null)
                return null;

            var profile = new MemberProfile(member);
            profile.SetInstance();
            return profile.CreateObject().Objects;
        }

        public static IEnumerable<object> CreateObject<T>(Expression<Func<T, ObjectConfigure>> expression)
        {
            var name = ExpressionHelper.FindProperty(expression).Name;
            return CreateObject<T>(name);
        }

        public static IEnumerable<TResult> CreateObject<TConfigure, TResult>(Expression<Func<TConfigure, ObjectConfigure>> expression)
        => CreateObject(expression).Cast<TResult>();

        public static ReturnObject CreateObject(this MemberProfile profile)
        {
            var param = new List<IEnumerable<object>>();
            foreach (var attr in profile.GetConfigureAttribute().SugmentConfigureAttribute())
                param.Add(attr.AnalyzeAttribute());

            if (!param?.Any() ?? true)
                return null;

            var configure = profile.GetConfigure();
            var result = param.Select(x => configure.CreateObject(x)).ToList();

            return new ReturnObject
            {
                Objects = result
            };
        }

        public static ObjectConfigure<T> CreateConfigure<T>(Func<ObjectConfigure<T>, ObjectConfigure<T>> func)
        => func(new ObjectConfigure<T>());
    }
}
