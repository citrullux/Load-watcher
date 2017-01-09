using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientSide
{
    public class Computer
    {
        
        public Computer()
        {
            power_on = 1;  
        }

        [System.ComponentModel.DisplayName("Имя компьютера")]
        public string machine_name { get; set; }

        [System.ComponentModel.DisplayName("Операционная система")]
        public string system_type { get; set; }

        [System.ComponentModel.DisplayName("Включен")]
        public int power_on { get; set; }

        [System.ComponentModel.DisplayName("Время работы")]
        public string uptime { get; set; }

        [System.ComponentModel.DisplayName("Нагрузка на процессор")]
        public int cpu_load { get; set; }


        [System.ComponentModel.DisplayName("Свободно памяти")]
        public float ram_load
        { get; set; }
    }
}
