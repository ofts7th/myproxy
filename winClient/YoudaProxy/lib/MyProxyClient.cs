using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace YoudaProxy.lib
{
    public class MyProxyClient
    {
        static List<YoudaProxy> proxies = new List<YoudaProxy>();
        public static void listen(int id)
        {
            YoudaProxy proxy = new YoudaProxy();
            proxy.Id = id;
            proxies.Add(proxy);

            var ts = new Thread(new ThreadStart(proxy.work));
            ts.IsBackground = true;
            ts.Start();
        }

        public static void stop(int id)
        {
            var proxy = proxies.First(x => x.Id == id);
            if (proxy != null)
            {
                proxy.stop();
            }
        }
    }

    public class YoudaProxy
    {
        public int Id { get; set; }
        public bool ShouldStop { get; set; }
        TcpListener listener = null;
        string guid = Guid.NewGuid().ToString("N");

        void showMessage(string str)
        {
            MyApplication.Action_ShowMessage?.Invoke(str);
        }

        public void work()
        {
            var _globalSrv = MyManager.GetConfig("server").MyVal.Split(':');
            var globalUserToken = MyManager.GetConfig("usertoken").MyVal;
            var globalSrvIp = _globalSrv[0];
            var globalSrvPort = int.Parse(_globalSrv[1]);

            var srv = MyManager.GetProxyServer(Id);
            listener = new TcpListener(IPAddress.Any, srv.LocalPort);
            try
            {
                showMessage("开始监听" + srv.LocalPort);
                listener.Start(5);
            }
            catch
            {
                showMessage("监听出现错误");
                return;
            }

            showMessage("等待连接");
            while (!ShouldStop)
            {
                try
                {
                    var socket = listener.AcceptSocket();
                    showMessage("收到转发请求");
                    var client = new TcpClient();
                    showMessage("开始连接服务器，" + globalSrvIp + ":" + globalSrvPort);
                    client.Connect(globalSrvIp, globalSrvPort);
                    var stream = client.GetStream();
                    showMessage("开始握手");
                    byte[] data = Encoding.UTF8.GetBytes("auth");
                    stream.Write(data, 0, data.Length);
                    byte[] receiveData = new byte[1024];
                    int len = stream.Read(receiveData, 0, receiveData.Length);
                    string str = Encoding.UTF8.GetString(receiveData, 0, len);

                    bool working = false;
                    if (str == "ok")
                    {
                        showMessage("握手成功，开始认证");
                        data = Encoding.UTF8.GetBytes(guid + "," + globalUserToken + "," + srv.Server);
                        stream.Write(data, 0, data.Length);
                        len = stream.Read(receiveData, 0, receiveData.Length);
                        str = Encoding.UTF8.GetString(receiveData, 0, len);
                        if (str == "ok")
                        {
                            showMessage("认证成功，开始转发");
                            var p = new Tuple<Socket, NetworkStream>(socket, stream);
                            Thread ts1 = new Thread(new ParameterizedThreadStart(this.forward1));
                            ts1.IsBackground = true;
                            ts1.Start(p);
                            Thread ts2 = new Thread(new ParameterizedThreadStart(this.forward2));
                            ts2.IsBackground = true;
                            ts2.Start(p);
                            working = true;
                        }
                        else
                        {
                            showMessage("认证失败");
                        }
                    }
                    else
                    {
                        showMessage("握手失败");
                    }

                    if (!working)
                    {
                        socket.Close();
                        client.Close();
                    }
                }
                catch (Exception ex)
                {
                    showMessage("程序异常：" + ex.Message);
                }
            }

            if (listener != null)
            {
                listener.Stop();
                listener = null;
            }
        }

        public void stop()
        {
            if (listener != null)
            {
                listener.Stop();
            }
            ShouldStop = true;
            showMessage("停止监听");
        }

        void forward1(object obj)
        {
            Tuple<Socket, NetworkStream> o = (Tuple<Socket, NetworkStream>)obj;
            var data = new byte[1024];
            bool good = true;
            while (good)
            {
                try
                {
                    int len = o.Item1.Receive(data);
                    if (len > 0)
                    {
                        o.Item2.Write(data, 0, len);
                    }
                    else
                    {
                        good = false;
                    }
                }
                catch (Exception ex)
                {
                    showMessage("转发异常：" + ex.Message);
                    good = false;

                    o.Item1.Close();
                    o.Item2.Close();
                }
            }
        }

        void forward2(object obj)
        {
            Tuple<Socket, NetworkStream> o = (Tuple<Socket, NetworkStream>)obj;
            var data = new byte[1024];
            bool good = true;
            while (good)
            {
                try
                {
                    int len = o.Item2.Read(data, 0, data.Length);
                    if (len > 0)
                    {
                        o.Item1.Send(data, len, SocketFlags.None);
                    }
                    else
                    {
                        good = false;
                    }
                }
                catch (Exception ex)
                {
                    showMessage("转发异常：" + ex.Message);
                    good = false;

                    o.Item1.Close();
                    o.Item2.Close();
                }
            }
        }
    }
}
