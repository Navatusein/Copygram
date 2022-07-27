using System;
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
    public partial class PrivateOverlay : UserControl
    {
        string WhoToAddress { get; set; }
        public PrivateOverlay()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        public static readonly RoutedEvent UserClickEvent = EventManager.RegisterRoutedEvent(
            "AddClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PrivateOverlay));

        public event RoutedEventHandler AddClick
        {
            add { AddHandler(UserClickEvent, value); }
            remove { RemoveHandler(UserClickEvent, value); }
        }

        void RaiseClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(PrivateOverlay.UserClickEvent);
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
        private void tbGotFocus(object sender, RoutedEventArgs e)
        {
            tbWhoToAddress.Clear();
        }
        private void tbLostFocus(object sender, RoutedEventArgs e)
        {
            tbWhoToAddress.Text = "Nickname";
        }
    }
}
