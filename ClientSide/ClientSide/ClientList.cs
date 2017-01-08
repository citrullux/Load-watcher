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

namespace ClientSide
{
    public partial class ClientList : Form
    {
        // This integer variable keeps track of the 
        // remaining time.
        int timeLeft;
        private BindingList<Computer> list;
        public Computer C1;
        public ClientList()
        {
            InitializeComponent();
            list = new BindingList<Computer>();
            //Привязки
            grid.DataSource = list;
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
                // If the user ran out of time, stop the timer, show
                // a MessageBox, and fill in the answers.
                Update_timer.Stop();
                list.Clear();
                //list.Add();
                C1 = new Computer();

                list.Add(C1);

                Update_timer.Start();
            }
        }
    }
}
