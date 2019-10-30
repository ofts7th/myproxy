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
    public partial class Page_EditProxy : Page
    {
        public int Id { get; set; }

        public Page_EditProxy()
        {
            InitializeComponent();
        }

        ProxyServer entity = null;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if(Id == 0)
            {
                this.lblTitle.Content = "添加服务器";
                entity = new ProxyServer();
            }
            else
            {
                this.lblTitle.Content = "编辑服务器";
                entity = MyManager.GetProxyServer(Id);
                this.txtServer.Text = entity.Server;
                this.txtLocalPort.Text = entity.LocalPort.ToString();
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            entity.LocalPort = int.Parse(this.txtLocalPort.Text);
            entity.Server = this.txtServer.Text;
            MyManager.SaveProxyServer(entity);
            NavigationService.GoBack();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}