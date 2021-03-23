using System.Collections.Generic;

namespace Stream.Core
{
    public interface IStreamController
    {
        /// <summary>
        /// 開啟控制器
        /// 開始斷線重新連線機制
        /// </summary>
        void Start();

        /// <summary>
        /// 關閉串流控制器
        /// </summary>
        void Close();
        /// <summary>
        /// 等待重新連線機制結束
        /// </summary>
        void Wait();

        /// <summary>
        /// 檢查串流是否存在
        /// </summary>
        /// <param name="name"> 名子 </param>
        /// <returns></returns>
        bool StreamExist(string name);

        /// <summary>
        /// 回傳所有串流的資訊 (已連線、斷線)
        /// </summary>
        /// <returns></returns>
        IEnumerable<StreamInformation> GetAllStreamsInfo();

        /// <summary>
        /// 回傳已連線串流的資訊
        /// </summary>
        /// <returns></returns>
        IEnumerable<StreamInformation> GetConnectedStreamsInfo();

        /// <summary>
        /// 回傳斷線串流的資訊
        /// </summary>
        /// <returns></returns>
        IEnumerable<StreamInformation> GetDisconnectedStreamsInfo();
    }
}
