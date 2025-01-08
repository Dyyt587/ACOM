
using System.Collections.Concurrent;
using System.Net.Sockets;
using STTech.BytesIO.Kcp;
using STTech.BytesIO.Serial;
using STTech.BytesIO.Udp;
using Windows.Media.Protection.PlayReady;
using BytesIO =STTech.BytesIO;
using System;
using System.Collections.Generic;
//using System.Windows.Forms;
using STTech.BytesIO.Core;
using System.IO.Ports;
using System.Diagnostics;
//using static SkiaSharp.HarfBuzz.SKShaper;
using ACOMv2.Views;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
//using CommunityToolkit.WinUI.UI.Controls.TextToolbarSymbols;
using Microsoft.Windows.Management.Deployment;
using System.Xml.Linq;
using Windows.Gaming.Preview.GamesEnumeration;
using System.Text;
using System.Runtime.CompilerServices;
using ACOMv2.Models.Processers;

using System;
using System.Runtime.InteropServices;
/**
 * 这个类用于管理IO输入输出
 * 
 * 
 * 
 * 
 */
namespace ACOM.Models
{
    using System.Drawing;
    using System.Management;
    //using System.Management;
    using System.Reflection;
    using System.Timers;
    using ACOMCommmon;
    using ACOMPlug;
    using ACOMv2.Persistence;
    using Amib.Threading;
    using CommunityToolkit.Mvvm.Messaging;
    using CommunityToolkit.Mvvm.Messaging.Messages;
    using Microsoft.Extensions.FileSystemGlobbing;
    using STTech.BytesIO.Tcp;
    using Windows.System;
    using WinUICommunity;
    using static ACOM.Models.IO_Manage;

    public class ChannelProcesser
    {
        //List<ChannelMassage> _data = new List<ChannelMassage>(8);
 
        List<byte> tmpbytes = new List<byte>(4096);
        List<byte[]> frameDatas = new();
        static int unNameIndex = 0;
        DateTime LastTime;
        Queue<RawDataMassage> noPloicyRawDataMassages = new Queue<RawDataMassage>(20);
        public RawDataMassage GetNopolicyData()
        {
            if (noPloicyRawDataMassages.Count > 0)
            {
                return noPloicyRawDataMassages.Dequeue();
            }
            else
            {
                return null;
            }
        }


        public ChannelProcesser()
        {
            
        }
         ~ChannelProcesser()
        {

        }


        public    void PrintProcessedData(List<ChannelMassage> data)
        {
            if (data == null)
            {
                Debug.WriteLine("No data\n");
                return;
            }
            foreach (var item in data)
            {
                Debug.WriteLine($"Name: {item.Name}, Type: {item.Type}, DateTime: {item.DateTime}");

                if (item.rawChannelData != null)
                {
                    Debug.WriteLine("Raw Data:");
                    Debug.WriteLine("Char\tHex");
                    for (int i = 0; i < item.rawChannelData.Length; i++)
                    {
                        char character = item.rawChannelData[i] >= 32 && item.rawChannelData[i] <= 126 ? (char)item.rawChannelData[i] : ' ';
                        string hexValue = item.rawChannelData[i].ToString("X2");
                        Debug.WriteLine($"{character}\t{hexValue}");
                    }
                }
                else
                {
                    Debug.WriteLine("Raw Data: null");
                }
                Debug.WriteLine(" ");
            }
        }

        //public bool Date_Search(string IsName)
        //{
        //    foreach (ChannelMassage item in _data)
        //    {
        //        if (item.Name.CompareTo(IsName)==0)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
        /// <summary>
        /// 用于处理传入的数据,并且当数据完成一次分包后返回有效数据，否则为null
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<ChannelMassage> Process(RawDataMassage data)
        {
            Debug.WriteLine("数据处理");
            foreach (byte b in data._data)
            {
                if (b == '\n')
                {
                    frameDatas.Add(tmpbytes.ToArray());
                    tmpbytes.Clear();
                }
                else
                {
                    tmpbytes.Add(b);
                }

            }
            if (frameDatas.Count != 0)
            {
                List<ChannelMassage> frames = new List<ChannelMassage>(frameDatas.Count);
                var span = (DateTime.Now - LastTime)/ frameDatas.Count;
                int j = frameDatas.Count;
                foreach (var frame in frameDatas)
                {
                    j--;
                    string frameString = Encoding.UTF8.GetString(frame);
                    if (frameString.Contains(":"))
                    {
                        var parts = frameString.Split(':');
                        var leftPart = string.Join(":", parts.Take(parts.Length - 1)).Split(',').ToList();
                        var rightPart = parts.Last().Split(',').ToList();

                        for (int i = 0; i < Math.Max(leftPart.Count, rightPart.Count); i++)
                        {
                            ChannelMassage channelMassage = new ChannelMassage
                            {
                                rawChannelData = i < rightPart.Count ? Encoding.UTF8.GetBytes(rightPart[i]) : null,
                                Name = i < leftPart.Count ? leftPart[i] : "AutoName" + unNameIndex++,
                                Type = "Processed",
                                DateTime = data._dateTime - span * j
                            };
                            frames.Add(channelMassage);
                        }
                    }
                    else
                    {
                        noPloicyRawDataMassages.Enqueue(new RawDataMassage(frame, data._dateTime, data._dateSource));
                    }
                    unNameIndex = 0;
                }
                LastTime = DateTime.Now;
                frameDatas.Clear();
                return frames;
            }
            else return null;
        }

