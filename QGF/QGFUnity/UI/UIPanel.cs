using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QGF.Event;
using UnityEngine;
using UnityEngine.EventSystems;
namespace QGF.Unity.UI
{
    public class UIPanel:MonoBehaviour,ILogTag
    {
        public virtual UITypeDef UIType { get { return UITypeDef.Unknown; } }

        private int mLayer = UILayerDef.Unknown;
        public int Layer { get { return mLayer; } set { mLayer = value; } }

        public QGFEvent<object> onClose = new QGFEvent<object>();//关闭时的事件

        [SerializeField]
        private AnimationClip mOpenClip;
        [SerializeField]
        private AnimationClip mCloseClip;//开始和关闭时的动画


        private float mCloseAnimTime;//关闭动画时间
        private object mCloseArg;//窗口关闭参数

        public bool IsOpen { get { return this.gameObject.activeSelf; } }

        private void Update()
        {
            if (mCloseAnimTime > 0)
            {
                mCloseAnimTime -= UnityEngine.Time.deltaTime;
                if (mCloseAnimTime <= 0)
                {
                    CloseWorker();
                }
            }
            OnUpdate();

        }

        public void Open(object arg = null)
        {
            LOG_TAG = GetType().Name;
            Debuger.Log("args:{0}", arg);
            //隐藏时显示
            if (!this.gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            OnOpen(arg);

            //播放启动动画
            if (mOpenClip!=null)
            {
                var animation = GetComponent<Animation>();
                if (animation != null)
                {
                    animation.Play(mOpenClip.name);
                }
                else
                {
                    Debuger.LogError("open anim setted, animation component not found!");
                }
            }
        }

        public void Close(object arg = null)
        {
            Debuger.Log("args:{0}", arg);
            mCloseArg = arg;
            mCloseAnimTime = 0;
            if (mCloseClip != null)
            {
                var animation = GetComponent<Animation>();
                if (animation != null)
                {
                    animation.Play(mCloseClip.name);
                    mCloseAnimTime = mCloseClip.length;
                }
                else
                {
                    Debuger.LogError("close anim setted, animation component not found!");
                    CloseWorker();
                }
            }
            //没有动画直接关闭
            else
            {
                CloseWorker();
            }

        }

        private void CloseWorker()
        {
            if (this.gameObject.activeSelf)
            {
                this.gameObject.SetActive(false);
            }
            OnClose(mCloseArg);
            onClose.Invoke(mCloseArg);

            mCloseArg = null;
        }

        //打开时调用
        protected virtual void OnOpen(object arg = null)
        {

        }

        //关闭时调用
        protected virtual void OnClose(object arg = null)
        {

        }

        protected virtual void OnUpdate()
        {

        }
        public string LOG_TAG { get; protected set; }

        //查找panel上的控件
        protected T Find<T>(string controlName) where T :UIBehaviour
        {
            Transform target = this.transform.Find(controlName);
            if (target != null)
            {
                return target.GetComponent<T>();
            }
            else
            {
                Debuger.LogError("cannot find UIControl: {0}", controlName);
                return default(T);
            }
        }

        #region UI事件监听辅助函数
        /// <summary>
        /// 为UIPanel内的脚本提供便捷的UI事件监听接口
        /// </summary>
        /// <param name="controlName"></param>
        /// <param name="listener"></param>
        protected void AddUIClickListener(string controlName, Action<string> listener)
        {
            Transform target = this.transform.Find(controlName);
            if (target != null)
            {
                UIEventTrigger.Get(target).onClickWithName += listener;
            }
            else
            {
                Debuger.LogError("未找到UI控件：{0}", controlName);
            }
        }

        /// <summary>
        /// 为UIPanel内的脚本提供便捷的UI事件监听接口
        /// </summary>
        /// <param name="controlName"></param>
        /// <param name="listener"></param>
        protected void AddUIClickListener(string controlName, Action listener)
        {
            Transform target = this.transform.Find(controlName);
            if (target != null)
            {
                UIEventTrigger.Get(target).onClick += listener;
            }
            else
            {
                Debuger.LogError("未找到UI控件：{0}", controlName);
            }
        }

        /// <summary>
        /// 为UIPanel内的脚本提供便捷的UI事件监听接口
        /// </summary>
        /// <param name="target"></param>
        /// <param name="listener"></param>
        protected void AddUIClickListener(UIBehaviour target, Action listener)
        {
            if (target != null)
            {
                UIEventTrigger.Get(target).onClick += listener;
            }
        }



        /// <summary>
        /// 移除UI控件的监听器
        /// </summary>
        /// <param name="controlName"></param>
        /// <param name="listener"></param>
        protected void RemoveUIClickListener(string controlName, Action<string> listener)
        {
            Transform target = this.transform.Find(controlName);
            if (target != null)
            {
                if (UIEventTrigger.HasExistOn(target))
                {
                    UIEventTrigger.Get(target).onClickWithName -= listener;
                }
            }
            else
            {
                Debuger.LogError("未找到UI控件：{0}", controlName);
            }
        }

        /// <summary>
        /// 移除UI控件的监听器
        /// </summary>
        /// <param name="controlName"></param>
        /// <param name="listener"></param>
        protected void RemoveUIClickListener(string controlName, Action listener)
        {
            Transform target = this.transform.Find(controlName);
            if (target != null)
            {
                if (UIEventTrigger.HasExistOn(target))
                {
                    UIEventTrigger.Get(target).onClick -= listener;
                }
            }
            else
            {
                Debuger.LogError("未找到UI控件：{0}", controlName);
            }
        }

        /// <summary>
        /// 移除UI控件的监听器
        /// </summary>
        /// <param name="target"></param>
        /// <param name="listener"></param>
        protected void RemoveUIClickListener(UIBehaviour target, Action listener)
        {
            if (target != null)
            {
                if (UIEventTrigger.HasExistOn(target.transform))
                {
                    UIEventTrigger.Get(target).onClick -= listener;
                }
            }
        }


        /// <summary>
        /// 移除UI控件的所有监听器
        /// </summary>
        /// <param name="controlName"></param>
        protected void RemoveUIClickListeners(string controlName)
        {
            Transform target = this.transform.Find(controlName);
            if (target != null)
            {
                if (UIEventTrigger.HasExistOn(target))
                {
                    UIEventTrigger.Get(target).onClick = null;
                }
            }
            else
            {
                Debuger.LogError("未找到UI控件：{0}", controlName);
            }
        }

        /// <summary>
        /// 移除UI控件的所有监听器
        /// </summary>
        /// <param name="target"></param>
        protected void RemoveUIClickListeners(UIBehaviour target)
        {
            if (target != null)
            {
                if (UIEventTrigger.HasExistOn(target.transform))
                {
                    UIEventTrigger.Get(target).onClick = null;
                }
            }
        }

        #endregion 
    }
}
