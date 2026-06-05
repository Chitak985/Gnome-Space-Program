// Source - https://stackoverflow.com/a/2412387
// https://stackoverflow.com/questions/642542/how-to-get-next-or-previous-enum-value-in-c-sharp
// Slightly modified
using System;

public static class Extensions
{
    public static string KiloFormat(this double num)
    {
        if (num >= 1000000000)
            return (num / 1000000000).ToString("#.0b");
        if (num >= 1000000)
            return (num / 1000000).ToString("#.0m");
        if (num >= 1000)
            return (num / 1000).ToString("#.0k");

        return num.ToString("#,0.0");
    } 

    public static T Next<T>(this T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src) + 1;
        return (Arr.Length==j) ? Arr[0] : Arr[j];            
    }
}