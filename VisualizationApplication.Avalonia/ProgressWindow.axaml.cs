using Avalonia.Controls;

namespace VisualizationApplication.Avalonia;

public partial class ProgressWindow : Window
{
    public ProgressWindow()
    {
        InitializeComponent();
    }

    public void SetMessage(string message)
    {
        MessageTextBlock.Text = message;
    }
}
