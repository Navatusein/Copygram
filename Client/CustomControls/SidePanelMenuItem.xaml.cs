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
    /// Interaction logic for SidePanelMenuItem.xaml
    /// </summary>
    public partial class SidePanelMenuItem : UserControl
    {
        public static readonly DependencyProperty IconImageSourceProperty
            = DependencyProperty.Register(
                "IconImageSource",
                typeof(ImageSource),
                typeof(SidePanelMenuItem));

        public ImageSource IconImageSource
        {
            get => (ImageSource)GetValue(IconImageSourceProperty);
            set => SetValue(IconImageSourceProperty, value);
        }

        public static readonly DependencyProperty ItemTextSourceProperty
            = DependencyProperty.Register(
                "TextSource",
                typeof(string),
                typeof(SidePanelMenuItem));

        public string TextSource
        {
            get => (string)GetValue(ItemTextSourceProperty);
            set => SetValue(ItemTextSourceProperty, value);
        }

        public static readonly RoutedEvent TapEvent = EventManager.RegisterRoutedEvent(
            "Tap", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SidePanelMenuItem));

        public event RoutedEventHandler Tap
        {
            add { AddHandler(TapEvent, value); }
            remove { RemoveHandler(TapEvent, value); }
        }

        void RaiseClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(SidePanelMenuItem.TapEvent);
            RaiseEvent(newEventArgs);
        }

        void OnClick()
        {
            RaiseClickEvent();
        }

        public SidePanelMenuItem()
        {
            InitializeComponent();
            this.DataContext = this;

            PreviewMouseLeftButtonUp += (sender, args) => OnClick();
        }
    }
}
