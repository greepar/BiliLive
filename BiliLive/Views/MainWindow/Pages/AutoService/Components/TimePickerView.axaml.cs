using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Transformation;

namespace BiliLive.Views.MainWindow.Pages.AutoService.Components
{
    public partial class TimePickerView : UserControl
    {
        public TimePickerView()
        {
            InitializeComponent();
            // Each time this control is added to a visual tree (i.e. the host
            // Flyout opens), kick the entrance transition by setting the
            // RenderTransform / Opacity to their resting values. The Border's
            // Transitions definition handles the easing.
            AttachedToVisualTree += (_, _) =>
            {
                if (this.FindControl<Border>("RootCard") is { } card)
                {
                    // Reset to the "from" state instantly...
                    card.Opacity = 0;
                    card.RenderTransform =
                        TransformOperations.Parse("scale(0.92) translateY(-12px)");

                    // ...then schedule the "to" state on the next layout pass
                    // so the transitions get a chance to interpolate.
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        card.Opacity = 1;
                        card.RenderTransform =
                            TransformOperations.Parse("scale(1) translateY(0)");
                    }, Avalonia.Threading.DispatcherPriority.Background);
                }
            };
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
