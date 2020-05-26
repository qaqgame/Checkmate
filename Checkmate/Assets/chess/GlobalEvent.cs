using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Checkmate.Global.Data;
using QGF.Event;
using QGF.Network.FSPLite;

namespace Assets.Chess
{
    public static class GlobalEvent
    {
        
        public static QGFEvent onUpdate=new QGFEvent();//更新事件
        public static QGFEvent onFixedUpdate=new QGFEvent();

        public static QGFEvent onLoginSuccess = new QGFEvent();//登录事件
        public static QGFEvent<int, string> onLoginFailed = new QGFEvent<int, string>();//登录失败

        public static QGFEvent<GameParam,FSPParam> onGameStart = new QGFEvent<GameParam,FSPParam>();//游戏开始事件
        public static QGFEvent onGameEnd = new QGFEvent();//游戏结束事件
    }
}
