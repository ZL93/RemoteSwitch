using System;
using System.Collections;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Configuration.Install;

namespace WindowsServiceClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string serviceFilePath = $"{Application.StartupPath}\\MyService.exe";
        string serviceName = "MyService";

        //事件：安装服务
        private void button1_Click(object sender, EventArgs e)
        {
            if (this.IsServiceExisted(serviceName)) this.UninstallService(serviceName);
            this.InstallService(serviceFilePath);
        }

        //事件：启动服务
        private void button2_Click(object sender, EventArgs e)
        {
            if (this.IsServiceExisted(serviceName)) this.ServiceStart(serviceName);
        }

        //事件：停止服务
        private void button3_Click(object sender, EventArgs e)
        {
            if (this.IsServiceExisted(serviceName)) this.ServiceStop(serviceName);
        }

        //事件：卸载服务
        private void button4_Click(object sender, EventArgs e)
        {
            if (this.IsServiceExisted(serviceName))
            {
                this.ServiceStop(serviceName);
                this.UninstallService(serviceFilePath);
            }
        }

        //判断服务是否存在
        private bool IsServiceExisted(string serviceName)
        {
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController sc in services)
            {
                if (sc.ServiceName.ToLower() == serviceName.ToLower())
                {
                    return true;
                }
            }
            return false;
        }

        //安装服务
        private void InstallService(string serviceFilePath)
        {
            using (AssemblyInstaller installer = new AssemblyInstaller())
            {
                installer.UseNewContext = true;
                installer.Path = serviceFilePath;
                IDictionary savedState = new Hashtable();
                installer.Install(savedState);
                installer.Commit(savedState);
            }
        }

        //卸载服务
        private void UninstallService(string serviceFilePath)
        {
            using (AssemblyInstaller installer = new AssemblyInstaller())
            {
                installer.UseNewContext = true;
                installer.Path = serviceFilePath;
                installer.Uninstall(null);
            }
        }
        //启动服务
        private void ServiceStart(string serviceName)
        {
            using (ServiceController control = new ServiceController(serviceName))
            {
                if (control.Status == ServiceControllerStatus.Stopped)
                {
                    control.Start();
                }
            }
        }

        //停止服务
        private void ServiceStop(string serviceName)
        {
            using (ServiceController control = new ServiceController(serviceName))
            {
                if (control.Status == ServiceControllerStatus.Running)
                {
                    control.Stop();
                }
            }
        }
    }
}