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
        static NetworkStream Connect(string ip, int port)
        {
            TcpClient newClient = new TcpClient();
            while (!newClient.Connected)
            {
                Console.WriteLine("Connecting to {0}:{1}...", ip, port);
                try
                {
                    newClient.Connect(ip, port);
                }
                catch
                {
                    Console.WriteLine("Server not found. Trying again...");
                }
                Thread.Sleep(5000);
            }
            Console.WriteLine("Connected!");
            return newClient.GetStream();
        }
 
        static void Main(string[] args)
        {
            string serverIP = "127.0.0.1";
            int serverPort = 4254;
            if (args.Length == 2)
            {
                serverIP = args[0];
                serverPort = Int32.Parse(args[1]);
            }
            NetworkStream netStream = Connect(serverIP, serverPort);
            var cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            var uptime = new PerformanceCounter("System", "System Up Time");
            var ram = new PerformanceCounter("Memory", "Available MBytes");
            cpu.NextValue();
            uptime.NextValue();
            ram.NextValue();

            var thisPC = new Info();
            thisPC.MachineName = Environment.MachineName;
            thisPC.SystemType = Environment.OSVersion.VersionString;
            // А потом повторяем
            while (true)
            {
                // Обновляем поля
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
                        netStream = Connect(serverIP, serverPort);
                    }
                }

                Thread.Sleep(500);
            }
        }
    }
}
