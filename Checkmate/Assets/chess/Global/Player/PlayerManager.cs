using Checkmate.Global.Data;
using QGF.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Game.Player
{
    //玩家管理的类
    public class PlayerManager : Singleton<PlayerManager>
    {
        public uint PID { get; set; }

        private Dictionary<uint, uint> mFriends;//友军标识
        private Dictionary<uint, uint> mEnemys;//敌军标识
        public void Init(PlayerTeamData data)
        {
            mFriends = new Dictionary<uint, uint>();
            mEnemys = new Dictionary<uint, uint>();

            foreach (var team in data.masks)
            {
                mFriends.Add(team.pid, team.friendMask);
                mEnemys.Add(team.pid, team.enemyMask);
            }
            IsWaiting = false;
        }


        public void Clear()
        {

        }

        private bool mOperating;

        public bool Operating
        {
            get
            {
                return mOperating;
            }
            set
            {
                mOperating = value;
            }
        }

        public bool CanOperate
        {
            get
            {
                return mOperating && !IsWaiting;
            }
        }

        public bool IsWaiting
        {
            get;set;
        }

        /// <summary>
        /// 判断dst是否是src的友方
        /// </summary>
        /// <param name="src">判断的原始id</param>
        /// <param name="dst">判断的目标id</param>
        /// <returns></returns>
        public bool IsFriend(int src,int dst)
        {
            uint friend = mFriends[(uint)src];
            return (friend & (1 << dst)) != 0;
        }
        public bool IsFriend(uint src, uint dst)
        {
            uint friend = mFriends[src];
            return (friend & (1 << ((int)dst))) != 0;
        }

        public bool IsEnemy(int src,int dst)
        {
            uint enemy = mEnemys[(uint)src];
            return (enemy & (1 << dst)) != 0;
        }
        public bool IsEnemy(uint src, uint dst)
        {
            uint enemy = mEnemys[(uint)src];
            return (enemy & (1 <<((int)dst))) != 0;
        }

        /// <summary>
        /// dst对于src而言是否是中立
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <returns></returns>
        public bool IsNeutual(int src,int dst)
        {
            return (!IsFriend(src, dst)) && (!IsEnemy(src, dst));
        }
        public bool IsNeutual(uint src, uint dst)
        {
            return (!IsFriend(src, dst)) && (!IsEnemy(src, dst));
        }

        public List<uint> GetAllPlayers()
        {
            return mFriends.Keys.ToList();
        }
    }
}
