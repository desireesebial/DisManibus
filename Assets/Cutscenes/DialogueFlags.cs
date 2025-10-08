using System.Collections.Generic;

public static class DialogueFlags
{
    static HashSet<string> flags = new HashSet<string>();

    public static bool Has(string key) => flags.Contains(key);
    public static void Set(string key) => flags.Add(key);
}
