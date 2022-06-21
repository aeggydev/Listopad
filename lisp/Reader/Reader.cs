using lisp.Primitive;

namespace lisp.Reader;

public static partial class Reader
{
    public static IExpression ReadFromString(string str)
    {
        var tokens = Tokenize(str)
            .Where(x => x is not WhitespaceToken)
            .ToList();
        var parsed = ParseTokens(tokens, true);
        return parsed;
    }
}