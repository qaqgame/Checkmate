using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Global.Data
{
    public enum GameAction
    {
        Move,
        Skill,
        Attack
    }

    [ProtoContract]
    public class GameActionData
    {
        [ProtoMember(1)]
        public GameAction OperationType;//操作类型
        [ProtoMember(2)]
        public byte[] OperationCnt;//操作内容
    }
}
