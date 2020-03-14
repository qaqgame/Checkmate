using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QGF.Event;

namespace Assets.Chess
{
    public static class GlobalEvent
    {
        public static QGFEvent<bool> onLogin=new QGFEvent<bool>();//登录事件
        public static QGFEvent onUpdate=new QGFEvent();//更新事件
        public static QGFEvent onFixedUpdate=new QGFEvent();
    }
}
