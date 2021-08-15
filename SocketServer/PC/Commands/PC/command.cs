using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.Commands.PC {
    public class command {
        public cmdType type { get; set; }
        public IPAddress client_ip { get; set; }

        public enum cmdType : int {
            Unknown,
            Auth
        }

        public command(cmdType type, IPAddress ip) {
            this.type = type;
            this.client_ip = ip;
        }
    }
}
