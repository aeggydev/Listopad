using System;
using System.Linq;
using lisp;
using lisp.Primitive;
using lisp.Reader;
using Xunit;

namespace tests;

public class InterpreterTests
{
    [Fact]
    public void ReaderTest1()
    {
        const string code = @"(+  1  2 3    4 (+ 1 2 5 ) 3  )";
        var data = Reader.ReadFromString(code).As<Cons>();
        Assert.Equal(7, data.Count());
        Assert.Equal(2, data[5]?.As<Cons>()[2]?.As<IntegerAtom>().Value);

        Interpreter interpreter = new();
        var result = interpreter.Evaluate(data);
        Assert.Equal(21 as object, result.As<IntegerAtom>().Value as object);
    }

    [Fact]
    public void ReaderTest2()
    {
        const string code = @"(begin (define foobar (lambda (lol) (inc (inc lol)))) (foobar 60))";
        var data = Reader.ReadFromString(code) as Cons;
        Assert.Equal("define", data?[1]?.As<Cons>()[0]?.As<SymbolAtom>().Value.Name);

        Interpreter interpreter = new();
        Assert.Equal(62 as object, interpreter.Evaluate(data).As<IntegerAtom>().Value as object);
    }

    [Fact]
    public void TrailingGarbageTest1()
    {
        const string code = @"5 (+ 1 1)";
        Interpreter interpreter = new();
        Assert.Throws<Exception>(() => interpreter.ReadAndEvalute(code));
    }
    
    [Fact]
    public void TrailingGarbageTest2()
    {
        const string code = @"(+ 1 1) 5";
        Interpreter interpreter = new();
        Assert.Throws<Exception>(() => interpreter.ReadAndEvalute(code));
    }

    [Fact]
    public void ConsTest1()
    {
        const string code = @"(cons 1 (cons 2 3))";
        Interpreter interpreter = new();
        Cons cons = null;
        var exception = Record.Exception(() => cons = interpreter.ReadAndEvalute(code).As<Cons>());
        Assert.Null(exception);
        Assert.Equal(3, cons?.Count());
    }

    [Fact]
    public void Eq1()
    {
        const string code = @"(eq 1 1)";
        Interpreter interpreter = new();
        var expression = interpreter.ReadAndEvalute(code);
        Assert.True(expression.As<BoolAtom>().Value);
    }

    [Fact]
    public void Concat1()
    {
        const string code = @"(concat '(1 2) '(3 4))";
        Interpreter interpreter = new();

        var expression = interpreter.ReadAndEvalute(code);
        Assert.Equal(4, expression.As<Cons>().Count());
    }
    
    [Fact]
    public void Concat2()
    {
        const string code = @"(concat '(1 2) '(3 4) '(5 6))";
        Interpreter interpreter = new();

        var expression = interpreter.ReadAndEvalute(code);
        Assert.Equal(6, expression.As<Cons>().Count());
    }

    [Fact]
    public void Backquote1()
    {
        const string code = @"`(1 1)";
        Interpreter interpreter = new();
        var expression = interpreter.ReadAndEvalute(code).As<Cons>();
        Assert.Equal(2, expression.Count());
    }
    
    [Fact]
    public void Backquote2()
    {
        const string code = @"`(1 ~(list 2 3 4))";
        Interpreter interpreter = new();
        var expression = interpreter.ReadAndEvalute(code).As<Cons>();
        Assert.Equal(2, expression.Count());
        Assert.Equal(3, expression.Last().As<Cons>().Count());
    }
    
    
    [Fact]
    public void Backquote3()
    {
        const string code = @"`(1 ~@(list 2 3 4))";
        Interpreter interpreter = new();
        var expression = interpreter.ReadAndEvalute(code).As<Cons>();
        Assert.Equal(4, expression.Count());
    }
}