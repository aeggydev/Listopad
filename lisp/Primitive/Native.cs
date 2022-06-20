using System.Diagnostics;

namespace lisp.Primitive;

// TODO: Make this less verbose
public abstract class Native : IAtom
{
    public virtual IExpression Evaluate(IEnvironment environment)
    {
        return this;
    }

    public virtual object ToCompare => this;

    public abstract IExpression Run(IEnvironment environment, Cons args);

    public virtual string GetString()
    {
        return $"#<FUNCTION ({GetType().Name})>";
        // TODO: Make it more detailed
    }
}

public class Car : Native
{
    public override IExpression Run(IEnvironment environment, Cons args)
    {
        var arg = args.Car?.Evaluate(environment);
        if (arg is not Cons cons) throw new Exception("car requires a list");
        return cons.Car;
    }
}

public class Plus : Native
{
    public override IExpression Run(IEnvironment environment, Cons args)
    {
        var argList = args
            .Select(x => x.Evaluate(environment))
            .ToList();
        if (argList.Count == 0) throw new Exception("+ requires at least one argument");
        var sum = 0;
        foreach (var arg in argList)
        {
            if (arg is not IntegerAtom atom) throw new Exception("+ requires arguments to be integers");
            sum += atom.Value;
        }

        return new IntegerAtom(sum);
    }
}

public class Minus : Native
{
    public override IExpression Run(IEnvironment environment, Cons args)
    {
        var argList = args
            .Select(x => x.Evaluate(environment))
            .ToList();
        if (argList.Any(x => x is not (IntegerAtom or FloatAtom)))
            throw new Exception("Arguments are not numbers");

        if (argList.Count == 0) throw new Exception("- requires at least one argument");
        if (argList.Count == 1)
        {
            if (argList.First() is not IntegerAtom or FloatAtom)
                throw new Exception("Argument is not an atom");
            switch (argList.First())
            {
                case IntegerAtom intAtom:
                    return new IntegerAtom(-intAtom.Value);
                case FloatAtom floatAtom:
                    return new FloatAtom(-floatAtom.Value);
            }
        }

        // TODO: Don't use floats if not needed
        var rest = argList.Skip(1).Select(x => x.AsValue<int>());
        var sum = Convert.ToSingle(argList.First().AsValue<int>());
        foreach (var value in rest)
        {
            sum -= Convert.ToSingle(value);
        }

        return argList.Any(x => x is FloatAtom)
            ? new FloatAtom(sum)
            : new IntegerAtom((int)sum);
        // TODO: This whole function is an ugly mess
    }
}

public class Cdr : Native
{
    public override IExpression Run(IEnvironment environment, Cons args)
    {
        var arg = args.Car?.Evaluate(environment);
        if (arg is not Cons cons) throw new Exception("cdr requires a list");
        return cons.Cdr;
    }
}

public class Quote : Native
{
    public override IExpression Run(IEnvironment environment, Cons args)
    {
        return args.Car;
    }
}

public class ConsFunc : Native
{
    public override IExpression Run(IEnvironment environment, Cons args)
    {
        var argList = args
            .Select(x => x.Evaluate(environment))
            .ToList();
        var car = argList[0];
        var cdr = argList[1];
        return new Cons { Car = car, Cdr = cdr };
    }
}

public class Eval : Native
{
    public override IExpression Run(IEnvironment environment, Cons args)
    {
        var arg = args.Car.Evaluate(environment);
        return arg.Evaluate(environment);
    }
}

public class Exit : Native
{
    public override IExpression Run(IEnvironment environment, Cons args)
    {
        var exitNumber = args.Car?.Evaluate(environment) is IntegerAtom atom
            ? atom.Value
            : 0;
        System.Environment.Exit(exitNumber);
        throw new Exception("Didn't exit");
    }
}

public class Eq : Native
{
    // TODO: Mimic behavior of Common Lisp
    public override IExpression Run(IEnvironment environment, Cons args)
    {
        var argList = args
            .Select(x => x.Evaluate(environment))
            .ToList();
        if (argList.Count != 2) throw new Exception("eq requires exactly two arguments");
        // TODO: Implement comparing atoms
        if (argList[0] is not IAtom atom1 || argList[1] is not IAtom atom2)
            throw new Exception("eq requires arguments to be atoms");
        
        // TODO: Implement comparing values in classes
        return new BoolAtom(atom1.ToCompare.Equals(atom2.ToCompare));
    }
}

public class And : Native
{
    public override IExpression Run(IEnvironment environment, Cons args)
    {
        var argList = args
            .Select(x => x.Evaluate(environment))
            .ToList();
        if (argList.Count == 0) throw new Exception("and requires at least one argument");

        foreach (var item in argList)
        {
            if (item is BoolAtom { Value: false } atom)
            {
                return atom;
            }
        }

        return argList.Last();
    }
}

public class Or : Native
{
    public override IExpression Run(IEnvironment environment, Cons args)
    {
        var argList = args
            .Select(x => x.Evaluate(environment))
            .ToList();
        if (argList.Count == 0) throw new Exception("or requires at least one argument");

        foreach (var item in argList)
        {
            if (item is not BoolAtom atom || atom.Value)
            {
                return item;
            }
        }

        return new BoolAtom(false);
    }
}

