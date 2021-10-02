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
        private Thread recv_t;
        private Thread isalive_t;
        private Semaphore sema = new Semaphore(1, 1);
        #endregion

        #region Constructor
        public Client(Socket soc) {
            this.socket = soc;

            this.IP = ((IPEndPoint)this.socket.RemoteEndPoint).Address;
            this.Port = ((IPEndPoint)this.socket.RemoteEndPoint).Port;

            this.stream = new NetworkStream(this.socket);

            this.msg = new Message(stream);

            this.recv_t = new Thread(() => Recv());
            this.recv_t.Start();

            this.isalive_t = new Thread(() => is_alive_t());
            this.isalive_t.Start();
        }
        #endregion

        #region Private Methods
        private bool is_alive = true;
        private void is_alive_t() {
            while(is_alive) {
                if(this.socket.Poll(1000, SelectMode.SelectRead) && this.socket.Available == 0) {
                    is_alive = false;
                    break;
                } else {
                    Thread.Sleep(120000);
                }
            }
        }
        private void Recv() {
            while(is_alive) {
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
            this.Disconnect();
        }

        private void Send(object send_obj) {
            if(send_obj != null) {
                try {
                    sema.WaitOne();
                    msg.write_int((int)((command)send_obj).type);

                    switch(((command)send_obj).type) {
                        case cmdType.Auth:
                            auth a = (auth)send_obj;
                            msg.write_int((int)a.info.code);

                            if(a.info.code != Auth_Codes.AuthSuccess) {
                                msg.send_data();
                                sema.Release();

                                this.OnCommandSent(new CommandEventArgs((auth)send_obj));
                                break;
                            }

                            foreach(uint i in Settings.inst.addrs) {
                                msg.write_uint(i);
                            }

                            msg.write_float(Settings.inst.menu_size);

                            msg.send_data();
                            sema.Release();

                            this.OnCommandSent(new CommandEventArgs((auth)send_obj));
                            break;
                    }
                } catch(Exception ex) {
                    Logger.inst.Error(ex.ToString());
                    sema.Release();
                    this.OnCommandFailed(new EventArgs());
                }
                send_obj = null;
            }
            GC.Collect();
        }
        #endregion

        #region Public Methods
        public void Send_Command(object o) {
            if(this.socket != null && this.socket.Connected) {
                Send(o);
            } else {
                this.OnCommandFailed(new EventArgs());
            }
        }
        public bool Disconnect() {
            if(this.socket != null && this.socket.Connected) {
                try {
                    if(is_alive)
                        is_alive = false;

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
            if(Disconnected != null)
                Disconnected(this, e);
        }
        #endregion
    }
}
