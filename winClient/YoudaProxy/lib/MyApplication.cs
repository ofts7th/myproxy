using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace YoudaProxy.lib
{
    public class MyApplication
    {
        static Dictionary<string, string> config = new Dictionary<string, string>();
        static MyApplication()
        {
            config["updateUrl"] = "http://dev.sdyouda.win:7722/ydworklog/youdaproxy/checkapp.htm";
        }

        public static List<ConfigItem> GetData()
        {
            List<ConfigItem> data = new List<ConfigItem>();
            DataTable dt = MyManager.ListAllProxyServer();
            foreach (DataRow dr in dt.Rows)
            {
                ConfigItem item = new ConfigItem();
                ProxyServer entity = new ProxyServer();
                entity.Id = (int)dr["Id"];
                entity.Server = dr["Server"].ToString();
                entity.LocalPort = (int)dr["LocalPort"];
                item.Entity = entity;
                data.Add(item);
            }
            return data;
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