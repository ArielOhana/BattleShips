using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace Admin
{
    public class ServerHandler
    {
        /// <summary>
        /// Server Handler stands for creating a connection with the server by using ServerPort port number and the IP in the Config file.
        /// Server Handler also stands for recieving and sending strings between the server and the client.
        /// </summary>
        public static TcpClient c = new TcpClient();

        /// <summary>
        /// Builder Function : The function use the Config file to get the IP and also Server Port number to create a connection between the client to the server.
        /// </summary>
        public ServerHandler()
        {
            string IP;
            int ServerPort = 7777;
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
        /// <summary>
        /// The function recieves a message from the server.
        /// </summary>
        /// <returns>Returns the message which recieved from the server</returns>
        public string ReadThread() 
        {
            byte[] data = new byte[1024];
            int len = c.GetStream().Read(data, 0, data.Length);
            string msg = Encoding.UTF8.GetString(data, 0, len);
            return msg;

        }
        /// <summary>
        /// The function recieves a message from the client and sends it to the server.
        /// </summary>
        /// <param name="msg"> the message the client sends to the server</param>
        public void WriteThread(string msg)
        {

            byte[] data = Encoding.UTF8.GetBytes(msg);
            c.GetStream().Write(data, 0, data.Length);
            c.GetStream().Flush();

        }
        /// <summary>
        /// The function closes the transmission with the server.
        /// </summary>
        public void Close() 
        {
            //close socket
            c.Close();

        }
    }
}

