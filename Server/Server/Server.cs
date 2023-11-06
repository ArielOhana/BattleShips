using System.Net;
using System.Net.Sockets;
namespace Server
{
    class Server
    {
        /// <summary>
        /// This class creates the server and sets it able to get more clients.
        /// </summary>
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
        /// <summary>
        ///  BUILDER - creates the server by the IP address of the computer and the given port.
        /// </summary>
        /// <param name="port"> the PORT number</param>
        public Server(int port)
        {
            var host = Dns.GetHostName();
            this.ip = IPAddress.Parse(Dns.GetHostByName(host).AddressList[0].ToString());
            this.port = port;
            this.listener = new TcpListener(ip, this.port);

        }
        /// <summary>
        /// This function recieves clients and creates a Clienthandler to handle the client.
        /// </summary>
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
