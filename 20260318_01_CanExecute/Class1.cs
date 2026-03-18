using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;

namespace _20260318_01_CanExecute
{




    public partial class Class1 : ObservableObject
    {
        [NotifyCanExecuteChangedFor(nameof(AgeCommand))]
        [NotifyCanExecuteChangedFor(nameof(SageCommand))]
        [ObservableProperty]
        private int _id;


        public Class1()
        {
            Id = 0;
        }


        private bool HanteiAge()
        {
            return Id < 10;
        }
        private bool HanteiSage()
        {
            return Id > 0;
        }

        [RelayCommand(CanExecute = nameof(HanteiAge))]
        private void Age()
        {
            Id++;
        }

        [RelayCommand(CanExecute = nameof(HanteiSage))]
        private void Sage() { Id--; }

    }
}
