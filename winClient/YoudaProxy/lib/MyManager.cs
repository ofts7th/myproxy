using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace YoudaProxy.lib
{
    public class MyManager
    {
        public static void SaveProxyServer(ProxyServer item)
        {
            DbHelper.Save(item);
        }

        public static DataTable ListAllProxyServer()
        {
            return DbHelper.ExecDataTable("Select * From ProxyServer");
        }

        public static ProxyServer GetProxyServer(int id)
        {
            return DbHelper.Get<ProxyServer>(id);
        }

        public static void DeleteProxyServer(int id)
        {
            DbHelper.Delete<ProxyServer>(id);
        }

        public static MyConfig GetConfig(string key)
        {
            return DbHelper.GetBySql<MyConfig>("Select * From MyConfig Where MyKey = '" + key + "'");
        }

        public static void SaveConfig(MyConfig item)
        {
            DbHelper.Save(item);
        }
    }
}
