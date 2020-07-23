using ContextHelper.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ContextHelper
{
    public class ObjectConfigure<TDestination> : ObjectConfigure
    {
        public ObjectConfigure(Type type) : base(type) { }

        public ObjectConfigure() : base(typeof(TDestination)) { }

        public ObjectConfigure<TDestination> SelectMember(Expression<Func<TDestination, object>> expr, object constant = null)
        {
            var member = ExpressionHelper.FindProperty<PropertyInfo>(expr);
            if (!properties.ContainsKey(member.Name))
                properties.Add(member.Name, BindingExpressionFactory.CreatePropertyBinding(member, constant));
            return this;
        }

        public ObjectConfigure<TDestination> SelectMemberWithMethod(Expression<Func<TDestination, object>> expr, Expression<Func<object>> func)
        {
            var member = ExpressionHelper.FindProperty<PropertyInfo>(expr);
            if (!properties.ContainsKey(member.Name))
                properties.Add(member.Name, BindingExpressionFactory.CreateMethodBinding(member, func));
            return this;
        }

        public ObjectConfigure<TDestination> SelectMember<TSubDestination>(Expression<Func<TDestination, object>> expr,
            Func<ObjectConfigure<TSubDestination>, ObjectConfigure<TSubDestination>> subConfigure)
        {
            var member = ExpressionHelper.FindProperty<PropertyInfo>(expr);
            if (!properties.ContainsKey(member.Name))
                properties.Add(member.Name, BindingExpressionFactory.CreateObjectBinding(member, subConfigure(new ObjectConfigure<TSubDestination>())));
            return this;
        }

        public ObjectConfigure<TDestination> SelectMemberConvert<TCovertSource, TCovertDest>(Expression<Func<TDestination, object>> expr,
            Expression<Func<TCovertSource, TCovertDest>> convert)
        {
            var member = ExpressionHelper.FindProperty<PropertyInfo>(expr);
            if (!properties.ContainsKey(member.Name))
                properties.Add(member.Name, BindingExpressionFactory.CreateConvertBinding(member, convert));
            return this;
        }
    }
    public class ObjectConfigure
    {
        public virtual Type Type { get; }

        protected Dictionary<string, IBindingExpression> properties = new Dictionary<string, IBindingExpression>();

        public IEnumerable<IBindingExpression> Bindings => properties.Values;

        public ObjectConfigure(Type type) { Type = type; }
    }
}
