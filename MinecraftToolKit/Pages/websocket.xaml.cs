using System;
using Fleck;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using MaterialDesignThemes.Wpf;
using System.Timers;
using System.Net;

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
            allScokets = new Dictionary<IWebSocketConnection, bool>();
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
            textWT.Enqueue(text + Environment.NewLine);
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
        public Dictionary<IWebSocketConnection, bool> allScokets = new Dictionary<IWebSocketConnection, bool>();
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
                var ip = "127.0.0.1";
                try
                {
                    ip = Dns.GetHostAddresses(Dns.GetHostName()).First(l => l.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();
                }
                catch (Exception) { }
                server = new WebSocketServer($"ws://0.0.0.0:{port}");
                server.Start(socket =>
                {
                    socket.OnOpen = () =>
                    {
                        try
                        {
                            OutPut($"A connection has been set up(GUID:{socket.ConnectionInfo.Id})");
                            allScokets.Add(socket, false);
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
                             JObject receive = JObject.Parse(message);
                             try { GetCMDresult[receive["header"]["requestId"].ToString()] = JArray.Parse(Regex.Unescape(receive["body"]["details"].ToString())); }
                             catch (Exception) { }
                             if (allScokets[socket])
                             {
                                 try
                                 {
                                     ListeningUUID.Remove(receive["header"]["requestId"].ToString());
                                     return;
                                 }
                                 catch (Exception) { }
                             }
                             try { WriteLog(message); } catch (Exception) { }
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
                                                             foreach (var MachedSocket in allScokets.Where(l => l.Key.ConnectionInfo.Id != socket.ConnectionInfo.Id))
                                                             { SendCommand(MachedSocket.Key, format(action["CommandRequest"].ToString(), Variables)); }
                                                         }
                                                         break;
                                                     case "all":
                                                         foreach (var MachedSocket in allScokets)
                                                         {
                                                             SendCommand(MachedSocket.Key, format(action["CommandRequest"].ToString(), Variables));
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
                StateTextBlock.Text = $"Listening on ws://0.0.0.0:{port}";
                OutPut($"Start listening at ws://0.0.0.0:{port}");
                OutPut($"Try to use \" /connect {ip}:{port} \" in Minecraft UWP to connect!");
            }
            catch (Exception err)
            {
                OutPut("Start Failed:" + err.ToString());
            }
        }
        public void UnInstall()
        {
            try
            {
                foreach (var item in allScokets) { item.Key.Close(); }
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
        private readonly JObject CMDmodel = JObject.Parse("{\"body\":{\"origin\":{\"type\":\"player\"},\"commandLine\":\"\",\"version\":1},\"header\":{\"requestId\":\"\",\"messagePurpose\":\"commandRequest\",\"version\":1,\"messageType\":\"commandRequest\"}}");
        private void SendCommand(IWebSocketConnection socket, string commandStr)
        {
            JObject model = CMDmodel;
            model["body"]["commandLine"] = Regex.Replace(commandStr, @"^/|\r", "");
            model["header"]["requestId"] = CreateUUID();
            socket.Send(model.ToString(Newtonsoft.Json.Formatting.None));
        }
        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand(allScokets.First(l => l.Key.ConnectionInfo.Id == (Guid)SelectedServer.SelectedItem).Key,
                string.Format("tellraw @a {{\"rawtext\":[{{\"text\":\"<{0}> {1}\"}}]}}",
                   nickNameTB.Text.Replace("{", "§’{").Replace("[", "§’[").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("”", "\\\"").Replace("“", "\\\""),
                   SendMessageTB.Text.Replace("{", "§’{").Replace("[", "§’[").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("”", "\\\"").Replace("“", "\\\"")
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
            IWebSocketConnection socket = allScokets.First(l => l.Key.ConnectionInfo.Id == (Guid)SelectedServer.SelectedItem).Key;
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
        #region Build
        public struct PointInfo
        {
            public long[] start, end, center;
        }
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            switch (TabSelectMode.SelectedIndex)
            {
                #region Square
                case 0:
                    if (string.IsNullOrEmpty(SquareSPx.Text) || string.IsNullOrEmpty(SquareSPy.Text) || string.IsNullOrEmpty(SquareSPz.Text) ||
       string.IsNullOrEmpty(SquareEPx.Text) || string.IsNullOrEmpty(SquareEPy.Text) || string.IsNullOrEmpty(SquareEPz.Text) ||
          string.IsNullOrEmpty(SquareBlockName.Text))
                    { OutPut("请填写完整!"); return; }
                    #region Func
                    List<PointInfo> GetPointAnalysis(PointInfo P)
                    {
                        List<PointInfo> PinfoOut = new List<PointInfo>();
                        long[] dt = new long[] { (P.end[0] - P.start[0]) / 2, (P.end[1] - P.start[1]) / 2, (P.end[2] - P.start[2]) / 2 };
                        if ((Math.Abs(P.end[0] - P.start[0]) + 1) * (Math.Abs(P.end[1] - P.start[1]) + 1) * (Math.Abs(P.end[2] - P.start[2]) + 1) < 32768) { PinfoOut.Add(P); }//在范围内直接添加
                        else
                        {//xyz分别取半分成8份
                            int[] i = new int[3];
                            for (i[0] = 0; i[0] < 2; i[0]++)
                            {
                                for (i[1] = 0; i[1] < 2; i[1]++)
                                {
                                    for (i[2] = 0; i[2] < 2; i[2]++)
                                    {
                                        long[] st = new long[3];
                                        long[] ed = new long[3];
                                        for (int index = 0; index < 3; index++)
                                        {
                                            st[index] = P.start[index] + dt[index] * i[index];
                                            ed[index] = P.end[index] - dt[index] * (1 - i[index]);
                                        }
                                        PinfoOut.AddRange(GetPointAnalysis(new PointInfo() { start = st, end = ed }));
                                    }
                                }
                            }
                        }
                        return PinfoOut;
                    }
                    #endregion
                    PointInfo pointInfo = new PointInfo()
                    {
                        start = new long[] { long.Parse(SquareSPx.Text), long.Parse(SquareSPy.Text), long.Parse(SquareSPz.Text), },
                        end = new long[] { long.Parse(SquareEPx.Text), long.Parse(SquareEPy.Text), long.Parse(SquareEPz.Text) }
                    };
                    List<PointInfo> get = new List<PointInfo>();
                    var GetBlock = SquareBlockName.Text.Split(':', ' ');
                    string block = GetBlock[0] + " ";
                    if (GetBlock.Length < 2)
                    { block += "0"; }
                    else { block += GetBlock[1]; }
                    MCFunctions.Text = $"#Convert use MCToolKit by gxh";
                    switch (SquareFillMode.SelectedIndex)
                    {
                        case 0:
                            get = GetPointAnalysis(pointInfo);
                            break;
                        case 1:
                            block += " keep";
                            get = GetPointAnalysis(pointInfo);
                            break;
                        case 2:
                            block += " destroy";
                            get = GetPointAnalysis(pointInfo);
                            break;
                        case 3:
                            get = GetPointAnalysis(pointInfo);
                            MCFunctions.Text += $"\n#clear {get.Count}";
                            foreach (var item in get)
                            { MCFunctions.Text += $"{Environment.NewLine}fill {item.start[0]} {item.start[1]} {item.start[2]} {item.end[0]} {item.end[1]} {item.end[2]} air 0"; }
                            get.Clear();
                            var SP = pointInfo.start;
                            var EP = pointInfo.end;
                            if (((ListBoxItem)SquareIncludeSurface.Items[0]).IsSelected)
                            { get.AddRange(GetPointAnalysis(new PointInfo() { start = SP, end = new long[] { EP[0], EP[1], SP[2] } })); }
                            if (((ListBoxItem)SquareIncludeSurface.Items[1]).IsSelected)
                            { get.AddRange(GetPointAnalysis(new PointInfo() { start = SP, end = new long[] { EP[0], SP[1], EP[2] } })); }
                            if (((ListBoxItem)SquareIncludeSurface.Items[2]).IsSelected)
                            { get.AddRange(GetPointAnalysis(new PointInfo() { start = SP, end = new long[] { SP[0], EP[1], EP[2] } })); }
                            if (((ListBoxItem)SquareIncludeSurface.Items[3]).IsSelected)
                            { get.AddRange(GetPointAnalysis(new PointInfo() { start = new long[] { SP[0], SP[1], EP[2] }, end = EP })); }
                            if (((ListBoxItem)SquareIncludeSurface.Items[4]).IsSelected)
                            { get.AddRange(GetPointAnalysis(new PointInfo() { start = new long[] { EP[0], SP[1], SP[2] }, end = EP })); }
                            if (((ListBoxItem)SquareIncludeSurface.Items[5]).IsSelected)
                            { get.AddRange(GetPointAnalysis(new PointInfo() { start = new long[] { SP[0], EP[1], SP[2] }, end = EP })); }
                            break;
                        case 4:
                            SP = pointInfo.start;
                            EP = pointInfo.end;
                            if (((ListBoxItem)SquareIncludeSurface.Items[0]).IsSelected)
                            { get.AddRange(GetPointAnalysis(new PointInfo() { start = SP, end = new long[] { EP[0], EP[1], SP[2] } })); }
                            if (((ListBoxItem)SquareIncludeSurface.Items[1]).IsSelected)
                            { get.AddRange(GetPointAnalysis(new PointInfo() { start = SP, end = new long[] { EP[0], SP[1], EP[2] } })); }
                            if (((ListBoxItem)SquareIncludeSurface.Items[2]).IsSelected)
                            { get.AddRange(GetPointAnalysis(new PointInfo() { start = SP, end = new long[] { SP[0], EP[1], EP[2] } })); }
                            if (((ListBoxItem)SquareIncludeSurface.Items[3]).IsSelected)
                            { get.AddRange(GetPointAnalysis(new PointInfo() { start = new long[] { SP[0], SP[1], EP[2] }, end = EP })); }
                            if (((ListBoxItem)SquareIncludeSurface.Items[4]).IsSelected)
                            { get.AddRange(GetPointAnalysis(new PointInfo() { start = new long[] { EP[0], SP[1], SP[2] }, end = EP })); }
                            if (((ListBoxItem)SquareIncludeSurface.Items[5]).IsSelected)
                            { get.AddRange(GetPointAnalysis(new PointInfo() { start = new long[] { SP[0], EP[1], SP[2] }, end = EP })); }
                            break;
                        case 5:
                            long[,] P = new long[2, 3] { { pointInfo.start[0], pointInfo.start[1], pointInfo.start[2] }, { pointInfo.end[0], pointInfo.end[1], pointInfo.end[2] } };
                            for (int i = 0; i < 3; i++)
                            {
                                get.AddRange(GetPointAnalysis(new PointInfo() { start = pointInfo.start, end = new long[] { P[i == 0 ? 1 : 0, 0], P[i == 1 ? 1 : 0, 1], P[i == 2 ? 1 : 0, 2] } }));
                                get.AddRange(GetPointAnalysis(new PointInfo() { start = new long[] { P[i == 0 ? 0 : 1, 0], P[i == 1 ? 0 : 1, 1], P[i == 2 ? 0 : 1, 2] }, end = pointInfo.end }));
                                get.AddRange(GetPointAnalysis(new PointInfo() { start = new long[] { P[i == 0 ? 1 : 0, 0], P[i == 1 ? 1 : 0, 1], P[i == 2 ? 1 : 0, 2] }, end = i == 1 ? new long[] { P[0, 0], P[1, 1], P[1, 2] } : new long[] { P[1, 0], P[0, 1], P[1, 2] } }));
                                get.AddRange(GetPointAnalysis(new PointInfo() { start = new long[] { P[i == 0 ? 1 : 0, 0], P[i == 1 ? 1 : 0, 1], P[i == 2 ? 1 : 0, 2] }, end = i == 2 ? new long[] { P[0, 0], P[1, 1], P[1, 2] } : new long[] { P[1, 0], P[1, 1], P[0, 2] } }));
                            }
                            break;
                        default:
                            break;
                    }
                    MCFunctions.Text += $"\n#{get.Count}";
                    foreach (var item in get)
                    { MCFunctions.Text += $"{Environment.NewLine}fill {item.start[0]} {item.start[1]} {item.start[2]} {item.end[0]} {item.end[1]} {item.end[2]} {block}"; }
                    break;
                #endregion
                #region Round 
                case 1:
                    if (string.IsNullOrEmpty(RoundCPx.Text) || string.IsNullOrEmpty(RoundCPy.Text) || string.IsNullOrEmpty(RoundCPz.Text) ||
                         string.IsNullOrEmpty(RoundBlockName.Text) || string.IsNullOrEmpty(RoundRadius.Text))
                    { OutPut("请填写完整!"); return; }
                    PointInfo rcpif = new PointInfo()
                    {
                        center = new long[] { long.Parse(RoundCPx.Text), long.Parse(RoundCPy.Text), long.Parse(RoundCPz.Text), }
                    };
                    List<Point> points = new List<Point>();
                    Dictionary<Point, Point> buildPoints = new Dictionary<Point, Point>();
                    #region Func
                    bool add0dot5Fix = RoundModeToggle.IsChecked == true;
                    void AddPoint(int x, int y)
                    {
                        if (add0dot5Fix)
                        {
                            if (RoundFillMode.SelectedIndex == 0)//fill
                            {
                                if (!buildPoints.ContainsKey(new Point(x + 1, y + 1))) { buildPoints.Add(new Point(x + 1, y + 1), new Point(x + 1, -y)); }
                                if (!buildPoints.ContainsKey(new Point(-x, y + 1))) { buildPoints.Add(new Point(-x, y + 1), new Point(-x, -y)); }
                                if (!buildPoints.ContainsKey(new Point(y + 1, x + 1))) { buildPoints.Add(new Point(y + 1, x + 1), new Point(y + 1, -x)); }
                                if (!buildPoints.ContainsKey(new Point(-y, x + 1))) { buildPoints.Add(new Point(-y, x + 1), new Point(-y, -x)); }
                            }
                            else
                            {
                                points.Add(new Point(x + 1, y + 1)); points.Add(new Point(y + 1, x + 1));
                                points.Add(new Point(-x, y + 1)); points.Add(new Point(-y, x + 1));
                                points.Add(new Point(x + 1, -y)); points.Add(new Point(y + 1, -x));
                                points.Add(new Point(-x, -y)); points.Add(new Point(-y, -x));
                            }
                        }
                        else
                        {
                            if (RoundFillMode.SelectedIndex == 0)//fill
                            {
                                if (!buildPoints.ContainsKey(new Point(x, y))) { buildPoints.Add(new Point(x, y), new Point(x, -y)); }
                                if (!buildPoints.ContainsKey(new Point(-x, y))) { buildPoints.Add(new Point(-x, y), new Point(-x, -y)); }
                                if (!buildPoints.ContainsKey(new Point(y, x))) { buildPoints.Add(new Point(y, x), new Point(y, -x)); }
                                if (!buildPoints.ContainsKey(new Point(-y, x))) { buildPoints.Add(new Point(-y, x), new Point(-y, -x)); }
                            }
                            else
                            {
                                points.Add(new Point(x, y)); points.Add(new Point(y, x));
                                points.Add(new Point(-x, y)); points.Add(new Point(-y, x));
                                points.Add(new Point(x, -y)); points.Add(new Point(y, -x));
                                points.Add(new Point(-x, -y)); points.Add(new Point(-y, -x));
                            }
                        }
                    }
                    void Bresenham_Circle(int r)
                    {
                        int x, y, d;
                        x = 0;
                        y = r;
                        d = 3 - 2 * r;
                        AddPoint(x, y);
                        while (x < y)
                        {
                            if (d < 0)
                            {
                                d = d + 4 * x + 6;
                            }
                            else
                            {
                                d = d + 4 * (x - y) + 10;
                                y--;
                            }
                            x++;
                            AddPoint(x, y);
                        }
                    }
                    #endregion
                    Bresenham_Circle(int.Parse(RoundRadius.Text));
                    GetBlock = RoundBlockName.Text.Split(':', ' ');
                    block = GetBlock[0] + " ";
                    if (GetBlock.Length < 2)
                    { block += "0"; }
                    else { block += GetBlock[1]; }
                    MCFunctions.Text = $"#Convert use MCToolKit by gxh";
                    MCFunctions.Text += $"\n#{points.Count + buildPoints.Count}";
                    double offsetX = double.Parse(RoundCPx.Text);
                    double offsetY = double.Parse(RoundCPy.Text);
                    double offsetZ = double.Parse(RoundCPz.Text);
                    void OutputCmds1(double x, double y, double z)
                    { MCFunctions.Text += $"{Environment.NewLine}setblock {x} {y} {z} {block}"; }
                    void OutputCmds2(double x, double y, double z, double ex, double ey, double ez)
                    { MCFunctions.Text += $"{Environment.NewLine}fill {x} {y} {z} {ex} {ey} {ez} {block}"; }
                    switch (RoundOrientation.SelectedIndex)
                    {
                        case 0:
                            foreach (var item in points) { OutputCmds1(item.X + offsetX, offsetY, item.Y + offsetZ); }
                            foreach (var item in buildPoints) { OutputCmds2(item.Key.X + offsetX, offsetY, item.Key.Y + offsetZ, item.Value.X + offsetX, offsetY, item.Value.Y + offsetZ); }
                            break;
                        case 1:
                            foreach (var item in points) { OutputCmds1(item.X + offsetX, item.Y + offsetY, offsetZ); }
                            foreach (var item in buildPoints) { OutputCmds2(item.Key.X + offsetX, item.Key.Y + offsetY, offsetZ, item.Value.X + offsetX, item.Value.Y + offsetY, offsetZ); }
                            break;
                        case 2:
                            foreach (var item in points) { OutputCmds1(offsetX, item.Y + offsetY, item.X + offsetZ); }
                            foreach (var item in buildPoints) { OutputCmds2(offsetX, item.Key.Y + offsetY, item.Key.X + offsetZ, offsetX, item.Value.Y + offsetY, item.Value.X + offsetZ); }
                            break;
                        default:
                            break;
                    }
                    break;
                #endregion
                default:
                    break;
            }
        }
        private void SquareFillMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (SquareFillMode.SelectedIndex == 3 || SquareFillMode.SelectedIndex == 4)
                { SquareSurfacePanel.Visibility = Visibility.Visible; }
                else
                {
                    try
                    { SquareSurfacePanel.Visibility = Visibility.Collapsed; }
                    catch (Exception) { return; }
                }
            }
            catch (Exception) { return; }
        }
        #endregion
        private void TextBoxTextCheck_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Regex.IsMatch(((TextBox)sender).Text, "[^\\d-]"))
            {
                ((TextBox)sender).Text = Regex.Replace(((TextBox)sender).Text, "[^\\d-]", "");
            }
        }
        Dictionary<string, Timer> ListeningUUID = new Dictionary<string, Timer>();
        private void PerformButton_Click(object sender, RoutedEventArgs e)
        {
            JObject model = CMDmodel;
            IWebSocketConnection socket = allScokets.First(l => l.Key.ConnectionInfo.Id == (Guid)SelectedServer.SelectedItem).Key;
            allScokets[socket] = true;
            var get = MCFunctions.Text;
            DoBgTask("Performing", () =>
            {
                ListeningUUID.Clear();
                string[] FTarray = get.Replace("\r", null).Split('\n');
                for (int i = 0; i < FTarray.Length; i++)
                {
                    if (!FTarray[i].StartsWith("#"))
                    {
                        model["body"]["commandLine"] = Regex.Replace(FTarray[i], @"^/|\r", "");
                        string uuid = CreateUUID();
                        model["header"]["requestId"] = uuid;
                        ListeningUUID.Add(uuid, new Timer(5000) { AutoReset = false });
                        ListeningUUID.Last().Value.Elapsed += (timeS, Te) =>
                       {
                           try
                           {
                               ListeningUUID.Remove(ListeningUUID.First(l => l.Value == timeS).Key);
                               ((Timer)timeS).Stop();
                               ((Timer)timeS).Dispose();
                           }
                           catch (Exception) { }
                       };
                        ListeningUUID.Last().Value.Start();
                        for (int t = 0; t < 500; t++)
                        {
                            System.Threading.Thread.Sleep(10);
                            if (ListeningUUID.Count < 10)
                            { break; }
                        }
                        socket.Send(model.ToString(Newtonsoft.Json.Formatting.None));
                        Dispatcher.Invoke(() => LoadingTip.Text = $"Performing...{i * 100 / FTarray.Length}%\n{i}complete/{FTarray.Length}command");
                    }
                }
                allScokets[socket] = false;
            });
        }
        #region Loading...
        private void DoBgTask(string tip, Action action) => DoBgTask(tip, action, errm => Dispatcher.Invoke(new Action(() => OutPut($"Error:{errm}"))));
        private void DoBgTask(string tip, Action action, Action<string> onErr)
        {
            _ = Task.Run(new Action(() =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    loadingIconComplete.Kind = PackIconKind.CheckBold;
                    loadingIconComplete.Foreground = new SolidColorBrush(Colors.ForestGreen);
                    loadingIconComplete.Visibility = Visibility.Collapsed;
                    loadingProgressBar.Visibility = Visibility.Visible;
                    LoadingDialog.IsOpen = true;
                    LoadingTip.Text = tip;
                }));
                int waitTime = 10;
                try
                {
                    Task BGT = Task.Run(() =>
                    {
                        try { action.Invoke(); }
                        catch (Exception err)
                        {
                            Dispatcher.Invoke(new Action(() =>
                            {
                                loadingIconComplete.Kind = PackIconKind.FileDocumentBoxRemoveOutline;
                                loadingIconComplete.Foreground = new SolidColorBrush(Colors.DarkRed);
                            }));
                            onErr.Invoke(err.Message);
                        }
                    });
                    while (!BGT.IsCompleted)
                    {
                        System.Threading.Thread.Sleep(200);
                        waitTime += waitTime / 2;
                    }
                    BGT.Dispose();
                }
                catch (Exception err)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        loadingIconComplete.Kind = PackIconKind.FileDocumentBoxRemoveOutline;
                        loadingIconComplete.Foreground = new SolidColorBrush(Colors.DarkRed);
                    }));
                    onErr.Invoke(err.Message);
                }
                System.Threading.Thread.Sleep(Math.Max(10, Math.Min(waitTime, 1000)));
                Dispatcher.Invoke(new Action(() =>
                {
                    loadingIconComplete.Visibility = Visibility.Visible;
                    loadingProgressBar.Visibility = Visibility.Collapsed;
                }));
                System.Threading.Thread.Sleep(Math.Max(10, Math.Min(waitTime, 500)));
                Dispatcher.Invoke(new Action(() => { LoadingDialog.IsOpen = false; }));
            }));
        }
        #endregion
        Dictionary<string, JToken> GetCMDresult = new Dictionary<string, JToken>();
        private void SquarePlaceBlock1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SquarePlaceBlock1.Content.ToString() == "Cancel")
                {
                    SquarePlaceBlock2.IsEnabled = true;
                    SquarePlaceBlock1.Content = "Get Your Position";
                    return;
                }
                SquarePlaceBlock2.IsEnabled = false;
                SquarePlaceBlock1.Content = "Cancel";
                JObject model = CMDmodel;
                model["body"]["commandLine"] = "querytarget @s";
                string uuid = CreateUUID();
                GetCMDresult.Add(uuid, null);
                model["header"]["requestId"] = uuid;
                allScokets.First(l => l.Key.ConnectionInfo.Id == (Guid)SelectedServer.SelectedItem).Key.Send(model.ToString(Newtonsoft.Json.Formatting.None));
                _ = Task.Run(() =>
                     {
                         for (int i = 0; i < 500; i++)
                         {
                             try
                             {
                                 if (Dispatcher.Invoke(() => SquarePlaceBlock1.Content.ToString()) != "Cancel") { break; }
                                 System.Threading.Thread.Sleep(10);
                                 if (GetCMDresult[uuid] != null)
                                 {
                                     Dispatcher.Invoke(() =>
                                     {
                                         SquareSPx.Text = Convert.ToInt64(decimal.Parse(GetCMDresult[uuid].First()["position"]["x"].ToString()) - (decimal)0.5).ToString();
                                         SquareSPy.Text = Convert.ToInt64(decimal.Parse(GetCMDresult[uuid].First()["position"]["y"].ToString()) - (decimal)1.8).ToString();
                                         SquareSPz.Text = Convert.ToInt64(decimal.Parse(GetCMDresult[uuid].First()["position"]["z"].ToString()) - (decimal)0.5).ToString();
                                     });
                                     GetCMDresult.Remove(uuid);
                                     break;
                                 }
                             }
                             catch (Exception) { }
                         }
                         Dispatcher.Invoke(() =>
                         {
                             SquarePlaceBlock2.IsEnabled = true;
                             SquarePlaceBlock1.Content = "Get Your Position";
                         });
                     });
            }
            catch (Exception)
            {
                OutPut("未选择服务器？or遇到了错误!");
                SquarePlaceBlock2.IsEnabled = true;
                SquarePlaceBlock1.Content = "(choose a client?&)Try again";
            }
        }
        private void SquarePlaceBlock2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SquarePlaceBlock2.Content.ToString() == "Cancel")
                {
                    SquarePlaceBlock1.IsEnabled = true;
                    SquarePlaceBlock2.Content = "Get Your Position";
                    return;
                }
                SquarePlaceBlock1.IsEnabled = false;
                SquarePlaceBlock2.Content = "Cancel";
                JObject model = CMDmodel;
                model["body"]["commandLine"] = "querytarget @s";
                string uuid = CreateUUID();
                GetCMDresult.Add(uuid, null);
                model["header"]["requestId"] = uuid;
                allScokets.First(l => l.Key.ConnectionInfo.Id == (Guid)SelectedServer.SelectedItem).Key.Send(model.ToString(Newtonsoft.Json.Formatting.None));
                _ = Task.Run(() =>
                {
                    for (int i = 0; i < 500; i++)
                    {
                        try
                        {
                            if (Dispatcher.Invoke(() => SquarePlaceBlock2.Content.ToString()) != "Cancel") { break; }
                            System.Threading.Thread.Sleep(10);
                            if (GetCMDresult[uuid] != null)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    SquareEPx.Text = Convert.ToInt64(decimal.Parse(GetCMDresult[uuid].First()["position"]["x"].ToString()) - (decimal)0.5).ToString();
                                    SquareEPy.Text = Convert.ToInt64(decimal.Parse(GetCMDresult[uuid].First()["position"]["y"].ToString()) - (decimal)1.8).ToString();
                                    SquareEPz.Text = Convert.ToInt64(decimal.Parse(GetCMDresult[uuid].First()["position"]["z"].ToString()) - (decimal)0.5).ToString();
                                });
                                GetCMDresult.Remove(uuid);
                                break;
                            }
                        }
                        catch (Exception) { }
                    }
                    Dispatcher.Invoke(() =>
                    {
                        SquarePlaceBlock1.IsEnabled = true;
                        SquarePlaceBlock2.Content = "Get Your Position";
                    });
                });
            }
            catch (Exception)
            {
                OutPut("未选择服务器？or遇到了错误!");
                SquarePlaceBlock1.IsEnabled = true;
                SquarePlaceBlock2.Content = "(choose a client?&)Try again";
            }
        }
        private void RoundPlaceBlock_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SquarePlaceBlock2.Content.ToString() == "Cancel")
                {
                    RoundPlaceBlock.Content = "Get Your Position";
                    return;
                }
                RoundPlaceBlock.Content = "Cancel";
                JObject model = CMDmodel;
                model["body"]["commandLine"] = "querytarget @s";
                string uuid = CreateUUID();
                GetCMDresult.Add(uuid, null);
                model["header"]["requestId"] = uuid;
                allScokets.First(l => l.Key.ConnectionInfo.Id == (Guid)SelectedServer.SelectedItem).Key.Send(model.ToString(Newtonsoft.Json.Formatting.None));
                _ = Task.Run(() =>
                {
                    for (int i = 0; i < 500; i++)
                    {
                        try
                        {
                            if (Dispatcher.Invoke(() => RoundPlaceBlock.Content.ToString()) != "Cancel") { break; }
                            System.Threading.Thread.Sleep(10);
                            if (GetCMDresult[uuid] != null)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    RoundCPx.Text = Convert.ToInt64(decimal.Parse(GetCMDresult[uuid].First()["position"]["x"].ToString()) - (decimal)0.5).ToString();
                                    RoundCPy.Text = Convert.ToInt64(decimal.Parse(GetCMDresult[uuid].First()["position"]["y"].ToString()) - (decimal)1.8).ToString();
                                    RoundCPz.Text = Convert.ToInt64(decimal.Parse(GetCMDresult[uuid].First()["position"]["z"].ToString()) - (decimal)0.5).ToString();
                                });
                                GetCMDresult.Remove(uuid);
                                break;
                            }
                        }
                        catch (Exception) { }
                    }
                    Dispatcher.Invoke(() =>
                    {
                        RoundPlaceBlock.Content = "Get Your Position";
                    });
                });
            }
            catch (Exception)
            {
                OutPut("未选择服务器？or遇到了错误!");
                RoundPlaceBlock.Content = "(choose a client?&)Try again";
            }
        }

        private void SaveMCFButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ImportMCFButton_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
