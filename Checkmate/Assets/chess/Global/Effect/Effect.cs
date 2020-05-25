using Checkmate.Game.Controller;
using Checkmate.Game.Utils;
using Checkmate.Global.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

namespace Checkmate.Game.Effect
{
    public class Effect : EffectController
    {
        public override int Type { get { return (int)ControllerType.Effect; } }


        private Dictionary<EffectTrigger,List<SkillAction>> mActions;//所有的活动

        public int EffectId
        {
            get;
            set;
        }

        [GetProperty]
        public int Life
        {
            get;
            set;
        }

        [GetProperty]
        public bool Timely
        {
            get;
            private set;
        }

        [GetProperty]
        public string Name
        {
            get;
            set;
        }

        [GetProperty]
        public string Description
        {
            get;
            private set;
        }

        //当前冷却回合,>maxTurn则可用
        [GetProperty]
        [SetProperty]
        public int CurTurn
        {
            get;
            set;
        }

        [GetProperty]
        [SetProperty]
        public int MaxTurn
        {
            get;
            set;
        }

        [GetProperty]
        public bool Available
        {
            get
            {
                return CurTurn >= MaxTurn;
            }
        }

        [GetProperty]
        public Position Position
        {
            get;
            set;
        }

        public void Execute(EffectTrigger trigger)
        {
            if (!mActions.ContainsKey(trigger) || mActions[trigger].Count == 0)
            {
                return;
            }
            GameExecuteManager.Instance.Add(mActions[trigger]);
            //foreach(var action in mActions)
            //{
            //    GameEnv.Instance.Current.ExecuteAction(action);
            //}
            //foreach (var r in Temp.mUsedRoles)
            //{
            //    r.TempMap.RemoveTrack(Temp);
            //}
            //Temp.Clear();
        }


        public override BaseController Clone()
        {
            Effect temp = new Effect();
            temp.mActions = mActions;
            temp.CurTurn = this.CurTurn;
            temp.MaxTurn = this.MaxTurn;
            temp.mExtraData = new Dictionary<string, object>(mExtraData);
            temp.Name = Name;
            temp.Description = Description;
            temp.Timely = Timely;
            temp.Life = Life;
            return temp;
        }

        #region 解析
        public void Parse(XmlNode node)
        {
            Name = node.Attributes["name"].Value;
            CurTurn = MaxTurn = int.Parse(node.Attributes["coolturn"].Value);
            Timely = false;

            Life = -1;
            if (node.Attributes["life"] != null)
            {
                Life = int.Parse(node.Attributes["life"].Value);
            }

            XmlNode description = node.SelectSingleNode("Description");
            Description = description.InnerText;

           
            
            //解析特殊字典
            XmlNode specialData = node.SelectSingleNode("SpecialData");
            if (specialData != null)
            {
                SetExtra(ObjectParser.GetExtraDataContent(Name, specialData.Attributes["file"].Value));
            }

            //解析字典
            XmlNode dataNode = node.SelectSingleNode("Data");
            if (dataNode.HasChildNodes)
            {
                var list = dataNode.ChildNodes;
                string tempKey;
                foreach (XmlNode l in list)
                {
                    object value = ObjectParser.ParseObject(l, out tempKey);
                    mExtraData.Add(tempKey, value);
                }
            }

            //解析内容
            XmlNode content = node.SelectSingleNode("Content");
            ParseContent(content);
        }

        private void ParseContent(XmlNode node)
        {
            mActions = new Dictionary<EffectTrigger, List<SkillAction>>();
            //解析所有的action
            XmlNodeList cl = node.ChildNodes;
            foreach (XmlNode l in cl)
            {
                List<EffectTrigger> triggers = ObjectParser.ParseEnums<EffectTrigger>(l.Attributes["trigger"].Value);
                if (triggers.Contains(EffectTrigger.Timely))
                {
                    Timely = true;
                }
                
                SkillAction action = new SkillAction(l);
                foreach(var t in triggers)
                {
                    if (!mActions.ContainsKey(t))
                    {
                        List<SkillAction> list = new List<SkillAction>();
                        mActions.Add(t, list);
                    }
                    mActions[t].Add(action);
                }
            }
        }
        #endregion
    }


    //技能解析类
    public static class EffectParser
    {
        static string RangeNameSpace = "Checkmate.Game.Effect.";
        public static Effect ParseEffect(XmlNode root)
        {
            //获取类名
            System.Type tp = System.Type.GetType(RangeNameSpace + root.Name);
            if (tp == null)
            {
                Debug.LogError("error get class:" + RangeNameSpace + root.Name);
            }
            ConstructorInfo constructor = tp.GetConstructor(System.Type.EmptyTypes);

            Effect effect = (Effect)constructor.Invoke(null);
            effect.Parse(root);

            return effect;
        }
    }
}
