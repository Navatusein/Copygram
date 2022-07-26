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
        public ContactsOverlay()
        {
            InitializeComponent();
            this.DataContext = this;

            ContactsList.PreviewMouseDoubleClick += (sender, args) => OnClick();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SeacrhBox.Clear();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SeacrhBox.Text = "Search";
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
    }
}
