using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {

            if (!File.Exists("log.txt"))
            {
                File.CreateText("log.txt");
            }
            Server s = new Server(9999);
           
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Title = "Server";
            Console.WriteLine("Server status: ON.\nDon't forget to reset the Config.txt on the Client's file to {0}\nWaiting for connection...", s.Ip);
            s.Start();
        }
    }
}
