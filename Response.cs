using BeeQ.ResultPattern;

namespace BeeQ;

/// <summary>
/// Objeto que contiene un Mensaje
/// </summary>
public class ResponseMessage
{
    /// <summary>
    /// Excepción que generó el mensaje
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore()]
    public Exception? Ex { get; set; }
    /// <summary>
    /// Tipo de excepción
    /// </summary>
    public string? ExceptionType { get; set; }
    /// <summary>
    /// Codigo único del mensaje
    /// </summary>
    public string? Code { get; set; }
    /// <summary>
    /// Texto descriptivo del mensaje
    /// </summary>
    public string? Text { get; set; }
    /// <summary>
    /// Indica si el mensaje es un error (true) o sólo un mensaje (false)
    /// </summary>
    public bool EsError { get; set; }
    /// <summary>
    /// Información adicional asociada a un Mensaje
    /// </summary>
    public object? Tag { get; set; }

    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    public ResponseMessage() { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="mensaje">Mensaje de tipo informativo</param>
    public ResponseMessage(string? mensaje) : this(mensaje, false, null, null) { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="mensaje">Mensaje</param>
    /// <param name="esError">Indica si es un mensaje de error o informativo</param>
    public ResponseMessage(string? mensaje, bool esError) : this(mensaje, esError, null, null) { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="ex">Genera un mensaje a partir de una excepción</param>
    public ResponseMessage(Exception? ex) : this(null, true, ex, null) { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="msg">Mensaje a agregar.</param>
    /// <param name="esError">Indica si es un mensaje de error o informativo</param>
    /// <param name="ex">Genera un mensaje a partir de una excepción. Sólo valido cuando <paramref name="msg"/> es null</param>
    /// <param name="tag">Información adicional asociada a un Mensaje</param>
    public ResponseMessage(string? msg, bool esError, Exception? ex, object? tag)
    {
        this.EsError = esError;
        this.Ex = ex;
        this.ExceptionType = ex?.GetType().FullName;
        this.Text = (ex != null && msg == null) ? ex.GetFullDescription() : msg;
        this.Tag = tag;
    }

    /// <summary>
    /// Establece el Tag. con la información adicional.
    /// </summary>
    /// <param name="tag">información adicional</param>
    /// <returns></returns>
    public ResponseMessage SetTag(object? tag)
    {
        this.Tag = tag;
        return this;
    }

    /// <summary>
    /// Muestra un texto amigable en representación del objeto
    /// </summary>
    /// <returns></returns>
    public override string? ToString()
    {
        return this.Text ?? this.Ex?.Message ?? base.ToString();
    }
}

/// <summary>
/// Lista de Mensajes
/// </summary>
public class ResponseMessageList : List<ResponseMessage>
{
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    public ResponseMessageList() { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="errors">listado de mensajes de error</param>
    public ResponseMessageList(IEnumerable<string> errors)
    {
        foreach (var err in errors)
            this.AddError(err);
    }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="messages">Listado de Mensajes</param>
    public ResponseMessageList(IEnumerable<ResponseMessage> messages)
    {
        foreach (var msg in messages)
            this.Add(msg);
    }

    /// <summary>
    /// Agrega un nuevo mensaje
    /// </summary>
    /// <param name="mensaje">mensaje</param>
    /// <param name="esError">indica si es un error</param>
    /// <returns></returns>
    public ResponseMessageList Add(string? mensaje, bool esError) => Add(mensaje, esError, null);
    /// <summary>
    /// Agrega un nuevo mensaje
    /// </summary>
    /// <param name="mensaje">mensaje</param>
    /// <param name="esError">indica si es un error</param>
    /// <param name="tag">objeto adjunto al mensaje</param>
    /// <returns></returns>
    public ResponseMessageList Add(string? mensaje, bool esError, object? tag)
    {
        this.Add(new ResponseMessage(mensaje, esError, null, tag));
        return this;
    }
    /// <summary>
    /// Agrega un nuevo error desde una Excepcion
    /// </summary>
    /// <param name="ex">Excepción</param>
    /// <returns></returns>
    public ResponseMessageList AddError(Exception? ex) => AddError(ex, null);
    /// <summary>
    /// Agrega un nuevo error desde una Excepcion
    /// </summary>
    /// <param name="ex">Excepción</param>
    /// <param name="tag">objeto adjunto al mensaje</param>
    /// <returns></returns>
    public ResponseMessageList AddError(Exception? ex, object? tag)
    {
        this.Add(new ResponseMessage(null, true, ex, tag));
        return this;
    }
    /// <summary>
    /// Agrega un mensaje de error
    /// </summary>
    /// <param name="mensaje">mensaje</param>
    /// <returns></returns>
    public ResponseMessageList AddError(string? mensaje)
    {
        this.Add(new ResponseMessage(mensaje, true));
        return this;
    }
    /// <summary>
    /// Agrega un mensaje
    /// </summary>
    /// <param name="mensaje">mensaje</param>
    /// <returns></returns>
    public ResponseMessageList AddMessage(string? mensaje)
    {
        this.Add(new ResponseMessage(mensaje));
        return this;
    }

    /// <summary>
    /// Devuelve un texto amigable de la entidad
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"Mensajes: {this.Count(p => !p.EsError)} Errores: {this.Count(p => p.EsError)}";
    }
}

/// <summary>
/// Clase base de Respuesta de un método o servicio. Que incluye el resultado y los mensajes de información y error.
/// </summary>
public class Response
{
    /// <summary>
    /// Indica si la operación resultó exitosa
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Indica si existe algún mensaje de error
    /// </summary>
    public bool ExistsErrorMessages
    {
        get { return this.Messages.Any(p => p.EsError); }
    }

    /// <summary>
    /// Lista de Mensajes
    /// </summary>
    //[System.Text.Json.Serialization.JsonIgnore()]
    public ResponseMessageList Messages { get; private set; } = [];

    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    public Response() { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="success">Indica si el proceso se ejecutó correctamente</param>
    public Response(bool success) : this(success, null, false, null) { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="error">Mensaje de error</param>
    public Response(string error) : this(false, error, true, null) { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="mensaje">Mensaje a devolver</param>
    /// <param name="esError">booleano que indica si es un error o un mensaje</param>
    public Response(string? mensaje, bool esError) : this(!esError, mensaje, esError, null) { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="ex">Excepción provocada en el proceso</param>
    public Response(Exception ex) : this(false, null, true, ex) { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="ex">Excepción provocada en el proceso</param>
    /// <param name="tag">Objeto con información adicional asociado al error</param>
    public Response(Exception ex, object? tag) : this(false, null, true, ex, tag) { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="success">Indica si el proceso se ejecutó correctamente</param>
    /// <param name="mensaje">Mensaje a devolver</param>
    /// <param name="esError">booleano que indica si es un error o un mensaje</param>
    /// <param name="ex">Excepción provocada en el proceso</param>
    public Response(bool success, string? mensaje, bool esError, Exception? ex) : this(success, mensaje, esError, ex, null) { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="success">Indica si el proceso se ejecutó correctamente</param>
    /// <param name="mensaje">Mensaje a devolver</param>
    /// <param name="esError">booleano que indica si es un error o un mensaje</param>
    /// <param name="ex">Excepción provocada en el proceso</param>
    /// <param name="tag">Objeto con información adicional asociado al error</param>
    public Response(bool success, string? mensaje, bool esError, Exception? ex, object? tag)
    {
        this.Success = success;

        if (!string.IsNullOrEmpty(mensaje))
            this.Messages.Add(mensaje, esError, tag);
        if (ex != null)
            this.Messages.AddError(ex, tag);
    }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="res">Crea la instancia basada en otra instancia</param>
    public Response(Response res) : this(res.Messages) { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="mensajes">Crea la instancia basado en una lista de mensajes</param>
    public Response(ResponseMessage mensajes) : this([mensajes]) { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="list">Crea la instancia basado en una lista de mensajes</param>
    public Response(ResponseMessageList? list)
    {
        this.Success = !(list?.Any(p => p.EsError) ?? false);

        if (list != null)
            this.Messages.AddRange(list);
    }

    /// <summary>
    /// Agrega un mensaje indicando el texto, si es error y con la posibilidad de adjuntar un objeto
    /// </summary>
    /// <param name="mensaje">Mensaje de retorno</param>
    /// <param name="esError">Indica si es un mensaje de error</param>
    /// <param name="tag">objeto adjuntado al mensaje</param>
    public Response AddMessage(string? mensaje, bool esError, object? tag = null)
    {
        this.Messages.Add(mensaje, esError, tag);
        return this;
    }
    /// <summary>
    /// Agrega un mensaje de error y cambia el Success a False
    /// </summary>
    public Response AddError(string? mensaje, object? tag = null)
    {
        this.Success = false;
        this.Messages.Add(mensaje, true, tag);
        return this;
    }
    /// <summary>
    /// Agrega un error y cambia el Success a False
    /// </summary>
    public Response AddError(Exception ex)
    {
        this.Success = false;
        this.Messages.AddError(ex);
        return this;
    }
    /// <summary>
    /// Agrega una lista de mensajes de error
    /// </summary>
    public Response AddError(ResponseMessageList messageList)
    {
        if (messageList == null)
            return this;

        Success = false;
        Messages.AddRange(messageList);
        return this;
    }

    /// <summary>
    /// Verifica una condición (isError) que si resulta positiva, agrega el mensaje de error al Response
    /// </summary>
    /// <param name="isError">Condición que se debe dar para agregar un error</param>
    /// <param name="mensaje">Mensaje de error</param>
    /// <param name="esMensaje">Indica si es un mensaje o un error (solo un error marca Success=false)</param>
    /// <param name="tag">Dato adicional para adjuntar al error</param>
    /// <returns></returns>
    public Response Validate(bool isError, string? mensaje, bool esMensaje = false, object? tag = null)
    {
        if (isError)
        {
            this.AddMessage(mensaje: mensaje, esError: !esMensaje, tag: tag);
            if (!esMensaje) this.Success = false;
        }
        return this;
    }

    /// <summary>
    /// Agrega las características de un Response
    /// </summary>
    /// <param name="res"></param>
    /// <returns></returns>
    public Response Add(Response res)
    {
        if (res?.Messages != null)
            this.Messages.AddRange(res.Messages);

        if (res != null && !res.Success)
            this.Success = false;

        return this;
    }

    /// <summary>
    /// Agrega las características de una lista de Responses
    /// </summary>
    /// <param name="res"></param>
    /// <returns></returns>
    public Response Add(IEnumerable<Response> res)
    {
        this.Add(res.Join());
        return this;
    }

    /// <summary>
    /// Devuelve un Exception basado en el objeto
    /// </summary>
    /// <returns></returns>
    public ResponseException GetException()
    {
        return new ResponseException(this);
    }

    public delegate Task DRunAsync(Response res);

    public override string ToString()
    {
        return System.Text.Json.JsonSerializer.Serialize(this);
    }
}

/// <summary>
/// Clase base de Respuesta de un método o servicio. Que incluye un resultado tipado y los mensajes de información y error.
/// </summary>
/// <typeparam name="T">Objeto de retorno</typeparam>
public class Response<T> : Response
{
    /// <summary>
    /// Objeto de retorno
    /// </summary>
    public virtual T? Data { get; set; }

    /// <summary>
    /// Crea una instancia
    /// </summary>
    public Response() { }
    /// <summary>
    /// Crea una instancia
    /// </summary>
    /// <param name="data">Objeto con el resultado</param>
    public Response(T? data) : base(true)
    {
        this.Data = data;
    }
    /// <summary>
    /// Crea una instancia
    /// </summary>
    /// <param name="success">Indica si el proceso se ejecutó correctamente</param>
    public Response(bool success) : base(success, null, false, null) { }
    /// <summary>
    /// Crea una instancia
    /// </summary>
    /// <param name="error">Mensaje de error</param>
    /// <param name="tag">Objeto con información adicional asociado al error</param>
    public Response(string? error, object? tag = null) : base(false, error, true, null, tag) { }
    /// <summary>
    /// Crea una instancia
    /// </summary>
    /// <param name="mensaje">Mensaje a devolver</param>
    /// <param name="esError">booleano que indica si es un error o un mensaje</param>
    public Response(string? mensaje, bool esError) : base(!esError, mensaje, esError, null) { }
    /// <summary>
    /// Crea una instancia
    /// </summary>
    /// <param name="ex">Excepción provocada en el proceso</param>
    public Response(Exception? ex) : base(false, null, true, ex) { }
    /// <summary>
    /// Crea una instancia
    /// </summary>
    /// <param name="ex">Excepción provocada en el proceso</param>
    /// <param name="tag">Objeto con información adicional asociado al error</param>
    public Response(Exception? ex, object? tag) : base(false, null, true, ex, tag) { }
    /// <summary>
    /// Crea una instancia
    /// </summary>
    /// <param name="success">Indica si el proceso se ejecutó correctamente</param>
    /// <param name="mensaje">Mensaje a devolver</param>
    /// <param name="esError">booleano que indica si es un error o un mensaje</param>
    /// <param name="ex">Excepción provocada en el proceso</param>
    public Response(bool success, string? mensaje, bool esError, Exception? ex) : base(success, mensaje, esError, ex) { }
    /// <summary>
    /// Crea una instancia
    /// </summary>
    /// <param name="res">Crea la instancia basada en otra instancia</param>
    public Response(Response? res) : base(res?.Messages) { }
    /// <summary>
    /// Crea una instancia
    /// </summary>
    /// <param name="mensajes">Crea la instancia basado en una lista de mensajes</param>
    public Response(ResponseMessage? mensajes) : base(mensajes == null ? [] : [mensajes]) { }
    /// <summary>
    /// Crea una instancia
    /// </summary>
    /// <param name="mensajes">Crea la instancia basado en una lista de mensajes</param>
    public Response(IEnumerable<ResponseMessage> mensajes) : base([.. mensajes]) { }
    /// <summary>
    /// Crea una instancia
    /// </summary>
    /// <param name="list">Crea la instancia basado en una lista de mensajes</param>
    public Response(ResponseMessageList list) : base(list) { }

    /// <summary>
    /// Establece el valor de retorno
    /// </summary>
    /// <param name="data">Objeto con el valor de retorno</param>
    /// <returns></returns>
    public Response<T> SetData(T data)
    {
        this.Data = data;
        return this;
    }

    /// <summary>
    /// Agrega un mensaje indicando el texto, si es error y con la posibilidad de adjuntar un objeto
    /// </summary>
    /// <param name="mensaje">Mensaje de retorno</param>
    /// <param name="esError">Indica si es un mensaje de error</param>
    /// <param name="tag">objeto adjuntado al mensaje</param>
    public new Response<T> AddMessage(string? mensaje, bool esError = false, object? tag = null)
    {
        base.AddMessage(mensaje, esError, tag);
        return this;
    }
    /// <summary>
    /// Agrega un mensaje de error y cambia el Success a False
    /// </summary>
    public new Response<T> AddError(string? mensaje, object? tag = null)
    {
        base.AddError(mensaje, tag);
        return this;
    }
    /// <summary>
    /// Agrega un error y cambia el Success a False
    /// </summary>
    public new Response<T> AddError(Exception ex)
    {
        base.AddError(ex);
        return this;
    }
    /// <summary>
    /// Agrega una lista de mensajes de error
    /// </summary>
    public new Response<T> AddError(ResponseMessageList messageList)
    {
        base.AddError(messageList);
        return this;
    }

    /// <summary>
    /// Verifica una condición (isError) que si resulta positiva, agrega el mensaje de error al Response
    /// </summary>
    /// <param name="isError">Condición que se debe dar para agregar un error</param>
    /// <param name="mensaje">Mensaje de error</param>
    /// <param name="esMensaje">Indica si es un mensaje o un error (solo un error marca Success=false)</param>
    /// <param name="tag">Dato adicional para adjuntar al error</param>
    /// <returns></returns>
    public new Response<T> Validate(bool isError, string? mensaje, bool esMensaje = false, object? tag = null)
    {
        if (isError)
        {
            this.AddMessage(mensaje: mensaje, esError: !esMensaje, tag: tag);
            if (!esMensaje) this.Success = false;
        }
        return this;
    }

    /// <summary>
    /// Agrega los mensajes de otro Response al Response actual
    /// </summary>
    /// <param name="res">Response externo</param>
    /// <returns></returns>
    public new Response<T> Add(Response res)
    {
        base.Add(res);
        return this;
    }

    /// <summary>
    /// Agrega los mensajes de otro Response al Response actual
    /// </summary>
    /// <param name="res">Response externo</param>
    /// <returns></returns>
    public Response<T> Add(Response<T> res)
    {
        base.Add(res);
        if (res.Success) this.Data = res.Data;
        return this;
    }

    /// <summary>
    /// Devuelve el valor de Data por Output
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public Response<T> Out(out T? data)
    {
        data = this.Data;
        return this;
    }

    public Response<object?> CastToObject()
    {
        return new Response<object?>((object?)this.Data).Add(this);
    }

    public Response<TDestiny> CastTo<TDestiny>(Func<T, TDestiny> cast)
    {
        var res = new Response<TDestiny>(true).Add(this);
        if (this.Data is not null)
            res.SetData(cast(this.Data));
        return res;
    }

    public override string ToString()
    {
        return System.Text.Json.JsonSerializer.Serialize(this);
    }
}


/// <summary>
/// Clase de extensiones de SIR
/// </summary>
public static class IResponseExtensions
{
    /// <summary>
    /// Unifica una lista de Responses en uno solo. Unificando mensajes y priorizando success false (con que uno no sea exitoso, el resultado será no exitoso)
    /// </summary>
    /// <param name="responses"></param>
    /// <returns></returns>
    public static Response Join(this IEnumerable<Response> responses)
    {
        var res = new Response(true);
        responses?.ToList().ForEach(r => res.Add(r));
        return res;
    }

    /// <summary>
    /// Unifica una lista de Responses en uno solo. Unificando mensajes y priorizando success false (con que uno no sea exitoso, el resultado será no exitoso). 
    /// El tipo de dato de retorno unificará los Data válidos en una enumeración
    /// </summary>
    /// <param name="responses"></param>
    /// <returns></returns>
    public static Response<T?[]> Join<T>(this IEnumerable<Response<T>> responses)
    {
        var res = new Response<T?[]>(true);
        if (responses == null) return res;

        responses.ToList().ForEach(r => res.Add(r));
        res.Data = [.. responses.Where(p => p.Success).Select(p => p.Data)];
        return res;
    }


    /// <summary>
    /// Unifica una lista de Responses en uno solo. Unificando mensajes y priorizando success false (con que uno no sea exitoso, el resultado será no exitoso). 
    /// El tipo de dato de retorno unificará los Data válidos en una enumeración
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<Response<T>> Out<T>(this IEnumerable<Response<T>> responses, out T?[] outs)
    {
        outs = [.. responses.Where(p => p.Success).Select(p => p.Data)];
        return responses;
    }
}