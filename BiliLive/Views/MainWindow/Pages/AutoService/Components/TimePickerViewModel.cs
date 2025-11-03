using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Input;
using BiliLive.Views.MainWindow.Pages.AutoService.Components.Utility;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BiliLive.Views.MainWindow.Pages.AutoService.Components
{
    public partial class TimePickerViewModel : ObservableObject
    {
        private bool _isDragging;
        private const double ClockCenter = 130;

        public event Action<DateTime?>? RequestClose;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsMinuteMode))]
        [NotifyPropertyChangedFor(nameof(HandLength))]
        private bool _isHourMode = true;

        public bool IsMinuteMode => !IsHourMode;

        [ObservableProperty]
        private int _displayHour;

        [ObservableProperty]
        private int _minute;

        [ObservableProperty]
        private bool _isAm = true;

        [ObservableProperty]
        private double _handAngle;

        public double HandLength => IsHourMode ? 85 : 110;

        public ObservableCollection<ClockTickViewModel> HourTicks { get; }
        public ObservableCollection<ClockTickViewModel> MinuteTicks { get; }

        public TimePickerViewModel()
        {
            var now = DateTime.Now;
            SetTime(now.Hour, now.Minute);

            HourTicks = new ObservableCollection<ClockTickViewModel>(
                Enumerable.Range(1, 12).Select(h => new ClockTickViewModel(h, h.ToString()))
            );
            MinuteTicks = new ObservableCollection<ClockTickViewModel>(
                Enumerable.Range(0, 12).Select(m => {
                    int value = m * 5;
                    string display = value.ToString("00");
                    return new ClockTickViewModel(value, display);
                })
            );

            UpdateHandAngle();
            UpdateSelectedTick();
        }

        [RelayCommand]
        private void SetMode(string mode)
        {
            IsHourMode = mode == "Hour";
            UpdateHandAngle();
            UpdateSelectedTick();
        }

        [RelayCommand]
        private void SetAmPm(string period)
        {
            IsAm = period == "AM";
        }

      [RelayCommand]
      private void SelectTime(ClockTickViewModel? tick)
      {
          if (tick == null) return;
      
          if (IsHourMode)
          {
              DisplayHour = tick.Value;
           
              SetMode("Minute");
          }
          else
          {
              Minute = tick.Value;
          }
          
          UpdateHandAngle();
          UpdateSelectedTick();
      }

        [RelayCommand]
        private void PointerPressed(PointerPressedEventArgs args)
        {
            var sourceVisual = args.Source as Visual;
            if (sourceVisual == null) return;

            _isDragging = true;
            UpdateClockFromPointer(args.GetPosition(sourceVisual));
        }

        [RelayCommand]
        private void PointerMoved(PointerEventArgs args)
        {
            if (!_isDragging) return;
            
            var sourceVisual = args.Source as Visual;
            if (sourceVisual == null) return;

            UpdateClockFromPointer(args.GetPosition(sourceVisual));
        }

        [RelayCommand]
        private void PointerReleased(PointerReleasedEventArgs args)
        {
            if (!_isDragging) return;
            _isDragging = false;
            if (IsHourMode)
            {
                SetMode("Minute");
            }
        }
        
        [RelayCommand]
        private void Confirm()
        {
            int hour24 = (IsAm, DisplayHour) switch
            {
                (true, 12) => 0,
                (false, 12) => 12,
                (true, _) => DisplayHour,
                (false, _) => DisplayHour + 12
            };
            var result = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour24, Minute, 0);
            RequestClose?.Invoke(result);
        }

        [RelayCommand]
        private void Cancel()
        {
            RequestClose?.Invoke(null);
        }

        private void SetTime(int hour24, int minute)
        {
            IsAm = hour24 < 12;
            
            if (hour24 == 0) DisplayHour = 12;
            else if (hour24 > 12) DisplayHour = hour24 - 12;
            else DisplayHour = hour24;
            
            Minute = minute;
        }

        private void UpdateClockFromPointer(Point pointerPosition)
        {
            var dx = pointerPosition.X - ClockCenter;
            var dy = pointerPosition.Y - ClockCenter;

            var angleInDegrees = Math.Atan2(dy, dx) * 180 / Math.PI;
            
            var logicalAngle = angleInDegrees + 90;
            if (logicalAngle < 0)
            {
                logicalAngle += 360;
            }
            
            HandAngle = (logicalAngle + 180) % 360;
            
            if (IsHourMode)
            {
                int hour = (int)Math.Round(logicalAngle / 30.0);
                if (hour == 0) hour = 12;
                DisplayHour = hour;
            }
            else
            {
                int minute = (int)Math.Round(logicalAngle / 6.0);
                if (minute == 60) minute = 0;
                Minute = minute;
            }
            UpdateSelectedTick();
        }

        private void UpdateHandAngle()
        {
            double logicalAngle;
            if (IsHourMode)
            {
                logicalAngle = (DisplayHour % 12) * 30;
            }
            else
            {
                logicalAngle = Minute * 6;
            }
            
            HandAngle = (logicalAngle + 180) % 360;
        }

      private void UpdateSelectedTick()
      {
          if (IsHourMode)
          {
              foreach (var tick in HourTicks)
              {
                  tick.IsSelected = tick.Value == DisplayHour;
              }
              foreach (var tick in MinuteTicks)
              {
                  tick.IsSelected = false;
              }
          }
          else 
          {
              foreach (var tick in MinuteTicks)
              {
                  tick.IsSelected = tick.Value == Minute;
              }
              foreach (var tick in HourTicks)
              {
                  tick.IsSelected = false;
              }
          }
      }
    }
}