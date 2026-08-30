using System.IO;
using System.Text.Json;

namespace CeyPASS.WFA;

/// <summary>
/// Yerel kullanıcı tercihleri (%LocalAppData%\CeyPASS).
/// WPF ile aynı klasör / dosya adları kullanılır.
/// </summary>
internal static class UiUserPrefs
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string RootDir
    {
        get
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CeyPASS");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    public static string? ReadText(string fileName)
    {
        try
        {
            var path = Path.Combine(RootDir, fileName);
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }
        catch
        {
            return null;
        }
    }

    public static void WriteText(string fileName, string value)
    {
        try
        {
            File.WriteAllText(Path.Combine(RootDir, fileName), value);
        }
        catch
        {
            // yok say
        }
    }

    public static T? ReadJson<T>(string fileName) where T : class
    {
        try
        {
            var text = ReadText(fileName);
            if (string.IsNullOrWhiteSpace(text)) return null;
            return JsonSerializer.Deserialize<T>(text, JsonOpts);
        }
        catch
        {
            return null;
        }
    }

    public static void WriteJson<T>(string fileName, T value)
    {
        try
        {
            WriteText(fileName, JsonSerializer.Serialize(value, JsonOpts));
        }
        catch
        {
            // yok say
        }
    }
}
