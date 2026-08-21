# BeeQ.ResultPattern
El Result Pattern es un patrón de diseño para resolver el problema de los múltiples retornos de las funciones, métodos, endpoints, etc que en general suelen devolver un dato u objeto pero tambien puede devolver excepciones. \
En este documento no nos adentraremos en el problema puntual que resuelve, pero mostraremos la solución aquí planteada.

## Objeto Response Principal
La librería incluye un objeto `Response` que contiene toda la información que se pueda requerir como retorno de una función. \
Posee múltiples constructores para su facil uso y veremos en esta documentación como usarlo de forma ágil y sencilla.
La estructura básica de sus propiedades es:
``` csharp
public class Response
{
    public bool Success { get; set; }
    public bool ExistsErrorMessages { get; }
    public ResponseMessageList Messages { get; }
}
```
Tambien posee una versión para contener un dato de un tipo especificado:
``` csharp
public class Response<T> : Response
{
    public T? Data { get; set; }
}
```
Por lo que en los casos de funciones `void` se puede cambiar su uso por `Response` y, por ejemplo, una función que devuelve un `int` se reemplazaría por `Response<int>`.
> [!NOTE]
> Se cambia el nombre `Response` a `ResponseBase` en este caso porque Typescript no puede usar los nombres `Response` y `Response<T>` a la vez, pero C# sí puede.
### Formato Json equivalente (Typescript):
``` typescript
export type ResponseBase = {
  success: boolean;
  error?: string;
  messages?: ResponseMessage[];
};

export type Response<T> = ResponseBase & {
  data?: T;
}

export type ResponseMessage = {
  code?: string;
  text: string;
  esError: boolean;
  tag?: any;
  exceptionType?: string;
}
```


## Uso básico
El objeto tiene diversos constructores que ayudan con su uso, veamos unos ejemplos: \
En el siguiente ejemplo, vemos como devolver un Response exitoso
``` csharp
public Response Test()
{
    ...
    return new Response(true);
}
```
o bien, devolviendo un valor:
``` csharp
public Response<int> Test()
{
    int value = ...;
    return new Response<int>(value);
}
```

### Mensajes y Errores
Para devolver un mensaje de error, la forma mas básica es la siguiente:
``` csharp
public Response Test(int value)
{
    if (value <= 0)
        return new Response("valor debe ser positivo");
    ...
}
```
tambien podemos devolver a partir de un `Exception`.
``` csharp
public Response Test()
{
    try 
    {
        ...
    }
    catch (Exception ex)
    {
        return new Response(ex);
    }
}
```
Tambien podemos devolver mensajes sin que estos impliquen un error:
``` csharp
public Response Test(int value)
{
    Response resultado = new Response(true)
    if (value <= 1)
    {
        value = 1;
        resultado.AddMessage("valor menor a 1. Se reestableció el valor en 1", esError: false);
    }
    ...

    // Tambien es válido un constructor con este mismo fin
    return new Response("valor menor a 1. Se reestableció el valor en 1", esError: false);
}
```

### Uso diario
Si bien el objeto tiene multiples constructores, muchas veces es útil crear el objeto al principio de la función e ir agregándole errores o mensajes, como en el caso anterior, por lo que tambien tiene las funciones adicionales `Add`, `AddError`, `AddMessage`, `SetData`, etc. \
A continuación veremos un ejemplo de SetData:
``` csharp
public Response<int> ForzarANumeroPositivo(int value)
{
    Response resultado = new Response(true)
    if (value <= 1)
    {
        resultado.SetData(1);

        // tambien es válido:
        resultado.Data = 1;

        // la funcion es util en los casos de querer hacer llamadas en cascada al Response, por ejemplo:
        resultado.SetData(1)
            .AddMessage("valor menor a 1. Se reestableció el valor en 1", esError: false);
    }
    return resultado;
}
```

#### Concatenación de Mensajes
La librería permite guardar multiples mensajes en un mismo `Response`.
``` csharp
public Response ValidarCliente(ClienteDto dto)
{
    Response resultado = new Response(true);
    if (dto.Nombre == null)
        resultado.AddError("Campo Nombre sin valor");
    if (dto.Direccion == null)
        resultado.AddError("Campo Dirección sin valor");

    return resultado;
}
```

#### Concatenación de Resultados
Muchas veces las funciones de validaciones contienen varias llamadas a otras funciones de validaciones y todas devolverían su propio `Response` y se vuelve engorroso validar uno por uno, por lo que una opción viable es unificar todos los Response en uno solo. \
Por poner un ejemplo:
``` csharp
public Response ValidarCliente(ClienteDto dto)
{
    Response resultado = new Response(true);
    resultado.Add(ValidarCliente_Proveedores(dto.Proveedores));
    resultado.Add(ValidarCliente_Productos(dto.Productos));
    resultado.Add(ValidarCliente_Sucursales(dto.Sucursales));

    return resultado;
}
```
De esta forma, los mensajes incorporados a todas las validaciones quedan unidos en un único `Response`.


