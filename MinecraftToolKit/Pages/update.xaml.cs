using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MinecraftToolKit.Pages
{
    /// <summary>
    /// update.xaml 的交互逻辑
    /// </summary>
    public partial class update : Page
    {
        public update()
        {
            InitializeComponent();
            //web.Source =new Uri( "https://github.com/littlegao233/MinecraftToolKit");
            // HttpHelper.GetHtmlStr("https://github.com/littlegao233/MinecraftToolKit");

            // web.Source = new Uri("https://baidu.com");
        }

        private void HitHubButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/littlegao233/MinecraftToolKit");
        }
    }
}
