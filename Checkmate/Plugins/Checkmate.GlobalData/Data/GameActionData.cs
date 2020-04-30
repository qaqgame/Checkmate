using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Global.Data
{
    public enum GameAction
    {
        Move
    }
    public class GameActionData
    {
        public GameAction OperationType;//操作类型
        public byte[] OperationCnt;//操作内容
    }
}
