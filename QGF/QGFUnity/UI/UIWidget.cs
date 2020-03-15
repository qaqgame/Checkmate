using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QGF.Unity.UI
{
    public abstract class UIWidget:UIPanel
    {
        public override UITypeDef UIType { get { return UITypeDef.Widget; } }
    }
}
