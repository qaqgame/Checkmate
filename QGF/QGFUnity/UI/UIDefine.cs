using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QGF.Unity.UI
{
    public enum UITypeDef
    {
        Unknown=0,
        Page=1,
        Window=2,
        Widget=3,
        Loading=4
    }

    public class UILayerDef
    {
        public const int Background = 0;
        public const int Page = 1000;
        public const int NormalWindow = 2000;
        public const int TopWindow = 3000;
        public const int Widget = 4000;
        public const int Loading = 5000;
        public const int Unknown = 9999;

        public static int GetDefaultLayer(UITypeDef type)
        {
            switch (type)
            {
                case UITypeDef.Loading: return Loading;
                case UITypeDef.Widget: return Widget;
                case UITypeDef.Window: return NormalWindow;
                case UITypeDef.Page: return Page;
                case UITypeDef.Unknown: return Unknown;
                default: return Unknown;
            }
        }
    }

}
