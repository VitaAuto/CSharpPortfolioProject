public static class UiConfig
{
    public static string UiBaseUrl => Environment.GetEnvironmentVariable("UI_BASE_URL") ?? "https://www.epam.com/";
}