using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TomPIT.Data.Storage;
internal static class QueryableMethods
{
    public static MethodInfo SingleWithoutPredicate { get; }
    public static MethodInfo SingleOrDefaultWithoutPredicate { get; }

    static QueryableMethods()
    {
        Dictionary<string, List<MethodInfo>> queryableMethodGroups = (from mi in typeof(Queryable)!.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public)
                                                                      group mi by mi.Name).ToDictionary((IGrouping<string, MethodInfo> e) => e.Key, (IGrouping<string, MethodInfo> l) => l.ToList());
        SingleWithoutPredicate = GetMethod("Single", 1, (Type[] types) => new Type[1] { typeof(IQueryable<>)!.MakeGenericType(types[0]) });
        SingleOrDefaultWithoutPredicate = GetMethod("SingleOrDefault", 1, (Type[] types) => new Type[1] { typeof(IQueryable<>)!.MakeGenericType(types[0]) });

        MethodInfo GetMethod(string name, int genericParameterCount, Func<Type[], Type[]> parameterGenerator)
        {
            Func<Type[], Type[]> parameterGenerator2 = parameterGenerator;
            return queryableMethodGroups[name].Single((MethodInfo mi) => ((genericParameterCount == 0 && !mi.IsGenericMethod) || (mi.IsGenericMethod && mi.GetGenericArguments().Length == genericParameterCount)) && (from e in mi.GetParameters()
                                                                                                                                                                                                                       select e.ParameterType).SequenceEqual(parameterGenerator2(mi.IsGenericMethod ? mi.GetGenericArguments() : Array.Empty<Type>())));
        }
    }
}
