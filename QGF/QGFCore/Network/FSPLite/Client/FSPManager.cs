using QGF.Codec;
using QGF.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QGF.Network.FSPLite.Client
{
    public class FSPManager:ILogTag
    {
        public string LOG_TAG { get; private set; }

        //来自帧同步服务器的事件
        public event Action<byte[]> onGameBegin;
        public event Action<byte[]> onRoundBegin;
        public event Action<byte[]> onControlStart;
        public event Action<byte[]> onRoundEnd;
        public event Action<byte[]> onGameEnd;
        public event Action<uint> onGameExit;

        //基础参数
        private bool mIsRunning = false;
        private FSPClient mClient;//通讯模块
        private FSPParam mParam;//帧同步参数
        private uint mMinePlayerId;//玩家id
        private Action<int, FSPMessage> mFrameListener;//帧同步接收事件
        private int mCurrentFrameIndex;//当前帧（不超过被锁帧）
        private int mLockedFrameIndex;//被锁帧（即来自服务器的帧）

        public uint MainPlayerId { get { return mMinePlayerId; } }


        //游戏状态
        private FSPGameState mGameState = FSPGameState.None;
        public FSPGameState GameState { get { return mGameState; } }


        //帧列表
        private DictionarySafe<int, FSPFrame> mFrameBuffer;//缓存的帧

        //本地模拟
        private FSPFrame mNextLocalFrame;//下一帧

        private FSPFrameController mFrameCtrl;

        //=================================
        //启动帧同步
        public void Start(FSPParam param, uint playerId)
        {
            mParam = param;
            mMinePlayerId = playerId;
            LOG_TAG = "FSPManager[" + playerId + "]";

            Debuger.Log();

            //使用本地模拟
            if (mParam.useLocal)
            {
                mLockedFrameIndex = param.maxFrameId;
            }
            //网络连接
            else
            {
                mClient = new FSPClient();
                mClient.Init(mParam.sid,playerId);
                mClient.SetFSPAuthInfo(param.authId);
                mClient.SetFSPListener(OnFSPListener);

                mClient.Connect(param.host, param.port);
                mClient.VerifyAuth();

                //开始时将帧限制到帧率-1
                mLockedFrameIndex = mParam.clientFrameRateMultiple - 1;
            }

            mIsRunning = true;
            mGameState = FSPGameState.Create;

            mFrameBuffer = new DictionarySafe<int, FSPFrame>();
            mCurrentFrameIndex = 0;

            mFrameCtrl = new FSPFrameController();
            mFrameCtrl.Start(param);
        }

        //停止帧同步
        public void Stop()
        {
            Debuger.Log();

            mGameState = FSPGameState.None;

            if (mClient != null)
            {
                mClient.Clear();
                mClient = null;
            }

            mFrameListener = null;
            mFrameCtrl.Stop();
            mFrameBuffer.Clear();
            mIsRunning = false;

            onGameBegin = null;
            onRoundBegin = null;
            onControlStart = null;
            onGameEnd = null;
            onRoundEnd = null;
        }
        //==============================
        //设置监听器
        public void SetFrameListener(Action<int,FSPMessage> listener)
        {
            Debuger.Log();
            mFrameListener = listener;
        }

        //接收帧的处理函数
        private void OnFSPListener(FSPFrame frame)
        {
            AddServerFrame(frame);
        }

        //将帧加入到缓存
        private void AddServerFrame(FSPFrame frame)
        {
            //表示流程控制的帧
            //直接执行并退出
            if (frame.frameId <= 0)
            {
                ExecuteFrame(frame.frameId, frame);
                return;
            }

            //将帧缓存，之后抛给上层
            frame.frameId = frame.frameId * mParam.clientFrameRateMultiple;//计算客户端对应的帧
            mLockedFrameIndex = frame.frameId + mParam.clientFrameRateMultiple - 1;//锁至服务器帧对应客户端的前一帧

            mFrameBuffer.Add(frame.frameId, frame);

            mFrameCtrl.AddFrameId(frame.frameId);
        }

        #region 对基础流程Cmd的处理

        public void SendGameBegin()
        {
            Debuger.Log();
            SendFSP(FSPCommand.GAME_BEGIN, "begin");
        }

        public void SendGameBegin<T>(T data)
        {
            Debuger.Log();
            SendFSP(FSPCommand.GAME_BEGIN, data);
        }

        private void Handle_GameBegin(byte[] content)
        {
            Debuger.Log(content);
            mGameState = FSPGameState.GameBegin;
            if (onGameBegin != null)
            {
                onGameBegin(content);
            }
        }

        public void SendRoundBegin()
        {
            Debuger.Log();
            SendFSP(FSPCommand.ROUND_BEGIN, "");
        }
        public void SendRoundBegin<T>(T data)
        {
            Debuger.Log();
            SendFSP(FSPCommand.ROUND_BEGIN, data);
        }
        private void Handle_RoundBegin(byte[] content)
        {
            Debuger.Log(content);
            mGameState = FSPGameState.RoundBegin;
            mCurrentFrameIndex = 0;

            if (!mParam.useLocal)
            {
                mLockedFrameIndex = mParam.clientFrameRateMultiple - 1;
            }
            else
            {
                mLockedFrameIndex = mParam.maxFrameId;
            }

            mFrameBuffer.Clear();

            if (onRoundBegin != null)
            {
                onRoundBegin(content);
            }
        }

        public void SendControlStart()
        {
            Debuger.Log();
            SendFSP(FSPCommand.CONTROL_START, "");
        }
        public void SendControlStart<T>(T data)
        {
            Debuger.Log();
            SendFSP(FSPCommand.CONTROL_START, data);
        }
        private void Handle_ControlStart(byte[] content)
        {
            Debuger.Log(content);
            mGameState = FSPGameState.ControlStart;
            if (onControlStart != null)
            {
                onControlStart(content);
            }
        }

        public void SendRoundEnd()
        {
            Debuger.Log();
            SendFSP(FSPCommand.ROUND_END, "");
        }
        public void SendRoundEnd<T>(T data)
        {
            Debuger.Log();
            SendFSP(FSPCommand.ROUND_END, data);
        }
        private void Handle_RoundEnd(byte[] content)
        {
            Debuger.Log(content);
            mGameState = FSPGameState.RoundEnd;
            if (onRoundEnd != null)
            {
                onRoundEnd(content);
            }
        }

        public void SendGameEnd()
        {
            Debuger.Log();
            SendFSP(FSPCommand.GAME_END, "end");
        }
        public void SendGameEnd<T>(T data)
        {
            Debuger.Log();
            SendFSP(FSPCommand.GAME_END, data);
        }
        private void Handle_GameEnd(byte[] content)
        {
            Debuger.Log(content);
            mGameState = FSPGameState.GameEnd;
            if (onGameEnd != null)
            {
                onGameEnd(content);
            }
        }


        public void SendGameExit()
        {
            Debuger.Log();
            SendFSP(FSPCommand.GAME_EXIT, "exit");
        }

        private void Handle_GameExit(uint playerId)
        {
            Debuger.Log(playerId);
            if (onGameExit != null)
            {
                onGameExit(playerId);
            }
        }


        #endregion

        //===========================
        //发送FSP
        public void SendFSP<T>(int cmd, T content)
        {
            if (!mIsRunning)
            {
                return;
            }
            byte[] data = PBSerializer.NSerialize(content);

            if (mParam.useLocal)
            {
                SendFSPLocal(cmd, data);
            }
            else
            {
                mClient.SendFSP( cmd, mCurrentFrameIndex, data);
            }
        }

        //本地模拟发送
        private void SendFSPLocal(int cmd, byte[] content)
        {

            if (mNextLocalFrame == null || mNextLocalFrame.frameId != mCurrentFrameIndex + 1)
            {
                mNextLocalFrame = new FSPFrame();
                mNextLocalFrame.frameId = mCurrentFrameIndex + 1;

                mFrameBuffer.Add(mNextLocalFrame.frameId, mNextLocalFrame);
            }

            FSPMessage msg = new FSPMessage();
            msg.cmd = cmd;
            msg.content = content;
            msg.ClientFrameId = mNextLocalFrame.frameId;
            msg.PlayerId = mMinePlayerId;
            mNextLocalFrame.msgs.Add(msg);

        }


        //外部调用
        public void Tick()
        {
            if (!mIsRunning)
            {
                return;
            }


            if (mParam.useLocal)
            {
                //本地模拟时，如果没到限制
                if (mLockedFrameIndex == 0 || mLockedFrameIndex > mCurrentFrameIndex)
                {
                    //执行当前帧
                    mCurrentFrameIndex++;
                    var frame = mFrameBuffer[mCurrentFrameIndex];
                    ExecuteFrame(mCurrentFrameIndex, frame);
                }
            }
            else
            {
                mClient.Tick();

                int speed = mFrameCtrl.GetFrameSpeed(mCurrentFrameIndex);
                //通过控制每次tick执行的帧数来控制速度
                while (speed > 0)
                {
                    if (mCurrentFrameIndex < mLockedFrameIndex)
                    {
                        mCurrentFrameIndex++;
                        var frame = mFrameBuffer[mCurrentFrameIndex];
                        ExecuteFrame(mCurrentFrameIndex, frame);
                    }
                    speed--;
                }

            }
        }



        /// <summary>
        /// 执行每一帧
        /// </summary>
        /// <param name="frameId"></param>
        /// <param name="frame"></param>
        private void ExecuteFrame(int frameId, FSPFrame frame)
        {

            if (frame != null && !frame.IsEmpty())
            {
                for (int i = 0; i < frame.msgs.Count; i++)
                {
                    var msg = frame.msgs[i];
                    switch (msg.cmd)
                    {
                        case FSPCommand.GAME_BEGIN: Handle_GameBegin(msg.content); break;
                        case FSPCommand.ROUND_BEGIN: Handle_RoundBegin(msg.content); break;
                        case FSPCommand.CONTROL_START: Handle_ControlStart(msg.content); break;
                        case FSPCommand.ROUND_END: Handle_RoundEnd(msg.content); break;
                        case FSPCommand.GAME_END: Handle_GameEnd(msg.content); break;
                        case FSPCommand.GAME_EXIT: Handle_GameExit(msg.playerId); break;
                        default:
                            {
                                if (mFrameListener != null)
                                {
                                    mFrameListener(frameId,msg);
                                }
                                break;
                            }
                    }

                }
            }

            
        }
    }
}
