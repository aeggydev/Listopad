using lisp;
using lisp.Primitive;
using lisp.Reader;

Interpreter interpreter = new();
while (true)
{
    // TODO: Make it accept multiple lines
    Console.Write("> ");
    try
    {
        var code = Console.ReadLine().TrimEnd();
        var read = Reader.ReadFromString(code);
        var returnValue = interpreter.Evaluate(read);

        Console.WriteLine(returnValue.GetString());
        Console.WriteLine();
    }
    catch (Exception e)
    {
        var atom = interpreter._environment.Get("*debug-on-exception*").As<ValueAtom<bool>>();
        switch (atom.Value)
        {
            case true:
                // TODO: It debugs either way
                break;
            case false:
                Console.WriteLine($"Error: {e.Message}\n{e.StackTrace}");
                break;
        }
    }
}