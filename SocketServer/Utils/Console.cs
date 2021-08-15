using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.Utils {
    public static class Console {
        public static void write(string s) {
            System.Console.Write(s);
        }
        public static void write(string s, ConsoleColor c) {
            System.Console.ForegroundColor = c;
            System.Console.Write(s);
            System.Console.ResetColor();
        }

        public static void writeline(string s) {
            System.Console.WriteLine(s);
        }
        public static void writeline(string s, ConsoleColor c) {
            System.Console.ForegroundColor = c;
            System.Console.WriteLine(s);
            System.Console.ResetColor();
        }

        public static void writetitle(string s) {
            write("====[ ", ConsoleColor.Gray);
            write($"{s}", ConsoleColor.Cyan);
            write(" ]====\n", ConsoleColor.Gray);
        }
        public static void writecommand(string name, string desc) {
            write($"{name}", ConsoleColor.Cyan);
            write(" - ", ConsoleColor.Gray);
            write($"{desc}\n", ConsoleColor.Cyan);
        }

        public static void clear() {
            System.Console.Clear();
        }
    }
}
