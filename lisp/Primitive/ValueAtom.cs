using System.Text.RegularExpressions;

namespace lisp.Primitive;

public record Symbol(string Name);

public class SymbolAtom : ValueAtom<Symbol>
{
    public SymbolAtom(Symbol value) : base(value) {}

    public override string TypeString => "symbol";
}
public class StringAtom : ValueAtom<string>
{
    public StringAtom(string value) : base(value) {}

    public override string TypeString => "string";
}
public class FloatAtom : ValueAtom<float>
{
    public FloatAtom(float value) : base(value) {}

    public override string TypeString => "float";
}

public class IntegerAtom : ValueAtom<int>
{
    public IntegerAtom(int value) : base(value) {}

    public override string TypeString => "integer";
}
public class BoolAtom : ValueAtom<bool>
{
    public BoolAtom(bool value) : base(value) {}

    public override string TypeString => "bool";
}

public abstract class ValueAtom<T> : IAtom
{
    private static readonly Regex _floatRegex = new(@"-?\d+\.\d+", RegexOptions.Compiled);
    private static readonly Regex _intRegex = new(@"-?\d+", RegexOptions.Compiled);

    public T GetValue()
    {
        return Value;
    }

    public object ToCompare => Value as object;

    protected ValueAtom(T value)
    {
        Value = value;
    }

    public T Value { get; init; }
    public abstract string TypeString { get; }

    public static IAtom ParseString(string str)
    {
        if (_floatRegex.IsMatch(str))
        {
            var val = float.Parse(str);
            return new FloatAtom(val);
        }

        if (_intRegex.IsMatch(str))
        {
            var val = int.Parse(str);
            return new IntegerAtom(val);
        }

        return str switch
        {
            "#t" => new BoolAtom(true),
            "#f" => new BoolAtom(false),
            "nil" => new Nil(),
            _ => new SymbolAtom(new Symbol(str))
        };
    }

    public bool Compare(object compareWith)
    {
        return Value as object == compareWith;
    }

    public IExpression Evaluate(IEnvironment environment)
    {
        return this switch
        {
            SymbolAtom symbolAtom => environment.Get(symbolAtom.Value.Name),
            _ => this
        };
    }

    public string GetString()
    {
        return this switch
        {
            BoolAtom atomBool => atomBool.Value ? "#t" : "#f",
            StringAtom atomString => $"\"{atomString.Value}\"",
            SymbolAtom atomSymbol => atomSymbol.Value.Name, // TODO: Should be uppercase
            _ => Value.ToString() ?? "ERROR"
        };
    }
}