using SocketServer.PS3.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.PS3.Commands {
    public class auth : command {
        public ClientInfo info;

        public auth(IPAddress ip, ClientInfo i) : base(cmdType.Auth, ip) {
            info = i;
        }
    }
}
