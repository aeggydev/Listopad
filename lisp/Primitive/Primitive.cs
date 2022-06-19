namespace lisp.Primitive;

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