using SocketAdmin.Commands;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CryptSharp;
using static SocketAdmin.Commands.command;

namespace SocketAdmin.ViewModel {
    public class Server {
        #region Vars
        public bool Connected {
            get {
                if(socket != null)
                    return socket.Connected;
                else
                    return false;
            }
        }

        public IPAddress ServerIP {
            get {
                if(Connected)
                    return server_ep.Address;
                else
                    return IPAddress.None;
            }
        }
        public int ServerPort {
            get {
                if(Connected)
                    return server_ep.Port;
                else
                    return -1;
            }
        }

        public IPAddress IP {
            get {
                if(Connected)
                    return ((IPEndPoint)socket.LocalEndPoint).Address;
                else
                    return IPAddress.None;
            }
        }
        public int Port {
            get {
                if(Connected)
                    return ((IPEndPoint)socket.LocalEndPoint).Port;
                else
                    return -1;
            }
        }

        public Socket socket;

        private NetworkStream stream;
        private Message msg;
        private BackgroundWorker recv_bw;
        private BackgroundWorker send_bw;
        private BackgroundWorker connector_bw;
        private IPEndPoint server_ep;
        private Semaphore sema = new Semaphore(1, 1);
        #endregion

        #region Contsructors
        public Server(IPEndPoint server) {
            server_ep = server;

            NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(NetworkChange_NetworkAvailabilityChanged);

            connector_bw = new BackgroundWorker();
            connector_bw.DoWork += new DoWorkEventHandler(Connect);
            connector_bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Connect_Completed);
        }

        public Server(IPAddress serverIP, int port) {
            server_ep = new IPEndPoint(serverIP, port);

            NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(NetworkChange_NetworkAvailabilityChanged);

            connector_bw = new BackgroundWorker();
            connector_bw.DoWork += new DoWorkEventHandler(Connect);
            connector_bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Connect_Completed);
        }
        #endregion

        #region Private Methods
        private void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e) {
            if(!e.IsAvailable) {
                OnNetworkDead(new EventArgs());
                OnDisconnectedFromServer(new EventArgs());
            } else
                OnNetworkAlived(new EventArgs());
        }

        private void Recv(object sender, DoWorkEventArgs e) {
            while(socket.Connected) {
                try {
                    msg.load_data();
                    cmdType cType = (cmdType)msg.read_int();

                    switch(cType) {
                        case cmdType.Login:
                            bool success = msg.read_bool();
                            this.OnCommandReceived(new CommandEventArgs(new login(success)));
                            break;
                        case cmdType.Update:
                            break;
                    }
                } catch(Exception ex) {
                    Console.WriteLine(ex.ToString());
                }
            }
            OnServerDisconnected(new ServerEventArgs(socket));
            Disconnect();
        }

        private void Send(object sender, DoWorkEventArgs e) {
            try {
                sema.WaitOne();
                object o = e.Argument;
                msg.write_int((int)((command)o).type);

                switch(((command)o).type) {
                    case cmdType.Login:
                        login l = (login)o;
                        Crypter.Blowfish.
                        msg.write_string(l.username);
                        msg.write_string(l.password);
                        break;
                    case cmdType.Disconnect:
                        break;
                    case cmdType.Ping:
                        break;
                }

                msg.send_data();
                sema.Release();
                e.Result = true;
            } catch {
                sema.Release();
                e.Result = false;
            }
        }
        private void Send_Completed(object sender, RunWorkerCompletedEventArgs e) {
            if(!e.Cancelled && e.Error == null && ((bool)e.Result))
                OnCommandSent(new EventArgs());
            else
                OnCommandFailed(new EventArgs());

            ((BackgroundWorker)sender).Dispose();
            GC.Collect();
        }

        private void Connect(object sender, DoWorkEventArgs e) {
            try {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                socket.Connect(server_ep);

                stream = new NetworkStream(socket);

                msg = new Message(stream);

                recv_bw = new BackgroundWorker();
                recv_bw.WorkerSupportsCancellation = true;
                recv_bw.DoWork += new DoWorkEventHandler(Recv);
                recv_bw.RunWorkerAsync();

                send_bw = new BackgroundWorker();
                send_bw.WorkerSupportsCancellation = true;
                send_bw.DoWork += new DoWorkEventHandler(Send);
                send_bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Send_Completed);

                e.Result = true;
            } catch {
                e.Result = false;
            }
        }
        private void Connect_Completed(object sender, RunWorkerCompletedEventArgs e) {
            if(!((bool)e.Result))
                OnConnectingFailed(new EventArgs());
            else
                OnConnectingSuccessed(new EventArgs());

            ((BackgroundWorker)sender).Dispose();
        }
        #endregion

        #region Public Methods
        public void Connect() {
            connector_bw.RunWorkerAsync();
        }

        public void Send_Command(object o) {
            if(socket != null && socket.Connected) {
                send_bw.RunWorkerAsync(o);
            } else {
                OnCommandFailed(new EventArgs());
            }
        }

        public bool Disconnect() {
            if(socket != null && socket.Connected) {
                try {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    recv_bw.CancelAsync();
                    OnDisconnectedFromServer(new EventArgs());
                    return true;
                } catch {
                    return false;
                }

            } else
                return true;
        }
        #endregion

        #region Events
        public event CommandReceivedEventHandler CommandReceived;
        protected virtual void OnCommandReceived(CommandEventArgs e) {
            if(CommandReceived != null) {
                CommandReceived(this, e);
            }
        }

        public event CommandSentEventHandler CommandSent;
        protected virtual void OnCommandSent(EventArgs e) {
            if(CommandSent != null) {
                CommandSent(this, e);
            }
        }

        public event CommandSendingFailedEventHandler CommandFailed;
        protected virtual void OnCommandFailed(EventArgs e) {
            if(CommandFailed != null) {
                CommandFailed(this, e);
            }
        }

        public event ServerDisconnectedEventHandler ServerDisconnected;
        protected virtual void OnServerDisconnected(ServerEventArgs e) {
            if(ServerDisconnected != null) {
                ServerDisconnected(this, e);
            }
        }

        public event DisconnectedEventHandler DisconnectedFromServer;
        protected virtual void OnDisconnectedFromServer(EventArgs e) {
            if(DisconnectedFromServer != null) {
                DisconnectedFromServer(this, e);
            }
        }

        public event ConnectingSuccessedEventHandler ConnectingSuccessed;
        protected virtual void OnConnectingSuccessed(EventArgs e) {
            if(ConnectingSuccessed != null) {
                ConnectingSuccessed(this, e);
            }
        }

        public event ConnectingFailedEventHandler ConnectingFailed;
        protected virtual void OnConnectingFailed(EventArgs e) {
            if(ConnectingFailed != null) {
                ConnectingFailed(this, e);
            }
        }

        public event NetworkDeadEventHandler NetworkDead;
        protected virtual void OnNetworkDead(EventArgs e) {
            if(NetworkDead != null) {
                NetworkDead(this, e);
            }
        }

        public event NetworkAlivedEventHandler NetworkAlived;
        protected virtual void OnNetworkAlived(EventArgs e) {
            if(NetworkAlived != null) {
                NetworkAlived(this, e);
            }
        }
        #endregion
    }
}
