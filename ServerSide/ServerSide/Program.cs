using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerSide
{    
    class Program
    {
        static string server_ip = "127.0.0.1";
        public Computer This_pc;
        static void Main(string[] args)
        {

            TcpClient New_client = new TcpClient();
            // Присоединяемся к "клиенту"
            // Клиент - тот кто принимает данные
            // и ведёт статистику
            New_client.Connect(server_ip, 4254);
            NetworkStream Net_stream = New_client.GetStream();

            // А потом повторяем
            while (true)
            {
                // Обновляем поле
                var This_pc = new Computer();

                //Console.WriteLine(This_pc.cpu_load);
                //byte[] data = This_pc.power_on.GetBytes();

                using (var memoryStream = new MemoryStream())
                {
                    // Переделываем в формат для передачи по сети
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(memoryStream, This_pc);
                    var bytes = memoryStream.ToArray();
                    // Передаём
                    Net_stream.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
                    Net_stream.Write(bytes, 0, bytes.Length);
                }

                Thread.Sleep(1000);
            }
        }
    }
}
