using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stream.Core
{
    public abstract class StreamController<T>
    {
        protected volatile bool isClose = true;
        /// <summary>
        /// Key is name.
        /// </summary>
        protected Dictionary<string, StreamParameter<T>> connectedStreams;
        protected Dictionary<string, StreamParameter<T>> disconnectedStreams;
        protected object streamDictionaryLock;

        private readonly int reconnectDelay;
        private Task reconnectTask;

        public StreamController(int reconnectDelay = 60000)
        {
            this.reconnectDelay = reconnectDelay;
            connectedStreams = new Dictionary<string, StreamParameter<T>>();
            disconnectedStreams = new Dictionary<string, StreamParameter<T>>();
            streamDictionaryLock = new object();
        }

        /// <summary>
        /// 回傳所有串流的資訊 (已連線、斷線)
        /// </summary>
        /// <returns></returns>
        public ICollection<StreamInformation> GetAllStreamsInfo()
        {
            lock (streamDictionaryLock)
                return connectedStreams.Values.Concat(disconnectedStreams.Values).ToArray();
        }

        /// <summary>
        /// 回傳已連線串流的資訊
        /// </summary>
        /// <returns></returns>
        public ICollection<StreamInformation> GetConnectedStreamsInfo()
        {
            lock (streamDictionaryLock)
                return connectedStreams.Values.ToArray();
        }

        /// <summary>
        /// 回傳斷線串流的資訊
        /// </summary>
        /// <returns></returns>
        public ICollection<StreamInformation> GetDisconnectedStreamsInfo()
        {
            lock (streamDictionaryLock)
                return disconnectedStreams.Values.ToArray();
        }

        /// <summary>
        /// 將串流從已連線改為斷線
        /// </summary>
        /// <param name="name"></param>
        protected void MoveToDisconnectedPuller(string name)
        {
            lock (streamDictionaryLock)
            {
                StreamParameter<T> puller = connectedStreams[name];
                connectedStreams.Remove(name);
                disconnectedStreams.Add(name, puller);
            }
        }

        /// <summary>
        /// 開啟控制器
        /// 開始斷線重新連線機制
        /// </summary>
        public void Start()
        {
            if (!isClose)
                return;

            isClose = false;
            reconnectTask = Task.Factory.StartNew(ReconnectWorker, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// 檢查串流是否存在
        /// </summary>
        /// <param name="name"> 名子 </param>
        /// <returns></returns>
        public bool StreamExist(string name)
        {
            lock (streamDictionaryLock)
                if (connectedStreams.ContainsKey(name) || disconnectedStreams.ContainsKey(name))
                    return true;

            return false;
        }

        /// <summary>
        /// 建立串流提供者
        /// 建立成功串流提供者參數會放置 connectedStreams
        /// 建立失敗串流提供者參數會放置 disconnectedStreams
        /// </summary>
        /// <param name="parameter"></param>
        protected async Task CreateProviderTaskAsync(StreamParameter<T> parameter)
        {
            if (StreamExist(parameter.Name))
                throw new StreamCoreException("Stream name already exists.");

            bool isInit = await Task.Run(() => parameter.CreateInstanceMethod.Invoke());

            lock (streamDictionaryLock)
                if (isInit)
                    connectedStreams.Add(parameter.Name, parameter);
                else
                    disconnectedStreams.Add(parameter.Name, parameter);
        }

        /// <summary>
        /// 關閉控制器
        /// 停止斷線重新連線機制
        /// </summary>
        protected void Close()
        {
            if (isClose)
                return;

            isClose = true;
            reconnectTask.Wait();
            reconnectTask.Dispose();
        }

        /// <summary>
        /// 重新連線機制
        /// </summary>
        private void ReconnectWorker()
        {
            while (!isClose)
            {
                Dictionary<string, StreamParameter<T>> disconnectedStreamsDuplicate = null;

                lock (streamDictionaryLock)
                {
                    if (disconnectedStreams.Any())
                    {
                        disconnectedStreamsDuplicate = disconnectedStreams;
                        disconnectedStreams = new Dictionary<string, StreamParameter<T>>();
                    }
                }

                if (disconnectedStreamsDuplicate != null)
                    Parallel.ForEach(disconnectedStreamsDuplicate, (disconnectedStream) => {
                        CreateProviderTaskAsync(disconnectedStream.Value).Wait();
                    });

                SpinWait.SpinUntil(() => isClose, reconnectDelay);
            }
        }
    }

    public class StreamInformation
    {
        public string Name { get; }
        public string Url { get; }

        public StreamInformation(string name, string url)
            => (Name, Url) = (name, url);
    }

    public class StreamParameter<T> : StreamInformation
    {
        public T StreamProvider { get; set; }

        /// <summary>
        /// 連接用來建立處理串流提供者實體的方法
        /// </summary>
        public Func<bool> CreateInstanceMethod { get; set; }

        public StreamParameter(string name, string url)
            : base(name, url) { }
    }
}
