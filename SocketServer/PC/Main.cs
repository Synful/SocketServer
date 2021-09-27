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
            ep = new IPEndPoint(IPAddress.Parse("192.168.1.8"), 38426);
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

        }

        public void Close() {
            if(admin_listener_t != null && admin_listener_t.ThreadState == ThreadState.Running) {
                if(admins != null) {
                    foreach(Admin a in admins) {
                        a.Disconnect();
                    }
                    admin_listener.Close();
                }
                GC.Collect();
            }
        }
        #endregion

    }
}
