using lisp;
using lisp.Primitive;
using lisp.Reader;

Interpreter interpreter = new();
while (true)
{
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
        var atom = interpreter._environment.Get("*debug-on-exception*").As<Atom>();
        var debug = atom.GetValue<bool>();
        switch (debug)
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