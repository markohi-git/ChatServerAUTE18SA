using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServerAUTE18SA
{
    class Program
    {
        static Dictionary<int, TcpClient> list_clients = new Dictionary<int, TcpClient>();
        static int count = 1;
        static void Main(string[] args)
        {
            TcpListener ServerSocket = new TcpListener(IPAddress.Any, 5000);
            ServerSocket.Start();
            while (true)
            {
                TcpClient client = ServerSocket.AcceptTcpClient();
                list_clients.Add(count, client);
                
                Console.WriteLine("user "+count+" connected");
                broadcast("user " + count + " connected");
                Thread t = new Thread(handle_clients);
                t.Start(count);

                count++;
            }
        }
        public static void handle_clients(object o)
        {
            int id = (int)o;
            TcpClient client = list_clients[id];
            while (true)
            {
                NetworkStream stream = client.GetStream();
                //taulukko tavujen tallentamista varten
                byte[] buffer = new byte[1024];
                //luetaan streamista taulukkoon tavaut
                int byte_count = stream.Read(buffer, 0, buffer.Length);
                //muunnetaan tavutaulukko merkkijonoksi
                string data = Encoding.UTF8.GetString(buffer, 0, byte_count).TrimEnd('\r','\n');
                
                if (data.Contains("exit"))
                {
                    break;
                }
                if (data.Length > 0)
                {
                    broadcast("user "+id+": "+data);
                }
                Console.WriteLine("user " + id + ": " + data);
            }
            broadcast("user " + id + " disconnected");
            list_clients.Remove(id);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
        //metodi jolla lähetätetään merkkijono kaikille asiakkaille
        public static void broadcast(string data)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(data+Environment.NewLine);
            foreach (TcpClient c in list_clients.Values)
            {
                NetworkStream stream = c.GetStream();
                stream.Write(buffer, 0, buffer.Length);
            }
        }
    }
}
