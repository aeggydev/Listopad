using lisp.Primitive;

namespace lisp.Interpreter;

public class Interpreter
{
    public readonly Environment _environment = new();

    public Interpreter()
    {
        _environment.Set("+", new Plus());
        _environment.Set("-", new Minus());
        _environment.Set("*", new Multiply());
        _environment.Set(">", new BiggerThan());
        
        _environment.Set("car", new Car());
        _environment.Set("cdr", new Cdr());
        _environment.Set("quote", new Quote());
        _environment.Set("cons", new ConsFunc());
        _environment.Set("eval", new Eval());
        _environment.Set("list", new ListFunc());
        _environment.Set("concat", new Concat());
        _environment.Set("begin", new BeginFunc());
        _environment.Set("apply", new ApplyFunc());
        _environment.Set("exit", new Exit());
        _environment.Set("debug", new Debug());

        _environment.Set("mapcar", new Mapcar());

        _environment.Set("eq", new Eq());
        _environment.Set("and", new And());
        _environment.Set("or", new Or());
        _environment.Set("not", new Not());

        _environment.Set("print", new Print());
        
        _environment.Set("if", new If());

        _environment.Set("define", new Define());
        _environment.Set("lambda", new LambdaFunc());

        _environment.Set("atomp", new AtomP());

        _environment.Set("*debug-on-exception*", new BoolAtom(true));

        ReadAndEvalute(Prelude);
    }

    public IExpression Evaluate(IExpression expression)
    {
        var returnValue = expression.Evaluate(_environment);
        return returnValue;
    }

    public IExpression ReadAndEvalute(string code)
    {
        return Evaluate(Reader.Reader.ReadFromString(code));
    }

    private const string Prelude = @"
(begin
  (define inc (lambda (x) (+ x 1)))
  (define dec (lambda (x) (- x 1)))
  (define abs (lambda (x) (if (< x 0) (* x -1) x)))

  (define < (lambda (x y) (if (eq x y) #f (not (> x y)))))
  (define >= (lambda (x y) (or (eq x y) (> x y))))
  (define <= (lambda (x y) (or (eq x y) (< x y))))

  (define max (lambda (x y) (if (> x y) x y)))
  (define min (lambda (x y) (if (< x y) x y)))

  (define when (lambda (x y) (if x (begin (eval y) #t) #f)))
  (define unless (lambda (x y) (if x #f (begin (eval y) #t))))

  (define toggle-debug (lambda () (print ""hi"") (define *debug-on-exception* (not *debug-on-exception*)))))"; 
}