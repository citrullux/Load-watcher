using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Diagnostics;
using OpenSSL.SSL;
using OpenSSL.Crypto;
using OpenSSL.Core;
using System.Management;
using System.Collections.Generic;

namespace ClientApp
{
    class Program
    {
        public static RSA rsa;
        public static int len = 4096;
        public static BIO bio = new BIO(new byte[len]);
        public static PasswordHandler pass;
        
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

        public static string GetHardwareInfo(string WIN32_Class, string ClassItemField)
        {
            List<string> result = new List<string>();

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM " + WIN32_Class);

            try
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    result.Add(obj[ClassItemField].ToString().Trim());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result.ToString();
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
            
            // Передача открытого ключа от сервера
            byte[] buf = new byte[4];
            try
            {
                netStream.Read(buf, 0, 4);
            }
            catch
            {
                Console.WriteLine("Key size missing.");
            }
            var size = BitConverter.ToInt32(buf, 0);
            var serverKey = new byte[size];
            try
            {
                netStream.Read(serverKey, 0, serverKey.Length);
            }
            catch
            {
                Console.WriteLine("Key missing.");
            }
            BIO pubkey = new BIO(serverKey);
            rsa = RSA.FromPublicKey(pubkey);
            // А достаточно нам его будет, так как мы только отправляем информацию
            // соответственно сервер должен будет пользоваться только своим закрытым ключом.


            var cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            var uptime = new PerformanceCounter("System", "System Up Time");
            var ram = new PerformanceCounter("Memory", "Available MBytes");
            cpu.NextValue();
            uptime.NextValue();
            ram.NextValue();


            //var manufacter = new PerformanceCounter("Processor", "Name");
            //manufacter.NextValue();
            //var cpuname = new PerformanceCounter("Processor", "Name");
            //cpuname.NextValue();
            string cpuname = "";
            string manufacter = "";


            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM " + "Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                cpuname = obj["Name"].ToString().Trim();
                manufacter = obj["Manufacturer"].ToString().Trim();
            }
            
            //var cpuname = GetHardwareInfo("Win32_Processor", "Name");
            //var manufacter = GetHardwareInfo("Win32_Processor", "Manufacturer");


            //var thisPC = new Info();
            //thisPC.MachineName = Environment.MachineName;
            //thisPC.SystemType = Environment.OSVersion.VersionString;

            // А потом повторяем
            while (true)
            {
                // Обновляем поля
                //thisPC.Uptime = TimeSpan.FromSeconds(uptime.NextValue()).ToString(@"%d' д. 'hh\:mm\:ss");
                //thisPC.CpuLoad = (int)cpu.NextValue();
                //thisPC.RamLoad = (int)ram.NextValue();

                // Приводим к общему формату(Кроссплатформенность!)

                string[] many = { "{", "\"<MachineName>k__BackingField\"", ":\"", Environment.MachineName.ToString(), "\",",
                    "\"<VendorId>k__BackingField\"", ":\"", manufacter.ToString(), "\",",
                    "\"<ModelName>k__BackingField\"", ":\"", cpuname.ToString(), "\",",
                    "\"<SystemType>k__BackingField\"", ":\"", Environment.OSVersion.VersionString, "\",",
                    "\"<Uptime>k__BackingField\"", ":\"", uptime.NextValue().ToString(),"\",",
                    "\"<CpuLoad>k__BackingField\"", ":\"", ((int)cpu.NextValue()).ToString(), "\",",
                    "\"<RamLoad>k__BackingField\"", ":\"", ((int)ram.NextValue()).ToString(), "\"}"
                };

                string thisPC = string.Concat(many);
                //Console.WriteLine(thisPC);

                //Console.WriteLine(This_pc.cpu_load);
                //byte[] data = This_pc.power_on.GetBytes();

                using (var memoryStream = new MemoryStream())
                {
                    // Переделываем в формат для передачи по сети
                    
                    var bytes = new MemoryStream(Encoding.UTF8.GetBytes(thisPC ?? "")).ToArray();
                    var crypedBytes = rsa.PublicEncrypt(bytes, RSA.Padding.PKCS1);
                    // Передаём
                    try
                    {
                        netStream.Write(BitConverter.GetBytes(crypedBytes.Length), 0, 4);
                        netStream.Write(crypedBytes, 0, crypedBytes.Length);
                    }
                    catch
                    {
                        netStream = Connect(serverIP, serverPort);
                    }
                }

                Thread.Sleep(1000);
            }
        }


    }


}
