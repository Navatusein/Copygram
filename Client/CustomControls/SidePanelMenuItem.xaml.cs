using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        public SidePanelMenuItem()
        {
            InitializeComponent();
            this.DataContext = this;
        }
    }
}
