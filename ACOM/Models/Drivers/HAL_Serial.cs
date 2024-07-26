using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using STTech.BytesIO;
using STTech.BytesIO.Core;
using STTech.BytesIO.Serial;
using Windows.Media.Protection.PlayReady;

namespace ACOM.Models.Drivers;
internal class HAL_Serial :  HAL_Driver
{
    private string[] _serialPorts;
    public HAL_Serial() {
        // 创建串口通信客户端
        client = new SerialClient();
        client.WriteTimeout = 1000;
        // 向下拉选项框中添加所有串口名称
        _serialPorts = client.GetPortNames();

        // 监听连接成功事件
        client.OnConnectedSuccessfully += Client_OnConnectedSuccessfully;
        // 监听连接失败事件
        client.OnConnectionFailed += Client_OnConnectionFailed;
        // 监听断开连接事件
        client.OnDisconnected += Client_OnDisconnected;
        // 监听接收数据事件
        client.OnDataReceived += Client_OnDataReceived;
        // 监听发送数据事件
        client.OnDataSent += Client_OnDataSent;
        // 监听发生异常事件
        client.OnExceptionOccurs += Client_OnExceptionOccurs;
    }

    private void Client_OnExceptionOccurs(object sender, ExceptionOccursEventArgs e)
    {
        Debug.WriteLine($"异常: {e.Exception.Message}");
    }

    private void Client_OnDataSent(object sender, DataSentEventArgs e)
    {
        Debug.WriteLine($"发送: {e.Data.ToHexCodeString()}({e.Data.EncodeToString()})");
    }

    private void Client_OnDataReceived(object sender, STTech.BytesIO.Core.DataReceivedEventArgs e)
    {
        Debug.WriteLine($"接收: {e.Data.ToHexCodeString()}({e.Data.EncodeToString()})");
    }

    private void Client_OnDisconnected(object sender, DisconnectedEventArgs e)
    {
        Debug.WriteLine("断开连接");
    }

    private void Client_OnConnectionFailed(object sender, ConnectionFailedEventArgs e)
    {
        Debug.WriteLine("连接失败");
    }

    private void Client_OnConnectedSuccessfully(object sender, ConnectedSuccessfullyEventArgs e)
    {
        Debug.WriteLine("连接成功");
    }

    public bool Open(string portName)
    {    
        client.PortName = portName;
        client.Connect();
        return client.IsConnected;
    }
    public override bool IsOpen()
    {
        return client.IsConnected;
    }
    public override void Close()
    {
        isOpen = false;
        client.Disconnect();
    }

    public int Write(byte[] bytes)
    {
        client.Send(bytes);

        return bytes.Length;
    }
    public override int Readable()
    {
        return 0;
    }
    public override int Read(byte[] bytes, int size)
    {
        return 0;
    }
    public void AddReceiveHandler(object sender, STTech.BytesIO.Core.DataReceivedEventArgs e)
    {
        return;
    }


}




