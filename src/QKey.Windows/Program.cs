using System.Windows.Forms;

namespace QKey.Windows;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        using var app = new QKeyApplicationContext();
        Application.Run(app);
    }
}
