using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Computer;
using System.Diagnostics;

namespace ServerSide
{
    class Program
    {
        static string serverIP = "127.0.0.1";

        static NetworkStream Connect()
        {
            TcpClient newClient = new TcpClient();
            // Присоединяемся к "клиенту"
            // Клиент - тот кто принимает данные
            // и ведёт статистику
            while (!newClient.Connected)
            {
                try
                {
                    newClient.Connect(serverIP, 4254);
                }
                catch
                {
                    Console.WriteLine("Server not found. Trying again...");
                }
                Thread.Sleep(5000);
            }
            return newClient.GetStream();
        }
 
        static void Main(string[] args)
        {
            NetworkStream netStream = Connect();
            var cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            var uptime = new PerformanceCounter("System", "System Up Time");
            var ram = new PerformanceCounter("Memory", "Available MBytes");
            cpu.NextValue();
            uptime.NextValue();
            ram.NextValue();
            // А потом повторяем
            while (true)
            {
                // Обновляем поле
                var thisPC = new Info();
                thisPC.MachineName = "localhost";
                thisPC.SystemType = "Windows";

                thisPC.Uptime = TimeSpan.FromSeconds(uptime.NextValue()).ToString(@"%d' д. 'hh\:mm\:ss");
                thisPC.CpuLoad = (int)cpu.NextValue();
                thisPC.RamLoad = (int)ram.NextValue();

                //Console.WriteLine(This_pc.cpu_load);
                //byte[] data = This_pc.power_on.GetBytes();

                using (var memoryStream = new MemoryStream())
                {
                    // Переделываем в формат для передачи по сети
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(memoryStream, thisPC);
                    var bytes = memoryStream.ToArray();
                    // Передаём
                    try
                    {
                        netStream.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
                        netStream.Write(bytes, 0, bytes.Length);
                    }
                    catch
                    {
                        netStream = Connect();
                    }
                }

                Thread.Sleep(500);
            }
        }
    }
}
