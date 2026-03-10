// Source - https://stackoverflow.com/a/2412387
// Slightly modified
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
}