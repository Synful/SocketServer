using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SocketAdmin.Commands {
    public class ping : command {
        public ping() : base(cmdType.Ping) {

        }
    }
}
