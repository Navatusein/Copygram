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
    /// Interaction logic for DonatePage.xaml
    /// </summary>
    public partial class DonatePage : UserControl
    {
        public DonatePage()
        {
            InitializeComponent();
        }

        private void IconButton_MouseEnter(object sender, MouseEventArgs e)
        {
            btExit.Background = new SolidColorBrush(Color.FromRgb(153, 0, 0));
        }

        private void IconButton_MouseLeave(object sender, MouseEventArgs e)
        {
            btExit.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }

    }
}
