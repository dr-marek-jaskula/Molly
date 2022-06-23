namespace Molly;

public static class Helpers
{
    public static List<string> SplitToWords(string input)
    {
        if (string.IsNullOrEmpty(input))
            return new();

        string correctedString = input.Replace(".", "").Replace(",", "");
        return correctedString.Split(" ").ToList();
    }

    public static string Capitalize(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return $"{input[0].ToString().ToUpper()}{input[1..].ToLower()}";
    }
}