public class If : Native
{
    // TODO: Implement cond or when instead
    public override IExpression Run(IEnvironment environment, Cons args)
    {
        var argList = args.ToList();
        if (argList.Count != 3) throw new Exception("if requires exactly three arguments");
        var predicate = argList.First().Evaluate(environment);
        var isFalse = predicate is BoolAtom { Value: false };

        return isFalse
            ? argList.Last().Evaluate(environment)
            : argList[1].Evaluate(environment);
    }
}

public class Define : Native
{
    public override IExpression Run(IEnvironment environment, Cons args)
    {
        var argList = args.ToList();
        if (argList.Count != 2) throw new Exception("define requires exactly two arguments");

        var name = argList.First();
        if (name is not SymbolAtom nameAtom)
            throw new Exception("define requires name to be a symbol");

        var value = argList.Last().Evaluate(environment);
        environment.Set(nameAtom.Value.Name, value);
        return value;
    }
}

public class Debug : Native
{
    public override IExpression Run(IEnvironment environment, Cons args)
    {
        if (!Debugger.IsAttached)
        {
            Console.WriteLine("Debugger isn't attached");
            return new BoolAtom(false);
            // TODO: Return nothing and make IsAttached into a computed variable
        }

        Debugger.Break();
        return new BoolAtom(true);
    }
}

public class LambdaFunc : Native
{
    public override IExpression Run(IEnvironment environment, Cons args)
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
    public override IExpression Run(IEnvironment environment, Cons args)
    {
        // TODO: Make cons implement ienumerable
        return Cons.FromIEnumerable(args.Select(x => x.Evaluate(environment)));
    }
}

public class BeginFunc : Native
{
    public override IExpression Run(IEnvironment environment, Cons args)
    {
        var argList = args.ToList();
        IExpression returnVal = null;
        foreach (var expression in argList)
        {
            returnVal = expression.Evaluate(environment);
        }

        return returnVal;
    }
}

public class ApplyFunc : Native
{
    public override IExpression Run(IEnvironment environment, Cons args)
    {
        var argList = args.ToList();
        if (argList.Count != 2) throw new Exception("apply requires two arguments");

        var function = args.Car?.Evaluate(environment);
        var arguments = argList[1].Evaluate(environment);

        return new Cons { Car = function, Cdr = arguments }.Evaluate(environment);
    }
}

public class AtomP : Native
{
    public override IExpression Run(IEnvironment environment, Cons args)
    {
        var value = args.Car?.Evaluate(environment);
        return new BoolAtom(value is IAtom or Cons { IsList: false });
    }
}

public class Concat : Native
{
    public override IExpression Run(IEnvironment environment, Cons args)
    {
        var argList = args
            .Select(x => x.Evaluate(environment))
            .ToList();
        var accumulator = argList.First().AsCons();
        foreach (var item in argList.Skip(1))
        {
            accumulator = Cons.FromIEnumerable(accumulator.Concat(item.AsCons()));
        }

        return accumulator;
    }
}

public class Print : Native
{
    public override IExpression Run(IEnvironment environment, Cons args)
    {
        var text = args.Car?.Evaluate(environment);
        switch (text)
        {
            case StringAtom atomString:
                Console.WriteLine(atomString.Value);
                break;
            default:
                Console.WriteLine(text?.GetString());
                break;
        }

        return new IntegerAtom(0);
        // Should return nil
    }
}

public class Not : Native
{
    public override IExpression Run(IEnvironment environment, Cons args)
    {
        var value = args.Car?.Evaluate(environment);
        return value switch
        {
            BoolAtom boolAtom => new BoolAtom(!boolAtom.Value),
            _ => throw new Exception("Input must be atom")
        };
    }
}

public class BiggerThan : Native
{
    public override IExpression Run(IEnvironment environment, Cons args)
    {
        var evaluted = args
            .Select(x => x.Evaluate(environment))
            .ToList();
        return evaluted[0].AsValue<int>() > evaluted[1].AsValue<int>()
            ? new BoolAtom(true)
            : new BoolAtom(false);
    }
}

public class Mapcar : Native
{
    public override IExpression Run(IEnvironment environment, Cons args)
    {
        var lambda = args.Car?.Evaluate(environment) as Lambda;
        var list = args.Cdr?.AsCons().Car?.Evaluate(environment).AsCons();

        List<IExpression> results = new();
        foreach (var item in list)
        {
            results.Add(lambda.Run(environment, new Cons {Car = item, Cdr = null}));
        }

        return Cons.FromIEnumerable(results);
    }
}

public class Multiply : Native
{
    public override IExpression Run(IEnvironment environment, Cons args)
    {
        var accumulator = args.Car.AsValue<int>();
        foreach (var item in args.Cdr.AsCons())
        {
            accumulator *= item.AsValue<int>();
        }

        return new IntegerAtom(accumulator);
    }
}