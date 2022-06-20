using System.Text;
using lisp.Primitive;

namespace lisp.Reader;

public record Token;

public record OpeningParenToken : Token;

public record ClosingParenToken : Token;

public record QuoteToken : Token;

public record BackquoteToken : Token;

public record TildeToken : Token;
public record AtToken : Token;

public record NilToken : Token;

public record AtomToken(string Name) : Token;

public record StringAtomToken(string Content) : Token;

public record WhitespaceToken(WhitespaceToken.WhitespaceType Type = WhitespaceToken.WhitespaceType.Unspecified) : Token
{
    public enum WhitespaceType
    {
        Space,
        Newline,
        Other,
        Unspecified
    }
};

public static class Reader
{
    private static IEnumerable<Token> Tokenize(string str)
    {
        List<Token> tokens = new();
        for (var i = 0; i < str.Length; i++)
        {
            var ch = str[i];
            switch (ch)
            {
                case '(':
                    tokens.Add(new OpeningParenToken());
                    break;
                case ')':
                    tokens.Add(new ClosingParenToken());
                    break;
                case '\'':
                    tokens.Add(new QuoteToken());
                    break;
                case '`':
                    tokens.Add(new BackquoteToken());
                    break;
                case '"':
                    StringBuilder stringContent = new();
                    char currentChar = str[++i];
                    do
                    {
                        stringContent.Append(currentChar);
                        currentChar = str[++i];
                    } while (currentChar != '"');

                    tokens.Add(new StringAtomToken(stringContent.ToString()));
                    break;
                case ' ' or '\r' or '\n':
                    switch (ch)
                    {
                        case ' ':
                            tokens.Add(new WhitespaceToken(WhitespaceToken.WhitespaceType.Space));
                            break;
                        case '\n':
                            tokens.Add(new WhitespaceToken(WhitespaceToken.WhitespaceType.Newline));
                            break;
                        case '\r':
                            var isNewline = str.ElementAt(i + 1) == '\n';
                            switch (isNewline)
                            {
                                case true:
                                    tokens.Add(new WhitespaceToken(WhitespaceToken.WhitespaceType.Newline));
                                    i++;
                                    break;
                                default:
                                    tokens.Add(new WhitespaceToken(WhitespaceToken.WhitespaceType.Other));
                                    break;
                            }
                            break;
                        default:
                            tokens.Add(new WhitespaceToken());
                            break;
                    }
                    break;
                case ';':
                    // TODO: Refactor this
                    var isDone = false;
                    var lastCarriageReturn = false;
                    var currentChar2 = str[++i];
                    do
                    {
                        if (currentChar2 == '\r')
                        {
                            lastCarriageReturn = true;
                        }

                        if (lastCarriageReturn && currentChar2 == '\n')
                        {
                            isDone = true;
                        }

                        if (lastCarriageReturn && currentChar2 != '\n')
                        {
                            lastCarriageReturn = false;
                            // This probably shouldn't happen
                        }

                        if (currentChar2 == '\n')
                        {
                            isDone = true;
                        }

                        if (str.Length == i + 1)
                            isDone = true;
                        else 
                            currentChar2 = str[++i];
                    } while (!isDone);
                    break;
                default:
                    // TODO: Handle illegal characters
                    StringBuilder symbolContent = new();
                    char currentChar3 = str[i];
                    var isOutOfBounds = false;
                    while (!isOutOfBounds && currentChar3 is not ' ' and not ')' and not '"' and not '\r' and not '\n')
                    {
                        symbolContent.Append(currentChar3);
                        isOutOfBounds = ++i >= str.Length;
                        currentChar3 = !isOutOfBounds ? str[i] : ' ';
                    }

                    i -= 1;
                    tokens.Add(new AtomToken(symbolContent.ToString()));
                    break;
            }
        }

        return tokens;
    }

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
                return new Cons { Car = quoted }
                    .Wrap(new SymbolAtom(new Symbol("quote")));
            case BackquoteToken:
                var quoted2 = ParseTokens(tokens);
                
                throw new NotImplementedException();
                // TODO: DRY this
                // if (quoted2 is not Cons)
                //     return new Cons
                //     {
                //         Car = new ValueAtom("quote", AtomTypes.Symbol),
                //         Cdr = new Cons{Car = quoted2}
                //     };
            case NilToken:
                throw new NotImplementedException();
            default:
                throw new ArgumentOutOfRangeException(nameof(token));
        }
    }

    public static IExpression ReadFromString(string str)
    {
        var tokens = Tokenize(str)
            .Where(x => x is not WhitespaceToken)
            .ToList();
        var parsed = ParseTokens(tokens, true);
        return parsed;
    }
}