using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using YoudaProxy.lib;

namespace YoudaProxy
{
    /// <summary>
    /// Page_AutoUpdate.xaml 的交互逻辑
    /// </summary>
    public partial class Page_Settings : Page
    {

        public Page_Settings()
        {
            InitializeComponent();
        }

        MyConfig cfgServer = null;
        MyConfig cfgUserToken = null;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            cfgServer = MyManager.GetConfig("server");
            if (cfgServer == null)
            {
                cfgServer = new MyConfig();
                cfgServer.MyKey = "server";
            }
            cfgUserToken = MyManager.GetConfig("usertoken");
            if (cfgUserToken == null)
            {
                cfgUserToken = new MyConfig();
                cfgUserToken.MyKey = "usertoken";
            }
            this.txtServer.Text = cfgServer.MyVal;
            this.txtUserToken.Text = cfgUserToken.MyVal;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            cfgServer.MyVal = this.txtServer.Text;
            cfgUserToken.MyVal = this.txtUserToken.Text;
            MyManager.SaveConfig(cfgServer);
            MyManager.SaveConfig(cfgUserToken);
            NavigationService.GoBack();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}