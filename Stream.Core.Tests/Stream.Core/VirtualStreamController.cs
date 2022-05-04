using System;
using System.Linq;

namespace Stream.Core.Tests.Stream.Core
{
    internal class VirtualStreamController : StreamController<int>
    {
        public VirtualStreamController() : base(1000) { }

        public void CreateStream(string name, string url)
        {
            lock (streamDictionaryLock)
            {
                disconnectedStreams.Add(name, new StreamParameter<int>(name, url));
                disconnectedStreams[name].StreamProvider = int.Parse(url);
                disconnectedStreams[name].CreateInstanceMethod = new Func<bool>(() => {
                    if (int.Parse(url) % 2 == 0)
                        return true;
                    else
                        return false;
                });
            }
        }

        public void RemoveStream(string name)
        {
            lock (streamDictionaryLock)
            {
                if (connectedStreams.ContainsKey(name))
                    connectedStreams.Remove(name);

                if (disconnectedStreams.ContainsKey(name))
                    disconnectedStreams.Remove(name);
            }
        }

        public void DisconnectionAllStreams()
        {
            var connectionsNames = connectedStreams.Select(item => item.Key);
            foreach (var connectionStream in connectionsNames)
                MoveToDisconnectedPuller(connectionStream);
        }

        public new void Close()
        {
            base.Close();
        }
    }
}
