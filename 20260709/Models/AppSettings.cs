using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;

//namespace _20260709.Models
//{
//    public class AppSettings
//    {
//        // --- ウィンドウ位置・サイズ ---
//        public double WindowLeft { get; set; } = 100;
//        public double WindowTop { get; set; } = 100;
//        public double WindowWidth { get; set; } = 800;
//        public double WindowHeight { get; set; } = 450;
//        public WindowState WindowState { get; set; } = WindowState.Normal;

//        // --- その他のアプリ固有の設定 ---
//        public string LastOpenedDirectory { get; set; } = string.Empty; // string型
//        public bool IsDarkTheme { get; set; } = false;                  // bool型
//        public int AutoSaveIntervalMinutes { get; set; } = 30;          // int型
//    }
//}

//namespace _20260709.Models
//{
//    public class AppSettings
//    {

//    }
//}

namespace _20260709.Models;

public class AppSettings
{
    // --- ウィンドウ位置・サイズ ---
    public double WindowLeft { get; set; } = 100;
    public double WindowTop { get; set; } = 100;
    public double WindowWidth { get; set; } = 800;
    public double WindowHeight { get; set; } = 450;
    public WindowState WindowState { get; set; } = WindowState.Normal;

    // --- その他のアプリ固有の設定 ---
    public string LastOpenedDirectory { get; set; } = string.Empty; // string型
    public bool IsDarkTheme { get; set; } = false;                  // bool型
    public int AutoSaveIntervalMinutes { get; set; } = 30;          // int型

}

