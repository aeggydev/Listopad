using System.Text.RegularExpressions;

namespace lisp.Primitive;

public enum AtomTypes
{
    Integer,
    Float,
    String,
    Symbol,
    Boolean
}

public class ValueAtom : Atom
{
    private static readonly Regex _floatRegex = new(@"-?\d+\.\d+", RegexOptions.Compiled);
    private static readonly Regex _intRegex = new(@"-?\d+", RegexOptions.Compiled);

    public T GetValue<T>()
    {
        return (T)Value;
    }

    public ValueAtom(object value, AtomTypes type)
    {
        Value = value;
        Type = type;
    }

    public ValueAtom(int value)
    {
        Value = value as object;
        Type = AtomTypes.Integer;
    }

    public ValueAtom(float value)
    {
        Value = value as object;
        Type = AtomTypes.Float;
    }

    public ValueAtom(bool value)
    {
        Value = value as object;
        Type = AtomTypes.Boolean;
    }

    public ValueAtom(string value)
    {
        Value = value;
        Type = AtomTypes.String;
    }

    public object Value { get; init; }
    public AtomTypes Type { get; init; }
    public string TypeString => Enum.GetName(Type)?.ToLower() ?? "error";

    public static ValueAtom ParseString(string str)
    {
        if (_floatRegex.IsMatch(str))
        {
            var val = float.Parse(str);
            return new ValueAtom(val as object, AtomTypes.Float);
        }

        if (_intRegex.IsMatch(str))
        {
            var val = int.Parse(str);
            return new ValueAtom(val as object, AtomTypes.Integer);
        }

        return str switch
        {
            "#t" => new ValueAtom(true as object, AtomTypes.Boolean),
            "#f" => new ValueAtom(false as object, AtomTypes.Boolean),
            _ => new ValueAtom(str, AtomTypes.Symbol)
        };
    }

    public override Expression Evaluate(IEnvironment environment)
    {
        return this switch
        {
            { Type: AtomTypes.Symbol } => environment.Get((string)Value),
            _ => this
        };
    }

    public override string GetString()
    {
        return this switch
        {
            { Type: AtomTypes.Boolean } atomBool => (bool)atomBool.Value ? "#t" : "#f",
            { Type: AtomTypes.String } atomString => $"\"{atomString.Value}\"",
            _ => Value.ToString() ?? "ERROR"
        };
    }
}