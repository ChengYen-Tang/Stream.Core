using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Stream.Core.Tests.Stream.Core
{
    [TestClass]
    public class StreamControllerTests
    {
        private VirtualStreamController sc;

        [TestInitialize]
        public void Init()
        {
            sc = new VirtualStreamController();
            sc.Start();
        }

        [TestCleanup]
        public void Clean()
            => sc.Close();

        [TestMethod]
        public async Task TestReconnection()
        {
            for (int i = 0; i < 6; i++)
            {
                sc.CreateStream(i.ToString(), i.ToString());
                Assert.IsTrue(sc.StreamExist(i.ToString()));
            }

            Assert.IsFalse(sc.StreamExist("§Ú¦n«Ó"));

            await Task.Delay(1000);
            Assert.IsTrue(sc.GetAllStreamsInfo().Count() == 6);
            Assert.IsTrue(sc.GetConnectedStreamsInfo().Count() == 3);
            Assert.IsTrue(sc.GetDisconnectedStreamsInfo().Count() == 3);
            sc.DisconnectionAllStreams();
            await Task.Delay(1000);
            Assert.IsTrue(sc.GetAllStreamsInfo().Count() == 6);
            Assert.IsTrue(sc.GetConnectedStreamsInfo().Count() == 3);
            Assert.IsTrue(sc.GetDisconnectedStreamsInfo().Count() == 3);
        }
    }
}
