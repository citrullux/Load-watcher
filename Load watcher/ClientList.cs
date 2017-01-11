using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Computer;
using System.Runtime.Serialization.Json;

namespace Load_watcher
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
        public IPAddress ipAddr = IPAddress.Parse("0.0.0.0");
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
                    tcpClients.Add(server.AcceptTcpClient());
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
                            var ms = new MemoryStream(bytes);

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
