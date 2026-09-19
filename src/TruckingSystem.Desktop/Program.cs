using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new TruckingAppContext());
    }
}
