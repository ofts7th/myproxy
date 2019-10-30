using System;
using System.Collections.Generic;
using System.Data;

namespace YoudaProxy.lib
{
    public class MyApplication
    {
        public static Action<string> Action_ShowMessage { get; set; }
        public static List<ConfigItem> GetData()
        {
            List<ConfigItem> data = new List<ConfigItem>();
            DataTable dt = MyManager.ListAllProxyServer();
            foreach (DataRow dr in dt.Rows)
            {
                ConfigItem item = new ConfigItem();
                ProxyServer entity = new ProxyServer();
                DbHelper.MapDataRowToObject(dr, entity);
                item.Entity = entity;
                if (isListening(entity.Id))
                {
                    item.StatusId = 1;
                }
                data.Add(item);
            }
            return data;
        }

        static List<int> listeningItems = new List<int>();
        public static void startListen(int id)
        {
            MyProxyClient.listen(id);
            listeningItems.Add(id);
        }

        public static bool isListening(int id)
        {
            return listeningItems.Contains(id);
        }

        public static void stopListen(int id)
        {
            MyProxyClient.stop(id);
            listeningItems.Remove(id);
        }
    }

    public class ConfigItem
    {
        public ProxyServer Entity { get; set; }
        public int StatusId { get; set; }
        public string StartButtonText
        {
            get
            {
                switch (StatusId)
                {
                    case 0:
                        return "启动";
                    case 1:
                        return "停止";
                }
                return "错误";
            }
        }
    }
}