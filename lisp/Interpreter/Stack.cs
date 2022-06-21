using System.Collections.Immutable;
using lisp.Primitive;

namespace lisp.Interpreter;

public interface IEnvironment
{
    IExpression Get(Symbol symbol);
    IExpression Set(Symbol symbol, IExpression value);
    IExpression Set(string symbolName, IExpression value);
    IExpression SetGlobal(Symbol symbol, IExpression value);
    
    Frame NewFrame();
    void PopFrame();

    IEnvironment Closure(); // create a closure
}

public class Frame
{
    private ImmutableDictionary<Symbol, IExpression> Variables { get; set; } =
        ImmutableDictionary<Symbol, IExpression>.Empty;

    public IExpression Get(Symbol symbol) => Variables[symbol];
    public IExpression Set(Symbol symbol, IExpression value)
    {
        Variables = Variables.SetItem(symbol, value);
        return value;
    }

    public bool Contains(Symbol symbol) => Variables.ContainsKey(symbol);
}
public class Environment : IEnvironment
{
    private readonly Frame _topFrame = new();
    private ImmutableStack<Frame> Stack = ImmutableStack<Frame>.Empty;

    public Environment()
    {
        Stack = Stack.Push(_topFrame);
    }

    public Environment(ImmutableStack<Frame> stack)
    {
        Stack = stack;
    }
        
    public IExpression Get(Symbol symbol)
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

    public IExpression Get(string symbolName) => Get(new Symbol(symbolName));

    public IExpression Set(Symbol symbol, IExpression value)
    {
        var frame = Stack.Peek();
        frame.Set(symbol, value);
        return frame.Get(symbol); // Is this necessary?
    }

    public IExpression Set(string symbolName, IExpression value) => Set(new Symbol(symbolName), value);

    public IExpression SetGlobal(Symbol symbol, IExpression value)
    {
        _topFrame.Set(symbol, value);
        return _topFrame.Get(symbol);
    }

    public Frame NewFrame()
    {
        var frame = new Frame();
        Stack = Stack.Push(frame);
        return frame;
    }

    public void PopFrame()
    {
        Stack = Stack.Pop();
    }

    public IEnvironment Closure()
    {
        return new Environment(Stack);
    }
}
