using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QGF.Network.FSPLite.Client
{
    public class FSPFrameController
    {
        //缓冲控制
        private int mClientFrameRateMultiple = 2;//客户端与服务端的帧率比
        private int mJitterBuffSize = 0;//缓冲大小(需始终保持该buffer存在内容）
        private bool mIsInBuffing = false;//是否正在缓冲
        private int mNewestFrameId;//最新帧


        //加速控制
        private bool mEnableSpeedUp = true;//是否允许加速
        private int mDefaultSpeed = 1;//默认速度
        private bool mIsInSpeedUp = false;//是否在加速

        //自动缓冲
        private bool mEnableAutoBuff = true;//是否自动缓冲
        private int mAutoBuffCnt = 0;//自动缓冲数
        private int mAutoBuffInterval = 15;//自动缓冲间隔帧数（即缓冲多少帧）


        public bool IsInBuffing { get { return mIsInBuffing; } }
        public bool IsInSpeedUp { get { return mIsInSpeedUp; } }
        public int JitterBufferSize { get { return mJitterBuffSize; } set { mJitterBuffSize = value; } }
        public int NewestFrameId { get { return mNewestFrameId; } }


        public void Start(FSPParam param)
        {
            SetParam(param);
        }

        public void Stop()
        {

        }

        public void SetParam(FSPParam param)
        {
            mClientFrameRateMultiple = param.clientFrameRateMultiple;
            mJitterBuffSize = param.jitterBufferSize;
            mEnableSpeedUp = param.enableSpeedUp;
            mDefaultSpeed = param.defaultSpeed;
            mEnableAutoBuff = param.enableAutoBuffer;

        }


        public void AddFrameId(int frameId)
        {
            mNewestFrameId = frameId;
        }


        public int GetFrameSpeed(int curFrameId)
        {
            int speed = 0;
            //获取新的帧数
            var newFrameNum = mNewestFrameId - curFrameId;

            //是否正在缓冲中
            if (!mIsInBuffing)
            {
                //没有在缓冲中

                if (newFrameNum == 0)
                {
                    //没有新的帧数
                    //需要缓冲一下
                    mIsInBuffing = true;
                    mAutoBuffCnt = mAutoBuffInterval;
                }
                else
                {
                    //因为即将播去这么多帧
                    newFrameNum -= mDefaultSpeed;

                    //获取需要加速的帧数(即新帧数-在缓冲区的帧数)
                    int speedUpFrameNum = newFrameNum - mJitterBuffSize;

                    //当多于下一服务帧应执行的帧数即可以加速
                    if (speedUpFrameNum >= mClientFrameRateMultiple)
                    {
                        //可以加速
                        if (mEnableSpeedUp)
                        {
                            speed = 2;
                            if (speedUpFrameNum > 100)
                            {
                                speed = 8;
                            }
                            else if (speedUpFrameNum > 50)
                            {
                                speed = 4;
                            }
                        }
                        else
                        {
                            speed = mDefaultSpeed;
                        }
                    }
                    else
                    {
                        //还达不到可加速的帧数
                        speed = mDefaultSpeed;

                        if (mEnableAutoBuff)
                        {

                            mAutoBuffCnt--;
                            //即若在15帧内都达不到加速的情况（代表网络状况不行）
                            if (mAutoBuffCnt <= 0)
                            {
                                mAutoBuffCnt = mAutoBuffInterval;
                                if (speedUpFrameNum < mClientFrameRateMultiple - 1)
                                {
                                    //这个时候大概率接下来会缺帧
                                    speed = 0;
                                }
                            }
                        }

                    }

                }
            }
            else
            {
                //正在缓冲中
                int speedUpFrameNum = newFrameNum - mJitterBuffSize;
                if (speedUpFrameNum > 0)
                {
                    mIsInBuffing = false;
                }
            }

            mIsInSpeedUp = speed > mDefaultSpeed;
            return speed;
        }
    }
}
