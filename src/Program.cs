namespace SmoothScrollLocal;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        using var settingsStore = new SettingsStore();
        using var animator = new SmoothScrollAnimator(settingsStore);
        using var hook = new MouseHook(settingsStore, animator);

        hook.Start();
        Application.Run(new TrayAppContext(settingsStore, hook));
    }
}
