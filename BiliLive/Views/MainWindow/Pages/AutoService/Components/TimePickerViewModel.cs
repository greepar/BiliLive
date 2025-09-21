using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using BiliLive.Views.MainWindow.Pages.AutoService.Components.Utility;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BiliLive.Views.MainWindow.Pages.AutoService.Components
{
    public partial class TimePickerViewModel : ObservableObject
    {
        public event Action<TimeSpan>? OnConfirm;
        public event Action? OnCancel;

        public enum EditMode
        {
            Hour, Minute
        }

        public enum AmPmState
        {
            AM, PM
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsHourMode)), NotifyPropertyChangedFor(nameof(IsMinuteMode))]
        [NotifyPropertyChangedFor(nameof(HandLength))]
        private EditMode _currentMode;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAm))]
        private AmPmState _currentAmPm;

        [ObservableProperty]
        private int _displayHour;

        [ObservableProperty]
        private int _minute;

        private bool _isDragging;
        private int _displayHour24;

        [ObservableProperty]
        private double _handAngle;

        public bool IsHourMode => CurrentMode == EditMode.Hour;
        public bool IsMinuteMode => CurrentMode == EditMode.Minute;
        public bool IsAm => CurrentAmPm == AmPmState.AM;

        public List<ClockTickViewModel> HourTicks
        {
            get; private set;
        }

        public List<ClockTickViewModel> MinuteTicks
        {
            get; private set;
        }

        public int DisplayHour24 => GetHour24();
        public double HandLength => IsHourMode ? 75 : 105;

        public TimePickerViewModel()
        {
            Initialize(DateTime.Now.TimeOfDay);
        }

        public TimePickerViewModel(TimeSpan initialTime)
        {
            Initialize(initialTime);
        }

        private void Initialize(TimeSpan time)
        {
            MinuteTicks = Enumerable.Range(0, 12)
                .Select(i => new ClockTickViewModel(i * 5, (i * 5).ToString("00")))
                .ToList();

            CurrentAmPm = time.Hours < 12 ? AmPmState.AM : AmPmState.PM;
            
            _displayHour24 = time.Hours;
            DisplayHour = _displayHour24 % 12;
            if (DisplayHour == 0) DisplayHour = 12;

            Minute = time.Minutes;
            CurrentMode = EditMode.Hour;
            UpdateHourTicks();
            
            UpdateHandAngle(false);
        }
        
        private double CalculateTargetHandAngle()
        {
            var hourForCalc = DisplayHour % 12;
            var baseAngle = CurrentMode switch
            {
                EditMode.Hour => (hourForCalc * 30.0) + (Minute * 0.5),
                EditMode.Minute => Minute * 6.0,
                _ => 0.0
            };
            
            return baseAngle - 180;
        }
        
        private void UpdateHandAngle(bool animate = true)
        {
            var targetAngle = CalculateTargetHandAngle();

            if (!animate)
            {
                HandAngle = targetAngle;
                return;
            }

            var currentAngle = HandAngle;
            
            var diff = targetAngle - currentAngle;
            while (diff < -180) diff += 360;
            while (diff > 180) diff -= 360;
            
            HandAngle = currentAngle + diff;
        }

        private void UpdateHourTicks()
        {
            List<int> hours;
            if (CurrentAmPm == AmPmState.AM)
            {
                hours = new List<int> { 12 };
                hours.AddRange(Enumerable.Range(1, 11));
            }
            else
            {
                hours = new List<int> { 24 };
                hours.AddRange(Enumerable.Range(13, 11));
            }

            HourTicks = hours
                .Select(h => new ClockTickViewModel(h, h == 24 ? "24" : h.ToString()))
                .ToList();

            UpdateTickSelection();
            OnPropertyChanged(nameof(HourTicks));
        }

        public int GetHour24()
        {
            if (CurrentAmPm == AmPmState.AM)
                return DisplayHour == 12 ? 0 : DisplayHour;
            else
                return DisplayHour == 12 ? 12 : (DisplayHour == 24 ? 24 : DisplayHour + 12);
        }

        partial void OnCurrentAmPmChanged(AmPmState value)
        {
            UpdateHourTicks();
            OnPropertyChanged(nameof(IsAm));
            UpdateDisplayHour24();
            UpdateHandAngle();
            OnPropertyChanged(nameof(DisplayHour24));
        }

        partial void OnDisplayHourChanged(int value)
        {
            UpdateDisplayHour24();
            UpdateTickSelection();
            UpdateHandAngle();
            OnPropertyChanged(nameof(DisplayHour24));
        }

        partial void OnMinuteChanged(int value)
        {
            UpdateTickSelection();
            UpdateHandAngle();
        }

        partial void OnCurrentModeChanged(EditMode value)
        {
            UpdateTickSelection();
            UpdateHandAngle();
        }

        private void UpdateTickSelection()
        {
            if (HourTicks != null)
                foreach (var tick in HourTicks)
                    tick.IsSelected = (CurrentMode == EditMode.Hour && tick.Value == DisplayHour);

            if (MinuteTicks != null)
                foreach (var tick in MinuteTicks)
                    tick.IsSelected = (CurrentMode == EditMode.Minute && tick.Value == (Minute / 5 * 5) && Minute % 5 == 0);
        }

        private void UpdateDisplayHour24()
        {
            _displayHour24 = DisplayHour24;
        }

        [RelayCommand] 
        private void SetMode(string mode) 
        { 
            if (Enum.TryParse<EditMode>(mode, true, out var editMode)) 
                CurrentMode = editMode; 
        }

        [RelayCommand] 
        private void SetAmPm(string state) 
        { 
            if (Enum.TryParse<AmPmState>(state, true, out var ampmState)) 
                CurrentAmPm = ampmState; 
        }

        [RelayCommand]
        private void SelectTime(ClockTickViewModel tick)
        {
            if (tick == null) return;

            if (CurrentMode == EditMode.Hour)
            {
                DisplayHour = tick.Value;
                UpdateDisplayHour24();
            }
            else if (CurrentMode == EditMode.Minute)
            {
                Minute = tick.Value;
            }
        }

        [RelayCommand]
        private void PointerPressed(PointerPressedEventArgs args)
        {
            if (args is null) return;
            var control = args.Source as Control;
            if (control is null) return;

            _isDragging = true;
            UpdateAngleFromPointer(args.GetPosition(control), control);
            args.Pointer.Capture(control);
        }

        [RelayCommand]
        private void PointerMoved(PointerEventArgs args)
        {
            if (args is null || !_isDragging) return;
            var control = args.Source as Control;
            if (control is null) return;

            UpdateAngleFromPointer(args.GetPosition(control), control);
        }

        [RelayCommand]
        private void PointerReleased(PointerReleasedEventArgs args)
        {
            if (args is null) return;
            _isDragging = false;
            args.Pointer.Capture(null);
        }

        private void UpdateAngleFromPointer(Point position, Control control)
        {
            // 计算中心点
            var centerX = control.Bounds.Width / 2;
            var centerY = control.Bounds.Height / 2;

            // 计算与中心点的角度
            var deltaX = position.X - centerX;
            var deltaY = position.Y - centerY;
            var angleRad = Math.Atan2(deltaY, deltaX);
            var angleDeg = (angleRad * 180 / Math.PI) + 90;
            if (angleDeg < 0) angleDeg += 360;

            if (CurrentMode == EditMode.Hour)
            {
                var hour = (int)Math.Round(angleDeg / 30.0);
                if (hour == 0) hour = 12;
                if (hour > 12) hour -= 12;
                DisplayHour = hour;
            }
            else if (CurrentMode == EditMode.Minute)
            {
                var minute = (int)Math.Round(angleDeg / 6.0);
                if (minute >= 60) minute = 0;
                Minute = minute;
            }
        }

        [RelayCommand]
        private void Confirm()
        {
            OnConfirm?.Invoke(new TimeSpan(_displayHour24, Minute, 0));
        }

        [RelayCommand]
        private void Cancel() => OnCancel?.Invoke();
    }
}
