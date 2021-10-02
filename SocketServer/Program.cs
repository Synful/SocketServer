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
using SocketServer.PS3;
using SocketServer.PC;

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

        static void Main(string[] args) {
            Program.inst.StartServer();
        }
        void StartServer() {
            MenuBase mb = MenuBase.inst;
            Database db = Database.inst;
            Settings s = Settings.inst;

            Main_PS3 ps3 = Main_PS3.inst;
            Main_PC pc = Main_PC.inst;
        }
    }
}
