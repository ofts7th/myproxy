using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace YoudaProxy.lib
{
    public class Util
    {
        public static string getWebData(string url)
        {
            HttpWebRequest webReq = null;
            try
            {
                webReq = (HttpWebRequest)WebRequest.Create(new Uri(url));
                webReq.Method = "GET";
                HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ret = sr.ReadToEnd();
                sr.Close();
                response.Close();
                return ret;
            }
            catch
            {
                return "";
            }
        }
    }
}
