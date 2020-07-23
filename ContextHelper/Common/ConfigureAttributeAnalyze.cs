using ContextHelper.Contract;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ContextHelper.Common
{
    public class ReturnObject
    {
        public Type Type => !Objects.Any() ? null : Objects.First().GetType();

        public IEnumerable<object> Objects { get; set; }
    }

    public class AttributeObject
    {
        public string Name { get; set; }
        public string FatherName { get; set; }
        public object[] data { get; set; }
        public LinkedList<AttributeObject> Childrends { get; set; }

        public AttributeObject(ConfigureAttribute attribute)
        {
            Name = attribute.Name;
            FatherName = attribute.FatherName;
            data = attribute.Parameters;
        }

        public static List<AttributeObject> CreateAttributeObjs(IEnumerable<ConfigureAttribute> attrs)
        {
            var objs = new List<AttributeObject>();
            var etor = attrs.GetEnumerator();
            if (!etor.MoveNext())
                return null;
            objs.Add(new AttributeObject(etor.Current));
            _(etor); void _(IEnumerator<ConfigureAttribute> _etor)
            {
                if (!_etor.MoveNext())
                    return;
                var curr = new AttributeObject(_etor.Current);
                objs.Add(curr);
                _(_etor);
                if (curr.FatherName != null)
                {
                    var father = objs.Where(x => x.Name == curr.FatherName).FirstOrDefault();
                    if (father != null)
                    {
                        father.Childrends = father.Childrends ?? new LinkedList<AttributeObject>();
                        father.Childrends.AddFirst(curr);
                    }
                }
                else
                {
                    objs.First().Childrends = objs.First().Childrends ?? new LinkedList<AttributeObject>();
                    objs.First().Childrends.AddFirst(curr);
                }
            }
            return objs;
        }
    }

    public static class ConfigureAttributeAnalyze
    {
        public static IEnumerable<IEnumerable<ConfigureAttribute>> SugmentConfigureAttribute(this IEnumerable<ConfigureAttribute> attrs)
        {
            var result = new List<List<ConfigureAttribute>>();
            var sub = new List<ConfigureAttribute>();
            var etor = attrs.GetEnumerator();
            if (!etor.MoveNext())
                return result;
            result.Add(sub);
            if (etor.Current.GetType() != typeof(ConfigureAttribute))
                sub.Add(etor.Current);
            while (etor.MoveNext())
            {
                var curr = etor.Current;
                if (curr.GetType() == typeof(ConfigureAttribute))
                {
                    sub = new List<ConfigureAttribute>();
                    result.Add(sub);
                }
                else
                    sub.Add(curr);
            }
            return result;
        }

        public static IEnumerable<object> AnalyzeAttribute(this IEnumerable<ConfigureAttribute> attrs)
        {
            var parameters = AttributeObject.CreateAttributeObjs(attrs);
            if (!parameters.Any())
                return null;
            var first = parameters.First();
            return _(first); IEnumerable<object> _(AttributeObject attribute)
            {
                var res = (attribute.data?.Any() ?? false) ? attribute.data.ToList() : new List<object>();
                if (attribute.Childrends == null)
                {
                    parameters.Remove(attribute);
                    return res;
                }

                foreach (var child in attribute.Childrends)
                {
                    var childres = _(child);
                    res.Add(childres);
                }
                parameters.Remove(attribute);
                return res;
            }
        }
    }
}
