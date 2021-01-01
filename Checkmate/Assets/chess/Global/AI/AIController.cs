using Checkmate.Game;
using Checkmate.Game.Controller;
using Checkmate.Game.Map;
using Checkmate.Game.Player;
using Checkmate.Game.Skill;
using Checkmate.Services.Game;
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
        // private int HP;
        public Position pos = new Position();
        public int ap;
        public int ap1;
        public RoleAttributeController roleAttributeController1;     
        private RoleAttributeController roleAttributeController;    // 需要随着模拟变化的
        public List<bool> isSkillInCD;          // 需要随着模拟而变化的
        public List<bool> isSkillInCD1;

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
         
        public State(RoleController rolecontroller, RoleAttributeController roleattr)
        {
            rc = rolecontroller;
            // HP = rc.Temp.Hp;
            pos = rc.Position;
            ap = APManager.Instance.GetCurAP();
            ap1 = ap;
            died = roleattr.Hp > 0;
            roleAttributeController = new RoleAttributeController();
            roleAttributeController.Copy(roleattr);
            roleAttributeController1 = roleattr;
            isSkillInCD = new List<bool>();
            isSkillInCD1 = new List<bool>(); // todo:判断技能是否在cd
        }
        
        public int currHp
        {
            get { return roleAttributeController.Hp; }
            set { roleAttributeController.Hp = value; }
        }

        public void Reset()
        {
            died = roleAttributeController1.Hp > 0;
            pos = rc.Position;
            ap = ap1;
            roleAttributeController.Copy(roleAttributeController1);
            for(int i = 0; i < isSkillInCD.Count; i++)
            {
                isSkillInCD[i] = isSkillInCD1[1];
            }
        }

        // 更新state
        public void Update(MTCSNode act)
        {
            switch(act.act)
            {
                case ActionType.Skill1:
                    //isSkillInCD1[0] = true;
                    ap1 -= act.apCost;
                    pos = act.targetPos;
                    if(act.targetPos == act.playerState.rc.Position)
                    {
                        act.playerState.roleAttributeController1.Hp -= act.skilldemage;
                    }
                    break;
                case ActionType.Skill2:
                    //isSkillInCD1[1] = true;
                    ap1 -= act.apCost;
                    pos = act.targetPos;
                    if (act.targetPos == act.playerState.rc.Position)
                    {
                        act.playerState.roleAttributeController1.Hp -= act.skilldemage;
                    }
                    break;
                case ActionType.Skill3:
                    //isSkillInCD1[2] = true;
                    ap1 -= act.apCost;
                    pos = act.targetPos;
                    if (act.targetPos == act.playerState.rc.Position)
                    {
                        act.playerState.roleAttributeController1.Hp -= act.skilldemage;
                    }
                    break;
                case ActionType.GeneralAttack:
                    pos = act.targetPos;
                    if (act.targetPos == act.playerState.rc.Position)
                    {
                        act.playerState.roleAttributeController1.Hp -= act.skilldemage;
                    }
                    break;
                case ActionType.MoveForth:
                    pos = act.targetPos;
                    ap1 -= act.apCost;
                    break;
            }
        }
    }

    public class MTCSNode
    {
        MTCSNode parents = null;                  // 当前节点的父节点
        public List<MTCSNode> childrens = new List<MTCSNode>();     // 当前节点的子节点

        int visitTimesAll = 0;   // 该节点的总访问次数，用于子节点引用父节点的改值来计算UCB，该值代表总访问次数
        float qualityValue = 0.0F;   // 选择该节点作为下一步的价值
        State aiState;               // ai的State
        public State playerState;           // 玩家的State
        public ActionType act;
        public Position targetPos;
        int simulateNums;            // 进行模拟的次数
        int winNums;                // 模拟中获得胜利的次数
        public double UCB;
        public int skilldemage;
        public int apCost;
        

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
            skilldemage = 0;
            apCost = 0;
            
        }

        public bool fullExtended
        {
            get { return childrens.Count == AIController.Instance.AvailableActionNum; }
            private set { fullExtended = value; }
        }

        public void Act(ActionType act, State holder, State enemy)         // 进行模拟动作
        {
            int mCurrSkill;
            BaseSkill skill;
            List<Position> pos;
            switch (act)
            {
                case ActionType.Skill1:                        // 模拟进行技能1
                    mCurrSkill = holder.rc.Skills[0];      // skill[0]代表第一个技能
                    skill = SkillManager.Instance.GetInstance(mCurrSkill);
                    if (holder.endState(skill.Cost))          // 无行动点可继续行动，退出该次模拟，并且代表这次模拟无价值
                    {
                        qualityValue += 0.0f;
                        targetPos = holder.rc.Position;
                        return;
                    }
                    pos = skill.GetMousePositions(holder.rc.Position);
                    if (!pos.Contains(enemy.rc.Position))      // 技能范围内没有敌人，代表这一步没有价值，停止模拟
                    {
                        qualityValue += 0.0f;
                        targetPos = holder.rc.Position;
                        return;
                    }
                    //else if (holder.isSkillInCD[0])                // 技能正在冷却
                    //{
                    //    qualityValue += 0.0f;
                    //    targetPos = holder.rc.Position;
                    //    return;
                    //}
                    else    // 技能范围内有敌人,进行操作
                    {
                        holder.ap -= skill.Cost;           // TODO: 在AI的回合，PlayerManager.Instance.PID应该代表ai
                        apCost = skill.Cost;
                        // TODO: 执行技能，注意要随时修改died属性
                        targetPos = enemy.rc.Position;
                        object damage = skill.GetValue("Damage");         // 获取技能伤害
                        if(damage == null)                       // 非伤害型技能，将cost作为qualityValue
                        {
                            skilldemage = 0;
                            qualityValue += skill.Cost;
                            return;
                        }
                        enemy.currHp -= (int)damage;              // 伤害型技能，将damage作为qualityValue
                        skilldemage = (int)damage;
                        qualityValue += (int)damage;
                        if(enemy.currHp <= 0)
                        {
                            enemy.Died = true;
                        }
                        //holder.isSkillInCD[0] = true;
                    }
                    break;
                case ActionType.Skill2:
                    mCurrSkill = holder.rc.Skills[1];      // todo: 确认skill[1]代表第2个技能，应该
                    skill = SkillManager.Instance.GetInstance(mCurrSkill);
                    if (holder.endState(skill.Cost))          // 无行动点可继续行动，退出该次模拟，并且代表这次模拟无价值
                    {
                        qualityValue += 0.0f;
                        targetPos = holder.rc.Position;
                        return;
                    }
                    pos = skill.GetMousePositions(holder.rc.Position);
                    if (!pos.Contains(enemy.rc.Position))      // 技能范围内没有敌人，代表这一步没有价值，停止模拟
                    {
                        qualityValue += 0.0f;
                        targetPos = holder.rc.Position;
                        return;
                    }
                    /*else if(holder.isSkillInCD[1])                // 技能正在冷却
                    {
                        qualityValue += 0.0f;
                        targetPos = holder.rc.Position;
                        return;
                    }*/
                    else    // 技能范围内有敌人,进行操作
                    {
                        holder.ap -= skill.Cost;           // TODO: 在AI的回合，PlayerManager.Instance.PID应该代表ai
                        apCost = skill.Cost;
                        // TODO: 执行技能，注意要随时修改died属性
                        targetPos = enemy.rc.Position;
                        object damage = skill.GetValue("Damage");         // 获取技能伤害
                        if (damage == null)                       // 非伤害型技能，将cost作为qualityValue
                        {
                            qualityValue += skill.Cost;
                            return;
                        }
                        enemy.currHp -= (int)damage;              // 伤害型技能，将damage作为qualityValue
                        skilldemage = (int)damage;
                        qualityValue += (int)damage;
                        if (enemy.currHp <= 0)
                        {
                            enemy.Died = true;
                        }
                        //holder.isSkillInCD[1] = true;
                    }
                    break;
                case ActionType.Skill3:
                    mCurrSkill = holder.rc.Skills[2];      // todo: 确认skill[2]代表第3个技能，应该
                    skill = SkillManager.Instance.GetInstance(mCurrSkill);
                    if (holder.endState(skill.Cost))          // 无行动点可继续行动，退出该次模拟，并且代表这次模拟无价值
                    {
                        qualityValue += 0.0f;
                        targetPos = holder.rc.Position;
                        return;
                    }
                    pos = skill.GetMousePositions(holder.rc.Position);
                    if (!pos.Contains(enemy.rc.Position))      // 技能范围内没有敌人，代表这一步没有价值，停止模拟
                    {
                        qualityValue += 0.0f;
                        targetPos = holder.rc.Position;
                        return;
                    }
                    /*else if(holder.isSkillInCD[2])                // 技能正在冷却
                    {
                        qualityValue += 0.0f;
                        targetPos = holder.rc.Position;
                        return;
                    }*/
                    else    // 技能范围内有敌人,进行操作
                    {
                        holder.ap -= skill.Cost;           // TODO: 在AI的回合，PlayerManager.Instance.PID应该代表ai
                        apCost = skill.Cost;
                        // TODO: 执行技能，注意要随时修改died属性
                        targetPos = enemy.rc.Position;
                        object damage = skill.GetValue("Damage");         // 获取技能伤害
                        if (damage == null)                       // 非伤害型技能，将cost作为qualityValue
                        {
                            qualityValue += skill.Cost;
                            return;
                        }
                        enemy.currHp -= (int)damage;              // 伤害型技能，将damage作为qualityValue
                        skilldemage = (int)damage;
                        qualityValue += (int)damage;
                        if (enemy.currHp <= 0)
                        {
                            enemy.Died = true;
                        }
                        //holder.isSkillInCD[2] = true;
                    }
                    break;
                case ActionType.GeneralAttack:             // TODO: 普攻的行动点消耗
                    // todo: apcost
                    int range = holder.rc.Temp.AttackRange;
                    if(HexMapUtil.GetDistance(holder.rc.Position, enemy.rc.Position) > range)
                    {
                        qualityValue += 0;
                        targetPos = holder.rc.Position;
                        return;
                    } else
                    {
                        targetPos = enemy.rc.Position;
                        int damage = holder.rc.Temp.Attack;
                        enemy.currHp -= (int)damage;              //将damage作为qualityValue
                        skilldemage = (int)damage;
                        qualityValue += (int)damage;
                        if (enemy.currHp <= 0)
                        {
                            enemy.Died = true;
                        }
                    }
                    
                    break;

                case ActionType.MoveForth:
                    int distance = HexMapUtil.GetDistance(holder.rc.Position, enemy.rc.Position);             // ai与玩家的距离
                    CellController temp = MapManager.Instance.GetCell(holder.rc.Position);
                    Random r = new Random();
                    CellController next = temp.GetNeighbor((HexDirection)r.Next(6));
                    int distanceT = HexMapUtil.GetDistance(next.Position, enemy.rc.Position);
                    for(int m = 0; m < Math.Min(holder.ap,10); m++)
                    {
                        if(distanceT > distance)
                        {
                            next = temp.GetNeighbor((HexDirection)r.Next(6));
                            distanceT = HexMapUtil.GetDistance(next.Position, enemy.rc.Position);
                        } else
                        {
                            temp = next;
                            next = temp.GetNeighbor((HexDirection)r.Next(6));
                            distanceT = HexMapUtil.GetDistance(next.Position, enemy.rc.Position);
                            distance = HexMapUtil.GetDistance(temp.Position, enemy.rc.Position);
                            m++;
                        }
                    }
                    qualityValue += Math.Min(holder.ap, 10);
                    targetPos = temp.Position;
                    holder.ap -= Math.Min(holder.ap, 10);
                    apCost = Math.Min(holder.ap, 10);
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

                    aiState.Reset();
                    playerState.Reset();
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
        private static RoleAttributeController airoleattr;
        private static RoleAttributeController plroleattr;
        
        void MTCSAction(RoleController aicontroller, RoleController plcontroller)
        {
            List<MTCSNode> acts = new List<MTCSNode>();
            roleController = aicontroller;
            airoleattr = new RoleAttributeController();
            airoleattr.Copy(aicontroller.Temp);
            plroleattr = new RoleAttributeController();
            plroleattr.Copy(plcontroller.Temp);
            mstate = new State(aicontroller,airoleattr);
            pstate = new State(plcontroller,plroleattr);

            while(!mstate.endState(0))
            {
                mstate.Reset();
                pstate.Reset();
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
                acts.Add(actBest);
                mstate.Update(actBest);
            }
            int Skillid;
            // 根据acts发送消息出去
            for(int i = 0; i < acts.Count; i++)
            {
                switch(acts[i].act)
                {
                    case ActionType.Skill1:
                        Skillid = roleController.Skills[0];
                        GameNetManager.Instance.Skill(Skillid, roleController, acts[i].targetPos);
                        break;
                    case ActionType.Skill2:
                        Skillid = roleController.Skills[1];
                        GameNetManager.Instance.Skill(Skillid, roleController, acts[i].targetPos);
                        break;
                    case ActionType.Skill3:
                        Skillid = roleController.Skills[2];
                        GameNetManager.Instance.Skill(Skillid, roleController, acts[i].targetPos);
                        break;
                    case ActionType.GeneralAttack:
                        GameNetManager.Instance.Attack(roleController, acts[i].targetPos);
                        break;
                    case ActionType.MoveForth:
                        GameNetManager.Instance.Move(roleController, acts[i].targetPos);
                        break;
                }
            }
        }

    }
}
