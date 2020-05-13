using Checkmate.Game.Player;
using Checkmate.Game.Role;
using QGF.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Game.Utils
{
    public interface IMode
    {
        bool CheckEnd(out int winner);
    }


    public class KillMode:IMode
    {
        List<uint> mActivePlayers = new List<uint>();
        public bool CheckEnd(out int winner)
        {
            mActivePlayers.Clear();

            //遍历所有玩家的存活角色
            List<uint> allPlayers = PlayerManager.Instance.GetAllPlayers();
            foreach(var uid in allPlayers)
            {
                List<int> activeRoles = RoleManager.Instance.GetActiveRolesOfPlayer(uid);
                if (activeRoles != null && activeRoles.Count > 0)
                {
                    mActivePlayers.Add(uid);
                }
            }

            //如果无人生还则结束并设胜者为-1
            if (mActivePlayers.Count <= 0)
            {
                winner = -1;
                return true;
            }

            //取第一个有存活的玩家id
            uint temp = mActivePlayers[0];
            //遍历所有现有存活玩家
            foreach(var player in mActivePlayers)
            {
                //如果非友方则代表剩余玩家非同一阵营
                if (!(PlayerManager.Instance.IsFriend(temp, player) || player == temp))
                {
                    winner = -1;
                    return false;
                }
            }

            //剩余同一阵营
            winner = (int)temp;
            return true;
        }
    }


    public static class ModeParser
    {
        static string ModeNameSpace = "Checkmate.Game.Utils.";
        public static IMode ParseMode(string type)
        {
            //获取类名
            System.Type tp = System.Type.GetType(ModeNameSpace + type);
            ConstructorInfo constructor = tp.GetConstructor(System.Type.EmptyTypes);

            IMode mode = (IMode)constructor.Invoke(null);

            return mode;
        }
    }
}
