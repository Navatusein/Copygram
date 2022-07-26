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
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for SignUp.xaml
    /// </summary>
    public partial class SignUp : Window
    {
        public SignUp()
        {
            InitializeComponent();
        }
        private void selectImageAction(object sender, RoutedEventArgs e)
        {
            // TODO
            // Upload image using image path.
        }

        private void signupAction(object sender, RoutedEventArgs e)
        {
            if (usernameTb.Text.ToString() != String.Empty && passwordTb.Text.ToString() != String.Empty)
            {
                // TODO
                // Sign up process. Send request to server.
            }
        }

        private void passwordAction(object sender, TextChangedEventArgs e)
        {
            // TODO
            // Check if not null
        }

        private void usernameAction(object sender, TextChangedEventArgs e)
        {
            // TODO
            // Check if not null
        }
    }
}
