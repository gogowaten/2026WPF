using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace _20260218_Mvvm
{
    //public partial class CounterViewModel : ObservableObject
    //{
    //    [ObservableProperty] private int count;
    //    [RelayCommand]
    //    private void Increment() { Count++; }

    //    [RelayCommand]
    //    private void Decrement() { Count--; }
    //    [RelayCommand]
    //    private void Reset() { Count = 0; }
    //}

    /*    public partial class CounterViewModel : ObservableObject
        {
            private readonly CounterModel _model = new();

            [ObservableProperty]
            private int _count;

            public CounterViewModel()
            {
                Count = _model.Count;
            }

            [RelayCommand]
            private void Increment()
            {
                _model.Increment();
                Count = _model.Count;
            }

            [RelayCommand]
            private void Decrement()
            {
                _model.Decrement();
                Count = _model.Count;
            }
            [RelayCommand]
            private void Reset()
            {
                _model.Reset();
                Count = _model.Count;
            }
        }
    */

    public partial class CounterViewModel : ObservableObject
    {  
        [ObservableProperty] private CounterModel _counterModel = new();
        [RelayCommand] public void Increment() { CounterModel.Increment(); }
        [RelayCommand] public void Reset() { CounterModel.Reset(); }

    }
}
