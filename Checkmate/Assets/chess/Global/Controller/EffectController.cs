using Checkmate.Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Game.Controller
{
    public abstract class EffectController:BaseController
    {
        public DataMap Temp=new DataMap(), Current=new DataMap();

        public virtual string GetIcon() { return null; }
        public virtual string GetString() { return ""; }
    }
}
