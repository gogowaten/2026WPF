using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace _20260306_DaraTemplateRectangle
{
    public partial class RectangleVM : ObservableObject
    {
        public RectangleVM()
        {
            this._data = new(50, 150);
        }

        [ObservableProperty] private RectangleData _data;


    }

    internal class ViewModels
    {
    }
}
