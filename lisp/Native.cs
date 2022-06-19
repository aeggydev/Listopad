﻿using System.Diagnostics;

namespace lisp;

// TODO: Make this less verbose
public abstract class Native : Expression
{
    public override Expression Evaluate(IEnvironment environment)
    {
        var args = environment.Get("*ARGS*") as Cons;
        return Run(environment, args);
    }

    protected abstract Expression Run(IEnvironment environment, Cons args);

    public override string Prin1ToString()
    {
        return $"#<FUNCTION ({GetType().Name})>";
        // TODO: Make it more detailed
    }
}

public class Car : Native
{
    protected override Expression Run(IEnvironment environment, Cons args)
    {
        var arg = args.Car.Evaluate(environment);
        if (arg is not Cons cons) throw new Exception("car requires a list");
        return cons.Car;
    }
}

public class Plus : Native
{
    protected override Expression Run(IEnvironment environment, Cons args)
    {
        var argList = args.ToIEnumerable().Select(x => x.Evaluate(environment))
            .ToList();
        if (argList.Count == 0) throw new Exception("+ requires at least one argument");
        var sum = 0;
        foreach (var arg in argList)
        {
            if (arg is not Atom atom) throw new Exception("+ requires arguments to be atoms");
            if (atom.Type is not AtomTypes.Integer) throw new Exception("Arguments not integer");
            sum += (int)atom.Value;
        }

        return new Atom(sum, AtomTypes.Integer);
    }
}

public class Minus : Native
{
    protected override Expression Run(IEnvironment environment, Cons args)
    {
        var argList = args.ToIEnumerable().Select(x => x.Evaluate(environment))
            .ToList();
        if (argList.Any(x => x is not Atom { Type: AtomTypes.Integer or AtomTypes.Float }))
            throw new Exception("Arguments are not numbers");

        if (argList.Count == 0) throw new Exception("- requires at least one argument");
        if (argList.Count == 1)
        {
            if (argList.First() is not Atom { Type: AtomTypes.Integer or AtomTypes.Float } numAtom)
                throw new Exception("Argument is not an atom");
            switch (numAtom.Value)
            {
                case int intAtom:
                    return new Atom(-intAtom as object, AtomTypes.Integer);
                case float floatAtom:
                    return new Atom(-floatAtom as object, AtomTypes.Float);
            }
        }

        // TODO: Don't use floats if not needed
        var first = argList.First() as Atom;
        var rest = argList.Skip(1).Cast<Atom>();
        var sum = Convert.ToSingle(first.Value);
        foreach (var atom in rest)
        {
            sum -= Convert.ToSingle(atom.Value);
        }

        var atomType = argList.Any(x => x is Atom { Type: AtomTypes.Float }) 
            ? AtomTypes.Float 
            : AtomTypes.Integer;
        return new Atom(sum, atomType);
    }
}

public class Cdr : Native
{
    protected override Expression Run(IEnvironment environment, Cons args)
    {
        var arg = args.Car.Evaluate(environment);
        if (arg is not Cons cons) throw new Exception("cdr requires a list");
        return cons.Cdr;
    }
}

public class Quote : Native
{
    protected override Expression Run(IEnvironment environment, Cons args)
    {
        return args.Car;
    }
}

public class ConsFunc : Native
{
    protected override Expression Run(IEnvironment environment, Cons args)
    {
        var argList = args.ToIEnumerable().Select(x => x.Evaluate(environment)).ToList();
        var car = argList[0];
        var cdr = argList[1];
        return new Cons { Car = car, Cdr = cdr };
    }
}

public class Eval : Native
{
    protected override Expression Run(IEnvironment environment, Cons args)
    {
        var arg = args.Car.Evaluate(environment);
        return arg.Evaluate(environment);
    }
}

public class Exit : Native
{
    protected override Expression Run(IEnvironment environment, Cons args)
    {
        var exitNumber = args?.Car?.Evaluate(environment) is Atom { Type: AtomTypes.Integer } atom
            ? (int)atom.Value
            : 0;
        System.Environment.Exit(exitNumber);
        throw new Exception("Didn't exit");
    }
}

