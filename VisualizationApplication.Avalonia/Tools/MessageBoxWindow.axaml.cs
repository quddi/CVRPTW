using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace VisualizationApplication.Avalonia.Tools;

public partial class MessageBoxWindow : Window
{
    public MessageBoxWindow()
    {
        InitializeComponent();
    }

    public MessageBoxWindow(string message) : this()
    {
        MessageTextBlock.Text = message;
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key is Key.Enter or Key.Escape)
        {
            Close();
        }
    }
}
