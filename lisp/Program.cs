﻿using lisp;

Interpreter interpreter = new();
while (true) {
    Console.Write("> ");
    var code = Console.ReadLine().TrimEnd();
    var read = Reader.ReadFromString(code);
    var returnValue = interpreter.Evaluate(read);
    
    Console.WriteLine(returnValue.Prin1ToString());
    //Console.WriteLine(returnValue is Atom atom ? atom.Value : $"DATA OF TYPE: {returnValue.GetType().Name}");
    // TODO: Replace with prin1
    Console.WriteLine();
}