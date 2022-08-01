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
    /// Логика взаимодействия для IconButton.xaml
    /// </summary>
    public partial class IconButton : UserControl
    {
        public static readonly DependencyProperty IconImageSourceProperty
            = DependencyProperty.Register(
                "IconImageSource",
                typeof(ImageSource),
                typeof(IconButton));

        public ImageSource IconImageSource
        {
            get => (ImageSource)GetValue(IconImageSourceProperty);
            set => SetValue(IconImageSourceProperty, value);
        }

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
            "Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(IconButton));

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        void RaiseClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(IconButton.ClickEvent);
            RaiseEvent(newEventArgs);
        }

        void OnClick()
        {
            RaiseClickEvent();
        }

        public static readonly DependencyProperty DesiredHeightProperty
            = DependencyProperty.Register(
        "DesiredHeight",
        typeof(int),
        typeof(IconButton));

        public int DesiredHeight
        {
            get => (int)GetValue(DesiredHeightProperty);
            set => SetValue(DesiredHeightProperty, value);
        }

        public static readonly DependencyProperty DesiredWidthProperty
            = DependencyProperty.Register(
        "DesiredWidth",
        typeof(int),
        typeof(IconButton));

        public int DesiredWidth
        {
            get => (int)GetValue(DesiredWidthProperty);
            set => SetValue(DesiredWidthProperty, value);
        }

        public IconButton()
        {
            InitializeComponent();
            this.DataContext = this;

            PreviewMouseLeftButtonUp += (sender, args) => OnClick();
        }
    }
}
