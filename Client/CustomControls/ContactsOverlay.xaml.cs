﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client.CustomControls
{
    /// <summary>
    /// Логика взаимодействия для ContactsOverlay.xaml
    /// </summary>
    public partial class ContactsOverlay : UserControl
    {
        string WhoToAddress { get; set; }
        public ContactsOverlay()
        {
            InitializeComponent();
            this.DataContext = this;
            string WhoToAddress = string.Empty;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        public static readonly RoutedEvent UserClickEvent = EventManager.RegisterRoutedEvent(
            "UserClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ContactsOverlay));

        public event RoutedEventHandler UserClick
        {
            add { AddHandler(UserClickEvent, value); }
            remove { RemoveHandler(UserClickEvent, value); }
        }

        void RaiseClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(ContactsOverlay.UserClickEvent);
            RaiseEvent(newEventArgs);
        }

        void OnClick()
        {
            RaiseClickEvent();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            WhoToAddress = tbWhoToAddress.Text.Trim();
            Visibility = Visibility.Collapsed;
        }

        private void IsGroup_Checked(object sender, RoutedEventArgs e)
        {
            lbGroup.IsEnabled = true;
            tbGroupName.IsEnabled = true;
            btSelectImage.IsEnabled = true;
        }

        private void IsGroup_Unchecked(object sender, RoutedEventArgs e)
        {
            lbGroup.IsEnabled = false;
            tbGroupName.IsEnabled = false;
            btSelectImage.IsEnabled = false;
        }
    }
}
