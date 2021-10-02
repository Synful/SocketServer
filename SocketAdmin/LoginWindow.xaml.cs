using SocketAdmin.Commands;
using SocketAdmin.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using static SocketAdmin.Commands.command;

namespace SocketAdmin {
    public partial class LoginWindow : Window {
        Server server;

        public LoginWindow() {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            server = new Server(IPAddress.Parse("192.168.1.2"), 38426);
            server.Connect();
            server.CommandReceived += CommandRecieved;
        }

        private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e) {
            if(e.LeftButton == MouseButtonState.Pressed) {
                DragMove();
            }
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }
        private void miniBtn_Click(object sender, RoutedEventArgs e) {
            this.WindowState = WindowState.Minimized;
        }

        private void loginBtn_Click(object sender, RoutedEventArgs e) {
            server.Send_Command(new login(usernameTxt.Text, passwordTxt.Password));
        }

        private void CommandRecieved(object sender, CommandEventArgs e) {
            switch(e.cmd.type) {
                case cmdType.Login:
                    break;
            }
        }
    }
}
