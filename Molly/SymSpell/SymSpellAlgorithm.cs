using static SymSpell;

namespace Molly.StringApproxAlgorithms;

public class SymSpellAlgorithm
{
    /// <summary>
    /// Returns the closest term to the input, based on the predefined directory (biggest frequency, lowest distance)
    /// </summary>
    /// <param name="stringToApproximate">Input string to approximate</param>
    /// <param name="symSpell">SymSpell object containing the dictionary to which name is approximated</param>
    /// <param name="maxEditDistance">Distance from the stringToApproximate</param>
    /// <returns>Output approximated string</returns>
    public static string FindBestSuggestion(string stringToApproximate, SymSpell symSpell, int maxEditDistance = 2)
    {
        var suggestions = symSpell.Lookup(stringToApproximate, Verbosity.Top, maxEditDistance);
        return suggestions.Count > 0 ? suggestions.First().term : string.Empty;
    }

    /// <summary>
    /// Returns the closest terms to the input, based on the predefined directory (biggest frequency, lowest distance)
    /// </summary>
    /// <param name="name">Input string to approximate</param>
    /// <param name="symSpell">SymSpell object containing the dictionary to which name is approximated</param>
    /// <param name="count">The number of results but at least two</param>
    /// <param name="maxEditDistance">Distance from the stringToApproximate</param>
    /// <returns>Three approximated strings</returns>
    public static List<string> FindMultipleBestSuggestions(string name, SymSpell symSpell, int count, int maxEditDistance = 2)
    {
        if (count <= 1)
            throw new ArgumentException("Count should be at least two");

        var suggestions = symSpell.Lookup(name, Verbosity.Closest, maxEditDistance);
        return suggestions.Take(count).Select(s => s.term).ToList();
    }
}