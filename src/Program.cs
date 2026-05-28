namespace SmoothScrollLocal;

internal static class Program
{
    private const string SingleInstanceMutexName = "Local\\SmoothScrollLocal.SingleInstance";

    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        using var singleInstanceMutex = new Mutex(
            initiallyOwned: true,
            name: SingleInstanceMutexName,
            createdNew: out var isFirstInstance);

        if (!isFirstInstance)
        {
            MessageBox.Show(
                "SmoothScroll Local da co mot ban dang chay truoc roi.",
                "SmoothScroll Local",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        using var settingsStore = new SettingsStore();
        using var animator = new SmoothScrollAnimator();
        using var hook = new MouseHook(settingsStore, animator);

        hook.Start();

        try
        {
            Application.Run(new TrayAppContext(settingsStore, hook));
        }
        finally
        {
            singleInstanceMutex.ReleaseMutex();
        }
    }
}
