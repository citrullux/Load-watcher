using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerSide
{    
    class Program
    {
        static string server_ip = "192.168.0.3";
        public Computer This_pc;
        static void Main(string[] args)
        {
            try
            {
                TcpClient New_client = new TcpClient();
                // Присоединяемся к "клиенту"
                // Клиент - тот кто принимает данные
                // и ведёт статистику
                New_client.Connect(server_ip, 4254);
                NetworkStream Net_stream = New_client.GetStream();
                int data = 1;
                //Net_stream.Write(data,0,data.Length);
            }
            catch(SocketException ex)
            {
                Console.WriteLine("Exception: " + ex.ToString());
            }

        }
    }
}
