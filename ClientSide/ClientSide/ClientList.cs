using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ClientSide
{
    public partial class ClientList : Form
    {
        // This integer variable keeps track of the 
        // remaining time.
        int timeLeft;
        private BindingList<Computer> list;
        public Computer Remote_computer;
        public TcpClient New_client;
        public TcpListener listener;
        public IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
        public ClientList()
        {
            InitializeComponent();
            list = new BindingList<Computer>();
            //Привязки
            grid.DataSource = list;
            listener = new TcpListener(ipAddr, 4254);
            //NetworkStream In_stream = listener.AcceptTcpClient
            listener.Start();
            New_client = listener.AcceptTcpClient();
        }

        public void Update_timer_Tick(object sender, EventArgs e)
        {
            if (timeLeft > 0)
            {
                // Display the new time left
                // by updating the Time Left label.
                timeLeft = timeLeft - 1;
            }
            else
            {
                // If the user ran out of time, stop the timer, show
                // a MessageBox, and fill in the answers.
                Update_timer.Stop();
                list.Clear();
                //list.Add();
                Remote_computer = new Computer();
                using (var memoryStream = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    byte[] buf = new byte[4];
                    New_client.GetStream().Read(buf, 0, 4);
                    var size = BitConverter.ToInt32(buf, 0);
                    var bytes = new byte[size]; 
                    New_client.GetStream().Read(bytes, 0, bytes.Length);
                    var ms = new MemoryStream(bytes);
                    var Remote_computer = (Computer) formatter.Deserialize(ms);
                    list.Add(Remote_computer);
                }
                Update_timer.Start();
            }
        }
    }
}
