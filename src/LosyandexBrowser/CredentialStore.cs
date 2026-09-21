using System.Security.Cryptography;
using System.Text.Json;

namespace LosyandexBrowser;

public sealed record SavedCredential(string Host, string UserName, string Password);

public sealed class CredentialStore
{
    private readonly string _file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LosyandexBrowser", "credentials.dat");
    private static readonly byte[] Entropy = "Лосяндекс Браузер"u8.ToArray();

    public List<SavedCredential> Load()
    {
        try
        {
            if (!File.Exists(_file)) return [];
            var encrypted = File.ReadAllBytes(_file);
            var json = ProtectedData.Unprotect(encrypted, Entropy, DataProtectionScope.CurrentUser);
            return JsonSerializer.Deserialize<List<SavedCredential>>(json) ?? [];
        }
        catch { return []; }
    }

    public void Save(IEnumerable<SavedCredential> credentials)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_file)!);
        var json = JsonSerializer.SerializeToUtf8Bytes(credentials.ToList());
        var encrypted = ProtectedData.Protect(json, Entropy, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(_file, encrypted);
    }
}
