using System;
using System.Net;
using uOSC;

namespace exiii.Unity.OSC
{
    public class UnityOscServer
    {
        public int Port { get; private set; } = 1234;

        public event Action<Message> OnDataReceived;

#if NETFX_CORE
        Udp udp_ = new uOSC.Uwp.Udp();
        Thread thread_ = new uOSC.Uwp.Thread();
#else
        private Udp m_Udp = new uOSC.DotNet.Udp();
        private Thread m_Thread = new uOSC.DotNet.Thread();
#endif
        private Parser m_Parser = new Parser();

        public void Open(int port)
        {
            Port = port;
            m_Udp.StartServer(Port);
            m_Thread.Start(UpdateMessage);
        }

        public void Close()
        {
            m_Thread.Stop();
            m_Udp.Stop();
        }

        private void UpdateMessage()
        {
            while (m_Udp.messageCount > 0)
            {
                var buf = m_Udp.Receive();
                int pos = 0;
                m_Parser.Parse(buf, ref pos, buf.Length);
            }

            while (m_Parser.messageCount > 0)
            {
                var message = m_Parser.Dequeue();
                 OnDataReceived.Invoke(message);
            }
        }
    }
}