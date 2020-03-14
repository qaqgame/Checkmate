using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace QGF.Unity.UI
{
    public class UIRoot:MonoBehaviour
    {

        //通过名字寻找其上的组件
        public static T Find<T>(string name)where T : MonoBehaviour
        {
            GameObject gobj = Find(name);
            if (gobj != null)
            {
                return gobj.GetComponent<T>();
            }
            return null;
        }

        public static GameObject Find(string name)
        {
            //名字必须不为空
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            Transform obj = null;
            GameObject root = FindUIRoot();
            //查找根节点下的所有子节点
            if (root != null)
            {
                Transform temp = root.transform;
                for(int i = 0; i < temp.childCount; ++i)
                {
                    Transform target = temp.GetChild(i);
                    if (target.name == name)
                    {
                        obj = target;
                        break;
                    }
                }
            }

            //如果结果不为空
            if (obj != null)
            {
                return obj.gameObject;
            }
            return null;
        }

        //获取根节点
        public static GameObject FindUIRoot()
        {
            GameObject root = GameObject.Find("UIRoot");
            if (root != null && root.GetComponent<UIRoot>() != null)
            {
                return root;
            }
            Debuger.LogError("UIRoot not exist!");
            return null;
        }

        //添加panel到root下
        public static void AddChild(UIPanel child)
        {
            GameObject root = FindUIRoot();
            if (root == null || child == null)
            {
                return;
            }
            child.transform.SetParent(root.transform, false);
        }

        //给窗口排序
        public static void Sort()
        {
            GameObject root = FindUIRoot();
            if (root == null)
            {
                return;
            }
            List<UIPanel> list = new List<UIPanel>();
            //查询所有的窗口，将layer大的排前面
            root.GetComponentsInChildren<UIPanel>(true, list);
            list.Sort((a, b) =>
            {
                return a.Layer - b.Layer;
            });
            //设置显示顺序
            for(int i = 0; i < list.Count; ++i)
            {
                list[i].transform.SetSiblingIndex(i);
            }
        }

        //==============================================================================
        //UIRoot本身的逻辑
        //==============================================================================
        private static Camera m_uiCamera;
        private static CanvasScaler m_canvasScaler;

        public Camera UICamera;

        void Awake()
        {
            //让UIRoot一直存在于所有场景中
            DontDestroyOnLoad(gameObject);
            m_uiCamera = UICamera;
            m_canvasScaler = GetComponent<CanvasScaler>();
        }

        public CanvasScaler GetUIScaler()
        {
            return m_canvasScaler;
        }

        public Camera GetUICamera()
        {
            return m_uiCamera;
        }
    }
}
