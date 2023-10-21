namespace TestPatternGenerator;

internal static class ListExtensions
{
    public static void RemoveLast<T>(this IList<T> list, T item)
    {
        for (var i = list.Count - 1; i >= 0; --i)
            if (Equals(list[i], item))
            {
                list.RemoveAt(i);
                break;
            }
    }
}