        protected  string IdentifyDataType(byte[] data, string Type = "Auto")
        {
            if (data == null || data.Length == 0)
            {
                Debug.WriteLine("Empty or null data");
                return "Empty or null data";
            }

            // 尝试将byte数组转换为String，使用默认的ANSI编码
            try
            {
                string ansiString = Encoding.Default.GetString(data);
                // 如果转换成功，且没有抛出DecoderFallbackException异常，则可能是ANSI编码的字符串
                //Debug.WriteLine("ANSI encoded string");
                return "str (ANSI encoding)";
            }
            catch (DecoderFallbackException)
            {
                // 如果转换失败，说明不是有效的ANSI编码
                try
                {
                    string strGbk = Encoding.GetEncoding("GBK").GetString(data);
                    return "GBK: " + strGbk;
                }
                catch (Exception)
                {
                    // 如果GBK转换也失败，返回错误信息
                    //return "无法识别的编码";
                }
            }

            // 根据字节长度判断基本数据类型
            switch (data.Length)
            {
                case 1:
                    Debug.WriteLine("byte or sbyte");
                    return "byte or sbyte";
                case 2:
                    Debug.WriteLine("short or ushort");
                    return "short or ushort";
                case 4:
                    Debug.WriteLine("int, uint, float, or ANSI encoded string (if length is 1)");
                    return "int, uint, float, or ANSI encoded string (if length is 1)";
                case 8:
                    Debug.WriteLine("long, ulong, double");
                    return "long, ulong, double";
                default:
                    break;
            }

            // 如果无法确定，返回其他
            Debug.WriteLine("unknown");
            return "unknown";
        }
        /// <summary>
        /// 用于处理传入的数据,并且将数据解析为需要显示的内容
        /// </summary>
        /// <param name="massages"></param>
        /// <returns></returns>
        public List<CannelData> Process(List<ChannelMassage> massages)
        {
           // Debug.WriteLine("get msg cnt is"+massages.Count());
            List < CannelData > result = new List<CannelData>();
            for (int i = 0; i < massages.Count; i++)
            {
                var data = massages[i].rawChannelData;
                var type = IdentifyDataType(data);

                var tt = new CannelData(massages[i].Name, System.Text.Encoding.UTF8.GetString(data));
                result.Add(tt);

            }
            return result;

        }
    }


    public class SerialDevice
    {
        public string PortName;
        public string FriendlyName;
        public bool IsConnect;

    }


    class IoDevice {

        BytesClient client;
        public string Name= "Unknown Name";
        public IoDevice(BytesClient client)
        {
            this.client = client;
            Name = client.ToString();
        }
        public IoDevice(BytesClient client, string Name)
        {
            this.client = client;
            this.Name = Name;
        }

    }


    class IO_Manage : Singleton<IO_Manage>
    {
        //页面文件
        public HomeLandingPage page;



        public PersistenceManager persistence = PersistenceManager.Instance;

        SmartThreadPool smartThreadPool = new SmartThreadPool();

        Queue<byte[]> bytes = new();
        //public Vector<ChannelMassage> ChannelDatas;

        /// <summary>
        /// 测试用的解析代码
        /// </summary>
          ChannelProcesser channelProcesser = new();

        /// <summary>
        /// 全局通道数据和通道显示数据
        /// </summary>
          Dictionary<string, ChannelMassage> GlobChannelMassage = new Dictionary<string, ChannelMassage>();
          List<CannelData> GlobChannelViewData = new();

