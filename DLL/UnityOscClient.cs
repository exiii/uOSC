using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using exiii.Unity.OSC;

namespace exiii.Unity.OSC
{
    public class UnityOscClient
    {
        private const int BufferSize = 8192;
        private const int MaxQueueSize = 100;

        private string m_RemoteIPString = "127.0.0.1";

        public IPAddress RemoteIP
        {
            get { return IPAddress.Parse(m_RemoteIPString); }
        }

        public int RemotePort { get; private set; } = 3333;

        public IPAddress LocalIP { get; private set; }

#if NETFX_CORE
        Udp udp_ = new uOSC.Uwp.Udp();
        Thread thread_ = new uOSC.Uwp.Thread();
#else
        private Udp m_Udp = new exiii.Unity.OSC.DotNet.Udp();
        private Thread m_Thread = new exiii.Unity.OSC.DotNet.Thread();
#endif
        private Queue<object> m_Messages = new Queue<object>();
        private object m_LockObject = new object();

        public void Open(string remoteIP, int remotePort, string localIP)
        {
            m_RemoteIPString = remoteIP;
            RemotePort = remotePort;

            if (IPAddress.TryParse(localIP, out IPAddress local))
            {
                LocalIP = local;
            }
            else
            {
                throw new Exception("LocalIP is not valid.");
            }

            m_Udp.StartClient(m_RemoteIPString, RemotePort);
            m_Thread.Start(UpdateSend);
        }

        public void Close()
        {
            m_Thread.Stop();
            m_Udp.Stop();
        }

        private void UpdateSend()
        {
            while (m_Messages.Count > 0)
            {
                object message;
                lock (m_LockObject)
                {
                    message = m_Messages.Dequeue();
                }

                using (var stream = new MemoryStream(BufferSize))
                {
                    if (message is Message)
                    {
                        ((Message)message).Write(stream);
                    }
                    else if (message is Bundle)
                    {
                        ((Bundle)message).Write(stream);
                    }
                    else
                    {
                        return;
                    }
                    m_Udp.Send(Util.GetBuffer(stream), (int)stream.Position);
                }
            }
        }

        private void Add(object data)
        {
            lock (m_LockObject)
            {
                m_Messages.Enqueue(data);

                while (m_Messages.Count > MaxQueueSize)
                {
                    m_Messages.Dequeue();
                }
            }
        }

        public void Send(string address, params object[] values)
        {
            Send(new Message()
            {
                address = address,
                values = values
            });
        }

        public void Send(Message message)
        {
            Add(message);
        }

        public void Send(Bundle bundle)
        {
            Add(bundle);
        }
    }
}