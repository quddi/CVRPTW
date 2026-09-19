using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace VisualizationApplication.Avalonia.Tools;

public static class MessageBox
{
    public static async Task Show(Window? owner, string message, string title = "Повідомлення")
    {
        owner ??= (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        var dialog = new MessageBoxWindow(message) { Title = title };
        if (owner != null)
        {
            await dialog.ShowDialog(owner);
        }
        else
        {
            dialog.Show();
        }
    }
}
