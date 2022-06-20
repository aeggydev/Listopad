using System.Collections;

namespace lisp.Primitive;

public interface ISeq : IExpression, IEnumerable<IExpression> { }

public record Nil : ISeq, IAtom
{
    public IExpression Evaluate(IEnvironment environment) => this;

    public string GetString() => "NIL";

    public object ToCompare => this;
    public IEnumerator<IExpression> GetEnumerator()
    {
        yield break;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class Cons : ISeq
{
    private IExpression? _cdr;
    public IExpression? Car { get; set; }

    public IExpression? Cdr
    {
        get => _cdr ?? new Nil();
        set => _cdr = value;
    }

    public bool IsList => Cdr is Cons or null;
    public object ToCompare => this;

    public IExpression Evaluate(IEnvironment environment)
    {
        var expression = Car switch
        {
            Cons consCar => consCar.Evaluate(environment),
            SymbolAtom atomCar => environment.Get(atomCar.Value.Name),
            Lambda => Car,
            Native => Car,
            _ => throw new Exception("Illegal function call")
        };

        switch (expression)
        {
            case Native native:
                return native.Run(environment, Cdr as Cons);
            case Lambda lambda:
                return lambda.Run(environment, Cdr as Cons);
            default:
                throw new Exception("Don't know");
        }
    }

    public virtual string GetString()
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

    public static Cons FromIEnumerable(IEnumerable<IExpression> enumerable)
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

    // Enable deconstructing
    public void Deconstruct(out IExpression car, out IExpression cdr)
    {
        car = Car;
        cdr = Cdr;
    }

    public IEnumerator<IExpression> GetEnumerator()
    {
        var current = this;
        while (current != null)
        {
            yield return current.Car;
            switch (current.Cdr)
            {
                case Cons cdrCons:
                    current = cdrCons;
                    break;
                case null or Nil:
                    current = null;
                    break;
                default:
                    yield return current.Cdr;
                    yield break;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IExpression? this[int i] => this.Skip(i).First();
}