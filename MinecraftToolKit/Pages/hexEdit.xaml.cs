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
    /// hexEdit.xaml 的交互逻辑
    /// </summary>
    public partial class HexEdit : Page
    {
        public HexEdit()
        {
            InitializeComponent();
        }

        private void ConvertToUTF8_Click(object sender, RoutedEventArgs e)
        {
            TextboxUtf8.Text = GetHexStr(TextboxHex.Text);
        }

        private void ConvertToHex_Click(object sender, RoutedEventArgs e)
        {
            TextboxHex.Text = GetStrFromHex(TextboxUtf8.Text);
        }

        private void TextboxHex_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SafeModeToggleButton.IsChecked == true)
            {
                TextboxUtf8.Text = GetHexStr(TextboxHex.Text);
            }
        }
        private static string GetHexStr(string inputText)
        {
            try
            {
                string hexString = inputText.Replace(" ", "");
                if (hexString.Length % 2 != 0) { throw new ArgumentException("参数长度不正确"); }
                byte[] returnBytes = new byte[hexString.Length / 2];
                for (int i = 0; i < returnBytes.Length; i++)
                { returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16); }
                return Encoding.UTF8.GetString(returnBytes);
            }
            catch (Exception) { }
            return null;
        }
        private static string GetStrFromHex(string inputText)
        {
            try
            {
                string utf8String = inputText;
                byte[] ba = Encoding.UTF8.GetBytes(utf8String);
                var hexString = BitConverter.ToString(ba);
                return hexString.Replace("-", "").ToLower();
            }
            catch (Exception) { }
            return null;
        }

        private void HexReplaceToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                replaceBefore.Text = GetStrFromHex(replaceBefore.Text);
                replaceAfter.Text = GetStrFromHex(replaceAfter.Text);
            }
            catch (Exception) { }
        }

        private void HexReplaceToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                replaceBefore.Text = GetHexStr(replaceBefore.Text);
                replaceAfter.Text = GetHexStr(replaceAfter.Text);
            }
            catch (Exception) { }
        }

        private void Replace_Click(object sender, RoutedEventArgs e)
        {
            string before = HexReplaceToggleButton.IsChecked == true ? replaceBefore.Text : GetStrFromHex(replaceBefore.Text);
            string after = HexReplaceToggleButton.IsChecked == true ? replaceAfter.Text : GetStrFromHex(replaceAfter.Text);
            if (string.IsNullOrEmpty(before) || string.IsNullOrEmpty(after)) { return; }
            TextboxHex.Text = TextboxHex.Text.Replace(before, after);
        }

        private void OpenFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var openJSONFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openJSONFileDialog.ShowDialog() != true) return;
            filePath = openJSONFileDialog.FileName;
            TextboxHex.Text = BitConverter.ToString(File.ReadAllBytes(filePath)).Replace("-", "").ToLower();
        }
        string filePath = null;
        private void SaveFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (TextboxHex.Text.Length % 2 != 0) { return; } //参数长度不正确
            var openJSONFileDialog = new Microsoft.Win32.SaveFileDialog()
            {
                FileName = filePath ?? Environment.CurrentDirectory
            };
            if (openJSONFileDialog.ShowDialog() != true) return;

            string hexString = TextboxHex.Text.Replace(" ", "");
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
            { returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16); }
            File.WriteAllBytes(openJSONFileDialog.FileName, returnBytes);
        }
    }
}
