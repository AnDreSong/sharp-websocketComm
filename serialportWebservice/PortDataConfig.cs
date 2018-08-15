using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace serialportWebservice
{
    public class PortDataConfig
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public int ServerPort { get; set; }

        public string Serverforward { get; set; }
    }
}