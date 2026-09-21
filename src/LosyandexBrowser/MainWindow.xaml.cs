using System.Windows;

namespace LosyandexBrowser;

public partial class MainWindow : Window
{
    private readonly BrowserController _browser;

    public MainWindow()
    {
        InitializeComponent();
        _browser = new BrowserController(BrowserTabs, AddressBar, StatusText, this);
        Loaded += async (_, _) => await _browser.InitializeAsync();
        Closing += (_, _) => _browser.SaveSession();
    }

    private async void AddressBar_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
            await _browser.NavigateAsync(AddressBar.Text);
    }

    private async void GoBack_Click(object sender, RoutedEventArgs e) => await _browser.GoBackAsync();
    private async void GoForward_Click(object sender, RoutedEventArgs e) => await _browser.GoForwardAsync();
    private void Reload_Click(object sender, RoutedEventArgs e) => _browser.Reload();
    private async void Home_Click(object sender, RoutedEventArgs e) => await _browser.NavigateAsync(BrowserController.HomeUrl);
    private void NewTab_Click(object sender, RoutedEventArgs e) => _browser.AddTab();
    private void Bookmark_Click(object sender, RoutedEventArgs e) => _browser.BookmarkCurrentPage();
    private void Passwords_Click(object sender, RoutedEventArgs e) => _browser.ShowSavedPasswords();
}
