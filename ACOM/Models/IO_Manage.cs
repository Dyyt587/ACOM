
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
/**
 * 这个类用于管理IO输入输出
 * 
 * 
 * 
 * 
 */
namespace ACOM.Models
{
    class IO_Manage
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
            Print($"接收: {e.Data.ToHexCodeString()}({e.Data.EncodeToString()})");
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

        List<BytesIO.Serial.SerialClient>  serialClients;
        List<BytesIO.Tcp.TcpClient> tcpClients;
        List<BytesIO.Kcp.KcpClient> kcpClients;
        List<BytesIO.Udp.UdpClient> udpClients;
        ConcurrentQueue<char> charQueue;

        public SerialClient Connect(string portName,int baudRate=115200,int dataBits=8,
            Parity parity = Parity.None, StopBits stopBits = StopBits.One)
        {
            SerialClient __client = new SerialClient();
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

            __client.PortName = portName;
            __client.BaudRate = baudRate;
            __client.DataBits = dataBits;
            __client.Parity = parity;
            __client.StopBits = stopBits;

            ConnectResult result = __client.Connect();
            if (result.IsSuccess)
            {
                serialClients.Add(__client);
                return __client;
            }
            else
            {
                return null;
            }
        }
        private void Print(string msg)
        {
            //object value = Invoke(new EventHandler(delegate
            //{
            //    tbRecv.AppendText($"[{DateTime.Now.ToLongTimeString()}] {msg}\r\n");
            //}));
        }

        IO_Manage()
        {

        }
        ~IO_Manage() { }
    }
}
