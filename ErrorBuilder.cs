using System.ComponentModel;

namespace BeeQ;

public static class ErrorBuilder
{
    public static string BuildCode(string ms, string module, int code) => $"{module}{code:D4}{ms}"; // AUT+0012+SEC => AUT0012SEC

    public static Response<T> Build<E, T>(E value, object? tag = null) where E : Enum
    {
        var attrE = (ErrorResponseEnumAttribute?)Attribute.GetCustomAttribute(typeof(E), typeof(ErrorResponseEnumAttribute)) ?? throw new InvalidOperationException($"El enum {typeof(E).Name} no está marcado con [ErrorResponseEnum].");
        var fld = typeof(E).GetField(value.ToString());
        var attr = (DescriptionAttribute?)Attribute.GetCustomAttribute(fld!, typeof(DescriptionAttribute));
        var desc = attr?.Description ?? "(empty)";

        return ErrorBuilder.Response<T>(attrE?.Entity ?? "XXX", attrE?.Entity ?? "X", Convert.ToInt32(value), desc, tag: tag);
    }

    public static Response Build<E>(E value, object? tag = null) where E : Enum
    {
        var attrE = (ErrorResponseEnumAttribute?)Attribute.GetCustomAttribute(typeof(E), typeof(ErrorResponseEnumAttribute)) ?? throw new InvalidOperationException($"El enum {typeof(E).Name} no está marcado con [ErrorResponseEnum].");
        var fld = typeof(E).GetField(value.ToString());
        var attr = (DescriptionAttribute?)Attribute.GetCustomAttribute(fld!, typeof(DescriptionAttribute));
        var desc = attr?.Description ?? "(empty)";

        return ErrorBuilder.Response(attrE?.Entity ?? "XXX", attrE?.Entity ?? "X", Convert.ToInt32(value), desc, tag: tag);
    }

    public static Response Response(string ms, string module, int code, string? message = null, bool esError = true, Exception? exception = null, object? tag = null)
    {
        return new(new ResponseMessage()
        {
            Code = ErrorBuilder.BuildCode(ms, module, code),
            Text = message ?? ErrorBuilder.BuildCode(ms, module, code),
            EsError = esError,
            Ex = exception,
            ExceptionType = exception?.GetType().FullName,
            Tag = tag
        });
    }

    public static Response<T> Response<T>(string ms, string module, int code, string? message = null, bool esError = true, Exception? exception = null, object? tag = null)
    {
        return new(new ResponseMessage()
        {
            Code = ErrorBuilder.BuildCode(ms, module, code),
            Text = message ?? ErrorBuilder.BuildCode(ms, module, code),
            EsError = esError,
            Ex = exception,
            ExceptionType = exception?.GetType().FullName,
            Tag = tag
        });
    }

    public static PaginatedResponse<T> PaginatedResponse<T>(string ms, string module, int code, string? message = null, bool esError = true, Exception? exception = null, object? tag = null)
    {
        return new(new ResponseMessage()
        {
            Code = ErrorBuilder.BuildCode(ms, module, code),
            Text = message ?? ErrorBuilder.BuildCode(ms, module, code),
            EsError = esError,
            Ex = exception,
            ExceptionType = exception?.GetType().FullName,
            Tag = tag
        });
    }
}