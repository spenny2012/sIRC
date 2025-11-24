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

    //private static int CompareAlphanumeric(string x, string y)
    //{
    //    int xIndex = 0;
    //    int yIndex = 0;

//    while (xIndex < x.Length && yIndex < y.Length)
//    {
//        // Check if we're at the start of a number in both strings
//        if (char.IsDigit(x[xIndex]) && char.IsDigit(y[yIndex]))
//        {
//            // Extract the complete numbers
//            int xNumStart = xIndex;
//            while (xIndex < x.Length && char.IsDigit(x[xIndex]))
//                xIndex++;

//            int yNumStart = yIndex;
//            while (yIndex < y.Length && char.IsDigit(y[yIndex]))
//                yIndex++;

//            // Parse the numeric values
//            string xNumStr = x.Substring(xNumStart, xIndex - xNumStart);
//            string yNumStr = y.Substring(yNumStart, yIndex - yNumStart);

//            // Compare by numeric value first
//            if (long.TryParse(xNumStr, out long xNum) && long.TryParse(yNumStr, out long yNum))
//            {
//                int numCompare = xNum.CompareTo(yNum);
//                if (numCompare != 0)
//                    return numCompare;

//                // If numbers are equal, compare by string length (e.g., "01" vs "1")
//                int lengthCompare = xNumStr.Length.CompareTo(yNumStr.Length);
//                if (lengthCompare != 0)
//                    return lengthCompare;
//            }
//        }
//        else
//        {
//            // Handle character-by-character comparison for non-numeric parts
//            char cx = x[xIndex];
//            char cy = y[yIndex];

//            // Define the sort order: symbols < digits < letters
//            int cxType = GetCharType(cx);
//            int cyType = GetCharType(cy);

//            if (cxType != cyType)
//                return cxType.CompareTo(cyType);

//            // Same type, compare characters directly
//            if (cx != cy)
//                return cx.CompareTo(cy);

//            xIndex++;
//            yIndex++;
//        }
//    }

//    // One string is a prefix of the other
//    return x.Length.CompareTo(y.Length);
//}

//private static int GetCharType(char c)
//{
//    if (char.IsSymbol(c) || char.IsPunctuation(c))
//        return 0; // Symbols/punctuation come first
//    if (char.IsDigit(c))
//        return 1; // Digits come second
//    if (char.IsLetter(c))
//        return 2; // Letters come last
//    return 0; // Treat everything else as symbol
//}