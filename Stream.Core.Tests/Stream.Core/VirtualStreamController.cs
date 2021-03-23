using System.Linq;

namespace Stream.Core.Tests.Stream.Core
{
    internal class VirtualStreamController : StreamController<int>
    {
        public VirtualStreamController() : base(100) { }

        public void CreateStream(string name, string url)
        {
            lock (streamDictionaryLock)
            {
                disconnectedStreams.Add(name, new StreamParameter<int>(name, url));
                disconnectedStreams[name].Puller = int.Parse(url);
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
                MoveDisconnectedPuller(connectionStream);
        }

        protected override void ReconnectHandler(StreamParameter<int> pullerParameter)
        {
            if (pullerParameter.Puller % 2 == 0)
                lock (streamDictionaryLock)
                    connectedStreams.Add(pullerParameter.Name, pullerParameter);
            else
                lock (streamDictionaryLock)
                    disconnectedStreams.Add(pullerParameter.Name, pullerParameter);
        }

        public new void Close()
        {
            base.Close();
        }
    }
}
