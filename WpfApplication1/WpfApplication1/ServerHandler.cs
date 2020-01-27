using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
namespace WpfApplication1
{
    public class ServerHandler
    {
        public static TcpClient c = new TcpClient();

        public ServerHandler()
        {
            string IP;
            int ServerPort = 9999;
            if (File.Exists("Config.txt"))
            {
                IP = File.ReadAllText("Config.txt");
            }
            else
            {
                IP = "127.0.0.1";
            }
            c.Connect(IPAddress.Parse(IP), ServerPort);
            
      
        }
        public string ReadThread()
        { 
                byte[] data = new byte[1024];
                int len = c.GetStream().Read(data, 0, data.Length);
                string msg = Encoding.UTF8.GetString(data, 0, len);
            return msg;
            
        }
        public void WriteThread(string msg)
        { 
                
                byte[] data = Encoding.UTF8.GetBytes(msg);
                c.GetStream().Write(data, 0, data.Length);
                c.GetStream().Flush();
            
        }

        public void Close()
        {
            //close socket
            c.Close();
            
        }
    }
}

