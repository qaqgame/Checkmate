using Checkmate.Game.Controller;
using QGF;
using System;
using System.Collections.Generic;
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

        private static void ExecuteTrack(BaseController controller, DataTrack track)
        {
            dynamic src = controller.GetValue(track.name);
            dynamic value = track.value;
            dynamic result;
            switch (track.opt)
            {
                case DataOpt.Add:
                    {
                        result = src + value;
                        break;
                    }
                case DataOpt.Sub:
                    {
                        result = src - value;
                        break;
                    }
                case DataOpt.Mul:
                    {
                        result = src * value;
                        break;
                    }
                case DataOpt.Div:
                    {
                        result = src / value;
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