        /// <summary>
        /// 定义委托定义事件
        /// </summary>
        public delegate void ReceivedCannelMsg(Dictionary<string, ChannelMassage> dataMap);
        public delegate void UpdateCannelViewMsg(List<CannelData> globChannelViewData);
        public delegate void UpdateDevices(List<SerialDevice> serialDevices);
        public delegate void ConnectLost(object sender, STTech.BytesIO.Core.DisconnectedEventArgs e);
        public delegate void UpdateLoadStats(object sender,double load_s);
        public   event ReceivedCannelMsg receivedCannelMsg;//当接收到通道原始数据时触发
        public   event UpdateCannelViewMsg updateCannelViewMsg;//当接收到通道处理显示数据时触发
        public   event UpdateDevices updateDevices;//当外部设备变化触发
        public   event ConnectLost connectLost;//当外部设备变化触发
        public   event UpdateLoadStats updateLoadStats;//当外部设备变化触发
        
        /**
         * 定义已经连接的设备
         */
        //List<BytesIO.Serial.SerialClient> serialClients = new();
        //List<BytesIO.Tcp.TcpClient> tcpClients = new();
        //List<BytesIO.Kcp.KcpClient> kcpClients = new();
        //List<BytesIO.Udp.UdpClient> udpClients = new();

        ConcurrentQueue<RawDataMassage> rawdataQueue = new();

        /// <summary>
        /// 插件管理
        /// </summary>
        Dictionary<object, IPlugProcessBase> channelProcesserMap = new();

          Dictionary<string, BytesIO.Serial.SerialClient> Dic_Serial = new();
          Dictionary<string, BytesIO.Tcp.TcpClient> Dic_Tcp = new();
          Dictionary<string, BytesIO.Kcp.KcpClient> Dic_Kcp = new();
          Dictionary<string, BytesIO.Udp.UdpClient> Dic_Udp = new();

        /// <summary>
        /// 已经存在的串口设备
        /// </summary>
        List<SerialDevice> serialDevices = new();

        // 串口负载率统计
        private Dictionary<string, long> serialLoadStats = new();
        private Dictionary<string, List<Double>> serialLoad = new();
        Stopwatch stopwatch1 = new Stopwatch();
        private DateTime LastTime;
        /// <summary>
        /// Union the glob channel massage.
        /// </summary>
        /// <param name="Data"></param>
        private   void UnionGlobChannelMassage(List<ChannelMassage> Data)
        {
            if (Data == null) return;

            foreach (var pair in Data)
            {
                if (GlobChannelMassage.ContainsKey(pair.Name)) GlobChannelMassage[pair.Name] = pair;
                else GlobChannelMassage.Add(pair.Name, pair);

                // result.Add(pair.Key, pair.Value);
            }
            //触发钩子函数
            receivedCannelMsg?.Invoke(GlobChannelMassage);

            //Debug 显示使用
            //ChannelProcesser.PrintProcessedData(GlobChannelMassage.Values.ToList());

            UnionGlobChannelViewMassage(channelProcesser.Process(Data));//更新成通道数据
        }

        /// <summary>
        /// Union the glob channel view massage.
        /// </summary>
        /// <param name="Data"></param>
        private   void UnionGlobChannelViewMassage(List<CannelData> Data)
        {
            if (Data == null) return;

            foreach (var dataItem in Data)
            {
                var existingItem = GlobChannelViewData.FirstOrDefault(item => item.DataName == dataItem.DataName);
                if (existingItem != null)
                {
                    // Replace existing item with new data
                    existingItem.Data = dataItem.Data;
                    existingItem.DateTime = dataItem.DateTime;
                }
                else
                {
                    dataItem.DataColor = ACOMCommmon.ColorHelper.AutoGetColor(GlobChannelViewData.Count);
                    // Add new item to the collection
                    GlobChannelViewData.Add(dataItem);
                }
            }

            if (updateCannelViewMsg != null)
                updateCannelViewMsg(GlobChannelViewData);

            //Debug.WriteLine("更新通道显示数据 " + Data.Count + " ");
        }

        private void Client_OnExceptionOccurs(object sender, STTech.BytesIO.Core.ExceptionOccursEventArgs e)
        {
            Print($"异常: {e.Exception.Message}");
        }

