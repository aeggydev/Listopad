using lisp.Primitive;

namespace lisp.Reader;

public static partial class Reader
{
    private static IExpression ParseTokens(List<Token> tokens, bool topLevel = false)
    {
        var token = tokens.Pop();

        switch (token)
        {
            case AtomToken atomToken:
                if (tokens.Any() && topLevel)
                    throw new Exception("Trailing garbage following expression");
                return ValueAtom<dynamic>.ParseString(atomToken.Name);
            case StringAtomToken stringAtomToken:
                return new StringAtom(stringAtomToken.Content);
            case OpeningParenToken:
                List<IExpression> expList = new();
                if (tokens.First() is ClosingParenToken)
                {
                    tokens.Pop();
                    return new Nil();
                }

                while (tokens.First() is not ClosingParenToken)
                    expList.Add(ParseTokens(tokens));
                tokens.Pop();

                if (tokens.Any() && topLevel)
                    throw new Exception("Trailing garbage following expression");
                return Cons.FromIEnumerable(expList);
            case ClosingParenToken:
                throw new Exception("Unexpected ')'");
            case QuoteToken:
                var quoted = ParseTokens(tokens);

                if (quoted is Nil nil)
                    return nil;

                return new Cons { Car = quoted }
                    .Wrap(new SymbolAtom(new Symbol("quote")));
            case NilToken:
                return new Nil();
            default:
                throw new NotImplementedException(nameof(token));
        }
    }
}