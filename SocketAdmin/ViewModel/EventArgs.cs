using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SocketAdmin.Commands;

namespace SocketAdmin.ViewModel {
    public delegate void CommandReceivedEventHandler(object sender, CommandEventArgs e);
    public delegate void CommandSentEventHandler(object sender, EventArgs e);
    public delegate void CommandSendingFailedEventHandler(object sender, EventArgs e);

    public delegate void ServerDisconnectedEventHandler(object sender, ServerEventArgs e);
    public delegate void DisconnectedEventHandler(object sender, EventArgs e);

    public delegate void ConnectingSuccessedEventHandler(object sender, EventArgs e);
    public delegate void ConnectingFailedEventHandler(object sender, EventArgs e);

    public delegate void NetworkDeadEventHandler(object sender, EventArgs e);
    public delegate void NetworkAlivedEventHandler(object sender, EventArgs e);

    public class CommandEventArgs : EventArgs {
        private command _cmd;
        public command cmd {
            get {
                return _cmd;
            }
        }

        private login _login;
        public login login {
            get {
                return _login;
            }
        }

        public CommandEventArgs(login l) {
            this._login = l;
            this._cmd = l;
        }
    }

    public class ServerEventArgs : EventArgs {
        private Socket socket;
        public IPAddress IP {
            get { return ((IPEndPoint)this.socket.RemoteEndPoint).Address; }
        }
        public int Port {
            get { return ((IPEndPoint)this.socket.RemoteEndPoint).Port; }
        }

        public ServerEventArgs(Socket clientSocket) {
            this.socket = clientSocket;
        }
    }
}
