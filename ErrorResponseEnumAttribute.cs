namespace BeeQ;

[AttributeUsage(AttributeTargets.Enum)]
public class ErrorResponseEnumAttribute(string module, string entity) : Attribute
{
    public string Module { get; private set; } = module;
    public string Entity { get; private set; } = entity;
}
