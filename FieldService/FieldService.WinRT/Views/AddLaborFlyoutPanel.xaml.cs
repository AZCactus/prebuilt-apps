﻿using FieldService.Utilities;
using FieldService.WinRT.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace FieldService.WinRT.Views
{
    public sealed partial class AddLaborFlyoutPanel : UserControl
    {
        LaborViewModel laborViewModel;
        public AddLaborFlyoutPanel()
        {
            this.InitializeComponent();

            DataContext =
                laborViewModel = ServiceContainer.Resolve<LaborViewModel>();
        }
    }
}
