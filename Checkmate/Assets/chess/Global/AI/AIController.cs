using Checkmate.Game;
using Checkmate.Game.Controller;
using Checkmate.Game.Map;
using Checkmate.Game.Player;
using Checkmate.Game.Skill;
using QGF.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Assets.chess.Global.AI
{
    public enum ActionType
    {
        Skill1,
        Skill2,
        Skill3,
        GeneralAttack,
        MoveForth
    }

    public class State
    {
        private bool died;
        public RoleController rc;
        private int HP;
        public Position pos = new Position();
        public int ap;

        public bool endState(int mcost)   // 是否在一回合内已经无法行动,可能时因为死亡或者无行动点
        {   
            // ai也受到playerManager管理
            if(ap-mcost < 0)
            {
                return true;
            }
            return died;
        }

        public bool Died
        {
            get { return Died; }
            set { died = value; }
        }
         
        public State(RoleController rolecontroller)
        {
            rc = rolecontroller;
            HP = rc.Current.Hp;
            pos = rc.Position;
            ap = APManager.Instance.GetCurAP();
        }
        
        public int currHp
        {
            get { return HP; }
            set { HP = value; }
        }
    }

    public class MTCSNode
    {
        MTCSNode parents = null;                  // 当前节点的父节点
        public List<MTCSNode> childrens = new List<MTCSNode>();     // 当前节点的子节点

        int visitTimesAll = 0;   // 该节点的总访问次数，用于子节点引用父节点的改值来计算UCB，该值代表总访问次数
        float qualityValue = 0.0F;   // 选择该节点作为下一步的价值
        State aiState;               // ai的State
        State playerState;           // 玩家的State
        ActionType act;
        Position targetPos;
        int simulateNums;            // 进行模拟的次数
        int winNums;                // 模拟中获得胜利的次数
        public double UCB;

        bool terminal
        {
            get { return aiState.endState(0) || playerState.endState(0); }
            set { terminal = value; }
        }// 判断是否终止
        int maxRounds;               // MTCS最多进行模拟的回合数
        public bool simulated
        {
            get;
            set;
        }

        public MTCSNode(State state1, State state2)
        {
            aiState = state1;
            playerState = state2;
            simulateNums = 1;
            terminal = false;
            maxRounds = 4;
            simulated = false;
        }

        public bool fullExtended
        {
            get { return childrens.Count == AIController.Instance.AvailableActionNum; }
            private set { fullExtended = value; }
        }

        public void Act(ActionType act, State holder, State enemy)         // 进行模拟动作
        {
            switch (act)
            {
                case ActionType.Skill1:                        // 模拟进行技能1
                    int mCurrSkill = holder.rc.Skills[0];      // todo: 确认skill[0]代表第一个技能，应该
                    BaseSkill skill = SkillManager.Instance.GetInstance(mCurrSkill);
                    if (holder.endState(skill.Cost))          // 无行动点可继续行动，退出该次模拟，并且代表这次模拟无价值
                    {
                        qualityValue += 0.0f;
                        targetPos = null;
                        return;
                    }
                    List<Position> pos = skill.GetMousePositions(holder.rc.Position);
                    if (!pos.Contains(enemy.rc.Position))      // 技能范围内没有敌人，代表这一步没有价值，停止模拟
                    {
                        qualityValue += 0.0f;
                        targetPos = null;
                        return;
                    }
                    else    // 技能范围内有敌人,进行操作
                    {
                        //APManager.Instance.ReduceAp((int)PlayerManager.Instance.PID,skill.Cost);
                        holder.ap -= skill.Cost;           // TODO: 在AI的回合，PlayerManager.Instance.PID应该代表ai
                        // TODO: 执行技能，注意要随时修改died属性
                        targetPos = enemy.rc.Position;
                        object damage = skill.GetValue("Damage");         // 获取技能伤害
                        if(damage == null)                       // 非伤害型技能，将cost作为qualityValue
                        {
                            qualityValue += skill.Cost;
                            return;
                        }
                        enemy.currHp -= (int)damage;              // 伤害型技能，将damage作为qualityValue
                        qualityValue += (int)damage;
                        if(enemy.currHp <= 0)
                        {
                            enemy.Died = true;
                        }
                    }
                    break;
                case ActionType.Skill2:

                    break;
                case ActionType.Skill3:

                    break;
                case ActionType.GeneralAttack:

                    break;

                case ActionType.MoveForth:
                    int distance = HexMapUtil.GetDistance(holder.rc.Position, enemy.rc.Position);             // ai与玩家的距离
                    CellController holderCell = MapManager.Instance.GetCell(holder.rc.Position);              // 操作者所处的cell
                    CellController enemyCell = MapManager.Instance.GetCell(enemy.rc.Position);                // 敌人所处的cell
                    Random r1 = new Random();
                    int i = r1.Next(5);
                    CellController t = enemyCell.GetNeighbor((HexDirection)i);                                // 随机移动
                    for (int j = 0; j < i; j++)
                    {
                        int dir = r1.Next(6);
                        if(t.GetNeighbor((HexDirection)dir).Position != enemyCell.Position)
                        {
                            t = t.GetNeighbor((HexDirection)dir);
                        }
                    }
                    holder.pos = t.Position;
                    int movdis = HexMapUtil.GetDistance(holder.rc.Position, holder.pos);
                    qualityValue += movdis;                                                       // 移动距离作为qualityValue
                    holder.ap -= movdis;
                    break;
            }
        }

        public MTCSNode Selection()
        {
            if (!terminal)
            {
                Random ran = new Random();
                while (childrens.Count != AIController.Instance.AvailableActionNum)
                {
                    int r = ran.Next(AIController.Instance.AvailableActionNum + 1);
                    foreach (var c in childrens)           // 如果子节点中已经存在，则不需要创建新节点，把simulateNums++即可
                    {
                        if (c.act == (ActionType)r)
                        {
                            c.simulateNums++;
                            break;
                        }
                    }
                    MTCSNode ChildNode = new MTCSNode(aiState, playerState);     // 子节点中不存在，创建新子节点
                    ChildNode.act = (ActionType)r;
                    childrens.Add(ChildNode);
                }
            }

            //返回值
            
            foreach (var c in childrens)           // 如果子节点中已经存在，则不需要创建新节点，把simulateNums++即可
            {
                if (!c.simulated)
                {
                    return c;
                }
            }
            return null;
        }
        public void Simulation()
        {
            for (int i = 0; i < simulateNums; i++)             //模拟simulateNums次
            {
                int count = 0;
                Random ran = new Random();
                while (count < maxRounds && !terminal)          // 只模拟接下来的maxRounds个回合，并且这期间没有达到终止。采用随机选择Action的方式进行模拟
                {
                    while(!aiState.endState(0))                // 一回合内ai随机行动
                    {
                        // ai随机Action操作
                        int r = ran.Next(AIController.Instance.AvailableActionNum + 1);
                        Act((ActionType)r, aiState, playerState);
                    }
                
                    while(!playerState.endState(0))              // 一回合内玩家随机行动
                    {
                        // 玩家随机Action操作
                        int r = ran.Next(AIController.Instance.AvailableActionNum + 1);
                        Act((ActionType)r, playerState, aiState);
                    }
                    count++;
                }
                if(playerState.Died)
                {
                    winNums++;
                }
            }
            simulated = true;
            // 计算UCB
            double v1 = (double)winNums / simulateNums + Math.Sqrt(qualityValue/simulateNums);
            double c = 1.96;
            double v2 = Math.Sqrt((c * Math.Log(visitTimesAll)) / simulateNums);
            UCB = v1 + v2;
        }

        public MTCSNode GetBestAct()
        {
            int index = 0;
            double maxUCB = 0;
            for(int i = 0; i < childrens.Count; i++)
            {
                if(childrens[i].UCB >= maxUCB)
                {
                    index = i;
                    maxUCB = childrens[i].UCB;
                }
            }
            return childrens[index];
        }
    }
    public class AIController : Singleton<AIController>
    {
        private RoleController roleController; //AI所控制的Role
        private int type = -1;  // 代表是AI
        private int availableActionNames = System.Enum.GetNames(typeof(ActionType)).Length;
        public int AvailableActionNum
        {
            get { return availableActionNames; }
        }// 获取枚举ActionType内元素的个数，作为MTCS的第二层节点数
        private static State mstate;
        private static State pstate;

        public void Init(RoleController aicontroller, RoleController plcontroller)
        {
            roleController = aicontroller;
            mstate = new State(DeepCopyByBinary<RoleController>(aicontroller));         // 深拷贝
            pstate = new State(DeepCopyByBinary<RoleController>(plcontroller));         // 深拷贝
        }
        
        void MTCSAction(State mstate, State pstate)
        {
            MTCSNode root = new MTCSNode(mstate, pstate);
            foreach (var c in root.childrens)           // 对子节点进行模拟，之后选出最优子节点
            {
                if (!c.simulated)
                {
                    MTCSNode act = root.Selection();
                    act.Simulation();
                }
            }
            MTCSNode actBest = root.GetBestAct();
            // TODO: 根据actBest进行act
        }

        // 深拷贝
        public static T DeepCopyByBinary<T>(T obj)
        {
            object retval;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                retval = bf.Deserialize(ms);
                ms.Close();
            }
            return (T)retval;
        }
    }
}