        private void Client_OnDataSent(object sender, STTech.BytesIO.Core.DataSentEventArgs e)
        {
            Print($"发送: {e.Data.ToHexCodeString()}({e.Data.EncodeToString()})");
        }

        private void ProcessReceivedDataAsync(byte[] data, string portName)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            bytes.Enqueue(data);
            _ = smartThreadPool.QueueWorkItem(() =>
            {
                byte[] d = bytes.Dequeue();
                var tt = channelProcesser.Process(new RawDataMassage(d, DateTime.Now, RawDataMassage.DateSource.Serial, portName));
                UnionGlobChannelMassage(tt);

                Debug.WriteLine("ProcessReceivedDataAsync");
                rawdataQueue.Enqueue(new RawDataMassage(d, DateTime.Now, RawDataMassage.DateSource.Serial, portName));
                page.TextAddLine(d.EncodeToString());
                WeakReferenceMessenger.Default.Send(new ValueChangedMessage<List<CannelData>>(GlobChannelViewData));

                _ = persistence.SaveReceivedDataToFileAsync(this, d);
                serialLoadStats[portName] += d.Length;

                if ((DateTime.Now - LastTime).TotalMilliseconds > 1000)
                {
                    stopwatch1.Stop();
                    LastTime = DateTime.Now;

                    updateLoadStats?.Invoke(portName, serialLoadStats[portName]);
                    serialLoadStats[portName] = 0;
                    stopwatch1.Start();
                }
            });
            stopwatch.Stop();
            Debug.WriteLine($"Client_OnDataReceived execution time: {stopwatch.Elapsed.TotalNanoseconds} us");

        }

        private void Client_OnDataReceived(object sender, STTech.BytesIO.Core.DataReceivedEventArgs e)
        {
            BytesIO.Serial.SerialClient client = (BytesIO.Serial.SerialClient)sender;

            ProcessReceivedDataAsync(e.Data, client.PortName);
        }

        private void Client_OnDisconnected(object sender, STTech.BytesIO.Core.DisconnectedEventArgs e)
        {
            Print("断开连接");
            channelProcesserMap.Remove(sender);
            connectLost?.Invoke(sender, e);
        }

        private void Client_OnConnectionFailed(object sender, STTech.BytesIO.Core.ConnectionFailedEventArgs e)
        {
            Print("连接失败");
        }

        private void Client_OnConnectedSuccessfully(object sender, STTech.BytesIO.Core.ConnectedSuccessfullyEventArgs e)
        {
            Print("连接成功");
            //IPlugProcessBase plug = Plugs.CreateInstance("ProcesserPlugWaterFire");
            //if(plug != null)
            //{
            //    channelProcesserMap.Add(sender, plug);
            //}
            serialLoadStats.Add(((BytesIO.Serial.SerialClient)sender).PortName, 0);
            serialLoad.Add(((BytesIO.Serial.SerialClient)sender).PortName, new());
        }

        public SerialClient Connect(string portName, int baudRate = 115200, int dataBits = 8,
            Parity parity = Parity.None, StopBits stopBits = StopBits.One)
        {
            SerialClient __client;
            foreach (BytesIO.Serial.SerialClient item in Dic_Serial.Values)
            {
                if (item.PortName == portName)
                {
                    __client = item;
                    goto NOT_INIT;
                }
            }
            __client = new SerialClient();
            __client.ReceiveBufferSize = 1024 * 1024;
            // 监听连接成功事件
            __client.OnConnectedSuccessfully += Client_OnConnectedSuccessfully;
            // 监听连接失败事件
            __client.OnConnectionFailed += Client_OnConnectionFailed;
            // 监听断开连接事件
            __client.OnDisconnected += Client_OnDisconnected;
            // 监听接收数据事件
            __client.OnDataReceived += Client_OnDataReceived;
            // 监听发送数据事件
            __client.OnDataSent += Client_OnDataSent;
            // 监听发生异常事件
            __client.OnExceptionOccurs += Client_OnExceptionOccurs;

        NOT_INIT:
            __client.PortName = portName;
            __client.BaudRate = baudRate;
            __client.DataBits = dataBits;
            __client.Parity = parity;
            __client.StopBits = stopBits;

            ConnectResult result = __client.Connect();
            if (result.IsSuccess || (result.ErrorCode == ConnectErrorCode.IsConnected))
            {
                //serialClients.Add(__client);

                Dic_Serial.Add(portName, __client);
                //Thread t = new Thread(new ThreadStart(Work));
                //t.Start();

            }
            else
            {
                __client = null;
            }
            return __client;
        }


