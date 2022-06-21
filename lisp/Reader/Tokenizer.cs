using System.Text;

namespace lisp.Reader;

public abstract record Token;

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


public static partial class Reader
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
                case '@':
                    tokens.Add(new AtToken());
                    break;
                case '~':
                    tokens.Add(new TildeToken());
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
}