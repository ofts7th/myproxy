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

        public void work()
        {
            var guid = Guid.NewGuid().ToString("N");
            var globalSrv = MyManager.GetConfig("server").MyVal.Split(':');
            var globalUserToken = MyManager.GetConfig("usertoken").MyVal;
            var globalSrvIp = globalSrv[0];
            var globalSrvPort = int.Parse(globalSrv[1]);

            var srv = MyManager.GetProxyServer(Id);
            listener = new TcpListener(IPAddress.Any, srv.LocalPort);
            try
            {
                listener.Start(10);
            }
            catch (Exception ex)
            {
                ShouldStop = true;
            }

            while (!ShouldStop)
            {
                try
                {
                    var socket = listener.AcceptSocket();
                    var client = new TcpClient();
                    client.Connect(globalSrvIp, globalSrvPort);
                    var stream = client.GetStream();

                    byte[] data = Encoding.UTF8.GetBytes("auth");
                    stream.Write(data, 0, data.Length);
                    byte[] receiveData = new byte[1024];
                    int len = stream.Read(receiveData, 0, receiveData.Length);
                    string str = Encoding.UTF8.GetString(receiveData, 0, len);
                    if (str == "ok")
                    {
                        data = Encoding.UTF8.GetBytes(guid + "," + globalUserToken + "," + srv.Server);
                        stream.Write(data, 0, data.Length);
                        len = stream.Read(receiveData, 0, receiveData.Length);
                        str = Encoding.UTF8.GetString(receiveData, 0, len);
                        if (str == "ok")
                        {
                            var p = new Tuple<Socket, NetworkStream>(socket, stream);
                            Thread ts1 = new Thread(new ParameterizedThreadStart(this.forward1));
                            ts1.Start(p);
                            Thread ts2 = new Thread(new ParameterizedThreadStart(this.forward2));
                            ts2.Start(p);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShouldStop = true;
                }
            }
        }

        public void stop()
        {
            if (listener != null)
            {
                listener.Stop();
            }
            ShouldStop = true;
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
                    good = false;
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
                    good = false;
                }
            }
        }
    }
}
