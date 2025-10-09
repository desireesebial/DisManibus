using System.Collections.Generic;

public static class DialogueFlags
{
    static HashSet<string> flags = new HashSet<string>();

    public static bool Has(string key) => flags.Contains(key);
    public static void Set(string key) => flags.Add(key);
    public static void Clear(string key) => flags.Remove(key);

    public static void ResetAll() => flags.Clear(); // optional helper
}
