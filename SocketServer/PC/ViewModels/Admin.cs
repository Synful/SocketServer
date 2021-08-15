using SocketServer.PC.Commands;
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
using static SocketServer.PC.Commands.command;

namespace SocketServer.PC.ViewModels {
    public class AdminInfo {
        public Admin user;

        public string name { get; set; }
        public string pin { get; set; }
        public IPAddress ip { get; set; }

        public AdminInfo(Admin c, string n, string p, IPAddress ip) {
            user = c;
            name = n;
            pin = p;
            this.ip = ip;
        }
    }

    public class Admin {
        #region Vars
        public AdminInfo info;

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
        public Admin(Socket soc) {
            //this.socket = soc;

            //this.IP = ((IPEndPoint)this.socket.RemoteEndPoint).Address;
            //this.Port = ((IPEndPoint)this.socket.RemoteEndPoint).Port;

            //this.stream = new NetworkStream(this.socket);

            //this.msg = new Message(stream);

            //this.recv_bw = new BackgroundWorker();
            //this.recv_bw.DoWork += new DoWorkEventHandler(Recv);
            //this.recv_bw.RunWorkerAsync();

            //this.send_bw = new BackgroundWorker();
            //this.send_bw.DoWork += new DoWorkEventHandler(Send);
            //this.send_bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Send_Completed);
        }
        #endregion

        #region Private Methods
        private void Recv(object sender, DoWorkEventArgs e) {
            //while(this.socket.Connected && this.socket.RemoteEndPoint != null) {
            //    try {
            //        cmdType cType = (cmdType)msg.read_int();

            //        switch(cType) {
            //            case cmdType.Auth:
            //                string username = msg.read_string(msg.read_int());
            //                string pin = msg.read_string(4);

            //                info = new AdminInfo(this, username, pin, this.IP);

            //                break;
            //        }
            //    } catch { }
            //}
            //this.OnDisconnected(new ClientEventArgs(this.IP, this.Port));
        }

        private void Send(object sender, DoWorkEventArgs e) {
            //try {
            //    sema.WaitOne();
            //    object o = e.Argument;
            //    //Type
            //    msg.write_int((int)((command)o).type);

            //    switch(((command)o).type) {
            //        case cmdType.Auth:
            //            auth a = (auth)o;
            //            msg.write_int((int)a.info.code);

            //            if(a.info.code > 0)
            //                break;

            //            //Start Writing Addreses
            //            //foreach(int i in Settings.instance.addrs) {
            //            //    msg.write_int(i);
            //            //}

            //            break;
            //    }
            //    msg.send_data();
            //    sema.Release();
            //    e.Result = e.Argument;
            //} catch {
            //    sema.Release();
            //    e.Result = null;
            //}
        }
        private void Send_Completed(object sender, RunWorkerCompletedEventArgs e) {
            //if(!e.Cancelled && e.Error == null && (e.Result != null)) {
            //    switch(((command)e.Result).type) {
            //        case cmdType.Auth:
            //            this.OnCommandSent(new CommandEventArgs((auth)e.Result));
            //            break;
            //        default:
            //            this.OnCommandSent(new CommandEventArgs((command)e.Result));
            //            break;
            //    }
            //} else
            //    this.OnCommandFailed(new EventArgs());

            //((BackgroundWorker)sender).Dispose();
            //GC.Collect();
        }
        #endregion

        #region Public Methods
        public void Send_Command(object o) {
            //if(this.socket != null && this.socket.Connected) {
            //    send_bw.RunWorkerAsync(o);
            //} else {
            //    this.OnCommandFailed(new EventArgs());
            //}
        }
        public bool Disconnect() {
            return false;
            //if(this.socket != null && this.socket.Connected) {
            //    try {
            //        this.socket.Shutdown(SocketShutdown.Both);
            //        this.socket.Close();
            //        return true;
            //    } catch {
            //        return false;
            //    }
            //} else {
            //    return true;
            //}
        }
        #endregion

        #region Events
        //public event CommandReceivedEventHandler CommandReceived;
        //protected virtual void OnCommandReceived(CommandEventArgs e) {
        //    if(CommandReceived != null)
        //        CommandReceived(this, e);
        //}

        //public event CommandSentEventHandler CommandSent;
        //protected virtual void OnCommandSent(CommandEventArgs e) {
        //    if(CommandSent != null)
        //        CommandSent(this, e);
        //}

        //public event CommandSendingFailedEventHandler CommandFailed;
        //protected virtual void OnCommandFailed(EventArgs e) {
        //    if(CommandFailed != null)
        //        CommandFailed(this, e);
        //}

        //public event DisconnectedEventHandler Disconnected;
        //protected virtual void OnDisconnected(ClientEventArgs e) {
        //    if(Disconnected != null) {
        //        Disconnected(this, e);
        //    } else {
        //        this.Disconnect();
        //    }
        //}
        #endregion
    }
}
