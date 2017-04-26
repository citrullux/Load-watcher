using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer
{
    [Serializable]
    public struct Info
    {
        [System.ComponentModel.DisplayName("Имя компьютера")]
        public string MachineName { get; set; }

        [System.ComponentModel.DisplayName("Производитель процессора")]
        public string VendorId { get; set; }

        [System.ComponentModel.DisplayName("Название процессора")]
        public string ModelName { get; set; }

        [System.ComponentModel.DisplayName("Операционная система")]
        public string SystemType { get; set; }

        [System.ComponentModel.DisplayName("Время работы")]
        public string Uptime { get; set; }

        [System.ComponentModel.DisplayName("Использование процессора")]
        public int CpuLoad { get; set; }

        [System.ComponentModel.DisplayName("Свободно памяти, МБ")]
        public int RamLoad { get; set; }
    }
}