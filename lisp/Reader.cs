﻿using System.Text;

namespace lisp.Reader;

public record Token();

public record OpeningParenToken : Token;

public record ClosingParenToken : Token;

public record QuoteToken : Token;
// TODO: Implement backquotes

public record NilToken : Token;

public record AtomToken(string Name) : Token;

public record StringAtomToken(string Content) : Token;

public record WhitespaceToken : Token;

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
                case '"':
                    StringBuilder stringContent = new();
                    i++;
                    char currentChar = str[i];
                    do
                    {
                        stringContent.Append(currentChar);
                        currentChar = str[++i];
                    } while (currentChar != '"');
                    tokens.Add(new StringAtomToken(stringContent.ToString()));
                    break;
                case ' ' or '\r' or '\n':
                    tokens.Add(new WhitespaceToken());
                    break;
                default:
                    // TODO: Handle illegal characters
                    StringBuilder symbolContent = new();
                    char currentChar2 = str[i];
                    var isOutOfBounds = false;
                    while (!isOutOfBounds && currentChar2 is not ' ' and not ')' and not '"' and not '\r' and not '\n'){
                        symbolContent.Append(currentChar2);
                        isOutOfBounds = ++i >= str.Length;
                        currentChar2 = !isOutOfBounds ? str[i] : ' ';
                    }

                    i -= 1;
                    tokens.Add(new AtomToken(symbolContent.ToString()));
                    break;
            }
        }

        return tokens;
    }

    private static Expression ParseTokens(List<Token> tokens)
    {
        var token = tokens.Pop();

        switch (token)
        {
            case AtomToken atomToken:
                return Atom.ParseString(atomToken.Name);
            case StringAtomToken stringAtomToken:
                return new Atom(stringAtomToken.Content);
            case OpeningParenToken:
                List<Expression> expList = new();
                while (tokens.First() is not ClosingParenToken)
                    expList.Add(ParseTokens(tokens));
                tokens.Pop();
                return Cons.FromIEnumerable(expList);
            case ClosingParenToken:
                throw new Exception("Unexpected ')'");
            case QuoteToken:
                var quoted = ParseTokens(tokens);
                return new Cons {
                    Car = new Atom("quote", AtomTypes.Symbol),
                    Cdr = new Cons {Car = quoted} 
                };
            case NilToken:
                throw new NotImplementedException();
            default:
                throw new ArgumentOutOfRangeException(nameof(token));
        }
    }

    public static Expression ReadFromString(string str)
    {
        var tokens = Tokenize(str)
            .Where(x => x is not WhitespaceToken)
            .ToList();
        var parsed = ParseTokens(tokens);
        return parsed;
    }
}