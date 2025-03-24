using System;
using System.Collections.Generic;
using ACOMCommmon;



//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using ACOMPlugin.Core;
using DryIoc.ImTools;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ShadowPluginLoader.MetaAttributes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.




namespace WidgetPlug.Button.Helpers
{
}
namespace WidgetPlug.Button
{
[AutoPluginMeta]
public partial class Button : ACOMPluginBase
{
        IconSourceElement icon = new IconSourceElement();

        public delegate void updateData(List<CannelData> newData);
        public updateData update;
        public override string DisplayName => throw new NotImplementedException();

        public override IEnumerable<string> ResourceDictionaries => base.ResourceDictionaries;

        public Button()
        {
            icon.IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = Symbol.Comment };
           /// OnSystemCommand(new List<SystemCommandEnum>() { SystemCommandEnum.Delete });
        }
        public override FrameworkElement Create()
        {
            WidgetButton widget = new WidgetButton();
            widget.slide = this;
            update += widget.updateData;
            return widget;
        }

        //public void  onSystemCommand(object sender, List<SystemCommandEnum> cmd)
        //{
        //    base.OnSystemCommand(sender, cmd);
        //}
        //public void onUpdateCommand(object sender, List<string[]> cmd)
        //{
        //    base.OnUpdateCommand(sender, cmd);
        //}

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override string GetName()
        {
            return "滑块";
        }
        //public void UpdateData(List<CannelData> newData)
        //{

        //}

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override IconSourceElement GetIcon()
        {
             return icon;
        }

        public override string GetLabel()
        {
            return "滑块";
        }

        public override string GetTag()
        {
            return "Button";
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override void UpdateData(List<CannelData> newData)
        {
            update?.Invoke(newData);
        }

        protected override void Disable()
        {
            base.Disable();
        }

        protected override void Enable()
        {
            base.Enable();
        }

    }
}
