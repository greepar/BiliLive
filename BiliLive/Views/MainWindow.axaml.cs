using Avalonia.Controls;

namespace BiliLive.Views.MainWindow;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

#if DEBUG
        // Topmost = true;
#endif
    }
}
