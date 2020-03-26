using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
    /// servers.xaml 的交互逻辑
    /// </summary>
    public partial class Servers : Page
    {
        public string filePath = null;
        public Servers()
        {
            InitializeComponent();
            infoPanel.ItemsSource = Data;
            filePath = Environment.GetEnvironmentVariable("LocalAppData") + @"\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\minecraftpe\external_servers.txt";
            FilePathTextBox.Text = filePath;
            bool existFIle = CheckFIle(filePath);
            if (existFIle)
            {
                string[] text = File.ReadAllLines(filePath);
                for (int i = 0; i < text.Length; i++)
                {
                    try
                    {
                        string[] server = text[i].Split(':');
                        Data.Add(
                        new Model
                        {
                            Order = server[0],
                            ServerName = server[1],
                            Address = server[2],
                            Port = server[3],
                            LastEditTime = server[4]
                        });
                        RefreshInfo(i);
                    }
                    catch (Exception)
                    { }
                }
            }
            //for (int i = 0; i < SSH.Servers.Length; i++)
            //{
            //   
            //    RefreshInfo(i, SSH.Servers[i].ssh.ConnectionInfo.Host, SSH.Servers[i].port);
            //}
        }
        private bool CheckFIle(string file_path)
        {
            bool existFile = File.Exists(file_path);
            returnInfo.Text = existFile ? "" : "(File not exist!)";
            return existFile;
        }

        private enum InfoList
        {
            type, description, connectionVer, gameVer, onlineplayers, maxPlayers,
            serverUID, mapName, defaultMode, isBDS, port, portv6
            //,
            //类别 = 0, 简介, 协议版本, 游戏版本, 在线人数, 在线在线人数,
            //客户端标识, 存档名称, 默认模式, _未知2, 端口, ipv6端口
        }
        private static Dictionary<InfoList, string> GetServerInfo(string address, int port)
        {
            byte[] sendData = Convert.FromBase64String("AQAAAAAAA2oHAP//AP7+/v79/f39EjRWeJx0FrwC/0lw");
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            byte[] receiveData = new byte[256];
            Task queryTask = Task.Run(() =>
            {
                try
                {
                    client.SendTo(sendData, new IPEndPoint(IPAddress.TryParse(address, out IPAddress ipAddress) ? ipAddress : Dns.GetHostAddresses(address).First(), port));
                    client.Receive(receiveData, receiveData.Length, SocketFlags.None);
                }
                catch (Exception) { }
            }
            );
            queryTask.Wait(TimeSpan.FromSeconds(10));
            if (!queryTask.IsCompleted || queryTask.IsFaulted) { throw new ArgumentNullException("Query Failed", "Unable to connect to the server!"); }
            queryTask.Dispose();
            int i = 0;
            return Encoding.UTF8.GetString(receiveData).Substring(31).Split(';').ToDictionary(x => (InfoList)i++, l => /*i == 2 ? System.Text.RegularExpressions.Regex.Replace(l, @"§[A-Za-z\d]", "") :*/ l);
        }
        private void RefreshInfo(int index)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    var get = GetServerInfo(Data[index].Address, int.Parse(Data[index].Port));
                    Data[index].Description = System.Text.RegularExpressions.Regex.Replace(get[InfoList.description], @"§[A-Za-z\d]", "");
                    Data[index].OnlinePlayer = get[InfoList.onlineplayers] + "/" + get[InfoList.maxPlayers];
                    Data[index].Version = get[InfoList.connectionVer] + ":MCBE" + get[InfoList.gameVer];
                }
                catch (Exception)
                {
                    Data[index].Description = "无法连接至世界...";
                    Data[index].OnlinePlayer = "...";
                    Data[index].Version = "...";
                }
                Dispatcher.Invoke(() => infoPanel.Items.Refresh());
            });
        }

        private ObservableCollection<Model> Data = new ObservableCollection<Model>();
        //private void OpenLinkButton_Click(object sender, EventArgs e)
        //{//minecraft:?addExternalServer=§a像素彼岸§b空岛§e服务器|132.232.21.86:19132

        //    //  System.Diagnostics.Process.Start("minecraft://","addExternalServer=§a像素彼岸§b空岛§e服务器|132.232.21.86:19132");
        //    Process.Start(((ListBoxItem)sender).Tag.ToString());
        //    //webBrowser.Source = new Uri(((ListBoxItem)sender).Tag.ToString());
        //    //webBrowser.Source = new Uri("minecraft://?addExternalServer=§a像素彼岸§b空岛§e服务器|132.232.21.86:19132");
        //}
        //private void RetryButton_Click(object sender, EventArgs e)
        //{
        //    int i = (int)((ListBoxItem)sender).Tag;
        //    RefreshInfo(i, SSH.Servers[i].ssh.ConnectionInfo.Host, SSH.Servers[i].port);
        //    Data[i].Description = "Loading...";
        //    Data[i].OnlinePlayer = "Loading...";
        //    Data[i].Version = "Loading...";
        //    infoPanel.Items.Refresh();
        //}
    }
    public class Model
    {
        public string Order { get; set; } = "Loading...";
        public string ServerName { get; set; } = "Loading...";
        public string Address { get; set; } = "Loading...";
        public string Port { get; set; } = "Loading...";
        public string Description { get; set; } = "Loading...";
        public string OnlinePlayer { get; set; } = "Loading...";
        public string Version { get; set; } = "Loading...";
        public string LastEditTime { get; set; } = "Loading...";
    }

}

