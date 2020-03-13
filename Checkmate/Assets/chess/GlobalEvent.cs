using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Chess
{
    public static class GlobalEvent
    {
        public static Action<bool> onLogin;//登录事件
        public static Action onUpdate;//更新事件
        public static Action onFixedUpdate;
    }
}
