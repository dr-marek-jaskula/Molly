namespace Molly;

public record AuthenticationSettings(string Key, string Region);

public static class GeneralSettings
{
    public static readonly string Path = AppDomain.CurrentDomain.BaseDirectory.Split(@"bin\", StringSplitOptions.None)[0];
}