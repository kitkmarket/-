# Лосяндекс Браузер

Минимальный браузер для Windows 10 на WPF и Microsoft Edge WebView2. Интерфейс вдохновлён Windows 7 Aero/скевоморфизмом, а поиск по умолчанию выполняется через Лосяндекс.

## Возможности

- вкладки, адресная строка, назад/вперёд, обновление и домашняя страница;
- поиск через `https://losyandex.lovable.app/search?q=...`;
- встроенный WebView2;
- закладки и история в `%LOCALAPPDATA%\\LosyandexBrowser`;
- зашифрованное хранилище учётных данных Windows DPAPI;
- базовое автозаполнение логина и пароля на HTTPS-страницах;
- режим восстановления последней вкладки;
- минимальная совместимость с Windows 10.

## Запуск

1. Установите [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) и [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/).
2. Откройте `LosyandexBrowser.sln` в Visual Studio 2022.
3. Выберите `net8.0-windows` и запустите проект.

Для публикации self-contained:

```powershell
dotnet publish .\\src\\LosyandexBrowser\\LosyandexBrowser.csproj -c Release -r win-x64 --self-contained true
```

Пароли не записываются в открытом виде: секреты шифруются через Windows DPAPI и доступны только текущему пользователю Windows.
