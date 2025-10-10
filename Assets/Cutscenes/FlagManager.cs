using System.Collections.Generic;
using UnityEngine;

public class FlagManager : MonoBehaviour
{
    private static HashSet<string> flags = new HashSet<string>();

    /// <summary>
    /// Sets a flag to true (adds it to the list).
    /// </summary>
    public static void SetFlag(string flag)
    {
        if (!string.IsNullOrEmpty(flag))
            flags.Add(flag);
    }

    /// <summary>
    /// Unsets a flag (removes it from the list).
    /// </summary>
    public static void UnsetFlag(string flag)
    {
        if (!string.IsNullOrEmpty(flag))
            flags.Remove(flag);
    }

    /// <summary>
    /// Checks if a flag is active (true).
    /// </summary>
    public static bool GetFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag))
            return false;
        return flags.Contains(flag);
    }

    /// <summary>
    /// Clears all flags (optional reset).
    /// </summary>
    public static void ClearAllFlags()
    {
        flags.Clear();
    }
}
