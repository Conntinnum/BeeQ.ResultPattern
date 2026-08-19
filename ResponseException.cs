namespace BeeQ.ResultPattern;

/// <summary>
/// Representa un Exception de tipo Mensaje de Response
/// </summary>
/// <param name="msg">Mensaje de error</param>
public class ResponseMessageException(ResponseMessage msg) : Exception(msg.Text)
{
    /// <summary>
    /// Mensaje asociado a la excepción
    /// </summary>
    public ResponseMessage ResponseMessage { get; } = msg;
}

/// <summary>
/// Representa un Exception convertible a Response
/// </summary>
/// <param name="response">Genera la instancia a partir de un Response</param>
public class ResponseException(Response response) : AggregateException(GetMessageExceptions(response))
{
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="error">Mensaje de error</param>
    /// <param name="tag">Información adicional</param>
    public ResponseException(string error, object? tag = null) : this(new Response(false, error, true, null, tag)) { }

    private static ResponseMessageException[] GetMessageExceptions(Response res)
    {
        return [.. res.Messages.Select(m => new ResponseMessageException(m))];
    }

    /// <summary>
    /// Devuelve el Exception en formato Response
    /// </summary>
    /// <returns></returns>
    public Response GetResponse()
    {
        return new Response([.. this.InnerExceptions.OfType<ResponseMessageException>().Select(p => p.ResponseMessage)]);
    }

    /// <summary>
    /// Devuelve el Exception en formato Response
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Response<T> GetResponse<T>()
    {
        return new Response<T>(GetResponse());
    }
}