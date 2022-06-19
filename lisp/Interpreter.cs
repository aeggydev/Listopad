namespace lisp;

public interface IEnvironment
{
    Expression Get(string symbol);
    Expression Set(string symbol, Expression value);
    Expression SetGlobal(string symbol, Expression value);
    
    void NewFrame();
    void PopFrame();

    IEnvironment Closure(); // create a closure
}

public class Frame
{
    public int Level { get; }
    public Dictionary<string, Expression> Variables { get; }
    
    public Frame(int level)
    {
        Level = level;
        Variables = new Dictionary<string, Expression>();
    }

    public Expression Get(string symbol) => Variables[symbol];
    public Expression Set(string symbol, Expression value) => Variables[symbol] = value;
    public bool Contains(string symbol) => Variables.ContainsKey(symbol);
}
public class Environment : IEnvironment
{
    private readonly Frame _topFrame = new(0);
    private readonly Stack<Frame> Stack = new();
    private int _level = 0;

    public Environment()
    {
        Stack.Push(_topFrame);
    }

    public Environment(Stack<Frame> stack)
    {
        Stack = stack;
    }
        
    public Expression Get(string symbol)
    {
        foreach (var frame in Stack)
        {
            if (frame.Contains(symbol))
            {
                return frame.Get(symbol);
            }
        }

        throw new Exception("Variable doesn't exist in scope");
    }

    public Expression Set(string symbol, Expression value)
    {
        var frame = Stack.Peek();
        frame.Set(symbol, value);
        return frame.Get(symbol); // Is this necessary?
    }

    public Expression SetGlobal(string symbol, Expression value)
    {
        _topFrame.Set(symbol, value);
        return _topFrame.Get(symbol);
    }

    public void NewFrame()
    {
        _level += 1;
        Stack.Push(new Frame(_level));
    }

    public void PopFrame()
    {
        _level -= 1;
        Stack.Pop();
    }

    public IEnvironment Closure()
    {
        return new Environment(new Stack<Frame>(Stack.Reverse()));
    }
}

public class Interpreter
{
    private readonly Environment _environment = new();

    public Interpreter()
    {
        _environment.Set("+", new Plus());
        _environment.Set("-", new Minus());
        _environment.Set("car", new Car());
        _environment.Set("cdr", new Cdr());
        _environment.Set("quote", new Quote());
        _environment.Set("cons", new ConsFunc());
        _environment.Set("eval", new Eval());
        _environment.Set("list", new ListFunc());
        _environment.Set("begin", new BeginFunc());
        _environment.Set("apply", new ApplyFunc());
        
        _environment.Set("exit", new Exit());
        _environment.Set("debug", new Debug());

        _environment.Set("eq", new Eq());
        _environment.Set("and", new And());
        _environment.Set("or", new Or());
        _environment.Set("if", new If());
        _environment.Set("pi", new Atom(314 as object, AtomTypes.Integer));

        _environment.Set("define", new Define());
        _environment.Set("lambda", new LambdaFunc());

        _environment.Set("atomp", new AtomP());

        ReadAndEvalute(Prelude);
    }

    public Expression Evaluate(Expression expression)
    {
        var returnValue = expression.Evaluate(_environment);
        return returnValue;
    }

    public Expression ReadAndEvalute(string code)
    {
        return Evaluate(Reader.ReadFromString(code));
    }

    private const string Prelude = @"
(begin
  (define inc (lambda (x) (+ x 1)))
  (define dec (lambda (x) (- x 1))))";
}