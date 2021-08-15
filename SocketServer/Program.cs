using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SocketServer.MenuHandler;
using SocketServer.Utils;
using SocketServer.PC.ViewModels;
using SocketServer.PS3.ViewModels;
using static SocketServer.PS3.Commands.command;

namespace SocketServer {
    class Program {
        static Program _inst;
        public static Program inst {
            get {
                if(_inst == null)
                    _inst = new Program();
                return _inst;
            }
        }

        MenuBase menu;

        Thread client_listener_t;
        public List<Client> clients;
        Socket client_listener;

        Thread admin_listener_t;
        public List<Admin> admins;
        Socket admin_listener;

        static void Main(string[] args) {
            Program.inst.StartServer();
        }
        void StartServer() {
            menu = new MenuBase();
            Database db = Database.instance;
            Settings s = Settings.instance;
            Logger.inst.Info("Starting Genisys Auth Server...");

            clients = new List<Client>();
            client_listener_t = new Thread(() => client_listener_listen());
            client_listener_t.Start();

            //admins = new List<Admin>();
            //admin_listener_t = new Thread(() => admin_listener_listen());
            //admin_listener_t.Start();
        }
        public void DisconnectServer() {
            if(clients != null) {
                foreach(Client c in clients) {
                    c.Disconnect();
                }
                client_listener.Close();
            }
            if(admins != null) {
                foreach(Admin a in admins) {
                    a.Disconnect();
                }
                admin_listener.Close();
            }
            GC.Collect();
        }

        #region Admin
        #region Handling
        private void admin_listener_listen() {
            admin_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            admin_listener.Bind(new IPEndPoint(IPAddress.Parse("192.168.1.8"), 999));
            admin_listener.Listen();
            while(true) {
                try {
                    AddAdmin(client_listener.Accept());
                } catch { }
            }
        }

        private void AddAdmin(Socket socket) {

        }
        #endregion
        #endregion

        #region Client
        #region Handling
        private void client_listener_listen() {
            client_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            client_listener.Bind(new IPEndPoint(IPAddress.Parse("147.135.120.177"), 25725));
            //client_listener.Bind(new IPEndPoint(IPAddress.Parse("192.168.1.8"), 666));
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
                    c.Disconnect();
                    clients.Remove(c);
                    Logger.inst.Info($"Removed Client: {c.IP}:{c.Port}");
                    return true;
                } else {
                    return false;
                }
            }
        }
        #endregion

        #region Events
        private void CommandReceived(object sender, CommandEventArgs e) {
            switch(e.cmd.type) {
                case cmdType.Auth:
                    ClientInfo i = e.auth.info;

                    i.code = Database.instance.AuthClient(i.lic, i.mac, i.psid, i.checksum, i.version);

                    if(!i.reauth)
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
        #endregion
    }
}
