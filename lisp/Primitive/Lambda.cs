namespace lisp.Primitive;

// TODO: Should be atom
public class Lambda : IAtom
{
    private readonly IEnvironment _environment;
    public int Arity { get; }
    private readonly List<string> _parameters;
    private readonly Cons _body;

    public virtual object ToCompare => this;

    public Lambda(IEnvironment environment, Cons args, Cons body)
    {
        _environment = environment;

        var argList = args.ToList();
        Arity = argList.Count;
        _parameters = argList.Select(x => x.As<SymbolAtom>().Value.Name).ToList();

        _body = body;
    }
    // TODO: Implement variadic functions

    public virtual IExpression Evaluate(IEnvironment environment)
    {
        return this;
    }

    public virtual string GetString()
    {
        return $"#<FUNCTION (LAMBDA ({string.Join(" ", _parameters)})) {{{GetHashCode()}}}>";
    }

    public IExpression Run(IEnvironment environment, Cons? args)
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

        IExpression returnVal = null;
        foreach (var expression in _body)
        {
            returnVal = expression.Evaluate(_environment);
        }

        _environment.PopFrame();

        return returnVal;
    }
}