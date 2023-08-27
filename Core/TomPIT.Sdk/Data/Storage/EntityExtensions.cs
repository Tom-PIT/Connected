using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace TomPIT.Data.Storage;
public static class EntityExtensions
{
    public static async Task<ImmutableList<TSource>> AsEntities<TSource>(this IEnumerable<TSource> source)
    {
        if (source is null)
            return ImmutableList<TSource>.Empty;

        await Task.CompletedTask;

        return source.ToImmutableList();
    }

    public static async Task<ImmutableList<TSource>> AsEntities<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (source is null)
            return ImmutableList<TSource>.Empty;

        var list = new List<TSource>();

        await foreach (TSource element in source.AsAsyncEnumerable().WithCancellation(cancellationToken))
            list.Add(element);

        return list.ToImmutableList();
    }

    public static async Task<TSource?> AsEntity<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (source is null)
            return default;

        await Task.CompletedTask;

        return Execute<TSource, TSource>(QueryableMethods.SingleOrDefaultWithoutPredicate, source, cancellationToken);
    }

    public static async Task<TSource?> AsEntity<TSource>(this IEnumerable<TSource> source)
    {
        if (source is null)
            return default;

        await Task.CompletedTask;

        return source.FirstOrDefault();
    }

    public static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(this IQueryable<TSource> source)
    {
        var asyncEnumerable = source as IAsyncEnumerable<TSource>;

        if (asyncEnumerable is not null)
            return asyncEnumerable;

        throw new InvalidOperationException();
    }

    public static IEnumerable<TEntity> WithArguments<TEntity>(this IEnumerable<TEntity> source, QueryArgs args)
    {
        return source.WithOrderBy(args).WithPaging(args);
    }

    private static IEnumerable<TEntity> WithPaging<TEntity>(this IEnumerable<TEntity> source, QueryArgs args)
    {
        if (args.Paging.Size < 1)
        {
            return source;
        }

        return source.Skip((args.Paging.Index - 1) * args.Paging.Size).Take(args.Paging.Size);
    }

    private static IEnumerable<TEntity> WithOrderBy<TEntity>(this IEnumerable<TEntity> entities, QueryArgs args)
    {
        var orderedQueryable = entities.AsQueryable() as IOrderedQueryable<TEntity>;

        if (orderedQueryable == null)
            return entities;

        bool flag = true;

        foreach (OrderByDescriptor item in args.OrderBy)
        {
            orderedQueryable = ((!flag) ? ((item.Mode != 0) ? orderedQueryable.ThenByDescending(ResolvePropertyPredicate<TEntity>(item.Property)) : orderedQueryable.ThenBy(ResolvePropertyPredicate<TEntity>(item.Property))) : ((item.Mode != 0) ? orderedQueryable.OrderByDescending(ResolvePropertyPredicate<TEntity>(item.Property)) : orderedQueryable.OrderBy(ResolvePropertyPredicate<TEntity>(item.Property))));
            flag = false;
        }

        return orderedQueryable;
    }

    private static Expression<Func<T, object>> ResolvePropertyPredicate<T>(string propToOrder)
    {
        ParameterExpression parameterExpression = Expression.Parameter(typeof(T));
        MemberExpression expression = Expression.Property(parameterExpression, propToOrder);
        UnaryExpression body = Expression.Convert(expression, typeof(object));

        return Expression.Lambda<Func<T, object>>(body, new ParameterExpression[1] { parameterExpression });
    }

    private static TResult? Execute<TSource, TResult>(MethodInfo operatorMethodInfo, IQueryable<TSource> source, Expression? expression, CancellationToken cancellationToken = default(CancellationToken))
    {
        var asyncQueryProvider = source.Provider as IAsyncQueryProvider;

        if (asyncQueryProvider is not null)
        {
            if (operatorMethodInfo.IsGenericMethod)
                operatorMethodInfo = ((operatorMethodInfo.GetGenericArguments().Length == 2) ? operatorMethodInfo.MakeGenericMethod(typeof(TSource), typeof(TResult)!.GetGenericArguments().Single()) : operatorMethodInfo.MakeGenericMethod(typeof(TSource)));

            return (TResult)asyncQueryProvider.Execute(Expression.Call(null, operatorMethodInfo, (expression != null) ? new Expression[2] { source.Expression, expression } : new Expression[1] { source.Expression }), cancellationToken);
        }

        throw new InvalidOperationException();
    }

    private static TResult? Execute<TSource, TResult>(MethodInfo operatorMethodInfo, IQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
    {
        return Execute<TSource, TResult>(operatorMethodInfo, source, null, cancellationToken);
    }

    public static TEntity Clone<TEntity>(this TEntity existing) where TEntity : IEntity
    {
        var result = Activator.CreateInstance<TEntity>();

        Merge(result, existing);

        return result;
    }

    private static void Merge<TEntity>(TEntity target, object source) where TEntity : IEntity
    {
        if ((source is null))
            return;

        var sourceProperties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);

        foreach (var property in sourceProperties)
        {
            var destinationProperty = target.GetType().GetProperty(property.Name);

            if (destinationProperty is null)
                destinationProperty = ResolveProperty(property.Name, target);

            if (destinationProperty is null)
                continue;

            if (!destinationProperty.CanWrite)
                continue;

            var value = property.GetValue(source);

            if (Types.TryConvert(value, out value))
                destinationProperty.SetValue(target, value);
        }
    }

    private static PropertyInfo? ResolveProperty(string name, object instance)
    {
        var properties = instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);

        foreach (var property in properties)
        {
            if (string.Equals(property.Name.ToLowerInvariant(), name.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
                return property;
        }

        return null;
    }
    public static TEntity Merge<TEntity>(this TEntity existing, object modifier, TransactionVerb verb, params object[] sources) where TEntity : IEntity
    {
        TEntity result = Activator.CreateInstance<TEntity>();

        Merge(result, modifier);
        Merge(result, new
        {
            verb
        });

        foreach (var item in sources)
            Merge(result, item);

        return result;
    }
}