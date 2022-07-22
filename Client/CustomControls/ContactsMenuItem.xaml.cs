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
    /// Interaction logic for ContactsMenuItem.xaml
    /// </summary>
    public partial class ContactsMenuItem : UserControl
    {
        public static readonly DependencyProperty ImageSourceProperty
            = DependencyProperty.Register(
            "ImageSource",
            typeof(ImageSource),
            typeof(ContactsMenuItem));

        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public static readonly DependencyProperty UsernameSourceProperty
            = DependencyProperty.Register(
            "UsernameSource",
            typeof(string),
            typeof(ContactsMenuItem));

        public string UsernameSource
        {
            get => (string)GetValue(UsernameSourceProperty);
            set => SetValue(UsernameSourceProperty, value);
        }

        public static readonly DependencyProperty UserStatusSourceProperty
            = DependencyProperty.Register(
            "UserStatusSource",
            typeof(string),
            typeof(ContactsMenuItem));

        public string UserStatusSource
        {
            get => (string)GetValue(UserStatusSourceProperty);
            set => SetValue(UserStatusSourceProperty, value);
        }

        public ContactsMenuItem()
        {
            InitializeComponent();
            this.DataContext = this;
        }
    }
}
