using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using YoudaProxy.lib;

namespace YoudaProxy
{
    /// <summary>
    /// Page_AutoUpdate.xaml 的交互逻辑
    /// </summary>
    public partial class Page_Proxy : Page
    {
        public Page_Proxy()
        {
            InitializeComponent();
        }

        List<ConfigItem> data = null;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.showMessage("欢迎使用友大代理转发程序");
            loadData();
        }

        void loadData()
        {
            data = MyApplication.GetData();
            this.dgData.ItemsSource = data;
        }

        void showMessage(string msg)
        {
            this.lblMessage.Content = msg;
        }

        private void ButtonCell_Click(object sender, RoutedEventArgs e)
        {
            Button btnSender = ((Button)sender);
            int configId = ((ConfigItem)dgData.SelectedValue).Entity.Id;
            switch (btnSender.CommandParameter)
            {
                case "start":
                    if (MyApplication.isListening(configId))
                    {
                        MyApplication.stopListen(configId);
                        btnSender.Content = "启动";
                    }
                    else
                    {
                        MyApplication.startListen(configId);
                        btnSender.Content = "停止";
                    }
                    break;
                case "edit":
                    var page = new Page_EditProxy();
                    page.Id = configId;
                    NavigationService.Navigate(page);
                    break;
                case "delete":
                    MyManager.DeleteProxyServer(configId);
                    loadData();
                    break;
            }
        }

        private void ButtonAddProxy_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page_EditProxy());
        }

        private void ButtonGlobalSetting_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page_Settings());
        }
    }
}