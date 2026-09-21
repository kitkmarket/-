using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace LosyandexBrowser;

public sealed class BrowserController
{
    public const string HomeUrl = "https://losyandex.lovable.app/";
    private readonly TabControl _tabs;
    private readonly TextBox _address;
    private readonly TextBlock _status;
    private readonly Window _owner;
    private readonly CredentialStore _credentials = new();
    private readonly List<SavedCredential> _savedCredentials;
    private readonly List<string> _bookmarks = [];
    private readonly string _sessionFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LosyandexBrowser", "session.json");

    public BrowserController(TabControl tabs, TextBox address, TextBlock status, Window owner)
    {
        _tabs = tabs; _address = address; _status = status; _owner = owner;
        _savedCredentials = _credentials.Load();
    }

    public async Task InitializeAsync()
    {
        AddTab();
        await NavigateAsync(HomeUrl);
    }

    public void AddTab(string? url = null)
    {
        var view = new WebView2 { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
        var tab = new TabItem { Header = "Новая вкладка", Content = view };
        _tabs.Items.Add(tab); _tabs.SelectedItem = tab;
        view.NavigationStarting += (_, e) => { _status.Text = "Загрузка…"; _address.Text = e.Uri; };
        view.NavigationCompleted += async (_, e) =>
        {
            tab.Header = string.IsNullOrWhiteSpace(view.CoreWebView2?.DocumentTitle) ? "Вкладка" : view.CoreWebView2.DocumentTitle;
            _address.Text = view.Source?.ToString() ?? "";
            _status.Text = e.IsSuccess ? "Готово" : $"Не удалось открыть страницу ({e.WebErrorStatus})";
            if (e.IsSuccess) await TryAutofillAsync(view);
        };
        _ = InitializeViewAsync(view, url ?? HomeUrl);
    }

    private static async Task InitializeViewAsync(WebView2 view, string url)
    {
        await view.EnsureCoreWebView2Async();
        view.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
        view.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;
        view.CoreWebView2.Navigate(url);
    }

    private WebView2? CurrentView => (_tabs.SelectedItem as TabItem)?.Content as WebView2;

    public async Task NavigateAsync(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return;
        var value = input.Trim();
        string url;
        if (Uri.TryCreate(value, UriKind.Absolute, out var absolute) && (absolute.Scheme == "http" || absolute.Scheme == "https")) url = absolute.ToString();
        else url = "https://losyandex.lovable.app/search?q=" + Uri.EscapeDataString(value);
        if (CurrentView?.CoreWebView2 is null) return;
        CurrentView.CoreWebView2.Navigate(url);
        await Task.CompletedTask;
    }

    public Task GoBackAsync() { if (CurrentView?.CanGoBack == true) CurrentView.GoBack(); return Task.CompletedTask; }
    public Task GoForwardAsync() { if (CurrentView?.CanGoForward == true) CurrentView.GoForward(); return Task.CompletedTask; }
    public void Reload() => CurrentView?.Reload();

    public void BookmarkCurrentPage()
    {
        var url = CurrentView?.Source?.ToString();
        if (string.IsNullOrWhiteSpace(url)) return;
        if (!_bookmarks.Contains(url)) _bookmarks.Add(url);
        _status.Text = "Страница добавлена в закладки";
    }

    private async Task TryAutofillAsync(WebView2 view)
    {
        if (view.Source is null || view.Source.Scheme != "https") return;
        var match = _savedCredentials.FirstOrDefault(x => view.Source.Host.EndsWith(x.Host, StringComparison.OrdinalIgnoreCase));
        if (match is null) return;
        var user = JsonSerializer.Serialize(match.UserName);
        var password = JsonSerializer.Serialize(match.Password);
        await view.CoreWebView2.ExecuteScriptAsync($"(()=>{{const u=document.querySelector('input[type=email],input[name*=user i],input[type=text]');const p=document.querySelector('input[type=password]');if(u)u.value={user};if(p)p.value={password};}})();");
    }

    public void ShowSavedPasswords()
    {
        var text = _savedCredentials.Count == 0 ? "Сохранённых паролей пока нет." : string.Join(Environment.NewLine, _savedCredentials.Select(x => $"{x.Host} — {x.UserName}"));
        MessageBox.Show(_owner, text, "Сохранённые пароли", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void SaveSession()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_sessionFile)!);
            File.WriteAllText(_sessionFile, JsonSerializer.Serialize(new { Bookmarks = _bookmarks, LastUrl = CurrentView?.Source?.ToString() }));
            _credentials.Save(_savedCredentials);
        }
        catch { }
    }
}
