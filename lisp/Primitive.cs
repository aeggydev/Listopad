using System.Collections;
using System.Text.RegularExpressions;

namespace lisp;

public abstract class Expression
{
    public abstract Expression Evaluate(IEnvironment environment);

    public abstract string GetString();

    public T As<T>() where T : Expression
    {
        return this switch
        {
            T thisT => thisT,
            _ => throw new Exception($"Expression is not {typeof(T).Name}")
        };
    }

    public Cons Wrap(Expression wrapWith)
    {
        return new Cons { Car = wrapWith, Cdr = this };
    }
}

public class Cons : Expression, IEnumerable<Expression>
{
    public Expression? Car { get; set; }
    public Expression? Cdr { get; set; }
    public bool IsList => Cdr is Cons or null;

    public override Expression Evaluate(IEnvironment environment)
    {
        var expression = Car switch
        {
            Cons consCar => consCar.Evaluate(environment),
            Atom { Type: AtomTypes.Symbol } atomCar => environment.Get(atomCar.GetValue<string>()),
            Lambda => Car,
            Native => Car,
            _ => throw new Exception("Illegal function call")
        };

        if (expression is Native native)
        {
            // TODO: Get rid of this ugly hack
            environment.Set("*ARGS*", Cdr);
            return native.Evaluate(environment);
        }

        if (expression is Lambda lambda)
        {
            return lambda.Run(environment, Cdr as Cons);
        }

        throw new Exception("Don't know");
    }

    public override string GetString()
    {
        if (IsList) // Is a list
        {
            var strings = this.Select(x => x.GetString());
            return $"({string.Join(" ", strings)})";
        }
        else // Is a pair
        {
            return $"({Car.GetString()} . {Cdr.GetString()})";
        }
    }

    public static Cons FromIEnumerable(IEnumerable<Expression> enumerable)
    {
        var (car, cdr) = enumerable.Cons();
        var list = new Cons { Car = car };
        var current = list;
        foreach (var item in cdr)
        {
            current.Cdr = new Cons { Car = item };
            current = (Cons)current.Cdr;
        }

        return list;
    }

    // TODO: Implement indexing

    // Enable deconstructing
    public void Deconstruct(out Expression car, out Expression cdr)
    {
        car = Car;
        cdr = Cdr;
    }

    public IEnumerator<Expression> GetEnumerator()
    {
        var current = this;
        while (current != null)
        {
            yield return current.Car;
            current = (Cons)current.Cdr;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Expression? this[int i] => this.Skip(i).First();
}

public enum AtomTypes
{
    Integer,
    Float,
    String,
    Symbol,
    Boolean,
    Lambda,
}

public class Atom : Expression
{
    private static readonly Regex _floatRegex = new(@"-?\d+\.\d+", RegexOptions.Compiled);
    private static readonly Regex _intRegex = new(@"-?\d+", RegexOptions.Compiled);

    public T GetValue<T>()
    {
        return (T)Value;
    }

    public Atom(object value, AtomTypes type)
    {
        Value = value;
        Type = type;
    }

    public Atom(int value)
    {
        Value = value as object;
        Type = AtomTypes.Integer;
    }

    public Atom(float value)
    {
        Value = value as object;
        Type = AtomTypes.Float;
    }

    public Atom(bool value)
    {
        Value = value as object;
        Type = AtomTypes.Boolean;
    }

    public Atom(string value)
    {
        Value = value;
        Type = AtomTypes.String;
    }

    public object Value { get; init; }
    public AtomTypes Type { get; init; }
    public string TypeString => Enum.GetName(Type)?.ToLower() ?? "error";

    public static Atom ParseString(string str)
    {
        if (_floatRegex.IsMatch(str))
        {
            var val = float.Parse(str);
            return new Atom(val as object, AtomTypes.Float);
        }

        if (_intRegex.IsMatch(str))
        {
            var val = int.Parse(str);
            return new Atom(val as object, AtomTypes.Integer);
        }

        if (str == "#t")
        {
            return new Atom(true as object, AtomTypes.Boolean);
        }

        if (str == "#f")
        {
            return new Atom(false as object, AtomTypes.Boolean);
        }

        return new Atom(str, AtomTypes.Symbol);
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

public class Lambda : Expression
{
    private readonly IEnvironment _environment;
    public int Arity { get; }
    private readonly List<string> _parameters;
    private readonly Cons _body;

    public Lambda(IEnvironment environment, Cons args, Cons body)
    {
        _environment = environment;

        var argList = args.ToList();
        Arity = argList.Count;
        _parameters = argList.Select(x => (x as Atom).Value as string).ToList();

        _body = body;
    }
    // TODO: Implement variadic functions

    public override Expression Evaluate(IEnvironment environment)
    {
        return this;
    }

    public override string GetString()
    {
        return $"#<FUNCTION (LAMBDA ({string.Join(" ", _parameters)})) {{{GetHashCode()}}}>";
    }

    public Expression Run(IEnvironment environment, Cons? args)
    {
        var argList = args?
            .Select(x =>
            {
                // TODO: This is a horrible way to do it; fix it
                try
                {
                    return x.Evaluate(_environment);
                }
                catch
                {
                    return x.Evaluate(environment);
                }
            })
            .ToList();
        if ((argList?.Count ?? 0) != Arity) throw new Exception("Wrong number of arguments");

        _environment.NewFrame();
        for (var i = 0; i < Arity; i++)
        {
            _environment.Set(_parameters[i], argList![i]);
        }

        Expression returnVal = null;
        foreach (var expression in _body)
        {
            returnVal = expression.Evaluate(_environment);
        }

        _environment.PopFrame();

        return returnVal;
    }
}