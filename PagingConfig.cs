namespace BeeQ.ResultPattern;

/// <summary>
/// Objeto con información de Paginado
/// </summary>
public class PagingConfig
{
    /// <summary>
    /// Número de página que se desea visualizar. Base 0
    /// </summary>
    public int PageIndex { get; set; } = 0;
    /// <summary>
    /// Tamaño de la pagina. null indica sin límite
    /// </summary>
    public int? PageSize { get; set; }
    /// <summary>
    /// Método de ordenación con formato "campo1 ASC, campo2 DESC"
    /// </summary>
    public string? Sort { get; set; }

    public string OrderByEmbeded(string orderDef)
    {
        var order = orderDef;
        if (!string.IsNullOrEmpty(Sort))
        {
            var parts = this.Sort.Split(' ');
            order = $@"""{parts[0]}"" {parts[1]}";
        }

        return $@"ORDER BY {order} {(this.Offset.HasValue ? $" OFFSET {this.Offset} ROWS FETCH NEXT {this.PageSize} ROWS ONLY" : "")}";
    }

    /// <summary>
    /// Indica si se debe paginar
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore()]
    public bool HasPaging => this.PageSize.HasValue;

    /// <summary>
    /// Indica si se posee ordenamiento
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore()]
    public bool HasSort => !string.IsNullOrWhiteSpace(this.Sort);

    public int? Offset => this.PageSize == null ? null : (this.PageIndex * this.PageSize);

    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    public PagingConfig() { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="index">Número de página que se desea visualizar. Base 0</param>
    /// <param name="size">Tamaño de la pagina. null indica sin límite</param>
    public PagingConfig(int? index, int? size) : this(index, size, null) { }
    /// <summary>
    /// Crea una nueva instancia
    /// </summary>
    /// <param name="index">Número de página que se desea visualizar. Base 0</param>
    /// <param name="size">Tamaño de la pagina. null indica sin límite</param>
    /// <param name="sort">Método de ordenación con formato "campo1 ASC, campo2 DESC"</param>
    public PagingConfig(int? index, int? size, string? sort)
    {
        this.PageIndex = index ?? 0;
        this.PageSize = size;
        this.Sort = sort;
    }

    public IQueryable<T> Apply<T>(IQueryable<T> query)
    {
        if (!string.IsNullOrEmpty(this.Sort))
            query = query.SortBySQLExpression(this.Sort);

        if (this.Offset.HasValue)
            query = query.Skip(this.Offset.Value).Take(this.PageSize!.Value);

        return query;
    }
}