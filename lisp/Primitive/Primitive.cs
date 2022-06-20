namespace lisp.Primitive;

public interface IExpression
{
    public IExpression Evaluate(IEnvironment environment);
    public string GetString();
    public object ToCompare { get; }
}

public static class ExpressionExtensions
{
    public static T As<T>(this IExpression expression) where T : IExpression
    {
        return expression switch
        {
            T thisT => thisT,
            _ => throw new Exception($"Expression is not {typeof(T).Name}")
        };
    }
    public static T AsValue<T>(this IExpression expression)
    {
        return expression switch
        {
            ValueAtom<T> valueAtom => valueAtom.Value,
            _ => throw new Exception($"Expression is not {typeof(T).Name}")
        };
    }

    public static Cons AsCons(this IExpression expression)
    {
        return expression switch
        {
            Cons valueCons => valueCons,
            _ => throw new Exception($"Expression is not cons")
        };
    }

    public static T UncheckedAs<T>(this IExpression expression) where T : IExpression
    {
        return (T)expression;
    }

    public static Cons Wrap(this IExpression toWrap, IExpression wrapWith)
    {
        return new Cons { Car = wrapWith, Cdr = toWrap };
    }
    
}