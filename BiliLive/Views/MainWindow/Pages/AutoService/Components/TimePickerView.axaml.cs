using Avalonia.Controls;
using Avalonia.Input;

namespace BiliLive.Views.MainWindow.Pages.AutoService.Components
{
    public partial class TimePickerView : UserControl
    {
        public TimePickerView()
        {
            InitializeComponent();
        }

        private void ClockCanvas_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is TimePickerViewModel viewModel && viewModel.PointerPressedCommand.CanExecute(e))
            {
                viewModel.PointerPressedCommand.Execute(e);
            }
        }

        private void ClockCanvas_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (DataContext is TimePickerViewModel viewModel && viewModel.PointerMovedCommand.CanExecute(e))
            {
                viewModel.PointerMovedCommand.Execute(e);
            }
        }

        private void ClockCanvas_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (DataContext is TimePickerViewModel viewModel && viewModel.PointerReleasedCommand.CanExecute(e))
            {
                viewModel.PointerReleasedCommand.Execute(e);
            }
        }
    }
}