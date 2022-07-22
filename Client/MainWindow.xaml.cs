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
using System.Windows.Threading;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isHidden;
        double panelWidth;

        DispatcherTimer timer;
        public MainWindow()
        {
            InitializeComponent();
            isHidden = true;
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            timer.Tick += Timer_Tick;

            panelWidth = sidePanel.Width;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            double tickSize = this.Width / 100;
            if (isHidden)
            {
                sidePanel.Width += tickSize;
                if (sidePanel.Width >= this.Width / 3)
                {
                    timer.Stop();
                    isHidden = false;
                }
            }
            else
            { 
                sidePanel.Width -= tickSize;
                if (sidePanel.Width <= this.Width / 3)
                {
                    timer.Stop();
                    isHidden = true;
                }
            }
        }

        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && timer.IsEnabled == false)
            {
                timer.Start();
            }
        }
    }
}
