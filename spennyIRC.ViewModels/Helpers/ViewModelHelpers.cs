using System.Collections.ObjectModel;

namespace spennyIRC.ViewModels.Helpers;

public static class ViewModelHelpers
{
    private static readonly Dictionary<char, int> accessModes = new()
    {
        { '~', 0 },
        { '&', 1 },
        { '@', 2 },
        { '%', 3 },
        { '+', 4 }
    };

    public static void UserAccessSort(this ObservableCollection<string> nicklist)
    {
        List<string> sortedList = [.. nicklist.OrderBy(nick =>
        {
            char mode = nick.Length > 0 ? nick[0] : '\0';
            bool hasMode = accessModes.TryGetValue(mode, out int order);
            if (!hasMode) order = int.MaxValue;

            return (order, nick);
        })];

        for (int i = 0; i < nicklist.Count; i++)
        {
            nicklist[i] = sortedList[i];
        }
    }

    public static void ComparisonSort<T>(this ObservableCollection<T> collection, Comparison<T> comparison)
    {
        List<T> sortedList = [.. collection];
        sortedList.Sort(comparison);
        for (int i = 0; i < sortedList.Count; i++)
        {
            collection[i] = sortedList[i];
        }
    }

    // TODO: this is very buggy. fix it
    public static void AlphaNumericSort(this ObservableCollection<IChatWindow> chatWindows)
    {
        chatWindows.ComparisonSort((x, y) =>
        {
            return CompareAlphanumeric(x.Name, y.Name);
        });
    }

    private static int CompareAlphanumeric(string x, string y)
    {
        int length = Math.Min(x.Length, y.Length);

        for (int i = 0; i < length; i++)
        {
            char cx = x[i];
            char cy = y[i];

            if (char.IsSymbol(cx) && !char.IsSymbol(cy)) return -1;
            if (!char.IsSymbol(cx) && char.IsSymbol(cy)) return 1;

            if (char.IsDigit(cx) && !char.IsDigit(cy)) return -1;
            if (!char.IsDigit(cx) && char.IsDigit(cy)) return 1;

            if (char.IsLetter(cx) && !char.IsLetter(cy)) return 1;
            if (!char.IsLetter(cx) && char.IsLetter(cy)) return -1;

            if (cx != cy) return cx.CompareTo(cy);
        }

        return x.Length.CompareTo(y.Length);
    }
}