namespace Molly.StringApproxAlgorithms;

public class SymSpellFactory
{
    /// <summary>
    /// Create a SymSpell instance for frequency English directory
    /// </summary>
    /// <returns>SymSpell instance for En dictionary</returns>
    public static SymSpell CreateSymSpell()
    {
        SymSpell symSpell = new(82834, 2);

        //Path to en (English) dictionary
        string dictionaryPath = @$"{GeneralSettings.Path}SymSpell\frequency_dictionary_en_82_765.txt";
        //column of the term in the dictionary text file
        int termIndex = 0;
        //column of the term frequency in the dictionary text file
        int countIndex = 1;

        if (!symSpell.LoadDictionary(dictionaryPath, termIndex, countIndex))
            throw new DirectoryNotFoundException("English frequency dictionary for SymSpell not found!");

        return symSpell;
    }

    /// <summary>
    /// Create a SymSpell instance. Dictionary is made by corpus aimed by path input
    /// </summary>
    /// <param name="path">Path to corpus</param>
    /// <param name="initialCapacity">Approximated dictionary capacity</param>
    /// <param name="maxDirectoryEditDistance">Maximal word distance</param>
    /// <returns>SymSpell instance</returns>
    public static SymSpell CreateSymSpell(string path, int initialCapacity, int maxDirectoryEditDistance = 2)
    {
        SymSpell symSpell = new(initialCapacity, maxDirectoryEditDistance);

        if (!symSpell.CreateDictionary(path))
            throw new DirectoryNotFoundException("Corpus dictionary for SymSpell not found!");

        return symSpell;
    }

    /// <summary>
    /// Create a SymSpell instance. Dictionary is made by adding an input string list
    /// </summary>
    /// <param name="approximateTo">A list of strings that words will approximate to</param>
    /// <param name="maxDirectoryEditDistance">Maximal word distance</param>
    /// <returns>SymSpell instance</returns>
    public static SymSpell CreateSymSpell(List<string> approximateTo, int maxDirectoryEditDistance = 2)
    {
        int initialCapacity = approximateTo.Count;

        SymSpell symSpell = new(initialCapacity, maxDirectoryEditDistance);

        foreach (string word in approximateTo)
            symSpell.CreateDictionaryEntry(word, 1);

        return symSpell;
    }
}