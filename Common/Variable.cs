namespace Common;

public record Variable(string Name, string Type, string Value)
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Value { get; set; }

    public override string ToString()
    {
        return $"{Name, -10} {Type,-10} {Value,-10}";
    }
}