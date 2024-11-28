
using System.Collections.Concurrent;
using System.Net.Sockets;
using STTech.BytesIO.Kcp;
using STTech.BytesIO.Serial;
using STTech.BytesIO.Udp;
using Windows.Media.Protection.PlayReady;
using BytesIO =STTech.BytesIO;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using STTech.BytesIO.Core;
using System.IO.Ports;
using System.Diagnostics;
using static SkiaSharp.HarfBuzz.SKShaper;
/**
 * 这个类用于管理IO输入输出
 * 
 * 
 * 
 * 
 */
namespace ACOM.Models
{
    public abstract class Singleton<T> where T : class
    {
        // 这里采用实现5的方案，实际可采用上述任意一种方案
        class Nested
        {
            // 创建模板类实例，参数2设为true表示支持私有构造函数
            internal static readonly T instance = Activator.CreateInstance(typeof(T), true) as T;
        }
        private static T instance = null;
        public static T Instance { get { return Nested.instance; } }
    }


    class DataMassage {
        public enum DateSource
        {
            Serial,
            TCP,
            UDP,
            KCP,
            Others,
            Unknowns,
        }
        public readonly byte[] _data;
        public DataMassage(byte[] data, DateTime dateTime, DateSource dateSource, string userdata="")
        {
            _data = data;
            _dateTime = dateTime;
            _dateSource = dateSource;
            this.userdata = userdata;
        }
        public DateTime _dateTime;
        public DateSource _dateSource;
        public string userdata;
    };
    class IO_Manage: Singleton<IO_Manage>
    {

        private void Client_OnExceptionOccurs(object sender, STTech.BytesIO.Core.ExceptionOccursEventArgs e)
        {
            Print($"异常: {e.Exception.Message}");
        }

        private void Client_OnDataSent(object sender, STTech.BytesIO.Core.DataSentEventArgs e)
        {
            Print($"发送: {e.Data.ToHexCodeString()}({e.Data.EncodeToString()})");
        }

        private void Client_OnDataReceived(object sender, STTech.BytesIO.Core.DataReceivedEventArgs e)
        {
            BytesIO.Serial.SerialClient client = (BytesIO.Serial.SerialClient)sender;
            Print($"接收: {e.Data.ToHexCodeString()}({e.Data.EncodeToString()})");
            charRecQueue.Enqueue(new DataMassage(e.Data,DateTime.Now,DataMassage.DateSource.Serial, client.PortName));
        }

        private void Client_OnDisconnected(object sender, STTech.BytesIO.Core.DisconnectedEventArgs e)
        {
            Print("断开连接");
        }

        private void Client_OnConnectionFailed(object sender, STTech.BytesIO.Core.ConnectionFailedEventArgs e)
        {
            Print("连接失败");
        }

        private void Client_OnConnectedSuccessfully(object sender, STTech.BytesIO.Core.ConnectedSuccessfullyEventArgs e)
        {
            Print("连接成功");
        }

        List<BytesIO.Serial.SerialClient>  serialClients = new();
        List<BytesIO.Tcp.TcpClient> tcpClients= new();
        List<BytesIO.Kcp.KcpClient> kcpClients= new();
        List<BytesIO.Udp.UdpClient> udpClients = new();
        ConcurrentQueue<DataMassage> charRecQueue=new();

        public SerialClient Connect(string portName,int baudRate=115200,int dataBits=8,
            Parity parity = Parity.None, StopBits stopBits = StopBits.One)
        {
            SerialClient __client;
            foreach (BytesIO.Serial.SerialClient item in serialClients)
            {
                if (item.PortName == portName)
                {
                    __client = item;
                    goto NOT_INIT;
                }
            }
             __client = new SerialClient();
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
                serialClients.Add(__client);
            }
            else
            {

                __client = null;
            }
            return __client;
        }

        public bool DisConnect(SerialClient client)
        {
            DisconnectResult result = client.Disconnect();
            return result.IsSuccess;
        }
        public bool DisConnect(string portName)
        {
            foreach (BytesIO.Serial.SerialClient item in serialClients)
            {
               if(item.PortName == portName)
                {
                    DisconnectResult result = item.Disconnect();
                    return result.IsSuccess;
                }
            }
            return false;

        }
        private void Print(string msg)
        {
            Debug.WriteLine(msg);
            //object value = Invoke(new EventHandler(delegate
            //{
            //    tbRecv.AppendText($"[{DateTime.Now.ToLongTimeString()}] {msg}\r\n");
            //}));
        }

        IO_Manage() { }
        ~IO_Manage() { }
    }
}
