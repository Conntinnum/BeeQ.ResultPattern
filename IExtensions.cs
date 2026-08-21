using BeeQ.ResultPattern;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace BeeQ;

public static class IExtensions
{
    #region Exceptions

    /// <summary>
    /// Devuelve una descripción completa logueable de una excepción
    /// </summary>
    /// <param name="exception">Objeto Excepción</param>
    /// <param name="tab">(opcional) tabulación</param>
    /// <returns></returns>
    public static string GetFullDescription(this Exception exception, string? tab = "")
    {
        var sb = new StringBuilder();

        var ex = exception;
        tab ??= string.Empty;
        while (ex != null)
        {
            if (ex is AggregateException aggexception)
            {
                sb.AppendLine($"{tab}\t- Aggregate Exception -");
                sb.AppendLine($"{tab}\tType:    {ex.GetType().FullName}");
                sb.AppendLine($"{tab}\tMessage: {ex.Message}");
                sb.AppendLine($"{tab}\tSource:  {ex.Source}");
                sb.AppendLine($"{tab}\tTarget:  {ex.TargetSite?.ToString()}");
                sb.AppendLine($"{tab}\tTrace:   {ex.StackTrace?.Replace($"{Environment.NewLine}", $"{Environment.NewLine}{tab}\t         ")}");

                foreach (var iex in aggexception.InnerExceptions)
                {
                    sb.AppendLine($"{tab}\t\t- Aggregate Inner Exception -");
                    sb.Append(GetFullDescription(iex, $"{tab}\t"));
                }

                ex = null; // el bucle por Agregate ya se está haciendo
            }
            else
            {
                if (ex != exception)
                    sb.AppendLine($"{tab}\t- Inner Exception -");
                sb.AppendLine($"{tab}\tType:    {ex.GetType().FullName}");
                sb.AppendLine($"{tab}\tMessage: {ex.Message}");
                sb.AppendLine($"{tab}\tSource:  {ex.Source}");
                sb.AppendLine($"{tab}\tTarget:  {ex.TargetSite?.ToString()}");
                sb.AppendLine($"{tab}\tTrace:   {ex.StackTrace?.Replace($"{Environment.NewLine}", $"{Environment.NewLine}{tab}\t         ")}");

                tab = $"{tab}\t";
                ex = ex.InnerException;
            }
        }

        return sb.ToString();
    }

    #endregion

    #region Expression

    /// <summary>
    /// Realiza una operación OR entre 2 Expresiones Lambda
    /// </summary>
    /// <typeparam name="T">Objeto de consulta</typeparam>
    /// <param name="expr1">Expresión Lambda 1</param>
    /// <param name="expr2">Expresión Lambda 2</param>
    /// <returns></returns>
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
    {
        var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
        return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
    }

    /// <summary>
    /// Realiza una operación AND entre 2 Expresiones Lambda
    /// </summary>
    /// <typeparam name="T">Objeto de consulta</typeparam>
    /// <param name="expr1">Expresión Lambda 1</param>
    /// <param name="expr2">Expresión Lambda 2</param>
    /// <returns></returns>
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
    {
        var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
    }

    /// <summary>
    /// Realiza un ORDER a un IQueriable
    /// </summary>
    /// <typeparam name="T">Objeto de consulta</typeparam>
    /// <param name="query">Query base a la que se le aplicará el filtro</param>
    /// <param name="propertyName">Nombre de la propiedad a ordenar</param>
    /// <param name="isAscending">indica si es orden ascendente o descendente</param>
    /// <returns></returns>
    public static IOrderedQueryable<T> SortBy<T>(this IQueryable<T> query, string propertyName, bool isAscending = true)
    {
        var sortExpression = BuildSearchExpression<T>(propertyName);
        if (sortExpression == null) return query.OrderBy(p => false);

        return isAscending
            ? query.OrderBy(sortExpression)
            : query.OrderByDescending(sortExpression);
    }

