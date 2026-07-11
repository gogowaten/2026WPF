using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace _20260710_Template;

public partial class MainViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;

    [ObservableProperty] private double _windowWidth;
    [ObservableProperty] private double _windowHeight;
    

    public MainViewModel()
    {
        _settingsService = new SettingsService();
        _settingsService.Load();

        var settings = _settingsService.MyData;
        WindowWidth = settings.WindowWidth;
        WindowHeight = settings.WindowHeight;

        
    }

    [RelayCommand]
    private void Save()
    {
        var settings = _settingsService.MyData;

        settings.WindowWidth = WindowWidth;
        settings.WindowHeight = WindowHeight;

        _settingsService.Save();
    }

}
