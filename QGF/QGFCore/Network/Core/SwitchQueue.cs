using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets.Kcp;
using System.Text;
using System.Threading.Tasks;

namespace QGF.Network.Core
{
    public class KcpSendHandler : IKcpCallback
    {
        public Action<byte[], int> handler;
        public void Output(IMemoryOwner<byte> buffer, int avalidLength)
        {
            handler(buffer.Memory.ToArray(), avalidLength);
        }
    }
    public class Utility
    {

        public static void Swap<QT>(ref QT t1, ref QT t2)
        {

            QT temp = t1;
            t1 = t2;
            t2 = temp;
        }
    }

    public class SwitchQueue<T> where T : class
    {

        private Queue mConsumeQueue;
        private Queue mProduceQueue;

        public SwitchQueue()
        {
            mConsumeQueue = new Queue(16);
            mProduceQueue = new Queue(16);
        }

        public SwitchQueue(int capcity)
        {
            mConsumeQueue = new Queue(capcity);
            mProduceQueue = new Queue(capcity);
        }

        // producer
        public void Push(T obj)
        {
            lock (mProduceQueue)
            {
                mProduceQueue.Enqueue(obj);
            }
        }

        // consumer.
        public T Pop()
        {

            return (T)mConsumeQueue.Dequeue();
        }

        public bool Empty()
        {
            return 0 == mConsumeQueue.Count;
        }

        public void Switch()
        {
            lock (mProduceQueue)
            {
                Utility.Swap(ref mConsumeQueue, ref mProduceQueue);
            }
        }

        public void Clear()
        {
            lock (mProduceQueue)
            {
                mConsumeQueue.Clear();
                mProduceQueue.Clear();
            }
        }
    }
}
