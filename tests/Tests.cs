using System;
using System.Linq;
using lisp;
using lisp.Interpreter;
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
        var exception = Record.Exception(() => cons = interpreter.ReadAndEvalute(code).AsCons());
        Assert.Null(exception);
        Assert.Equal(3, cons?.Count());
    }

    [Fact]
    public void Eq1()
    {
        const string code1 = @"(eq 1 1)";
        const string code2 = @"(eq ""foo"" ""bar"")";
        Interpreter interpreter = new();
        Assert.True(interpreter.ReadAndEvalute(code1)
            .As<BoolAtom>().Value);
        Assert.False(interpreter.ReadAndEvalute(code2)
            .As<BoolAtom>().Value);
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
    public void Comment()
    {
        const string code = @"(+ 5 5) ; Adds 5 and 5 together";
        Interpreter interpreter = new();
        int result = 0;
        var exception = Record.Exception(() => result = interpreter.ReadAndEvalute(code).AsValue<int>());
        Assert.Null(exception);
        Assert.Equal(10, result);
    }

    [Fact]
    public void Nil()
    {
        const string code1 = @"((lambda nil 12))";
        const string code2 = @"((lambda () 12))";
        const string code3 = @"((lambda '() 12))";
        Interpreter interpreter = new();
        var expression1 = interpreter.ReadAndEvalute(code1);
        var expression2 = interpreter.ReadAndEvalute(code2);
        var expression3 = interpreter.ReadAndEvalute(code3);
        Assert.Equal(12, expression1.AsValue<int>());
        Assert.Equal(12, expression2.AsValue<int>());
        Assert.Equal(12, expression3.AsValue<int>());
    }
    
    [Fact]
    public void Backquote1()
    {
        const string code = @"`(1 1)";
        Interpreter interpreter = new();
        var expression = interpreter.ReadAndEvalute(code).AsCons();
        Assert.Equal(2, expression.Count());
    }

    
    [Fact]
    public void Backquote2()
    {
        const string code = @"`(1 ~(list 2 3 4))";
        Interpreter interpreter = new();
        var expression = interpreter.ReadAndEvalute(code).AsCons();
        Assert.Equal(2, expression.Count());
        Assert.Equal(3, expression.Last().AsCons().Count());
    }
    
    [Fact]
    public void Backquote3()
    {
        const string code = @"`(1 ~@(list 2 3 4))";
        Interpreter interpreter = new();
        var expression = interpreter.ReadAndEvalute(code).AsCons();
        Assert.Equal(4, expression.Count());
    }

    [Fact]
    public void GetStringSymbol1()
    {
        const string code = @"(list 1 (list 'list 2 3 4))";
        Interpreter interpreter = new();
        var asString = interpreter.ReadAndEvalute(code).GetString();
        Assert.Equal("(1 (list 2 3 4))", asString);
    }
}