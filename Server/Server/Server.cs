using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
namespace Server
{
    class Server
    {
        private TcpListener listener;
        private IPAddress ip;
        private int port;

        ~Server()
        {
            listener.Stop();
        }

        public TcpListener Listener
        {
            get
            {
                return listener;
            }

            set
            {
                listener = value;
            }
        }



        public IPAddress Ip
        {
            get
            {
                return ip;
            }

            set
            {
                ip = value;
            }
        }

        public int Port
        {
            get
            {
                return port;
            }

            set
            {
                port = value;
            }
        }

        public  Server(int port)
        {
            var host = Dns.GetHostName();
            this.ip = IPAddress.Parse(Dns.GetHostByName(host).AddressList[0].ToString());
            this.port = port;
            this.listener = new TcpListener(ip, this.port);

        }

        public void Start()
        {
            listener.Start();

            while (true)
            {
                var newClient = listener.AcceptTcpClient();
                ClientHandler ch = new ClientHandler(newClient);
                ClientHandler.clntList.Add(ch);

            }
        }




    }
}
