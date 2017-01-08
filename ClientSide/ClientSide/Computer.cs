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
            system_type = "Windows";
            power_on = 1;  
        }

        [System.ComponentModel.DisplayName("Операционная система")]
        public string system_type { get; set; }

        [System.ComponentModel.DisplayName("Включен")]
        public int power_on { get; set; }

        [System.ComponentModel.DisplayName("Время работы")]
        public string uptime
        {
            get
            {
                using (var uptime = new PerformanceCounter("System", "System Up Time"))
                {
                    uptime.NextValue();       //Call this an extra time before reading its value
                    return TimeSpan.FromSeconds(uptime.NextValue()).ToString(@"%d' д. 'hh\:mm\:ss");
                    //return 
                }
            }
        }

        [System.ComponentModel.DisplayName("Нагрузка на процессор")]
        public float cpu_load {
            get
            {
                using (var cpu_counter = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
                {
                    cpu_counter.NextValue();
                    
                    // В чём проблема? Тут просто не было обновления если не поставить Wait
                    // ну правда, там было либо 0 либо 100... Теперь же работает.
                    Thread.Sleep(500);
                    return cpu_counter.NextValue();
                }
            }
        }

        [System.ComponentModel.DisplayName("Свободно памяти")]
        public float ram_load
        {
            get
            {
                using (var ram_counter = new PerformanceCounter("Memory", "Available MBytes"))
                {
                    ram_counter.NextValue();       //Call this an extra time before reading its value
                    return ram_counter.NextValue();
                }
            }
        }
    }
}
