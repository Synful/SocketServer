using MySql.Data.MySqlClient;
using SocketServer.PS3.Commands;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SocketServer.PS3.Commands.command;
using static SocketServer.PS3.ViewModels.Database;

namespace SocketServer.PS3.ViewModels {
    public class ClientInfo {
        public Client user;

        public string name { get; set; }
        public IPAddress ip { get; set; }
        public string lic { get; set; }
        public string mac { get; set; }
        public string psid { get; set; }
        public string checksum { get; set; }
        public string version { get; set; }
        public Auth_Codes code { get; set; }
        public bool reauth { get; set; }

        public bool banned { get; set; }
        public double enddate { get; set; }
        public bool setlock { get; set; }
        public string dbmac { get; set; }
        public string dbpsid { get; set; }

        public ClientInfo(Client c, IPAddress ip, string l, string m, string p, string cs, string ver, bool r) {
            user = c;
            this.ip = ip;
            lic = l;
            mac = m;
            psid = p;
            checksum = cs;
            version = ver;
            reauth = r;
        }
    }

    public class Client {
        #region Vars
        public ClientInfo info;

        public IPAddress IP;
        public int Port;

        private Socket socket;
        private NetworkStream stream;
        private Message msg;
        private BackgroundWorker recv_bw;
        private BackgroundWorker send_bw;
        private Semaphore sema = new Semaphore(1, 1);
        #endregion

        #region Constructor
        public Client(Socket soc) {
            this.socket = soc;

            this.IP = ((IPEndPoint)this.socket.RemoteEndPoint).Address;
            this.Port = ((IPEndPoint)this.socket.RemoteEndPoint).Port;

            this.stream = new NetworkStream(this.socket);

            this.msg = new Message(stream);

            this.recv_bw = new BackgroundWorker();
            this.recv_bw.DoWork += new DoWorkEventHandler(Recv);
            this.recv_bw.RunWorkerAsync();

            this.send_bw = new BackgroundWorker();
            this.send_bw.DoWork += new DoWorkEventHandler(Send);
            this.send_bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Send_Completed);
        }
        #endregion

        #region Private Methods
        private void Recv(object sender, DoWorkEventArgs e) {
            while(this.socket.Connected && this.socket.RemoteEndPoint != null) {
                try {
                    msg.load_data();
                    cmdType cType = (cmdType)msg.read_int();

                    switch(cType) {
                        case cmdType.Auth:
                            string lic = msg.read_string();
                            string mac = msg.read_string();
                            string psid = msg.read_string();
                            string cs = msg.read_string();
                            string version = msg.read_string();
                            bool reauth = msg.read_bool();

                            info = new ClientInfo(this, this.IP, lic, mac, psid, cs, version, reauth);

                            this.OnCommandReceived(new CommandEventArgs(new auth(IP, info)));
                            break;
                        case cmdType.Disconnect:
                            this.OnCommandReceived(new CommandEventArgs(new command(cmdType.Disconnect, IP)));
                            break;
                    }
                } catch (Exception ex) {
                   // Logger.inst.Error(ex.ToString());
                }
            }
            this.OnDisconnected(new ClientEventArgs(this.IP, this.Port));
        }

        private void Send(object sender, DoWorkEventArgs e) {
            try {
                sema.WaitOne();
                object o = e.Argument;
                //Type
                msg.write_int((int)((command)o).type);

                switch(((command)o).type) {
                    case cmdType.Auth:
                        auth a = (auth)o;
                        msg.write_int((int)a.info.code);

                        if(a.info.code != Auth_Codes.AuthSuccess)
                            break;

                        foreach(uint i in Settings.instance.addrs) {
                            msg.write_uint(i);
                        }

                        msg.write_float(Settings.instance.menu_size);

                        break;
                }
                msg.send_data();
                sema.Release();
                e.Result = e.Argument;
            } catch (Exception ex) {
                Logger.inst.Error(ex.ToString());
                sema.Release();
                e.Result = null;
            }
        }
        private void Send_Completed(object sender, RunWorkerCompletedEventArgs e) {
            if(!e.Cancelled && e.Error == null && (e.Result != null)) {
                switch(((command)e.Result).type) {
                    case cmdType.Auth:
                        this.OnCommandSent(new CommandEventArgs((auth)e.Result));
                        break;
                    default:
                        this.OnCommandSent(new CommandEventArgs((command)e.Result));
                        break;
                }
            } else
                this.OnCommandFailed(new EventArgs());

            ((BackgroundWorker)sender).Dispose();
            GC.Collect();
        }
        #endregion

        #region Public Methods
        public void Send_Command(object o) {
            if(this.socket != null && this.socket.Connected) {
                send_bw.RunWorkerAsync(o);
            } else {
                this.OnCommandFailed(new EventArgs());
            }
        }
        public bool Disconnect() {
            if(this.socket != null && this.socket.Connected) {
                try {
                    this.socket.Shutdown(SocketShutdown.Both);
                    this.socket.Close();
                    return true;
                } catch (Exception ex) {
                    Logger.inst.Error(ex.ToString());
                    return false;
                }
            } else {
                return true;
            }
        }
        #endregion

        #region Events
        public event CommandReceivedEventHandler CommandReceived;
        protected virtual void OnCommandReceived(CommandEventArgs e) {
            if(CommandReceived != null)
                CommandReceived(this, e);
        }

        public event CommandSentEventHandler CommandSent;
        protected virtual void OnCommandSent(CommandEventArgs e) {
            if(CommandSent != null)
                CommandSent(this, e);
        }

        public event CommandSendingFailedEventHandler CommandFailed;
        protected virtual void OnCommandFailed(EventArgs e) {
            if(CommandFailed != null)
                CommandFailed(this, e);
        }

        public event DisconnectedEventHandler Disconnected;
        protected virtual void OnDisconnected(ClientEventArgs e) {
            if(Disconnected != null) {
                Disconnected(this, e);
            } else {
                this.Disconnect();
            }
        }
        #endregion
    }
}
