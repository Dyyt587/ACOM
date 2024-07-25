using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACOM.Models.Drivers;
internal class HAL_Driver
{
    protected bool isOpen = false;

    public string DriverName { get; set; }
        = "Unknown Device Name";
    public string DriverVersion { get; set; }
        = string.Empty;

    public virtual bool Open()
    {
        //user need change isOpen to true!!!!!!!
        return false;
    }
    public bool IsOpen()
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
