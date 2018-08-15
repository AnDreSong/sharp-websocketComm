using System;
using System.IO;
using WebSocketSharp.Server;
using System.IO.Ports;
using WebSocketSharp;
using ErrorEventArgs = WebSocketSharp.ErrorEventArgs;

namespace serialportWebservice
{
    public class SerialPortServer : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            if (CodecontxtData.Port != null && CodecontxtData.Port.IsOpen)
            {
                CodecontxtData.Port.Close();
                CodecontxtData.Port.DataReceived -= Portdata_DataReceived;
            }
        }

        protected override void OnOpen()
        {
            try
            {
                using (var errowrite = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\Log.log", true))
                {
                    errowrite.WriteLine("Client Open");

                    errowrite.Flush();
                    errowrite.Close();
                }
                if (CodecontxtData.Port.IsOpen)
                {
                    CodecontxtData.Port.DataReceived += Portdata_DataReceived;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            base.OnOpen();
        }

        private void Portdata_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var data = CodecontxtData.Port.ReadExisting();
            if (!string.IsNullOrEmpty(data))
            {
                Send(data);
            }

            //using (var errowrite = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\Log.log", true))
            //{
            //    errowrite.WriteLine("Send Data" + data);

            //    errowrite.Flush();
            //    errowrite.Close();
            //}
        }

        protected override void OnError(ErrorEventArgs e)
        {
            base.OnError(e);
        }
    }
}