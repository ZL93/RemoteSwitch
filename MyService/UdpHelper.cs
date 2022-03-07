using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;

namespace MyService
{
    public class UdpHelper : IDisposable
    {
        #region 构造函数
        public UdpHelper(string ip, int port)
        {
            IP = ip;
            Port = port;
        }
        #endregion

        #region 变量
        private UdpClient _udpClient;
        private string _ip;
        /// <summary>
        /// UDP监听端口
        /// </summary>
        private int _port = 1234;
        private bool _started;
        #endregion

        #region 属性
        [Description("UDP监听IP")]
        [Category("UDP服务端")]
        public string IP
        {
            get { return _ip; }
            set { _ip = value; }
        }

        [Description("UDP监听端口")]
        [Category("UDP服务端")]
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        [Description("UDP客户端")]
        [Category("UDP服务端")]
        internal UdpClient UdpClient
        {
            get
            {
                if (_udpClient == null)
                {
                    bool success = false;
                    while (!success)
                    {
                        try
                        {
                            _udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(_ip), _port));
                            success = true;
                        }
                        catch (SocketException ex)
                        {
                            success = true;
                            throw ex;
                        }
                    }

                    uint IOC_IN = 0x80000000;
                    uint IOC_VENDOR = 0x18000000;
                    uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                    _udpClient.Client.IOControl(
                        (int)SIO_UDP_CONNRESET,
                        new byte[] { Convert.ToByte(false) },
                        null);
                }
                return _udpClient;
            }
        }
        #endregion

        #region 方法
        public void Start()
        {
            if (!_started)
            {
                _started = true;
                ReceiveInternal();
            }
        }

        public void Stop()
        {
            try
            {
                _started = false;
                UdpClient.Close();
                _udpClient = null;
            }
            catch
            {
            }
        }

        public void Send(byte[] buffer, string IP, int port)
        {
            SendInternal(buffer, new IPEndPoint(IPAddress.Parse(IP), port));
        }

        public void Send(byte[] buffer, IPEndPoint remoteIP)
        {
            SendInternal(buffer, remoteIP);
        }

        protected void SendInternal(byte[] buffer, IPEndPoint remoteIP)
        {
            if (!_started)
            {
                throw new ApplicationException("UDP Closed.");
            }
            try
            {
                UdpClient.BeginSend(
                   buffer,
                   buffer.Length,
                   remoteIP,
                   new AsyncCallback(SendCallback),
                   null);
            }
            catch (SocketException ex)
            {
                throw ex;
            }
        }

        public bool IsStarted()
        {
            return _started;
        }
        protected void ReceiveInternal()
        {
            if (!_started)
            {
                return;
            }
            try
            {
                UdpClient.BeginReceive(
                   new AsyncCallback(ReceiveCallback),
                   null);
            }
            catch (SocketException ex)
            {
                //_started = false;
                throw ex;
            }
        }

        private void SendCallback(IAsyncResult result)
        {
            try
            {
                UdpClient.EndSend(result);
            }
            catch (SocketException ex)
            {
                throw ex;
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            if (!_started)
            {
                return;
            }
            IPEndPoint remoteIP = new IPEndPoint(IPAddress.Any, 0);
            byte[] buffer = null;
            try
            {
                buffer = UdpClient.EndReceive(result, ref remoteIP);
            }
            catch (SocketException ex)
            {
                throw ex;
            }
            finally
            {
                ReceiveInternal();
            }

            OnReceiveData(new ReceiveDataEventArgs(buffer, remoteIP));
        }
        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            _started = false;
            if (_udpClient != null)
            {
                _udpClient.Close();
                _udpClient = null;
            }
        }

        #endregion

        #region 事件
        public event ReceiveDataEventHandler ReceiveData;
        [Description("UDP服务端接收数据事件")]
        [Category("UDPServer事件")]
        protected virtual void OnReceiveData(ReceiveDataEventArgs e)
        {
            if (ReceiveData != null)
            {
                ReceiveData(this, e);
            }
        }
        #endregion
    }

    public delegate void ReceiveDataEventHandler(
       object sender,
       ReceiveDataEventArgs e);

    public class ReceiveDataEventArgs : EventArgs
    {
        private byte[] _buffer;
        private IPEndPoint _remoteIP;

        public ReceiveDataEventArgs() { }

        public ReceiveDataEventArgs(byte[] buffer, IPEndPoint remoteIP)
            : base()
        {
            _buffer = buffer;
            _remoteIP = remoteIP;
        }

        public byte[] Buffer
        {
            get { return _buffer; }
            set { _buffer = value; }
        }

        public IPEndPoint RemoteIP
        {
            get { return _remoteIP; }
            set { _remoteIP = value; }
        }
    }
}
