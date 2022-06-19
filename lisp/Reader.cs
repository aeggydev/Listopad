using System.Text;
using System.Text.RegularExpressions;

namespace lisp;

public static class Reader
{
    private static IEnumerable<string> Tokenize(string str)
    {
        var tokens = Regex.Split(str
                .Replace("()", "nil")
                .Replace("(", " ( ")
                .Replace(")", " ) ")
                .Replace("\"", " \" ")
                .Replace("'", " ' "), @"\s+")
            .Where(x => x.Length > 0);
        return tokens;
    }

    private static Expression ParseTokens(List<string> tokens)
    {
        // TODO: Rewrite to not use tokenizer
        // TODO: Actually parse strings and escape characters
        var token = tokens.Pop();

        switch (token)
        {
            case "\"":
                StringBuilder sb = new();
                while (tokens.First() != "\"")
                    sb.Append(tokens.Pop());
                tokens.Pop();
                return Atom.FromString(sb.ToString());
            case ")":
                throw new Exception("Unexpected ')'");
            case "(":
                List<Expression> expList = new();
                while (tokens.First() != ")")
                    expList.Add(ParseTokens(tokens));
                tokens.Pop();
                return Cons.FromIEnumerable(expList);
            default:
                return Atom.ParseString(token);
        }
    }

    public static Expression ReadFromString(string str)
    {
        var tokens = Tokenize(str).ToList();
        return ParseTokens(tokens);
    }
}