using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// whitelistEdit.xaml 的交互逻辑
    /// </summary>
    public partial class whitelistEdit : Page
    {
        public whitelistEdit()
        {
            InitializeComponent();
        }
        private List<string> LinqWhitelistJson(string jsonText)
        {
            return JArray.Parse(jsonText).Children()["name"].ToList().ConvertAll(x => x.ToString());
        }

        private void 转换_Click(object sender, RoutedEventArgs e)
        {
            #region 建立数组
            TextRange textRange = new TextRange(inputName.Document.ContentStart, inputName.Document.ContentEnd);
            string white_list_all = textRange.Text.Replace("\r", "").Replace("\n", ";").Replace(";;", ";");
            statistics.AppendText(white_list_all);
            string[] white_list = white_list_all.Split(';');
            List<string> al = white_list.ToList();
            al.RemoveAt(white_list.Length - 1);
            white_list = al.ToArray();
            #endregion
            #region 信息统计
            statistics.Document.Blocks.Clear();
            statistics.AppendText("玩家总计:" + Convert.ToString(white_list.Length));
            string output = "[\n";
            for (int i = 0; i < white_list.Length; i++)
            {
                statistics.AppendText("\n" + Convert.ToString((decimal)i + 1) + ":" + white_list[i]);
            }
            #endregion
            #region 核心转换
            foreach (string name in white_list)
            {
                output = output + "  {\n  \"ignoresPlayerLimit\":" + Convert.ToString(ignores_player_limit.IsChecked).ToLower() + ",\n  \"name\":\"" + name.Replace(" ", "#空格") + "\"\n  },\n";
            }
            if ((bool)OneLine.IsChecked)
            {
                output = output.Replace("\n", "").Replace(" ", "").Replace("#空格", " ");
                output = output.Substring(0, output.Length - 1) + "]";
            }
            else
            { output = output.Substring(0, output.Length - 2).Replace("#空格", " ") + "\n]"; }
            outputJson.Document.Blocks.Clear();
            outputJson.AppendText(output);
            #endregion
        }
        private void 提取_Click(object sender, RoutedEventArgs e)
        {
            #region 核心代码
            TextRange textRange = new TextRange(outputJson.Document.ContentStart, outputJson.Document.ContentEnd);
            string white_list_all = textRange.Text.Replace("\" ", "\"").Replace(" \"", "\"").Replace("\r", "");
            if (white_list_all != "")
            {
                List<string> white_list = LinqWhitelistJson(white_list_all);


                statistics.Document.Blocks.Clear();
                inputName.Document.Blocks.Clear();
                statistics.AppendText("玩家总计:" + white_list.Count);
                for (int i = 0; i < white_list.Count; i++)
                {
                    statistics.AppendText("\n" + Math.Truncate((decimal)i + 1) + ":" + white_list[i]);
                    inputName.AppendText(white_list[i]);
                    inputName.AppendText(Environment.NewLine);
                }
            }
            #endregion
        }

        private void MCBBS_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.mcbbs.net/thread-901503-1-1.html");
        }

        private void Github_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/littlegao233/BDS_whitelist_conversion/releases");

        }

        public string[] file_info = { null, null };

        private void Open_json_Click(object sender, RoutedEventArgs e)
        {
            var openJSONFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "JSON文件|whitelist.json"
            };
            var result = openJSONFileDialog.ShowDialog();
            if (result == true)
            {
                string File_path = openJSONFileDialog.FileName;
                file_info[0] = "json"; file_info[1] = File_path.Substring(0, File_path.LastIndexOf('.'));
                MenuItem file_path_object = new MenuItem { Header = File_path, Height = 30, Name = "file_history" };
                file_path_object.Click += new RoutedEventHandler(Open_history_path_json_Click);
                open_whitelist_json.Items.Add(file_path_object);
                statistics.Document.Blocks.Clear();
                inputName.Document.Blocks.Clear();
                outputJson.Document.Blocks.Clear();
                outputJson.AppendText(File.ReadAllText(File_path, Encoding.Default));
                Title = file_info[0] + ">" + File_path;
            }
        }
        private void Open_history_path_json_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string File_path = Convert.ToString(((MenuItem)sender).Header);
                file_info[0] = "json"; file_info[1] = File_path.Substring(0, File_path.LastIndexOf('.'));
                statistics.Document.Blocks.Clear();
                inputName.Document.Blocks.Clear();
                outputJson.Document.Blocks.Clear();
                outputJson.AppendText(File.ReadAllText(File_path, Encoding.Default));
                Title = file_info[0] + ">" + File_path;
            }
            catch (Exception a) { MessageBox.Show("ERROR{" + a.Message + "}"); }
        }

        private void Open_text_Click(object sender, RoutedEventArgs e)
        {
            var openTXTFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "文本文件|*.txt"
            };
            var result = openTXTFileDialog.ShowDialog();
            if (result == true)
            {
                string File_path = openTXTFileDialog.FileName;
                file_info[0] = "txt"; file_info[1] = File_path.Substring(0, File_path.LastIndexOf('.'));
                MenuItem file_path_object = new MenuItem { Header = File_path, Height = 30, Name = "file_history" };
                file_path_object.Click += new RoutedEventHandler(Open_history_path_text_Click);
                open_whitelist_text.Items.Add(file_path_object);
                statistics.Document.Blocks.Clear();
                inputName.Document.Blocks.Clear();
                outputJson.Document.Blocks.Clear();
                inputName.AppendText(File.ReadAllText(File_path, Encoding.Default));
                Title = file_info[0] + ">" + File_path;
            }
        }
        private void Open_history_path_text_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string File_path = Convert.ToString(((MenuItem)sender).Header);
                file_info[0] = "txt"; file_info[1] = File_path.Substring(0, File_path.LastIndexOf('.'));
                statistics.Document.Blocks.Clear();
                inputName.Document.Blocks.Clear();
                outputJson.Document.Blocks.Clear();
                inputName.AppendText(File.ReadAllText(File_path, Encoding.Default));
                Title = file_info[0] + ">" + File_path;
            }
            catch (Exception a) { MessageBox.Show("ERROR{" + a.Message + "}"); }
        }

        private void Save_json_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(outputJson.Document.ContentStart, outputJson.Document.ContentEnd);
            File.WriteAllText(file_info[1] + ".json", textRange.Text);
            MessageBox.Show($"已保存到{file_info[1]}.json");
        }

        private void Save_txt_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(inputName.Document.ContentStart, inputName.Document.ContentEnd);
            File.WriteAllText(file_info[1] + ".txt", textRange.Text);
            MessageBox.Show($"已保存到{file_info[1]}.txt");
        }
        private void Save_to_json_Click(object sender, RoutedEventArgs e)
        {
            var SaveJSONFileDialog = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "JSON文件|whitelist.json"
            };
            var result = SaveJSONFileDialog.ShowDialog();
            if (result == true)
            {
                string File_path = SaveJSONFileDialog.FileName;
                file_info[0] = "json"; file_info[1] = File_path.Substring(0, File_path.LastIndexOf('.'));
                MenuItem file_path_object = new MenuItem { Header = File_path, Height = 30, Name = "file_history" };
                file_path_object.Click += new RoutedEventHandler(Open_history_path_text_Click);
                open_whitelist_json.Items.Add(file_path_object);
                Title = file_info[0] + ">" + File_path;
                TextRange textRange = new TextRange(outputJson.Document.ContentStart, outputJson.Document.ContentEnd);
                File.WriteAllText(file_info[1] + ".json", textRange.Text);
                MessageBox.Show($"已保存到{file_info[1]}.json");

            }
        }

        private void Save_to_txt_Click(object sender, RoutedEventArgs e)
        {
            var SaveJSONFileDialog = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "文本文件|*.txt"
            };
            var result = SaveJSONFileDialog.ShowDialog();
            if (result == true)
            {
                string File_path = SaveJSONFileDialog.FileName;
                file_info[0] = "txt"; file_info[1] = File_path.Substring(0, File_path.LastIndexOf('.'));
                MenuItem file_path_object = new MenuItem { Header = File_path, Height = 30, Name = "file_history" };
                file_path_object.Click += new RoutedEventHandler(Open_history_path_text_Click);
                open_whitelist_text.Items.Add(file_path_object);
                Title = file_info[0] + ">" + File_path;
                TextRange textRange = new TextRange(inputName.Document.ContentStart, inputName.Document.ContentEnd);
                File.WriteAllText(file_info[1] + ".txt", textRange.Text);
                MessageBox.Show($"已保存到{file_info[1]}.txt");
            }
        }

        private void Save_whitelist_json_GotFocus(object sender, RoutedEventArgs e)
        {
            if (file_info[0] == null || file_info[1] == null)
            {
                save_json.Visibility = Visibility.Hidden;
                save_json.Height = 0;
            }
            else
            {
                save_json.Visibility = Visibility.Visible;
                save_json.Height = 30;
                save_json.Header = $"保存至{file_info[1]}.json";
            }
        }
        private void Save_whitelist_text_GotFocus(object sender, RoutedEventArgs e)
        {
            if (file_info[0] == null || file_info[1] == null)
            {
                save_txt.Visibility = Visibility.Hidden;
                save_txt.Height = 0;
            }
            else
            {
                save_txt.Visibility = Visibility.Visible;
                save_txt.Height = 30;
                save_txt.Header = $"保存至{file_info[1]}.txt";
            }
        } 
    }
}