public class Eq : Native
{
    // TODO: Mimic behavior of Common Lisp
    protected override Expression Run(IEnvironment environment, Cons args)
    {
        var argList = args.ToIEnumerable().Select(x => x.Evaluate(environment)).ToList();
        if (argList.Count != 2) throw new Exception("eq requires exactly two arguments");
        if (argList[0] is not Atom atom1 || argList[1] is not Atom atom2)
            throw new Exception("eq requires arguments to be atoms");
        return atom1.Value.Equals(atom2.Value)
            ? new Atom(true as object, AtomTypes.Boolean)
            : new Atom(false as object, AtomTypes.Boolean);
    }
}

public class And : Native
{
    protected override Expression Run(IEnvironment environment, Cons args)
    {
        var argList = args.ToIEnumerable().Select(x => x.Evaluate(environment)).ToList();
        if (argList.Count == 0) throw new Exception("and requires at least one argument");

        foreach (var item in argList)
        {
            if (item is Atom { Type: AtomTypes.Boolean } atom && (bool)atom.Value == false)
            {
                return atom;
            }
        }

        return argList.Last();
    }
}

public class Or : Native
{
    protected override Expression Run(IEnvironment environment, Cons args)
    {
        var argList = args.ToIEnumerable().Select(x => x.Evaluate(environment)).ToList();
        if (argList.Count == 0) throw new Exception("or requires at least one argument");

        foreach (var item in argList)
        {
            if (item is not Atom { Type: AtomTypes.Boolean } atom || (bool)atom.Value)
            {
                return item;
            }
        }

        return new Atom(false as object, AtomTypes.Boolean);
    }
}

public class If : Native
{
    // TODO: Implement cond or when instead
    protected override Expression Run(IEnvironment environment, Cons args)
    {
        var argList = args.ToIEnumerable().ToList();
        if (argList.Count != 3) throw new Exception("if requires exactly three arguments");
        var predicate = argList.First().Evaluate(environment);
        var isFalse = predicate is Atom { Type: AtomTypes.Boolean } predicateAtom && (bool)predicateAtom.Value == false;
        if (isFalse)
        {
            argList.Last().Evaluate(environment);
        }

        return argList[1].Evaluate(environment);
    }
}

public class Define : Native
{
    protected override Expression Run(IEnvironment environment, Cons args)
    {
        var argList = args.ToIEnumerable().ToList();
        if (argList.Count != 2) throw new Exception("define requires exactly two arguments");

        var name = argList.First();
        if (name is not Atom { Type: AtomTypes.Symbol } nameAtom)
            throw new Exception("define requires name to be a symbol");

        var value = argList.Last().Evaluate(environment);
        environment.Set(nameAtom.Value as string, value);
        return value;
    }
}

public class Debug : Native
{
    protected override Expression Run(IEnvironment environment, Cons args)
    {
        if (!Debugger.IsAttached)
        {
            Console.WriteLine("Debugger isn't attached");
            return new Atom(false as object, AtomTypes.Boolean);
            // TODO: Return nothing and make IsAttached into a computed variable
        }

        Debugger.Break();
        return new Atom(true as object, AtomTypes.Boolean);
    }
}

public class LambdaFunc : Native
{
    protected override Expression Run(IEnvironment environment, Cons args)
    {
        var closure = environment.Closure();
        var (car, cdr) = args;
        if (car is not Cons argsCons)
            throw new Exception("lambda requires a symbol as its first argument");
        if (cdr is not Cons bodyCons)
            throw new Exception("lambda requires a list as its second argument");

        var lambda = new Lambda(closure, argsCons, bodyCons);
        return lambda;
    }
}

public class ListFunc : Native
{
    protected override Expression Run(IEnvironment environment, Cons args)
    {
        // TODO: Make cons implement ienumerable
        return Cons.FromIEnumerable(args.ToIEnumerable().Select(x => x.Evaluate(environment)));
    }
}

public class BeginFunc : Native
{
    protected override Expression Run(IEnvironment environment, Cons args)
    {
        var argList = args.ToIEnumerable().ToList();
        Expression returnVal = null;
        foreach (var expression in argList)
        {
            returnVal = expression.Evaluate(environment);
        }

        return returnVal;
    }
}

public class ApplyFunc : Native
{
    protected override Expression Run(IEnvironment environment, Cons args)
    {
        var argList = args.ToIEnumerable().ToList();
        if (argList.Count != 2) throw new Exception("apply requires two arguments");

        var function = args.Car.Evaluate(environment);
        var arguments = argList[1].Evaluate(environment);

        return new Cons { Car = function, Cdr = arguments }.Evaluate(environment);
    }
}