namespace Common;

public record Variable(string Name, string Type, string Value)
{
    public string Name { get; set; } = Name;
    public string Type { get; set; } = Type;
    public string Value { get; set; } = Value;

    public override string ToString()
    {
        return $"{Name, -10} {Type,-10} {Value,-10}";
    }
}