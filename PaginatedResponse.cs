using BeeQ.ResultPattern;

namespace BeeQ;

/// <summary>
/// Objeto de respuesta de un método o servicio que posee un listado de resultados paginados
/// </summary>
/// <typeparam name="T">Objeto que se lista</typeparam>
public class PaginatedResponse<T> : Response<IEnumerable<T>>
{
    /// <summary>
    /// Número de página. Base 0
    /// </summary>
    public int PageIndex { get; set; }
    /// <summary>
    /// Tamaño de la página. null indica que no se está paginando
    /// </summary>
    public int? PageSize { get; set; }
    /// <summary>
    /// Total de registros que devuelve la consulta sin el paginado
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    public PaginatedResponse() { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="data">Listado de datos ya paginados</param>
    public PaginatedResponse(IEnumerable<T> data) : base(data) { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="success">Indica si el proceso es exitóso</param>
    public PaginatedResponse(bool success) : base(success, null, false, null) { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="error">Mensaje de error</param>
    /// <param name="tag">Información adicional</param>
    public PaginatedResponse(string error, object? tag = null) : base(error, tag) { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="mensaje">Mensaje</param>
    /// <param name="esError">Indica si es un error o un mensaje de información</param>
    public PaginatedResponse(string? mensaje, bool esError) : base(!esError, mensaje, esError, null) { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="ex">Excepción encontrada</param>
    public PaginatedResponse(Exception? ex) : base(false, null, true, ex) { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="ex">Excepcion encontrada</param>
    /// <param name="tag">Información adicional</param>
    public PaginatedResponse(Exception? ex, object? tag) : base(ex, tag) { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="success">Indica si el proceso es exitóso</param>
    /// <param name="mensaje">Mensaje</param>
    /// <param name="esError">Indica si es un error o un mensaje de información</param>
    /// <param name="ex">Excepcion encontrada</param>
    public PaginatedResponse(bool success, string? mensaje, bool esError, Exception? ex) : base(success, mensaje, esError, ex) { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="res">Crea basado en otro Resultado</param>
    public PaginatedResponse(Response<T[]> res) : base(res.Messages) { this.Data = res.Data; }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="res">Crea basado en otro Resultado</param>
    public PaginatedResponse(Response res) : base(res.Messages) { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="mensajes">Crea basado en los mensajes de otro Resultado</param>
    public PaginatedResponse(ResponseMessage mensajes) : base([mensajes]) { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="mensajes">Crea basado en una lista de mensajes</param>
    public PaginatedResponse(IEnumerable<ResponseMessage> mensajes) : base([.. mensajes]) { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="list">Crea basado en una lista de mensajes</param>
    public PaginatedResponse(ResponseMessageList list) : base(list) { }

    /// <summary>
    /// Establece los parámetros de Paginación en el Response
    /// </summary>
    /// <param name="index">Número de pagina de resultados (Base 0)</param>
    /// <param name="size">[Opcional] Cantidad de resultados por página. Sólo válido para paginaciones válidas</param>
    /// <param name="total">Total de elementos de consula</param>
    /// <returns></returns>
    public PaginatedResponse<T> SetPaging(int index, int? size, int total)
    {
        this.PageIndex = index;
        this.PageSize = size;
        this.Total = total;
        return this;
    }

    /// <summary>
    /// Establece los parámetros de Paginación en el Response
    /// </summary>
    /// <param name="index">Número de pagina de resultados (Base 0)</param>
    /// <param name="size">[Opcional] Cantidad de resultados por página. Sólo válido para paginaciones válidas</param>
    /// <param name="total">Total de elementos de consula</param>
    /// <returns></returns>
    public PaginatedResponse<T> SetPaging(PagingConfig paging, int total)
    {
        this.PageIndex = paging.PageIndex;
        this.PageSize = paging.PageSize;
        this.Total = total;
        return this;
    }

    /// <summary>
    /// Establece los datos en un PaginatedResponse desde un IQueryable
    /// </summary>
    /// <typeparam name="E"></typeparam>
    /// <param name="qry">IQueryable con el query listo para ejecutarse</param>
    /// <param name="paging">configuración de paginado</param>
    /// <param name="onMap">funcion de mapeo de elementos</param>
    /// <returns></returns>
    public PaginatedResponse<T> SetPaginedData<E>(IQueryable<E> qry, PagingConfig paging, Func<E, T> onMap)
    {
        paging ??= new PagingConfig();

        var total = qry.Count();
        if (paging.HasSort)
            qry = qry.SortBySQLExpression(paging.Sort!);

        IQueryable<E> data = qry;
        if (paging.HasPaging)
            data = data.Skip(paging.PageIndex * paging.PageSize!.Value).Take(paging.PageSize!.Value);

        this.SetData(data.ToList().Select(p => onMap(p)));
        return this.SetPaging(paging.PageIndex, paging.PageSize, total);
    }

    public PaginatedResponse<TDestiny> CastTo<TDestiny>(Func<T, TDestiny> cast)
    {
        var res = new PaginatedResponse<TDestiny>(this);
        res.SetPaging(this.PageIndex, this.PageSize, this.Total);

        if (this.Data != null)
            res.SetData(this.Data.Select(d => cast(d)));

        return res;
    }
}