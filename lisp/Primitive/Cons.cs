﻿using System.Collections;

namespace lisp.Primitive;

public class Cons : Expression, IEnumerable<Expression>
{
    public Expression? Car { get; set; }
    public Expression? Cdr { get; set; }
    public bool IsList => Cdr is Cons or null;
    public override object ToCompare => this;

    public override Expression Evaluate(IEnvironment environment)
    {
        var expression = Car switch
        {
            Cons consCar => consCar.Evaluate(environment),
            SymbolAtom atomCar => environment.Get(atomCar.Value.Name),
            Lambda => Car,
            Native => Car,
            _ => throw new Exception("Illegal function call")
        };

        switch (expression)
        {
            case Native native:
                // TODO: Get rid of this ugly hack
                return native.Run(environment, Cdr as Cons);
            case Lambda lambda:
                return lambda.Run(environment, Cdr as Cons);
            default:
                throw new Exception("Don't know");
        }
    }

    public override string GetString()
    {
        if (IsList) // Is a list
        {
            var strings = this.Select(x => x.GetString());
            return $"({string.Join(" ", strings)})";
        }
        else // Is a pair
        {
            return $"({Car.GetString()} . {Cdr.GetString()})";
        }
    }

    public static Cons FromIEnumerable(IEnumerable<Expression> enumerable)
    {
        var (car, cdr) = enumerable.Cons();
        var list = new Cons { Car = car };
        var current = list;
        foreach (var item in cdr)
        {
            current.Cdr = new Cons { Car = item };
            current = (Cons)current.Cdr;
        }

        return list;
    }

    // TODO: Implement indexing

    // Enable deconstructing
    public void Deconstruct(out Expression car, out Expression cdr)
    {
        car = Car;
        cdr = Cdr;
    }

    public IEnumerator<Expression> GetEnumerator()
    {
        var current = this;
        while (current != null)
        {
            yield return current.Car;
            switch (current.Cdr)
            {
                case Cons cdrCons:
                    current = cdrCons;
                    break;
                case null:
                    current = null;
                    break;
                default:
                    yield return current.Cdr;
                    yield break;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Expression? this[int i] => this.Skip(i).First();
}