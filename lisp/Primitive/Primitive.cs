namespace lisp.Primitive;

public abstract class Expression
{
    public abstract Expression Evaluate(IEnvironment environment);

    public abstract string GetString();
    public abstract object ToCompare { get; }

    public T As<T>() where T : Expression
    {
        return this switch
        {
            T thisT => thisT,
            _ => throw new Exception($"Expression is not {typeof(T).Name}")
        };
    }
    public T AsValue<T>()
    {
        return this switch
        {
            ValueAtom<T> valueAtom => valueAtom.Value,
            _ => throw new Exception($"Expression is not {typeof(T).Name}")
        };
    }

    public Cons AsCons()
    {
        return this switch
        {
            Cons valueCons => valueCons,
            _ => throw new Exception($"Expression is not cons")
        };
    }

    public T UncheckedAs<T>() where T : Expression
    {
        return (T)this;
    }

    public Cons Wrap(Expression wrapWith)
    {
        return new Cons { Car = wrapWith, Cdr = this };
    }
}