        public TcpClient Connect()
        {
            return null;
        }

        public TcpServer Connect(string a)
        {
            return null;
        }

        public TcpListener Connect(string s,string b)
        {
            return null;

        }




        public bool DisConnect(SerialClient client)
        {
            DisconnectResult result = client.Disconnect();
            return result.IsSuccess;
        }

        public bool DisConnect(string portName)
        {
            try
            {
                Dic_Serial[portName].Disconnect();
                Dic_Serial.Remove(portName);
                return true;
            }
            catch (Exception e)
            {
                _ = e;
                return false;
            }
        }

        private void Print(string msg)
        {
            Debug.WriteLine(msg);
            //object value = Invoke(new EventHandler(delegate
            //{
            //    tbRecv.AppendText($"[{DateTime.Now.ToLongTimeString()}] {msg}\r\n");
            //}));
        }

        public int doing = 1;
 
        public void updateSerialDevce()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // 更新串口设备列表和友好名称
            serialDevices.Clear();
            foreach (string portName in SerialPort.GetPortNames())
            {
                SerialDevice device = new SerialDevice
                {
                    PortName = portName,
                    FriendlyName = GetFriendlyName(portName),
                    IsConnect = false
                };
                serialDevices.Add(device);
            }

            stopwatch.Stop();
            //Debug.WriteLine($"updateSerialDevce execution time: {stopwatch.ElapsedMilliseconds} ms");

            if (updateDevices != null)
                updateDevices(serialDevices);
        }

        private void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            if (doing == 1)
            {
                doing = 0;
                //Print("update" + e.ToString() + sender.GetType().ToString());
                System.Timers.Timer timer = new System.Timers.Timer();
                timer.Interval = 100; // 设置时间间隔为100毫秒（0.1秒）
                timer.Elapsed += new System.Timers.ElapsedEventHandler(((sender ,e) => {
                    IO_Manage.Instance.doing = 1;
                    IO_Manage.Instance.updateSerialDevce();
                }));
                timer.AutoReset = false; // 设置定时器在触发一次后不自动重置
                timer.Start();
            }
            else
            {
                return;
            }
        }

        private Dictionary<string, string> portFriendlyNameCache = new Dictionary<string, string>();
 
        public string GetFriendlyName(string portName)
        {
            if (portFriendlyNameCache.ContainsKey(portName))
            {
                return portFriendlyNameCache[portName];
            }

            try
            {
                var searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%({portName})%'");
                
                    foreach (var obj in searcher.Get())
                    {
                        var name = obj["Name"]?.ToString();
                        if (!string.IsNullOrEmpty(name))
                        {
                            portFriendlyNameCache[portName] = name;
                            return name;
                        }
                    }
                searcher = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting friendly name for {portName}: {ex.Message}");
            }

            return "error name";
        }

 

        IO_Manage()
        {
            //ManagementEventWatcher watcher;

            ////创建ManagmentEventWatcher 对象
            //watcher = new ManagementEventWatcher("SELECT *FROM Win32_DeviceChangeEvent WHERE EventType = 2 or EventType = 3 ");
            ////添加设备变化事件处理程序
            //watcher.EventArrived += Watcher_EventArrived;
            //开始监听
            //watcher.Start();

            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 100; // 设置时间间隔为100毫秒（0.1秒）
            timer.Elapsed += new System.Timers.ElapsedEventHandler(((sender, e) =>
            {
                IO_Manage.Instance.doing = 1;
                IO_Manage.Instance.updateSerialDevce();
            }));
            timer.AutoReset = true; // 设置定时器在触发一次后不自动重置
            timer.Start();

            smartThreadPool.MaxQueueLength = 128;
            smartThreadPool.MinThreads = 8;
            smartThreadPool.MaxThreads = 32;


   
            _ = Plugs.InitAsync("C:\\Users\\80520\\source\\repos\\ACOM\\ACOMv2\\Assets\\Plugs\\");
        }


        public  void SerialSend(string portName, byte[] bytes)
        {
            if (Dic_Serial.TryGetValue(portName, out var client))
            {
                client.Send(bytes);
                _=persistence.SaveSendDataToFileAsync(client, bytes);

            }
            else
            {
                Debug.WriteLine($"Port {portName} not found.");
            }
        }
        ~IO_Manage() { }
    }

    public static class ByteArrayExtensions
    {
        public static string EncodeToString(this byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }
    }
}
