using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YoudaProxy.lib;

namespace YoudaProxy
{
    /// <summary>
    /// Page_AutoUpdate.xaml 的交互逻辑
    /// </summary>
    public partial class Page_AutoUpdate : Page
    {
        public Page_AutoUpdate()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.showMessage("版本更新");
        }

        void showMessage(string msg)
        {
            this.lblMessage.Content = msg;
        }
    }
}