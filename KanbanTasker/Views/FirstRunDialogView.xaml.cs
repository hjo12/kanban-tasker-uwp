﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KanbanTasker.Views
{
    public sealed partial class FirstRunDialogView : ContentDialog
    {
        public FirstRunDialogView()
        {
            this.InitializeComponent();
        }

        private void btnCloseDialog_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}