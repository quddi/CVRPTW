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

    // null повертає нескінченну смугу, коли частка виконаного невідома.
    public void SetFraction(double? fraction)
    {
        Bar.IsIndeterminate = fraction == null;
        Bar.Value = fraction ?? 0;
    }
}
