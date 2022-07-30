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
        string local_ip = "127.0.0.1";
        int local_port = 8000;
        string remote_IP = "127.0.0.2";
        int remote_Port = 8800;
        protected override void OnStart(string[] args)
        {
            try
            {
                local_ip = ConfigurationManager.AppSettings["IP"];
                local_port = int.Parse(ConfigurationManager.AppSettings["Port"]);
                remote_IP = ConfigurationManager.AppSettings["remote_IP"];
                remote_Port = int.Parse(ConfigurationManager.AppSettings["remote_Port"]);
                udp = new UdpHelper(local_ip, local_port);
                udp.ReceiveData += Udp_ReceiveData;
                udp.Start();

                udp.Send(new byte[] { 255, 255, 255, 255 }, remote_IP, remote_Port);
                Log("服务启动");
            }
            catch (Exception e)
            {
                Log("服务启动失败" + e.ToString());
            }
        }

        protected override void OnStop()
        {
            udp.Stop();
            Log("服务停止");
        }

        protected override void OnShutdown()
        {
            udp.Send(new byte[] { 0, 0, 0, 0 }, remote_IP, remote_Port);
            Log("关闭电脑");
            base.OnShutdown();
        }
        private void Udp_ReceiveData(object sender, ReceiveDataEventArgs e)
        {
            byte[] buffer = new byte[e.Buffer.Length];
            Array.Copy(e.Buffer, buffer, e.Buffer.Length);
            Log("data: " + BitConverter.ToString(buffer));
            if (buffer.Length >= 32)
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