## Uso de información adicional (tags)
Muchas veces, nos encontramos en otras librerías o soluciones caseras donde además de devolver un mensaje necesitamos adjuntar información a ese mensaje, información que muchas veces no puede viajar en el cuerpo del mensaje o bien porque no es un texto o bien porque no queremos cambiar el texto del mensaje y solo adjuntar un dato adicional.\
Para esto es el atributo Tag de los mensajes, para adjutar información que luego puede ser relevante:
``` csharp
public Response Test(ClienteDto dto)
{
    Response resultado = new Response(true);
    if (dto.Nombre == null || dto.Nombre.Contains(CARACTERES_INVALIDOS))
        resultado.AddError("Campo Nombre inválido", tag: dto.Nombre);
}
```
De esta manera, el objeto `Response` no solo contiene el mensaje, sino que también contiene la información adicional, que en este caso es el nombre del cliente que falló. \
Lo que tambien facilita mucho el log de la aplicación sin tener que cambiar el mensaje de error original.
``` csharp
public Response ProcesoPrincipal(ClienteDto dto)
{
    var resultado = ValidarCliente(dto);
    if (!resultado.Success)
    {
        foreach (var msg in resultado.Messages)
        {
            var logTemplate = $"{msg} {{tag}}";
            if (msg.EsError)
                logger.LogError(logTemplate, msg.Tag);
            else
                logger.LogInformation(logTemplate, msg.Tag);
        }
    }
}
```

## Resultados Paginados
Ocurre en muchas ocaciones donde el resultado que necesitamos devolver está paginado, es decír, que tiene un número de página, un tamaño de página y un total de registros pero solo se devuelven los resultados de esa pagina en particular. \
Esto está resuelto en la estructura `PaginatedReponse<T>` donde ya está resuelto el paginado y solo debemos establecer `T`. \
En el siguiente ejemplo introducimos la clase `PagingConfig` nativa de esta librería que nos ayudará con el paginado definiendo el index, size, total e incluso order. Tambien utilizaremos de ejemplo el DbContext de EF y finalmente el Mapeo automático de BeeQ.Mapper aunque se puede utilizar cualquier librería o escribir el mapeo de forma custom. \

``` csharp
public void GetClientes(int index, int size, ...)
{
    var res = new PaginatedResponse<ClienteDto>(true);

    var pag = new PagingConfig(index, size);
    var convert = (Cliente db) => db.Mapping<ClienteDto>();

    var qry = DbContext.Cliente.Where(...);

    return res.SetPaginedData<ClienteDto>(qry, pag, convert);
}
```
Este ejemplo utiliza `IQueriable` por lo que al hacer el `Cliente.Where(...)` no es necesario establecer los `limits`, ni `top` ya que lo hace la función `SetPaginedData`

### Alternativa Custom
Si bien lo mas cómodo es trabajar con `IQueriable` ya que es la manera de bajar el consumo de acceso a datos al mínimo, existe una manera custom de completar un `PaginedData` solo que en este caso, el cálculo de total se debe hacer de forma personalizada.
``` csharp
public void GetClientes(int index, int size, ...)
{
    var res = new PaginatedResponse<ClienteDto>(true);

    var datos = ...;
    var total_rows = ...;

    return res.SetData(datos)
        .SetPaging(index, size, total_rows);
}
```

## Manejo de Errores
La librería tambien contiene una forma opcional de manejo de errores que creemos que es muy útil ya que permite generar códigos de errores únicos, traceables y traducibles. \
La idea es que los posibles errores de la aplicación estén en una o más enumeraciones, estas estarán marcadas con el atributo `[ErrorResponseEnum]` al cual se le indicará de forma única un módulo y una entidad a la cual se refiere la lista de errores. \
Por ejemplo, para los posibles errores con el Usuario en el módulo de Seguridad:
``` csharp
[ErrorResponseEnum(module:"SEC", entity: "USR")]
public enum UserErrors
{
    [Description("Usuario no existe")]
    UsuarioNotExists = 1,

    [Description("Contraseña inválida")]
    PasswordInvald = 2,

    [Description("Login duplicado")]
    LoginDuplicated = 3,
}
```
Esta configuración generará los códigos de errores `SEC0001USR`, `SEC0002USR` y `SEC0003USR`. \
Para evitar códigos repetidos, evite colocar la misma combinación de modulo y entidad en las enumeraciones. \
Además, evite eliminar los valores de Enumeración que ya no se usen, ya que puede venir un developer y reutilizar el mismo número, esto se soluciona dejando comentado el valor o colocando el atributo `[Obsolete]`.

### Ejemplos de uso
La forma mas simple es utilizar directamente la enumeración, invocando al método `GetError` que puede ser tipado según se necesite como salida de la función, por ejemplo:

``` csharp
public Response<string> Login(AuthDto auth)
{
    var user = GetUser(auth.Username);
    if (user == null)
        return UserErrors.UsuarioNotExists.GetError<string>();
    ...
    return new(token);
}
```

otra forma es usar directamente el Builder de errores:
``` csharp
public Response<string> Login(AuthDto auth)
{
    var user = GetUser(auth.Username);
    if (user == null)
        return ErrorBuilder.Build<UserErrors, string>(UserErrors.UsuarioNotExists);
    ...
    return new(token);
}
```

> [!NOTE]
> Consideramos que de esta forma se están atacando muchos problemas a la vez, y principalmente, el código es muy simple y legible.

### Conclusiones
* Genera código simple y legible.
* Se genera un código único de error muy facil de rastear
* Muy facil de utilizar para traductores ya que se puede usar como `key` único en un archivo de Recursos, i18n, etc.
* Es muy facil de usar y de mantener, agregar nuevos errores es muy simple.
* El código de error viaja al igual que el `Tag`, en el mensaje a través del campo `Code` por lo que no se pisan, sino que se complementan.