    /// <summary>
    /// Realiza un THENBY a un IQueriable (Orden en cascada)
    /// </summary>
    /// <typeparam name="T">Objeto de consulta</typeparam>
    /// <param name="query">Query base a la que se le aplicará el filtro</param>
    /// <param name="propertyName">Nombre de la propiedad a ordenar</param>
    /// <param name="isAscending">indica si es orden ascendente o descendente</param>
    /// <returns></returns>
    public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> query, string propertyName, bool isAscending = true)
    {
        var sortExpression = BuildSearchExpression<T>(propertyName);

        if (sortExpression == null) return query.ThenBy(p => false);
        return isAscending
            ? query.ThenBy(sortExpression)
            : query.ThenByDescending(sortExpression);
    }

    internal static Expression<Func<T, object>>? BuildSearchExpression<T>(string propertyName)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        /* Se puede pasar Propiedad.Propiedad.Propiedad... */
        var arg = Expression.Parameter(typeof(T), "p");
        Expression propertyAccess = arg;
        var type = typeof(T);
        PropertyInfo? property = null;

        foreach (var prop in propertyName.Split('.'))
        {
            property = type.GetProperty(prop, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) ?? throw new ResponseException("No se encontró la propiedad " + prop);
            propertyAccess = Expression.PropertyOrField(propertyAccess, property.Name);

            type = property.PropertyType;
        }

        if (property == null)
        {
            return null;
        }

        if (!property.PropertyType.IsValueType)
        {
            return Expression.Lambda<Func<T, object>>(propertyAccess, arg);
        }

        Expression conversion = Expression.Convert(propertyAccess, typeof(object));
        return Expression.Lambda<Func<T, object>>(conversion, arg);
    }

    /// <summary>
    /// Realiza un ORDER a un IQueriable 
    /// </summary>
    /// <typeparam name="T">Objeto de consulta</typeparam>
    /// <param name="query">Query base a la que se le aplicará el filtro</param>
    /// <param name="sortString">ordenamiento con formato SQL ej:"campo1 ASC, campo2 DESC"</param>
    /// <returns></returns>
    public static IQueryable<T> SortBySQLExpression<T>(this IQueryable<T> query, string sortString)
    {
        ArgumentNullException.ThrowIfNull(sortString);

        var sorts = sortString.Split(',');

        if (sorts.Length == 0 || string.IsNullOrWhiteSpace(sorts[0]))
            throw new System.Data.NoNullAllowedException("sortString");

        string col = sorts[0], sortCol = col;

        if (col.ToLower().EndsWith(" desc"))
        {
            sortCol = col[0..^5];
            query = query.SortBy(sortCol.Trim(), false);
        }
        else
        {
            if (col.ToLower().EndsWith(" asc"))
                sortCol = col[0..^4];
            query = query.SortBy(sortCol.Trim());
        }

        for (var i = 1; i < sorts.Length; i++)
        {
            col = sorts[i];

            if (col.ToLower().EndsWith(" desc"))
            {
                sortCol = col[0..^5];
                query = query.SortBy(sortCol.Trim(), false);
            }
            else
            {
                if (col.ToLower().EndsWith(" asc"))
                    sortCol = col[0..^4];
                query = query.SortBy(sortCol.Trim());
            }
        }

        return query;
    }

    #endregion

    #region Error Response

    /// <summary>
    /// Devuelve un Response con el código de error y la descripción del Enum
    /// </summary>
    /// <remarks>
    /// Asegurarse de que la anumeración implemente el atributo [ErrorResponseEnum] y que cada valor tenga un [Description("...")]
    /// </remarks>
    public static Response<T> GetError<T>(this Enum value, object? tag = null)
    {
        var type = value.GetType();

        var attrE = (ErrorResponseEnumAttribute?)Attribute.GetCustomAttribute(type, typeof(ErrorResponseEnumAttribute)) ?? throw new InvalidOperationException($"El enum {type.Name} no está marcado con [ErrorResponseEnum].");

        var fld = type.GetField(value.ToString());
        var attr = (DescriptionAttribute?)Attribute.GetCustomAttribute(fld!, typeof(DescriptionAttribute));
        var desc = attr?.Description ?? "(empty)";

        return ErrorBuilder.Response<T>(attrE?.Entity ?? "XXX", attrE?.Entity ?? "X", Convert.ToInt32(value), desc, tag: tag);
    }

    /// <summary>
    /// Devuelve un Response con el código de error y la descripción del Enum
    /// </summary>
    /// <remarks>
    /// Asegurarse de que la anumeración implemente el atributo [ErrorResponseEnum] y que cada valor tenga un [Description("...")]
    /// </remarks>
    public static Response GetError(this Enum value, object? tag = null)
    {
        var type = value.GetType();

        var attrE = (ErrorResponseEnumAttribute?)Attribute.GetCustomAttribute(type, typeof(ErrorResponseEnumAttribute)) ?? throw new InvalidOperationException($"El enum {type.Name} no está marcado con [ErrorResponseEnum].");

        var fld = type.GetField(value.ToString());
        var attr = (DescriptionAttribute?)Attribute.GetCustomAttribute(fld!, typeof(DescriptionAttribute));
        var desc = attr?.Description ?? "(empty)";

        return ErrorBuilder.Response(attrE?.Entity ?? "XXX", attrE?.Entity ?? "X", Convert.ToInt32(value), desc, tag: tag);
    }

    /// <summary>
    /// Devuelve un PaginatedResponse con el código de error y la descripción del Enum
    /// </summary>
    /// <remarks>
    /// Asegurarse de que la anumeración implemente el atributo [ErrorResponseEnum] y que cada valor tenga un [Description("...")]
    /// </remarks>
    public static PaginatedResponse<T> GetPaginatedError<T>(this Enum value, object? tag = null)
    {
        var type = value.GetType();

        var attrE = (ErrorResponseEnumAttribute?)Attribute.GetCustomAttribute(type, typeof(ErrorResponseEnumAttribute)) ?? throw new InvalidOperationException($"El enum {type.Name} no está marcado con [ErrorResponseEnum].");

        var fld = type.GetField(value.ToString());
        var attr = (DescriptionAttribute?)Attribute.GetCustomAttribute(fld!, typeof(DescriptionAttribute));
        var desc = attr?.Description ?? "(empty)";

        return ErrorBuilder.PaginatedResponse<T>(attrE?.Entity ?? "XXX", attrE?.Entity ?? "X", Convert.ToInt32(value), desc, tag: tag);
    }
    #endregion
}
