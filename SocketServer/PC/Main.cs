using SocketServer.PC.ViewModels;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SocketServer.PC.Commands.command;

namespace SocketServer.PC {
    public class Main_PC {
        private static Main_PC _inst;
        public static Main_PC inst {
            get {
                if(_inst == null) {
                    _inst = new Main_PC();
                }
                return _inst;
            }
        }

        private IPEndPoint ep;

        Thread admin_listener_t;
        public List<Admin> admins;
        Socket admin_listener;

        /*
         To send packets back and fourth for admin panel just serialize the command to json and deserialzie it
         
         */

        public Main_PC() {
#if DEBUG
            Logger.inst.Info("Starting Genisys Admin Server in debug mode...");
            ep = new IPEndPoint(IPAddress.Parse("192.168.1.2"), 38426);
#else
            Logger.inst.Info("Starting Genisys Admin Server...");
            ep = new IPEndPoint(IPAddress.Parse("147.135.120.177"), 38426);
#endif

            admins = new List<Admin>();
            admin_listener_t = new Thread(() => admin_listener_listen());
            admin_listener_t.Start();
        }

        #region Handling
        private void admin_listener_listen() {
            admin_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            admin_listener.Bind(ep);
            admin_listener.Listen();
            while(true) {
                try {
                    AddAdmin(admin_listener.Accept());
                } catch { }
            }
        }

        private void AddAdmin(Socket socket) {
            Admin admin = new Admin(socket);
            admin.CommandReceived += new CommandReceivedEventHandler(CommandReceived);
            admin.Disconnected += new DisconnectedEventHandler(ClientDisconnected);
            admin.CommandSent += new CommandSentEventHandler(CommandSent);

            RemoveClient(admin.IP);
            admins.Add(admin);
            Logger.inst.Info($"{admin.IP}:{admin.Port} Admin Connected.");
        }

        private bool RemoveClient(IPAddress ip) {
            lock(this) {
                Admin c = admins.Find(x => x.IP.ToString() == ip.ToString());
                if(c != null) {
                    c.Disconnect();
                    admins.Remove(c);
                    Logger.inst.Info($"Removed Admin: {c.IP}:{c.Port}");
                    return true;
                } else {
                    return false;
                }
            }
        }
        private bool RemoveClient(IPAddress ip, int port) {
            lock(this) {
                Admin c = admins.Find(x => ((x.IP.ToString() == ip.ToString()) && (x.Port == port)));
                if(c != null) {
                    admins.Remove(c);
                    return true;
                } else {
                    return false;
                }
            }
        }

        public void Close() {
            if(admin_listener_t != null && admin_listener_t.ThreadState == ThreadState.Running) {
                if(admins != null) {
                    foreach(Admin c in admins) {
                        c.Disconnect();
                    }
                    admin_listener.Close();
                }
                GC.Collect();
            }
        }
        #endregion

        #region Events
        private void CommandReceived(object sender, CommandEventArgs e) {
            switch(e.cmd.type) {
                case cmdType.Auth:

                    break;
                case cmdType.Disconnect:
                    RemoveClient(e.cmd.client_ip);
                    break;
            }
        }
        private void CommandSent(object sender, CommandEventArgs e) {
            switch(e.cmd.type) {
                case cmdType.Auth:
                    break;
                default:
                    break;
            }
        }
        private void ClientDisconnected(object sender, AdminEventArgs e) {
            if(RemoveClient(e.IP, e.Port))
                Logger.inst.Info($"{e.IP}:{e.Port} Disconnected.");
        }
        #endregion

    }
}
