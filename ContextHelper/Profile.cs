using ContextHelper.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ContextHelper
{
    public class MemberProfile : Profile
    {
        public bool IsStatic { get; private set; }

        public MemberTypes MemberType => Member.MemberType;

        public MemberInfo Member { get; }

        private ObjectConfigure _configure { get; set; }

        public MemberProfile(MemberInfo member)
        {
            Member = member;
            Regular();
        }

        public ObjectConfigure GetConfigure()
        {
            if (_configure != null)
                return _configure;

            if (Type != typeof(ObjectConfigure))
                throw new Exception($"Return type is not {nameof(ObjectConfigure)}");

            if (MemberType == MemberTypes.Method)
                _configure = (ObjectConfigure)((MethodInfo)Member).Invoke(_Instance, null);
            else
            {
                if (MemberType == MemberTypes.Field)
                    _configure = (ObjectConfigure)((FieldInfo)Member).GetValue(_Instance);
                else
                    _configure = (ObjectConfigure)((PropertyInfo)Member).GetValue(_Instance);
            }
            return _configure;
        }

        public IEnumerable<ConfigureAttribute> GetConfigureAttribute()
            => Member.GetCustomAttributes().OfType<ConfigureAttribute>();

        public override void SetInstance(object instance = null)
        {
            if (_Instance != null)
                return;

            if (instance != null || IsStatic)
                _Instance = instance;
            else
                _Instance = Activator.CreateInstance(Member.DeclaringType);
        }

        private void Regular()
        {
            switch (Member.MemberType)
            {
                case MemberTypes.Method:
                    IsStatic = ((MethodInfo)Member).IsStatic;
                    Type = ((MethodInfo)Member).ReturnType;
                    break;
                case MemberTypes.Field:
                    IsStatic = ((FieldInfo)Member).IsStatic;
                    Type = ((FieldInfo)Member).FieldType;
                    break;
                case MemberTypes.Property:
                    IsStatic = ((PropertyInfo)Member).GetAccessors().Any(x => x.IsStatic);
                    Type = ((PropertyInfo)Member).PropertyType;
                    break;
                default:
                    break;
            }
        }
    }

    public class ClassProfile : Profile
    {
        private MemberProfile[] _members { get; set; }

        public ClassProfile(Type type)
        {
            if (!type.IsClass)
                throw new ArgumentException("Not class");
            Type = type;
            SetInstance(null);
        }

        public MemberProfile[] GetMembers()
        {
            if (_members != null)
                return _members;

            _members = CreateMembers().ToArray();

            return _members;
        }

        public IEnumerable<MemberProfile> CreateMembers()
        {
            foreach (var member in Type.GetMembers())
            {
                var res = new MemberProfile(member);
                if (res.Type == typeof(ObjectConfigure))
                    yield return res;
            }
        }

        public override void SetInstance(object instance)
        {
            foreach (var member in GetMembers().Where(x => !x.IsStatic))
            {
                if (_Instance == null)
                    _Instance = Activator.CreateInstance(Type);
                member.SetInstance(_Instance);
            }
        }
    }

    public abstract class Profile
    {
        public Type Type { get; protected set; }

        protected object _Instance { get; set; }

        public abstract void SetInstance(object instance);
    }
}
