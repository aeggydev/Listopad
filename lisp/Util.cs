namespace lisp;

public static class EnumerableExtensions
{
    public static T Pop<T>(this List<T> list)
    {
        var item = list.First();
        list.RemoveAt(0);
        return item;
    }

    public static (T Car, IEnumerable<T> Cdr) Cons<T>(this IEnumerable<T> enumerable)
    {
        var data = enumerable.ToList();
        return (data.First(), data.Skip(1));
    }
}