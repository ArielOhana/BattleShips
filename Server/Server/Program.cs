using System;
using System.IO;
namespace Server
{
    class Program
    {
        /// <summary>
        /// This class stands for checking all the files that should to be are there, else it creates them
        /// This class also stands for running the server.
        /// </summary>
        static void Main(string[] args)
        {
            DateTime now = DateTime.Now;
            Server s = new Server(7777);

            if (!File.Exists("chat.txt"))
            {
                File.CreateText("chat.txt");
            }
            if (!File.Exists("log.txt"))
            {
                File.CreateText("log.txt");
            }
            if (!File.Exists("database.txt"))
            {
                File.CreateText("database.txt");
            }          
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Title = "Server";
            Console.WriteLine("Server status: ON.\nDon't forget to reset the Config.txt on the Client's file to {0}\nWaiting for connection...", s.Ip);
    
            using (StreamWriter sw = new StreamWriter("log.txt", true)) // Saves "towrite" on the log file.
            {
                sw.WriteLine("Restarts the server at: " + now.ToString() + "\nIP: " + s.Ip );
                sw.Flush();
            }
            s.Start();
        }
    }
}
