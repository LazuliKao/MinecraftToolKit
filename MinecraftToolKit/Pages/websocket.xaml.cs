using System;
using Fleck;
using System.Collections.Generic;
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
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;

namespace MinecraftToolKit.Pages
{
    /// <summary>
    /// websocket.xaml 的交互逻辑
    /// </summary>
    public partial class Websocket : Page
    {
        public Websocket()
        {
            InitializeComponent();
            _ = GetConfig();
            allScokets = new List<IWebSocketConnection>();
        }
        public void OutPut(string text)
        {
            Dispatcher.Invoke(() => OutPutRichTextBox.AppendText(DateTime.Now.ToString("s") + ">" + text + Environment.NewLine));
            WriteLog(DateTime.Now.ToString("s") + ">" + text);
        }
        private Queue<string> textWT = new Queue<string>();
        Task writeTask = Task.Run(() => { });
        private void WriteLog(string text)
        {
            textWT.Enqueue(text+Environment.NewLine);
            if (writeTask.IsCompleted)
            {
                writeTask = Task.Run(() =>
                 {
                     try
                     {
                         while (textWT.Count > 0)
                         {
                             File.AppendAllText(Environment.CurrentDirectory + "\\websocket.log", textWT.Peek());
                             textWT.Dequeue();
                         }
                     }
                     catch (Exception)
                     { return; }
                 });
            }
        }
        public List<IWebSocketConnection> allScokets = new List<IWebSocketConnection>();
        public WebSocketServer server;
        public JObject ws_config = null;
        private JObject GetConfig()
        {
            if (ws_config == null)
            {
                if (!File.Exists(Environment.CurrentDirectory + "\\ws_config.json"))
                {
                    File.WriteAllBytes(Environment.CurrentDirectory + "\\ws_config.json", Res.ws_config);
                }
                ws_config = JObject.Parse(File.ReadAllText(Environment.CurrentDirectory + "\\ws_config.json"));
                configFile.Text += Environment.CurrentDirectory + "\\ws_config.json";
            }
            return ws_config;
        }
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                server = new WebSocketServer($"ws://127.0.0.1:{port}");
                server.Start(socket =>
                {
                    socket.OnOpen = () =>
                    {
                        try
                        {
                            OutPut($"A connection has been set up(GUID:{socket.ConnectionInfo.Id})");
                            allScokets.Add(socket);
                            Dispatcher.Invoke(() =>
                            {
                                SelectedServer.Items.Add(socket.ConnectionInfo.Id);
                                StateTextBlock.Text = $"Listening on 127.0.0.1:{port}\t {allScokets.Count} active connection";
                            });
                            JObject model = JObject.Parse("{\"body\":{\"eventName\":\"PlayerMessage\"},\"header\":{\"requestId\":\"00\",\"messagePurpose\":\"subscribe\",\"version\":1,\"messageType\":\"commandRequest\"}}");
                            foreach (var item in (JObject)GetConfig()["Triggers"])
                            {
                                OutPut("Load:" + item.Key);
                                model["body"]["eventName"] = item.Key;
                                model["header"]["requestId"] = CreateUUID();
                                socket.Send(model.ToString(Newtonsoft.Json.Formatting.None));
                            }
                        }
                        catch (Exception err) { OutPut("Error:" + err.Message); }
                    };
                    socket.OnClose = () =>
                    {
                        try
                        {
                            OutPut($"A client disconnected(GUID:{socket.ConnectionInfo.Id}");
                            allScokets.Remove(socket);
                            Dispatcher.Invoke(() =>
                            {
                                if (StartButton.IsEnabled)
                                { StateTextBlock.Text = $"Not opened"; }
                                else
                                {
                                    SelectedServer.Items.Remove(socket.ConnectionInfo.Id);
                                    StateTextBlock.Text = string.Format("Listening on 127.0.0.1:{0}\t{1}", port, allScokets.Count == 0 ? "No connection" : allScokets.Count + "active connection");
                                }
                            });
                        }
                        catch (Exception err) { OutPut("Error:" + err.Message); }
                    };
                    socket.OnMessage = message =>
                     {
                         try
                         {
                             WriteLog(message);
                         }
                         catch (Exception) { }
                         try
                         {
                             JObject receive = JObject.Parse(message);
                             string messagePurpose = receive["header"]["messagePurpose"].ToString();
                             if (messagePurpose == "commandResponse")
                             {
                                 if ((int)receive["body"]["statusCode"] == 0)
                                 {
                                     OutPut($"Success:\n{string.Join("\n", receive["body"].Where(l => l.Path.ToString() != "body.statusCode").ToList().ConvertAll(l => ">" + l.ToString(Newtonsoft.Json.Formatting.None).Replace("\\n", Environment.NewLine))) }");
                                 }
                                 else
                                 {
                                     OutPut($"Error:{receive["body"]["statusMessage"]}\t(code:{receive["body"]["statusCode"]})");
                                 }
                             }
                             else if (messagePurpose == "event")
                             {
                                 string receiveEventName = receive["body"]["eventName"].ToString();
                                 if (((JObject)GetConfig()["Triggers"]).ContainsKey(receiveEventName))
                                 {
                                     foreach (JObject item in GetConfig()["Triggers"][receiveEventName])
                                     {
                                         #region 变量创建
                                         Dictionary<string, string> Variables = new Dictionary<string, string>();
                                         foreach (JObject variable in item["Variables"])
                                         {
                                             try
                                             {
                                                 Variables.Add(variable["Name"].ToString(), GetSubItem(receive, variable["Path"].ToList().ConvertAll(l => l.ToString())));
                                             }
                                             catch (Exception err) { OutPut("貌似有个变量加载失败!" + err.Message); }
                                         }
                                         #endregion
                                         #region 计算操作处理
                                         foreach (JObject operation in item["Operations"])
                                         {
                                             try
                                             {
                                                 if (operation.ContainsKey("Type"))
                                                 {
                                                     switch (operation["Type"].ToString())
                                                     {
                                                         case "Replace":
                                                             try
                                                             {
                                                                 if (CalculateExpressions(operation["Filter"], receive, Variables))
                                                                 {
                                                                     string TargetVariable = operation["TargetVariable"].ToString();
                                                                     Variables[TargetVariable] = Variables[TargetVariable].Replace(operation["Find"].ToString(), operation["Replacement"].ToString());
                                                                 }
                                                             }
                                                             catch (Exception)
                                                             { OutPut("参数缺失或变量不存在"); }
                                                             break;
                                                         case "RegexReplace":
                                                             try
                                                             {
                                                                 if (CalculateExpressions(operation["Filter"], receive, Variables))
                                                                 {
                                                                     string TargetVariable = operation["TargetVariable"].ToString();
                                                                     Variables[TargetVariable] = Regex.Replace(Variables[TargetVariable], operation["Pattern"].ToString(), operation["Replacement"].ToString());
                                                                 }
                                                             }
                                                             catch (Exception)
                                                             { OutPut("参数缺失或变量或正则表达式有误不存在"); }
                                                             break;
                                                         default:
                                                             break;
                                                     }
                                                 }
                                                 else
                                                 { OutPut("未指明Type参数"); }
                                             }
                                             catch (Exception err) { OutPut("貌似有个操作执行失败!" + err.Message); }
                                         }
                                         #endregion
                                         #region 过滤条件计算
                                         if (CalculateExpressions(item["Filter"], receive, Variables))
                                         {
                                             OutPut("Matched");
                                             #region format
                                             string format(string input, Dictionary<string, string> vars)
                                             {
                                                 string processText = input;
                                                 while (true)
                                                 {
                                                     Match match = Regex.Match(processText, @"%(\w+?)%");
                                                     if (match.Success)
                                                     {
                                                         string searched = "null";
                                                         try
                                                         { searched = vars[match.Groups[1].Value]; }
                                                         catch (Exception) { }
                                                         processText = processText.Replace(match.Value, searched);
                                                     }
                                                     else { break; }
                                                 }
                                                 return processText;
                                             }
                                             #endregion
                                             #region Actions
                                             foreach (JObject action in item["Actions"])
                                             {
                                                 switch (action["Target"].ToString())
                                                 {
                                                     case "console":
                                                         if (action.ContainsKey("Echo"))
                                                         { OutPut(format(action["Echo"].ToString(), Variables)); }
                                                         break;
                                                     case "sender":
                                                         if (action.ContainsKey("CommandRequest"))
                                                         { SendCommand(socket, format(action["CommandRequest"].ToString(), Variables)); }
                                                         break;
                                                     case "other":
                                                         if (action.ContainsKey("CommandRequest"))
                                                         {
                                                             foreach (var MachedSocket in allScokets.Where(l => l.ConnectionInfo.Id != socket.ConnectionInfo.Id))
                                                             { SendCommand(MachedSocket, format(action["CommandRequest"].ToString(), Variables)); }
                                                         }
                                                         break;
                                                     case "all":
                                                         foreach (var MachedSocket in allScokets)
                                                         {
                                                             SendCommand(MachedSocket, format(action["CommandRequest"].ToString(), Variables));
                                                         }
                                                         break;
                                                     default:
                                                         break;
                                                 }
                                             }
                                             #endregion   }
                                             #endregion
                                         }
                                     }
                                 }
                             }

                         }
                         catch (Exception err) { OutPut("Error:" + err.ToString()); }
                     };
                });
                StartButton.IsEnabled = false;
                StopButton.IsEnabled = true;
                StateTextBlock.Text = $"Listening on 127.0.0.1:{port}";
                OutPut($"Start listening at 127.0.0.1:{port}");
                OutPut($"Enter \" /connect 127.0.0.1:{port} \" in Minecraft UWP to connect!");
            }
            catch (Exception err)
            {
                OutPut("Start Failed:" + err.Message);
            }
        }
        public void UnInstall()
        {
            try
            {
                foreach (var item in allScokets) { item.Close(); }
                server.ListenerSocket.Close();
                server.ListenerSocket.Dispose();
                server.Dispose();
                allScokets.Clear();
                SelectedServer.Items.Clear();
            }
            catch (Exception) { }
        }
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            UnInstall();
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            StateTextBlock.Text = $"Not opened";
        }
        JObject CMDmodel = JObject.Parse("{\"body\":{\"origin\":{\"type\":\"player\"},\"commandLine\":\"\",\"version\":1},\"header\":{\"requestId\":\"\",\"messagePurpose\":\"commandRequest\",\"version\":1,\"messageType\":\"commandRequest\"}}");
        private void SendCommand(IWebSocketConnection socket, string commandStr)
        {
            JObject model = CMDmodel;
            model["body"]["commandLine"] = Regex.Replace(commandStr, @"^/|\r", "");
            model["header"]["requestId"] = CreateUUID();
            socket.Send(model.ToString(Newtonsoft.Json.Formatting.None));
        }
        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand(allScokets.First(l => l.ConnectionInfo.Id == (Guid)SelectedServer.SelectedItem),
                string.Format("tellraw @a {{\"rawtext\":[{{\"text\":\"<{0}> {1}\"}}]}}",
                   nickNameTB.Text.Replace("{", "§’{").Replace("[", "§’[").Replace("\"", "§’\"").Replace("\\", "\\\\"),
                   SendMessageTB.Text.Replace("\n", "\\\n").Replace("{", "§’{").Replace("[", "§’[").Replace("\"", "§’\"").Replace("\\", "\\\\").Replace("\r", "")
              ));
        }
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> cmds = new List<string>();
            if (MutiCMD.IsChecked == true)
            {
                foreach (var cmd in SendText.Text.Split('\n'))
                { cmds.Add(Regex.Replace(cmd, @"^/", "")); }
            }
            else { cmds.Add(SendText.Text); }
            IWebSocketConnection socket = allScokets.First(l => l.ConnectionInfo.Id == (Guid)SelectedServer.SelectedItem);
            foreach (var cmd in cmds) { SendCommand(socket, cmd); }
        }
        private void OutPutRichTextBox_TextChanged(object sender, TextChangedEventArgs e) => OutPutRichTextBox.ScrollToEnd();
        int port = 19130;
        private void PortTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(((TextBox)sender).Text))
            {
                ((TextBox)sender).Text = "19130";
            }
            if (Regex.IsMatch(((TextBox)sender).Text, @"^[0-9]*$"))
            {
                int input = int.Parse(((TextBox)sender).Text);
                if (input < 1 || input > 65535)
                {
                    OutPut("Irregular port!(1~65535)");
                    ((TextBox)sender).Text = port.ToString();
                }
            }
            else
            {
                OutPut("Irregular input!");
                ((TextBox)sender).Text = Regex.Replace(((TextBox)sender).Text, @"[^\d]", "");
            }
            port = int.Parse(((TextBox)sender).Text);
        }
        public bool CalculateExpressions(JToken Filters, JToken Source, Dictionary<string, string> Variables)
        {
            if (Filters.Type == JTokenType.Boolean) { return (bool)Filters; }
            if (Filters.Type == JTokenType.Object)
            {
                if (((JObject)Filters).ContainsKey("any_of"))
                { return ((JArray)((JObject)Filters)["any_of"]).Any(l => CalculateExpressions(l, Source, Variables)); }
                else if (((JObject)Filters).ContainsKey("all_of"))
                { return ((JArray)((JObject)Filters)["all_of"]).All(l => CalculateExpressions(l, Source, Variables)); }
                else if (((JObject)Filters).ContainsKey("Path") && ((JObject)Filters).ContainsKey("Operator") && ((JObject)Filters).ContainsKey("Value"))
                {
                    if (((JObject)Filters)["Path"].Type == JTokenType.Array)
                    {
                        string get = GetSubItem(Source, ((JObject)Filters)["Path"].ToList().ConvertAll(l => l.ToString()));
                        string value = ((JObject)Filters)["Value"].ToString();
                        switch (((JObject)Filters)["Operator"].ToString())
                        {
                            case "==": case "is": return get == value;
                            case "!=": case "not": return get != value;
                            default:
                                return ValueCompare(get, ((JObject)Filters)["Operator"].ToString(), value);
                        }
                    }
                }
                else if (((JObject)Filters).ContainsKey("Variable") && ((JObject)Filters).ContainsKey("Operator") && ((JObject)Filters).ContainsKey("Value"))
                {
                    if (((JObject)Filters)["Variable"].Type == JTokenType.String)
                    {
                        string get = Variables[((JObject)Filters)["Variable"].ToString()];
                        string value = ((JObject)Filters)["Value"].ToString();
                        switch (((JObject)Filters)["Operator"].ToString())
                        {
                            case "==": case "is": return get == value;
                            case "!=": case "not": return get != value;
                            default:
                                return ValueCompare(get, ((JObject)Filters)["Operator"].ToString(), value);
                        }
                    }
                }
            }
            throw new Exception("格式不规范，未能计算Filters的返回值");
        }
        private string GetSubItem(JToken souData, List<string> pathList)
        {
            var operated = souData[pathList.First()];
            Console.WriteLine(string.Join(".", pathList) + ":" + operated.ToString(Newtonsoft.Json.Formatting.None));
            pathList.RemoveAt(0);
            if (pathList.Count > 0)
            { return GetSubItem(operated, pathList); }
            else { return operated.ToString(); }
        }
        #region String转long比较
        private bool ValueCompare(string Strvalue1, string operation, string Strvalue2)
        {
            try
            {
                long value1 = long.Parse(Strvalue1);
                long value2 = long.Parse(Strvalue2);
                switch (operation)
                {
                    case "<": return value1 < value2;
                    case ">": return value1 > value2;
                    case ">=": return value1 >= value2;
                    case "<=": return value1 <= value2;
                    default:
                        break;
                }
            }
            catch (Exception) { }
            return false;
        }
        #endregion 
        private string CreateUUID() => Guid.NewGuid().ToString();

        private void EditConfigButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Environment.CurrentDirectory);
        }

        private void EditOpenButton_Click(object sender, RoutedEventArgs e)
        {
          
            EditDialog.IsOpen = true;
        }

     }
}
