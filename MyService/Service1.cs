using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MyService
{
    public partial class Service1 : ServiceBase
    {
        string filePath = @"D:\MyServiceLog.txt";
        private UdpHelper udp;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            string ip = ConfigurationManager.AppSettings["IP"];
            string port = ConfigurationManager.AppSettings["Port"];
            udp = new UdpHelper(ip, int.Parse(port));
            udp.ReceiveData += Udp_ReceiveData;
            udp.Start();
            Log("服务启动");
        }

        protected override void OnStop()
        {
            udp.Stop();
            Log("服务停止");
        }

        private void Udp_ReceiveData(object sender, ReceiveDataEventArgs e)
        {
            byte[] buffer = new byte[e.Buffer.Length];
            Array.Copy(e.Buffer, buffer, e.Buffer.Length);
            if (buffer.Length == 32)
            {
                bool shutdown = true;
                foreach (byte b in buffer)
                {
                    if (b != 255)
                    {
                        shutdown = false;
                    }
                }
                if (shutdown)
                {
                    Log("开始关机");
                    using (Process p = new Process())
                    {
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.RedirectStandardError = true;
                        p.StartInfo.RedirectStandardInput = true;
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.CreateNoWindow = true;
                        p.Start();
                        p.StandardInput.AutoFlush = true;
                        p.StandardInput.WriteLine("shutdown -s -t 1");
                        p.StandardInput.WriteLine("exit");
                        p.WaitForExit();
                        p.Close();
                    }
                }
            }
        }
        private void Log(string msg)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Append))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.WriteLine($"{DateTime.Now} {msg}");
            }
        }
    }
}
