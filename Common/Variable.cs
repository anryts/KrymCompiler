namespace Common;

public record struct Variable(string Name, string Type, string Value, List<string> Operations)
{
    public Variable(string Name, string Type, string Value) : this(Name, Type, Value, new List<string>()) { }

    public string Name { get; set; } = Name;
    public string Type { get; set; } = Type;
    public string Value { get; set; } = Value;

    public List<string> Operations { get; set; } = Operations;

    public override string ToString()
    {
        return $"{Name,-10} {Type,-10} {Value,-10}";
    }
}