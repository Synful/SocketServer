using SocketServer.PC.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.PC.Commands {
    public class auth : command {
        public AdminInfo info;

        public auth(IPAddress ip, AdminInfo i) : base(cmdType.Auth, ip) {
            info = i;
        }
    }
}
