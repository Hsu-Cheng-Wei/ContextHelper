using ContextHelper.Common;
using System.Collections.Generic;
using System.Linq;

namespace ContextHelper
{
    public static class CreateObject<T>
    {
        public static IEnumerable<object> Build()
        {
            return typeof(T).CreateObject().SelectMany(x => x.Objects);
        }
    }
}
