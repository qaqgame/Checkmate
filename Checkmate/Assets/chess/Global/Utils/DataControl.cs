using Checkmate.Game.Controller;
using QGF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Game.Utils
{
    public enum DataOpt
    {
        Add,
        Sub,
        Mul,
        Div,
        Set
    }

    public class DataUtil
    {
        //更新controller的数据
        public static void Execute(BaseController controller,List<DataTrack> tracks)
        {
            foreach (var track in tracks)
            {
                ExecuteTrack(controller, track);
            }
        }

        //private static void ExecuteTrack(BaseController controller, DataTrack track)
        //{
        //    dynamic src = controller.GetValue(track.name);
        //    dynamic value = track.value;
        //    dynamic result;
        //    switch (track.opt)
        //    {
        //        case DataOpt.Add:
        //            {
        //                result = src + value;
        //                break;
        //            }
        //        case DataOpt.Sub:
        //            {
        //                result = src - value;
        //                break;
        //            }
        //        case DataOpt.Mul:
        //            {
        //                result = src * value;
        //                break;
        //            }
        //        case DataOpt.Div:
        //            {
        //                result = src / value;
        //                break;
        //            }
        //        case DataOpt.Set:
        //            {
        //                result = value;
        //                break;
        //            }
        //        default:
        //            {
        //                result = src;
        //                break;
        //            }
        //    }
        //    string msg = result.ToString();
        //    Debuger.Log("update data{0} with {1}", track.name, msg);
        //    controller.SetValue(track.name, result);
        //}

        private static void ExecuteTrack(BaseController controller, DataTrack track)
        {
            object src = controller.GetValue(track.name);
            object value = track.value;
            object result;
            switch (track.opt)
            {
                case DataOpt.Add:
                    {
                        result = Add(src, value);
                        break;
                    }
                case DataOpt.Sub:
                    {
                        result = Sub(src, value);
                        break;
                    }
                case DataOpt.Mul:
                    {
                        result = Mul(src, value);
                        break;
                    }
                case DataOpt.Div:
                    {
                        result = Div(src, value);
                        break;
                    }
                case DataOpt.Set:
                    {
                        result = value;
                        break;
                    }
                default:
                    {
                        result = src;
                        break;
                    }
            }
            string msg = result.ToString();
            Debuger.Log("update data{0} with {1}", track.name, msg);
            controller.SetValue(track.name, result);
        }

        public static T Add<T>(T x, T y) where T : IConvertible
        {

            var type = typeof(T);
            if (type == typeof(String) ||
            type == typeof(DateTime))
                throw new ArgumentException(String.Format("The type {0} is not supported", type.FullName), "T");
            try
            {
                return (T)(Object)(x.ToDouble(NumberFormatInfo.CurrentInfo) + y.ToDouble(NumberFormatInfo.CurrentInfo));
            }
            catch (Exception ex)
            {
                throw new ApplicationException("The operation failed.", ex);
            }
        }

        public static T Sub<T>(T x, T y) where T : IConvertible
        {

            var type = typeof(T);
            if (type == typeof(String) ||
            type == typeof(DateTime))
                throw new ArgumentException(String.Format("The type {0} is not supported", type.FullName), "T");
            try
            {
                return (T)(Object)(x.ToDouble(NumberFormatInfo.CurrentInfo) - y.ToDouble(NumberFormatInfo.CurrentInfo));
            }
            catch (Exception ex)
            {
                throw new ApplicationException("The operation failed.", ex);
            }
        }

        public static T Mul<T>(T x, T y) where T : IConvertible
        {

            var type = typeof(T);
            if (type == typeof(String) ||
            type == typeof(DateTime))
                throw new ArgumentException(String.Format("The type {0} is not supported", type.FullName), "T");
            try
            {
                return (T)(Object)(x.ToDouble(NumberFormatInfo.CurrentInfo) * y.ToDouble(NumberFormatInfo.CurrentInfo));
            }
            catch (Exception ex)
            {
                throw new ApplicationException("The operation failed.", ex);
            }
        }
        public static T Div<T>(T x, T y) where T : IConvertible
        {

            var type = typeof(T);
            if (type == typeof(String) ||
            type == typeof(DateTime))
                throw new ArgumentException(String.Format("The type {0} is not supported", type.FullName), "T");
            try
            {
                return (T)(Object)(x.ToDouble(NumberFormatInfo.CurrentInfo) / y.ToDouble(NumberFormatInfo.CurrentInfo));
            }
            catch (Exception ex)
            {
                throw new ApplicationException("The operation failed.", ex);
            }
        }

        #region 直接操作

        private static object Add(object a, object b)
        {
            if (a.GetType() == typeof(int))
            {
                return _Add((int)a, b);
            }
            else if (a.GetType() == typeof(float))
            {
                return _Add((float)a, b);
            }
            return a;
        }

        private static object Sub(object a, object b)
        {
            if (a.GetType() == typeof(int))
            {
                return _Sub((int)a, b);
            }
            else if (a.GetType() == typeof(float))
            {
                return _Sub((float)a, b);
            }
            return a;
        }

        private static object Mul(object a, object b)
        {
            if (a.GetType() == typeof(int))
            {
                return _Mul((int)a, b);
            }
            else if (a.GetType() == typeof(float))
            {
                return _Mul((float)a, b);
            }
            return a;
        }

        private static object Div(object a, object b)
        {
            if (a.GetType() == typeof(int))
            {
                return _Div((int)a, b);
            }
            else if (a.GetType() == typeof(float))
            {
                return _Div((float)a, b);
            }
            return a;
        }
        #endregion

        #region 基本运算
        private static int _Add(int a, object b)
        {
            if (b.GetType() == typeof(int) || b.GetType() == typeof(float))
            {
                return a + (int)b;
            }
            return a;
        }

        private static float _Add(float a, object b)
        {
            if (b.GetType() == typeof(int) || b.GetType() == typeof(float))
            {
                return a + (float)b;
            }
            return a;
        }

        private static int _Sub(int a, object b)
        {
            if (b.GetType() == typeof(int) || b.GetType() == typeof(float))
            {
                return a - (int)b;
            }
            return a;
        }

        private static float _Sub(float a, object b)
        {
            if (b.GetType() == typeof(int) || b.GetType() == typeof(float))
            {
                return a - (float)b;
            }
            return a;
        }

        private static int _Mul(int a, object b)
        {
            if (b.GetType() == typeof(int) || b.GetType() == typeof(float))
            {
                return a * (int)b;
            }
            return a;
        }

        private static float _Mul(float a, object b)
        {
            if (b.GetType() == typeof(int) || b.GetType() == typeof(float))
            {
                return a * (float)b;
            }
            return a;
        }

        private static int _Div(int a, object b)
        {
            if (b.GetType() == typeof(int) || b.GetType() == typeof(float))
            {
                return a / (int)b;
            }
            return a;
        }

        private static float _Div(float a, object b)
        {
            if (b.GetType() == typeof(int) || b.GetType() == typeof(float))
            {
                return a / (float)b;
            }
            return a;
        }

        #endregion

    }

    public class DataTrack
    {
        public RoleController controller;//目标
        public string name;//数据名
        public DataOpt opt;//操作类型
        public object value;//值类型（与数据同类型）
    }

    public class DataMap
    {
        public List<DataTrack> Tracks=new List<DataTrack>();//所有数据操作的追踪

        public Action onChanged = null;

        public List<RoleController> mUsedRoles=new List<RoleController>();//改变了属性了的角色
        private List<int> mRoleCount = new List<int>();//角色使用计数器
        public void AddTrack(DataTrack track)
        {
            Debuger.Log("add attr track:{0}", track.name);
            Tracks.Add(track);
            if (!mUsedRoles.Contains(track.controller))
            {
                mUsedRoles.Add(track.controller);
                mRoleCount.Add(1);
                Debuger.Log("{0} attr track added", track.controller.Name);
            }
            else
            {
                int idx = mUsedRoles.IndexOf(track.controller);
                mRoleCount[idx]+=1;
            }
            if (onChanged != null)
            {
                onChanged.Invoke();
            }
        }

        public void RemoveTrack(DataTrack track) {
            Tracks.Remove(track);
            int idx = mUsedRoles.IndexOf(track.controller);
            mRoleCount[idx] -= 1;
            if (mRoleCount[idx] == 0)
            {
                mUsedRoles.RemoveAt(idx);
                mRoleCount.RemoveAt(idx);
            }
            if (onChanged != null)
            {
                onChanged.Invoke();
            }
        }

        public void RemoveTrack(DataMap map)
        {
            foreach(var track in map.Tracks)
            {
                Tracks.Remove(track);
                int idx = mUsedRoles.IndexOf(track.controller);
                mRoleCount[idx] -= 1;
                if (mRoleCount[idx] == 0)
                {
                    mUsedRoles.RemoveAt(idx);
                    mRoleCount.RemoveAt(idx);
                }
            }
            if (onChanged != null)
            {
                onChanged.Invoke();
            }
        }

        public void Clear()
        {
            Tracks.Clear();
            mUsedRoles.Clear();
            mRoleCount.Clear();
            if (onChanged != null)
            {
                onChanged.Invoke();
            }
        }
       
    }
}
