using System;
using System.Collections.Generic;

namespace QGF.Event
{
    public class QGFEventBase
    {
        protected List<Delegate> m_methods;
        private int m_removed = 0;
        private const float MaxDirty = 0.50f;
        

        public QGFEventBase()
        {
            m_methods = new List<Delegate>();
        }

        protected void AddListener(Delegate del)
        {
            int c = m_methods.Count;
            for (int i = 0; i < c; i++)
            {
                if (m_methods[i] == del)
                {
                    return;
                }
            }
            m_methods.Add(del);
        }

        protected void RemoveListener(Delegate del)
        {
            int c = m_methods.Count;
            for (int i = 0; i < c; i++)
            {
                if (m_methods[i] == del)
                {
                    m_methods[i] = null;
                    m_removed++;
                }
            }
        }

        public void RemoveAllListeners()
        {
            m_methods.Clear();
        }

        protected void ClearEvent()
        {
            if (m_methods.Count == 0 || m_removed / m_methods.Count < MaxDirty)
            {
                return;
            }

            int c = m_methods.Count - 1;
            for (int i = c; i >= 0; i--)
            {
                if (m_methods[i] == null)
                {
                    m_methods.RemoveAt(i);
                }
            }

            m_removed = 0;
        }

        public void Invoke()
        {
            ClearEvent();
            int c = m_methods.Count;
            for (int i = 0; i < c; i++)
            {
                var method = m_methods[i] as Action;
                if (method != null)
                {
                    method();
                }
            }
        }

    }

    public class QGFEvent : QGFEventBase
    {
        public static QGFEvent operator +(QGFEvent p1, Action p2)
        {
            p1.AddListener(p2);
            return p1;
        }

        public static QGFEvent operator -(QGFEvent p1, Action p2)
        {
            p1.RemoveListener(p2);
            return p1;
        }

        public void AddListener(Action a)
        {
            base.AddListener(a);
        }

        public void RemoveListener(Action a)
        {
            base.RemoveListener(a);
        }
    }

    public class QGFEvent<T> : QGFEventBase
    {
        public static QGFEvent<T> operator +(QGFEvent<T> p1, Action<T> p2)
        {
            p1.AddListener(p2);
            return p1;
        }

        public static QGFEvent<T> operator -(QGFEvent<T> p1, Action<T> p2)
        {
            p1.RemoveListener(p2);
            return p1;
        }

        public void AddListener(Action<T> a)
        {
            base.AddListener(a);
        }

        public void RemoveListener(Action<T> a)
        {
            base.RemoveListener(a);
        }

        public void Invoke(T t)
        {
            ClearEvent();
            int c = m_methods.Count;
            for (int i = 0; i < c; i++)
            {
                var method = m_methods[i] as Action<T>;
                if (method != null)
                {
                    method(t);
                }
            }
        }
    }

    public class QGFEvent<T1, T2> : QGFEventBase
    {
        public static QGFEvent<T1, T2> operator +(QGFEvent<T1, T2> p1, Action<T1, T2> p2)
        {
            p1.AddListener(p2);
            return p1;
        }

        public static QGFEvent<T1, T2> operator -(QGFEvent<T1, T2> p1, Action<T1, T2> p2)
        {
            p1.RemoveListener(p2);
            return p1;
        }

        public void AddListener(Action<T1, T2> a)
        {
            base.AddListener(a);
        }

        public void RemoveListener(Action<T1, T2> a)
        {
            base.RemoveListener(a);
        }

        public void Invoke(T1 t1, T2 t2)
        {
            ClearEvent();
            int c = m_methods.Count;
            for (int i = 0; i < c; i++)
            {
                var method = m_methods[i] as Action<T1,T2>;
                if (method != null)
                {
                    method(t1,t2);
                }
            }
        }
    }

    public class QGFEvent<T1, T2, T3> : QGFEventBase
    {
        public static QGFEvent<T1, T2, T3> operator +(QGFEvent<T1, T2, T3> p1, Action<T1, T2, T3> p2)
        {
            p1.AddListener(p2);
            return p1;
        }

        public static QGFEvent<T1, T2, T3> operator -(QGFEvent<T1, T2, T3> p1, Action<T1, T2, T3> p2)
        {
            p1.RemoveListener(p2);
            return p1;
        }

        public void AddListener(Action<T1, T2, T3> a)
        {
            base.AddListener(a);
        }

        public void RemoveListener(Action<T1, T2, T3> a)
        {
            base.RemoveListener(a);
        }

        public void Invoke(T1 t1, T2 t2, T3 t3)
        {
            ClearEvent();
            int c = m_methods.Count;
            for (int i = 0; i < c; i++)
            {
                var method = m_methods[i] as Action<T1,T2,T3>;
                if (method != null)
                {
                    method(t1,t2,t3);
                }
            }
        }
    }

    public class QGFEvent<T1, T2, T3, T4> : QGFEventBase
    {
        public static QGFEvent<T1, T2, T3, T4> operator +(QGFEvent<T1, T2, T3, T4> p1, Action<T1, T2, T3, T4> p2)
        {
            p1.AddListener(p2);
            return p1;
        }

        public static QGFEvent<T1, T2, T3, T4> operator -(QGFEvent<T1, T2, T3, T4> p1, Action<T1, T2, T3, T4> p2)
        {
            p1.RemoveListener(p2);
            return p1;
        }

        public void AddListener(Action<T1, T2, T3, T4> a)
        {
            base.AddListener(a);
        }

        public void RemoveListener(Action<T1, T2, T3, T4> a)
        {
            base.RemoveListener(a);
        }

        public void Invoke(T1 t1, T2 t2, T3 t3, T4 t4)
        {
            ClearEvent();
            int c = m_methods.Count;
            for (int i = 0; i < c; i++)
            {
                var method = m_methods[i] as Action<T1,T2,T3,T4>;
                if (method != null)
                {
                    method(t1,t2,t3,t4);
                }
            }
        }
    }


    public class QGFEventTable<TEvent> where TEvent:QGFEventBase, new()
    {
        private Dictionary<string, TEvent> m_mapEvents;


        /// <summary>
        /// 获取Type所指定的ModuleEvent（它其实是一个EventTable）
        /// 如果不存在，则实例化一个
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public TEvent GetEvent(string type)
        {
            if (m_mapEvents == null)
            {
                m_mapEvents = new Dictionary<string, TEvent>();
            }
            if (!m_mapEvents.ContainsKey(type))
            {
                m_mapEvents.Add(type, new TEvent());
            }
            return m_mapEvents[type] as TEvent;
        }

        public void Clear()
        {
            if (m_mapEvents != null)
            {
                foreach (var @event in m_mapEvents)
                {
                    @event.Value.RemoveAllListeners();
                }
                m_mapEvents.Clear();
            }
        }
    }

    


}
