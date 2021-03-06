using SocketServer.PS3.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static SocketServer.PS3.Commands.command;

namespace SocketServer.PS3.ViewModels {
    public delegate void CommandReceivedEventHandler(object sender, CommandEventArgs e);
    public delegate void CommandSentEventHandler(object sender, CommandEventArgs e);
    public delegate void CommandSendingFailedEventHandler(object sender, EventArgs e);
    public delegate void DisconnectedEventHandler(object sender, ClientEventArgs e);

    public class CommandEventArgs : EventArgs {
        private command _cmd;
        public command cmd {
            get {
                return _cmd;
            }
        }

        private auth _auth;
        public auth auth {
            get {
                return _auth;
            }
        }

        public CommandEventArgs(command a) {
            this._cmd = a;
        }
        public CommandEventArgs(auth a) {
            this._auth = a;
            this._cmd = a;
        }
    }

    public class ClientEventArgs : EventArgs {
        public IPAddress IP;
        public int Port;
        public ClientEventArgs(IPAddress ip, int port) {
            this.IP = ip;
            this.Port = port;
        }
    }
}
