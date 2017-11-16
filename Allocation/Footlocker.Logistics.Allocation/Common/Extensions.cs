using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using Telerik.Web.Mvc;

namespace Footlocker.Logistics.Allocation.Common
{
    public static class Extensions
    {
        /// <summary>
        /// This is a generic method to parse and apply the builtin filters server side which is supplied from the telerik grid system.
        /// Please ensure you are passing the right filters with the correct model since the dynamic expressions being built are comparing
        /// the model and filter properties.
        /// </summary>
        /// <typeparam name="T">Generic type of Model being filtered with supplied list</typeparam>
        /// <param name="data">The data to be filtered</param>
        /// <param name="rawFilters">Filters from the GridCommand which is passed from the telerik grids</param>
        /// <returns>The filtered data</returns>
        public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> data, IList<IFilterDescriptor> rawFilters)
        {
            ParameterExpression sParam = Expression.Parameter(typeof(T), "s");
            Expression left = null;
            Expression right = null;
            Expression conditional = null;
            MethodCallExpression whereCallExpression = null;
            MethodInfo method = null;

            // parse "raw" filters into uniform filters of type FilterDescriptor (Raw filters come with both CompositeFilterDescriptors and FilterDescriptors)
            List<FilterDescriptor> filters = ParseFilters(rawFilters);

            foreach (var filter in filters)
            {
                Type propertyType = typeof(T).GetProperty(filter.Member.ToString()).PropertyType;
                left = Expression.Property(sParam, typeof(T).GetProperty(filter.Member.ToString()));
                right = Expression.Constant(Convert.ChangeType(filter.ConvertedValue, Nullable.GetUnderlyingType(propertyType) ?? propertyType));

                if (IsNullabeType(left.Type) && !IsNullabeType(right.Type))
                {
                    right = Expression.Convert(right, left.Type);
                }
                else if (!IsNullabeType(left.Type) && IsNullabeType(right.Type))
                {
                    left = Expression.Convert(left, right.Type);
                }

                method = null;

                switch (filter.Operator)
                {
                    case FilterOperator.Contains:
                        method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        break;
                    case FilterOperator.IsEqualTo:
                        conditional = Expression.Equal(left, right);
                        break;
                    case FilterOperator.IsNotEqualTo:
                        conditional = Expression.NotEqual(left, right);
                        break;
                    case FilterOperator.IsLessThan:
                        conditional = Expression.LessThan(left, right);
                        break;
                    case FilterOperator.IsLessThanOrEqualTo:
                        conditional = Expression.LessThanOrEqual(left, right);
                        break;
                    case FilterOperator.IsGreaterThan:
                        conditional = Expression.GreaterThan(left, right);
                        break;
                    case FilterOperator.IsGreaterThanOrEqualTo:
                        conditional = Expression.GreaterThanOrEqual(left, right);
                        break;
                    case FilterOperator.StartsWith:
                        method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
                        break;
                    case FilterOperator.EndsWith:
                        method = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
                        break;
                }
                if (method != null)
                {
                    MethodCallExpression callExp = Expression.Call(left, method, right);
                    whereCallExpression = Expression.Call
                        (
                            typeof(Queryable),
                            "Where",
                            new Type[] { data.ElementType },
                            data.Expression,
                            Expression.Lambda<Func<T, bool>>(callExp, new ParameterExpression[] { sParam })
                        );
                }
                else
                {
                    whereCallExpression = Expression.Call
                        (
                            typeof(Queryable),
                            "Where",
                            new Type[] { data.ElementType },
                            data.Expression,
                            Expression.Lambda<Func<T, bool>>(conditional, new ParameterExpression[] { sParam })
                        );
                }

                data = data.Provider.CreateQuery<T>(whereCallExpression).AsQueryable();
            }
            return data;
        }

        /// <summary>
        /// Helper method to determine if a certain is nullable.
        /// </summary>
        /// <param name="t">Type for evaluation</param>
        /// <returns>true or false dependent if the type is nullable or not</returns>
        private static bool IsNullabeType(Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// Helper method to parse filters that are supplied from the GridCommand class.
        /// The way the "raw filters" come is of the following:
        /// 1) CompositeFilterDescriptor --> contains 2 FilterDescriptors and a logical operator for combination.
        /// This parsing will essentially grab all FilterDescriptor objects and add them to one central list.
        /// IMPORTANT!! This parsing will ignore the logical operators for each CompositeFilterDescriptor since 
        /// the original reason for building this method was for a massive AND statement.  If there is a scenario
        /// where you are parsing filters that have logical 'OR' operators, then you may need to modify this
        /// solution.
        /// </summary>
        /// <param name="filters">filters to be parsed</param>
        /// <returns>centralized list of all FilterDescriptor objects</returns>
        private static List<FilterDescriptor> ParseFilters(this IList<IFilterDescriptor> filters)
        {
            List<FilterDescriptor> result = new List<FilterDescriptor>();
            foreach (var filter in filters)
            {
                var descriptor = filter as FilterDescriptor;
                if (descriptor != null)
                {
                    result.Add(descriptor);
                }
                else if (filter is CompositeFilterDescriptor)
                {
                    result.AddRange(ParseFilters(((CompositeFilterDescriptor)filter).FilterDescriptors));
                }
            }
            return result;
        }
    }
}