using Assets.Chess;
using QGF.Common;
using QGF.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Services.Version
{
    public class VersionManager:Singleton<VersionManager>
    {
        public static QGFEvent onUpdateComplete = new QGFEvent();
        public static QGFEvent<float> onUpdateProgress = new QGFEvent<float>();
        private float m_progress = 0;

        public void Init()
        {
            GlobalEvent.onUpdate.AddListener(OnUpdate);
        }


        public void Clean()
        {
            GlobalEvent.onUpdate.RemoveListener(OnUpdate);
        }

        private void OnUpdate()
        {
            m_progress += 0.05f;
            if (m_progress > 1)
            {
                m_progress = 1;
            }
            onUpdateProgress.Invoke(m_progress);

            Console.Write("模拟版本更新:" + (int)(m_progress * 100) + "%\r");

            if (m_progress >= 1)
            {
                Console.WriteLine();
                GlobalEvent.onUpdate.RemoveListener(OnUpdate);
                onUpdateComplete.Invoke();
            }
        }
    }
}
