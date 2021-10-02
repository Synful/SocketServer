using SocketServer.PS3.ViewModels;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SocketServer.PS3.Commands.command;

namespace SocketServer.PS3 {
    public class Main_PS3 {
        private static Main_PS3 _inst;
        public static Main_PS3 inst {
            get {
                if(_inst == null) {
                    _inst = new Main_PS3();
                }
                return _inst;
            }
        }

        private IPEndPoint ep;

        public List<Client> clients;
        Thread client_listener_t;
        Socket client_listener;

        public Main_PS3() {
#if DEBUG
            Logger.inst.Info("Starting Genisys User Server in debug mode...");
            ep = new IPEndPoint(IPAddress.Parse("192.168.1.2"), 25725);
#else
            Logger.inst.Info("Starting Genisys User Server...");
            ep = new IPEndPoint(IPAddress.Parse("147.135.120.177"), 25725);
#endif

            clients = new List<Client>();
            client_listener_t = new Thread(() => client_listener_listen());
            client_listener_t.Start();
        }

        #region Handling
        private void client_listener_listen() {
            client_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            client_listener.Bind(ep);
            client_listener.Listen();
            while(true) {
                try {
                    AddClient(client_listener.Accept());
                } catch { }
            }
        }

        private void AddClient(Socket socket) {
            Client client = new Client(socket);
            client.CommandReceived += new CommandReceivedEventHandler(CommandReceived);
            client.Disconnected += new DisconnectedEventHandler(ClientDisconnected);
            client.CommandSent += new CommandSentEventHandler(CommandSent);

            RemoveClient(client.IP);
            clients.Add(client);
            Logger.inst.Info($"{client.IP}:{client.Port} Connected.");
        }

        private bool RemoveClient(IPAddress ip) {
            lock(this) {
                Client c = clients.Find(x => x.IP.ToString() == ip.ToString());
                if(c != null) {
                    c.Disconnect();
                    clients.Remove(c);
                    Logger.inst.Info($"Removed Client: {c.IP}:{c.Port}");
                    return true;
                } else {
                    return false;
                }
            }
        }
        private bool RemoveClient(IPAddress ip, int port) {
            lock(this) {
                Client c = clients.Find(x => ((x.IP.ToString() == ip.ToString()) && (x.Port == port)));
                if(c != null) {
                    clients.Remove(c);
                    return true;
                } else {
                    return false;
                }
            }
        }

        public void Close() {
            if(client_listener_t != null && client_listener_t.ThreadState == ThreadState.Running) {
                if(clients != null) {
                    foreach(Client c in clients) {
                        c.Disconnect();
                    }
                    client_listener.Close();
                }
                GC.Collect();
            }
        }
        #endregion

        #region Events
        private void CommandReceived(object sender, CommandEventArgs e) {
            switch(e.cmd.type) {
                case cmdType.Auth:
                    ClientInfo i = e.auth.info;

                    i.code = Database.inst.AuthClient(i);

                    if(i.name != "")
                        Logger.inst.Auth(i);

                    i.user.Send_Command(e.auth);

                    break;
                case cmdType.Disconnect:
                    RemoveClient(e.cmd.client_ip);
                    break;
            }
        }
        private void CommandSent(object sender, CommandEventArgs e) {
            switch(e.cmd.type) {
                case cmdType.Auth:
                    Logger.inst.Info($"Sending Auth Command To {e.cmd.client_ip}");
                    break;
                default:
                    Logger.inst.Error($"Sending Unk Command To {e.cmd.client_ip}");
                    break;
            }
        }
        private void ClientDisconnected(object sender, ClientEventArgs e) {
            if(RemoveClient(e.IP, e.Port))
                Logger.inst.Info($"{e.IP}:{e.Port} Disconnected.");
        }
        #endregion
    }
}
