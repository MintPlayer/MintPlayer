using MintPlayer.Data.Exceptions.Paging;
using System.Linq;
using System.Linq.Expressions;

namespace MintPlayer.Data.Extensions
{
    internal static class IQueryableExtensions
    {
        internal static IOrderedQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> query, string propertyName)
        {
            return query.OrderByBase(propertyName, true);
        }
        internal static IOrderedQueryable<TSource> OrderByDescending<TSource>(this IQueryable<TSource> query, string propertyName)
        {
            return query.OrderByBase(propertyName, false);
        }

        private static IOrderedQueryable<TSource> OrderByBase<TSource>(this IQueryable<TSource> query, string propertyName, bool ascending)
        {
            var entityType = typeof(TSource);
            var info = entityType.GetProperty(propertyName);
            if (info == null)
                throw new InvalidSortPropertyException();

            //Create x=>x.PropName
            // The parameter for our lambda expression
            var parameter = Expression.Parameter(typeof(TSource), "a");
            // A reference to the property
            var property = Expression.Property(parameter, propertyName);
            // Our lambda expression
            var lambda = Expression.Lambda(property, parameter);

            //Get System.Linq.Queryable.OrderBy() method.
            var enumarableType = typeof(Queryable);
            var method = enumarableType.GetMethods()
                 .Where(m => m.Name == (ascending ? "OrderBy" : "OrderByDescending") && m.IsGenericMethodDefinition)
                 .Where(m => {
                     var parameters = m.GetParameters().ToList();
                     //Put more restriction here to ensure selecting the right overload                
                     return parameters.Count == 2;//overload that has 2 parameters
                 }).Single();

            //The linq's OrderBy<TSource, TKey> has two generic types, which provided here
            var genericMethod = method.MakeGenericMethod(entityType, info.PropertyType);

            /*
             * Call query.OrderBy(selector), with query and selector: x=> x.PropName
             * Note that we pass the selector as Expression to the method and we don't compile it.
             * By doing so EF can extract "order by" columns and generate SQL for it.
             */
            var newQuery = (IOrderedQueryable<TSource>)genericMethod.Invoke(genericMethod, new object[] { query, lambda });
            return newQuery;
        }
    }
}
