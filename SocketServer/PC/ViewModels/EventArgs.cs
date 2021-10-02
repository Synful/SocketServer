using SocketServer.PC.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.PC.ViewModels {
    public delegate void CommandReceivedEventHandler(object sender, CommandEventArgs e);
    public delegate void CommandSentEventHandler(object sender, CommandEventArgs e);
    public delegate void CommandSendingFailedEventHandler(object sender, EventArgs e);
    public delegate void DisconnectedEventHandler(object sender, AdminEventArgs e);

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

    public class AdminEventArgs : EventArgs {
        public IPAddress IP;
        public int Port;
        public AdminEventArgs(IPAddress ip, int port) {
            this.IP = ip;
            this.Port = port;
        }
    }
}
