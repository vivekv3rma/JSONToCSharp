using Terminal.Gui;

namespace JsonToCSharp.Tui;

public class Program
{
    public static void Main(string[] args)
    {
        Application.Init();

        var mainWindow = new MainWindow();
        Application.Run(mainWindow);

        Application.Shutdown();
    }
}
