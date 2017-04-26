using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using Computer;
using System.Runtime.Serialization.Json;
using OpenSSL.SSL;
using OpenSSL.Crypto;
using OpenSSL.Core;
using System.Text;

namespace ServerApp
{
    public partial class ClientList : Form
    {
        // This integer variable keeps track of the 
        // remaining time.
        int timeLeft;
        public DataContractJsonSerializer ser;
        public List<TcpClient> tcpClients;
        private BindingList<Info> list;
        public TcpListener server;
        //public RSA RSA;
        public IPAddress ipAddr = IPAddress.Parse("0.0.0.0");
        public RSA rsa = new RSA();
        public const int len = 4096;
        public BIO bio = new BIO(new byte[2*len]);
        public ClientList()
        {
            InitializeComponent();
            list = new BindingList<Info>();
            //Привязки
            grid.DataSource = list;
            tcpClients = new List<TcpClient>();
            server = new TcpListener(ipAddr, 4254);
            ser = new DataContractJsonSerializer(typeof(Info));
            //NetworkStream In_stream = server.AcceptTcpClient
            server.Start();
            rsa.GenerateKeys(len, 3, null, null);
        }

        private void Update_timer_Tick(object sender, EventArgs e)
        {

            if (timeLeft > 0)
            {
                // Display the new time left
                // by updating the Time Left label.
                timeLeft = timeLeft - 1;
            }
            else
            {
                while (server.Pending())
                {
                    var client = server.AcceptTcpClient();
                    tcpClients.Add(client);
                    // Отправка публичного ключа
                    var netStream = client.GetStream();
                    var keyBytes = Encoding.ASCII.GetBytes(rsa.PublicKeyAsPEM);
                    netStream.Write(BitConverter.GetBytes(keyBytes.Length), 0, 4);
                    netStream.Write(keyBytes, 0, keyBytes.Length);
                }
                // If the user ran out of time, stop the timer, show
                // a MessageBox, and fill in the answers.
                Update_timer.Stop();
                list.Clear();
                //list.Add();
                using (var memoryStream = new MemoryStream())
                {
                    foreach (var client in tcpClients.ToArray())
                    {
                        if (!client.Connected)
                        {
                            tcpClients.Remove(client);
                            continue;
                        }
                        byte[] buf = new byte[4];
                        try
                        {
                            client.GetStream().Read(buf, 0, 4);
                        }
                        catch
                        {
                            continue;
                        }
                        var size = BitConverter.ToInt32(buf, 0);
                        var bytes = new byte[size];
                        try
                        {
                            client.GetStream().Read(bytes, 0, bytes.Length);
                        }
                        catch
                        {
                            continue;
                        }
                        if (bytes.Length > 0)
                        {
                            var bytesDecryped = rsa.PrivateDecrypt(bytes, RSA.Padding.PKCS1);
                            var ms = new MemoryStream(bytesDecryped);

                            var remoteComputer = (Info)ser.ReadObject(ms);
                            remoteComputer.Uptime = TimeSpan.FromSeconds(Double.Parse(remoteComputer.Uptime)).ToString(@"%d' д 'hh\:mm\:ss");
                            list.Add(remoteComputer);
                        }

                    }
                }
                Update_timer.Start();
            }
        }
    }

    public static class ISynchronizeInvokeExtensions
    {
        /// <summary>
        /// Этот класс используется для безопасного вызова UI методов из другого потока
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_this"></param>
        /// <param name="action"></param>
        public static void InvokeEx<T>(this T _this, Action<T> action) where T : ISynchronizeInvoke
        {
            if (_this.InvokeRequired)
            {
                _this.Invoke(action, new object[] { _this });
            }
            else
            {
                action(_this);
            }
        }
    }
}