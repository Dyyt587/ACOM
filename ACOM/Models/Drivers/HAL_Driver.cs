using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STTech.BytesIO.Serial;

namespace ACOM.Models.Drivers;
internal class HAL_Driver
{
    protected bool isOpen = false;
    // 串口连接客户端
    protected SerialClient client;
    public string DriverName { get; set; }
        = "Unknown Device Name";
    public string DriverVersion { get; set; }
        = string.Empty;


    public HAL_Driver()
    {
      

    }

    public virtual bool Open()
    {
        //user need change isOpen to true!!!!!!!
        return false;
    }
    public virtual bool IsOpen()
    {
        return isOpen;
    }
    public virtual void Close()
    {
        isOpen = false;
    }

    public virtual int Write(byte[] bytes,int size)
    {
        return 0;
    }
    public virtual int Readable()
    {
        return 0;
    }
    public virtual int Read(byte[] bytes, int size)
    {
        return 0;
    }
    public virtual void AddReceiveHandler()
    {
        return ;
    }
}
