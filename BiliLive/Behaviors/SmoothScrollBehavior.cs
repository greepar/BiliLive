using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

namespace BiliLive.Behaviors;

public static class SmoothScrollBehavior
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, bool>(
            "IsEnabled",
            typeof(SmoothScrollBehavior));

    private static readonly AttachedProperty<State?> StateProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, State?>(
            "State",
            typeof(SmoothScrollBehavior));

    static SmoothScrollBehavior()
    {
        IsEnabledProperty.Changed.AddClassHandler<ScrollViewer>((scrollViewer, args) =>
        {
            if (args.NewValue is true)
            {
                if (scrollViewer.GetValue(StateProperty) != null) return;

                var state = new State(scrollViewer);
                scrollViewer.SetValue(StateProperty, state);
                scrollViewer.AddHandler(InputElement.PointerWheelChangedEvent, state.OnPointerWheelChanged, handledEventsToo: false);
            }
            else if (scrollViewer.GetValue(StateProperty) is { } state)
            {
                scrollViewer.RemoveHandler(InputElement.PointerWheelChangedEvent, state.OnPointerWheelChanged);
                state.Dispose();
                scrollViewer.ClearValue(StateProperty);
            }
        });
    }

    public static void SetIsEnabled(AvaloniaObject element, bool value) => element.SetValue(IsEnabledProperty, value);

    public static bool GetIsEnabled(AvaloniaObject element) => element.GetValue(IsEnabledProperty);

    private sealed class State(ScrollViewer scrollViewer) : IDisposable
    {
        private const double WheelStep = 72;
        private const double DurationMs = 180;

        private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(16) };
        private Vector _startOffset;
        private Vector _targetOffset;
        private DateTime _startedAt;

        public void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            if (scrollViewer.Extent.Width <= scrollViewer.Viewport.Width &&
                scrollViewer.Extent.Height <= scrollViewer.Viewport.Height)
            {
                return;
            }

            var current = _timer.IsEnabled ? _targetOffset : scrollViewer.Offset;
            var horizontal = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
            var delta = new Vector(
                horizontal ? -e.Delta.Y * WheelStep : -e.Delta.X * WheelStep,
                horizontal ? -e.Delta.X * WheelStep : -e.Delta.Y * WheelStep);

            var maxX = Math.Max(0, scrollViewer.Extent.Width - scrollViewer.Viewport.Width);
            var maxY = Math.Max(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height);
            var next = new Vector(
                Math.Clamp(current.X + delta.X, 0, maxX),
                Math.Clamp(current.Y + delta.Y, 0, maxY));

            if (next == current) return;

            e.Handled = true;

            _startOffset = scrollViewer.Offset;
            _targetOffset = next;
            _startedAt = DateTime.UtcNow;

            if (_timer.IsEnabled) return;

            _timer.Tick += OnTick;
            _timer.Start();
        }

        private void OnTick(object? sender, EventArgs e)
        {
            var progress = Math.Clamp((DateTime.UtcNow - _startedAt).TotalMilliseconds / DurationMs, 0, 1);
            var eased = 1 - Math.Pow(1 - progress, 3);

            scrollViewer.Offset = new Vector(
                Lerp(_startOffset.X, _targetOffset.X, eased),
                Lerp(_startOffset.Y, _targetOffset.Y, eased));

            if (progress < 1) return;

            _timer.Stop();
            _timer.Tick -= OnTick;
            scrollViewer.Offset = _targetOffset;
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Tick -= OnTick;
        }

        private static double Lerp(double from, double to, double progress) => from + (to - from) * progress;
    }
}
