using System;
using System.Linq;
using lisp;
using lisp.Reader;
using Xunit;

namespace tests;

public class InterpreterTests
{
    [Fact]
    public void ReaderTest1()
    {
        const string code = @"(+  1  2 3    4 (+ 1 2 5 ) 3  )";
        var data = Reader.ReadFromString(code) as Cons;
        Assert.Equal(7, data.Count());
        Assert.Equal(new Atom(2).Value, ((data[5] as Cons)[2] as Atom).Value);

        Interpreter interpreter = new();
        var result = interpreter.Evaluate(data);
        Assert.Equal(21 as object, (result as Atom).Value);
    }

    [Fact]
    public void ReaderTest2()
    {
        const string code = @"(begin (define foobar (lambda (lol) (inc (inc lol)))) (foobar 60))";
        var data = Reader.ReadFromString(code) as Cons;
        Assert.Equal("define", ((data[1] as Cons)[0] as Atom).Value);

        Interpreter interpreter = new();
        Assert.Equal(62 as object, (interpreter.Evaluate(data) as Atom).Value);
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
}