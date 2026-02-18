using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace _20260218_Mvvm
{
    //public class CounterModel
    //{
    //    public int Count { get; private set; }
    //    public void Increment() { Count++; }
    //    public void Decrement() { Count--; }
    //    public void Reset() { Count = 0; }
    //}

    public partial class CounterModel : ObservableObject
    {
        [ObservableProperty] private int _counter;

        public void Increment() { Counter++; }

        public void Reset() => Counter = 0;
    }
}
