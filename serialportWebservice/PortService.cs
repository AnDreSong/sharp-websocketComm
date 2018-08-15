using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebSocketSharp.Server;

namespace serialportWebservice
{
    public partial class PortService : ServiceBase
    {
        private readonly string _configFile = AppDomain.CurrentDomain.BaseDirectory + "\\Config.xml";

        public PortService()
        {
            InitializeComponent();
            PortDataConfig config = null;
            if (!File.Exists(_configFile))
            {
                config = new PortDataConfig
                {
                    BaudRate = 9600,
                    PortName = "COM10",
                    ServerPort = 9100,
                    Serverforward = "/SerialWebServer"
                };
                Serializer.ToXml(config, _configFile);
            }
            else
            {
                config = Serializer.FromXml<PortDataConfig>(_configFile);
            }
            CodecontxtData.Config = config;
            CodecontxtData.Port = new SerialPort(config.PortName)
            {
                BaudRate = config.BaudRate,
                Encoding = Encoding.UTF8,
                Handshake = Handshake.None,
                DataBits = 8
            };
            try
            {
                CodecontxtData.Port.Open();
            }
            catch (Exception e)
            {
                using (var errowrite = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\Log.log", true))
                {
                    errowrite.WriteLine("Com open erro" + e.Message);

                    errowrite.Flush();
                    errowrite.Close();
                }
            }
        }

        private WebSocketServer wssv = null;

        protected override void OnStart(string[] args)
        {
            try
            {
                using (var errowrite = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\Log.log", true))
                {
                    errowrite.WriteLine("Service Start");

                    errowrite.Flush();
                    errowrite.Close();
                }
                wssv = new WebSocketServer(CodecontxtData.Config.ServerPort);
                wssv.AddWebSocketService<SerialPortServer>(CodecontxtData.Config.Serverforward);
                wssv.Start();
            }
            catch (Exception e)
            {
                using (var errowrite = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\Log.log", true))
                {
                    errowrite.WriteLine(e.Message);
                    errowrite.WriteLine(e.StackTrace);
                    errowrite.Flush();
                    errowrite.Close();
                }
            }
        }

        protected override void OnStop()
        {
            if (wssv != null && wssv.IsListening)
            {
                wssv.Stop();
            }
            using (var errowrite = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\Log.log", true))
            {
                errowrite.WriteLine("Service Stop");

                errowrite.Flush();
                errowrite.Close();
            }
            if (CodecontxtData.Port != null)
                CodecontxtData.Port.Close();
            CodecontxtData.Port = null;
        }
    }
}