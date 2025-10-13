using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using BiliLive.Core.Interface;
using BiliLive.Resources;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace BiliLive.Views.MainWindow.Pages.HomePage.Components;

public partial class MainAreas(string areaName, Action<string> selectMainArea) : ObservableObject
{
    [ObservableProperty] private string _areaName = areaName;
    [RelayCommand] private void OnSelectArea() => selectMainArea(AreaName);
}

public partial class SubAreas(string subAreaName, Action<string> selectSubArea) : ObservableObject
{
    [ObservableProperty] private string _subAreaName = subAreaName;
    [RelayCommand] private void OnSelectSubArea() => selectSubArea(SubAreaName);
}

public partial class AreaSelectorViewModel : ViewModelBase
{
    [ObservableProperty]private ObservableCollection<MainAreas> _areasGroup = [];
    [ObservableProperty]private ObservableCollection<SubAreas> _subAreaGroup = [];
    [ObservableProperty]private string _selectedSubArea = "子分区";
    [ObservableProperty]private string? _selectedMainArea;

    private JsonElement _areasElement;
    public IBiliService? BiliService;
    
    public AreaSelectorViewModel()
    {
        if (!Design.IsDesignMode) return;
        AreasGroup.Add(new MainAreas("游戏12345", SelectMainArea));
        AreasGroup.Add(new MainAreas("娱乐1234", SelectMainArea));
        AreasGroup.Add(new MainAreas("生活1223", SelectMainArea));
        SubAreaGroup.Add(new SubAreas("单机游戏12312", SelectSubArea));
        SubAreaGroup.Add(new SubAreas("手游123213", SelectSubArea));
        SubAreaGroup.Add(new SubAreas("桌游棋牌123213", SelectSubArea));
    }
    
    [RelayCommand]
    public async Task RefreshAreasAsync()
    {
        if (BiliService == null) return;
        SelectMainArea(string.IsNullOrWhiteSpace(SelectedMainArea) ? AreasGroup[0].AreaName : SelectedMainArea);
        SelectedSubArea = (await BiliService.GetMyLastChooseAreaAsync()).GetProperty("name").GetString() ?? "子分区";
    }
    
    [RelayCommand]
    private async Task LoadAreasAsync()
    {
        try
        {
            if (BiliService == null) return; 
            var areas = (await BiliService.GetAreasListAsync()).GetProperty("data").GetProperty("area_v1_info");
            _areasElement = areas;
            foreach (var areaName in areas.EnumerateArray().Select(area => area.GetProperty("name").GetString()).OfType<string>())
            {
                AreasGroup.Add(new MainAreas(areaName, SelectMainArea));
            }
            await RefreshAreasAsync();
        }
        catch { 
            // ignored
        }
    }
    
    [RelayCommand]
    private void SelectMainArea(string targetArea)
    {
        try
        {
            SelectedMainArea = targetArea;
            SubAreaGroup.Clear();
            _areasElement.EnumerateArray()
                .FirstOrDefault(area => area.GetProperty("name").GetString() == targetArea)
                .TryGetProperty("list", out var subAreasElement);
            foreach (var subAreaName in subAreasElement.EnumerateArray().Select(subArea => subArea.GetProperty("name").GetString()).OfType<string>())
            {
                SubAreaGroup.Add(new SubAreas(subAreaName, SelectSubArea));
            }
        }
        catch { 
            // ignored
        }
    }

    private void SelectSubArea(string targetSubArea)
    {
        try
        { 
            _areasElement.EnumerateArray().SelectMany(area => area.GetProperty("list").EnumerateArray())
                .FirstOrDefault(subArea => subArea.GetProperty("name").GetString() == targetSubArea)
                .TryGetProperty("id", out var subAreaId);
            int.TryParse(subAreaId.GetString(), out var areaId);
            Task.Run(async () =>
            {
                if (BiliService == null) return;
                try
                {
                    await BiliService.ChangeRoomAreaAsync(areaId);
                    SelectedSubArea = targetSubArea;
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                            $"已切换分区至{targetSubArea}", 
                            Geometry.Parse(MdIcons.Check)));
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            });
        }
        catch
        {
            // ignored
        }
    }
}