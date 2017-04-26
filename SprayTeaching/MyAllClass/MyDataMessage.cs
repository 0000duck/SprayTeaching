using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SprayTeaching.BaseClassLib;

namespace SprayTeaching.MyAllClass
{
    public class MyDataMessage
    {
        private Queue<byte[]> _queueReceiveDataMessage = new Queue<byte[]>();       // 临时存放日志消息的队列 

        private Thread _thrdDataMessageHandler;                                     // 数据消息处理的线程
        private bool _bolIsThreadAlive = true;                                      // 控制线程活着，true为活着，false为死亡
        private AutoResetEvent _autoEvent = new AutoResetEvent(false);              // 控制数据消息处理的线程，控制它的睡眠和唤醒

        public event UpdateMessageStateInformEventHandler UpdateMcuIsConnected;                     // 下位机发送上来的连接响应
        public event UpdateMessageStateInformEventHandler UpdateMcuIsReady;                         // 下位机发送上来的准备就绪是否可以开始采集数据
        public event UpdateMessageStateInformEventHandler UpdateSDIsReaded;                         // 下位机发送上来的SD卡读取完毕
        public event UpdateMessageStateInformEventHandler UpdateSampleIsStopped;                    // 下位机发送上来的采集数据是否停止
        public event UpdateMessageStateInformEventHandler UpdateLance1IsOpened;                     // 下位机发送上来的喷枪1是否打开
        public event UpdateMessageStateInformEventHandler UpdateMotorIsRan;                         // 下位机发送上来的电机是否开转
        public event UpdateMessageStateInformEventHandler UpdateLance2IsOpened;                     // 下位机发送上来的喷枪2是否打开

        public event UpdateMessageSampleInformEventHandler UpdateSampleInform;                      // 下位机发送上来的采样频率和采样周期
        public event UpdateMessageAxisErrorEventHandler UpdateAxisError;                            // 下位机发送上来的哪个轴没有响应
        public event UpdateMessageAxisAddressEventHandler UpdateAxisAddress;                        // 下位机发送上来的各个轴地址
        public event UpdateMessageAxisDataEventHandler UpdateAxisData;                              // 下位机发送上来的各个轴的数据
        public event UpdateMessageAxisModifiedIsSuccessEventHandler UpdateAxisModifiedIsSuccess;    // 下位机发送上来的轴地址修改是否成功
        public event UpdateMessageSetFrequentIsSuccessed UpdateSetFrequentIsSuccess;                // 下位机发送上来的频率设置是否成功

        public MyDataMessage()
        {
            this._thrdDataMessageHandler = new Thread(ThreadDataMessageHandler);            // 初始化将数据消息处理事件的线程
            this._thrdDataMessageHandler.IsBackground = true;                               // 设置成后台线程，在前台线程结束时，所有剩余的后台线程都会停止且不会完成    
            this._thrdDataMessageHandler.Name = "DataMessageHandler";                       // 设置线程的名字
            this._thrdDataMessageHandler.Start();                                           // 启动线程
        }

        #region 关闭资源相关方法
        /// <summary>
        /// 关闭MyLog的所有资源,先关闭线程，再使变量无效
        /// </summary>
        public void Close()
        {
            this.CloseThreadDataMessageHandler();
            this.CloseAllVariable();
        }

        /// <summary>
        /// 关闭所有变量，使它们都invalidition
        /// </summary>
        private void CloseAllVariable()
        {
            this._thrdDataMessageHandler = null;
            this._bolIsThreadAlive = false;
        }

        /// <summary>
        /// 关闭写日志到文件的线程
        /// </summary>
        private void CloseThreadDataMessageHandler()
        {
            if (this._thrdDataMessageHandler != null)
            {
                this._bolIsThreadAlive = false;             // 关闭线程
                Thread.Sleep(110);                          // 等待线程关闭
                this._thrdDataMessageHandler.Abort();
                this._thrdDataMessageHandler = null;
            }
        }
        #endregion

        #region 数据消息处理部分
        /// <summary>
        /// 处理数据消息的线程
        /// </summary>
        private void ThreadDataMessageHandler()
        {
            while (this._bolIsThreadAlive)
            {
                if (this._queueReceiveDataMessage.Count != 0)
                {
                    DataMessageHandler();
                    Thread.Sleep(100);
                }
                else
                {
                    this._autoEvent.WaitOne();                      // 阻止当前线程，直到当前 WaitHandle 收到信号为止
                }
            }
        }

        /// <summary>
        /// 处理数据消息
        /// </summary>
        private void DataMessageHandler()
        {
            if (this._queueReceiveDataMessage.Count == 0)
                return;
            byte[] byteDataMessage = this._queueReceiveDataMessage.Dequeue();           // 出队，获取数据
            this.DataRecognition(byteDataMessage);                                      // 对数据进行识别
        }


        /// <summary>
        /// 对数据进行识别
        /// </summary>
        /// <param name="byteDataMessage">传进来的数据</param>
        private void DataRecognition(byte[] byteDataMessage)
        {
            byte byteTmp = byteDataMessage[0];
            switch (byteTmp)
            {
                case 0xC0:  // 下位机发送上来的采样频率和采样周期
                    this.UpdateSampleInformHandler(byteDataMessage);
                    break;
                case 0xE0:  // 下位机发送上来的哪个轴没有响应
                    this.UpdateAxisErrorHandler(byteDataMessage);
                    break;
                case 0xF0:  // 下位机发送上来的连接响应
                    this.UpdateMcuIsConnectedHandler(byteDataMessage);
                    break;
                case 0xF1:  // 下位机发送上来的准备就绪是否可以开始采集数据
                    this.UpdateMcuIsReadyHandler(byteDataMessage);
                    break;
                case 0xF2:  // 下位机发送上来的SD卡是否读取完毕
                    this.UpdateSDIsReadedHandler(byteDataMessage);
                    break;
                case 0xF3:  // 下位机发送上来的频率设置成功
                case 0xFE:  // 下位机发送上来的频率设置失败
                    this.UpdateSetFrequentIsSuccessHandler(byteDataMessage);
                    break;
                case 0xF4:  // 下位机发送上来的采集数据是否停止
                    this.UpdateSampleIsStoppedHandler(byteDataMessage);
                    break;
                case 0xF5:  // 下位机发送上来的喷枪1打开
                case 0xF6:  // 下位机发送上来的喷枪1关闭
                    this.UpdateLance1IsOpenedHandler(byteDataMessage);
                    break;
                case 0xF7:  // 下位机发送上来的电机是否开转
                    this.UpdateMotorIsRanHandler(byteDataMessage);
                    break;
                case 0xF8:  // 下位机发送上来的喷枪2打开
                case 0xF9:  // 下位机发送上来的喷枪2关闭
                    this.UpdateLance2IsOpenedHandler(byteDataMessage);
                    break;
                case 0xFA:  // 下位机发送上来的各个轴地址
                    this.UpdateAxisAddressHandler(byteDataMessage);
                    break;
                case 0xFB:  // 下位机发送上来的轴地址修改成功
                case 0xFC:  // 下位机发送上来的轴地址修改失败
                    this.UpdateAxisModifiedIsSuccessHandler(byteDataMessage);
                    break;
                case 0xFD:  // 下位机发送上来的各个轴的数据
                    this.UpdateAxisDataHandler(byteDataMessage);
                    break;
            }
        }

        /// <summary>
        /// 下位机发送上来的连接响应
        /// </summary>
        /// <param name="byteDataMessage">数据消息</param>
        private void UpdateMcuIsConnectedHandler(byte[] byteDataMessage)
        {
            bool bolIsConnected = false;

            if (this.UpdateMcuIsConnected != null)
                this.UpdateMcuIsConnected(bolIsConnected);
        }

        /// <summary>
        /// 下位机发送上来的准备就绪是否可以开始采集数据
        /// </summary>
        /// <param name="byteDataMessage">数据消息</param>
        private void UpdateMcuIsReadyHandler(byte[] byteDataMessage)
        {
            bool bolIsReady = false;
            if (this.UpdateMcuIsReady != null)
                this.UpdateMcuIsReady(bolIsReady);
        }

        /// <summary>
        /// 下位机发送上来的SD卡是否读取完毕
        /// </summary>
        /// <param name="byteDataMessage">数据消息</param>
        private void UpdateSDIsReadedHandler(byte[] byteDataMessage)
        {
            bool bolIsReaded = false;
            if (this.UpdateSDIsReaded != null)
                this.UpdateSDIsReaded(bolIsReaded);
        }

        /// <summary>
        /// 下位机发送上来的采集数据是否停止
        /// </summary>
        /// <param name="byteDataMessage">数据消息</param>
        private void UpdateSampleIsStoppedHandler(byte[] byteDataMessage)
        {
            bool bolIsStopped = false;
            if (this.UpdateSampleIsStopped != null)
                this.UpdateSampleIsStopped(bolIsStopped);
        }

        /// <summary>
        /// 下位机发送上来的喷枪1是否打开
        /// </summary>
        /// <param name="byteDataMessage">数据消息</param>
        private void UpdateLance1IsOpenedHandler(byte[] byteDataMessage)
        {
            bool bolIsOpened = false;
            if (this.UpdateLance1IsOpened != null)
                this.UpdateSampleIsStopped(bolIsOpened);
        }

        /// <summary>
        /// 下位机发送上来的电机是否开转
        /// </summary>
        /// <param name="byteDataMessage">数据消息</param>
        private void UpdateMotorIsRanHandler(byte[] byteDataMessage)
        {
            bool bolIsRan = false;
            if (this.UpdateMotorIsRan != null)
                this.UpdateMotorIsRan(bolIsRan);
        }

        /// <summary>
        /// 下位机发送上来的喷枪2是否打开
        /// </summary>
        /// <param name="byteDataMessage">数据消息</param>
        private void UpdateLance2IsOpenedHandler(byte[] byteDataMessage)
        {
            bool bolIsOpened = false;
            if (this.UpdateLance2IsOpened != null)
                this.UpdateLance2IsOpened(bolIsOpened);
        }

        /// <summary>
        /// 下位机发送上来的采样频率和采样周期
        /// </summary>
        /// <param name="byteDataMessage">数据消息</param>
        private void UpdateSampleInformHandler(byte[] byteDataMessage)
        {
            double dblSampleFrequent = 0.0;
            double dblSampleTime = 0.0;
            if (this.UpdateSampleInform != null)
                this.UpdateSampleInform(dblSampleFrequent, dblSampleTime);
        }

        /// <summary>
        /// 下位机发送上来的哪个轴没有响应
        /// </summary>
        /// <param name="byteDataMessage">数据消息</param>
        private void UpdateAxisErrorHandler(byte[] byteDataMessage)
        {
            int intAxisID = 0;
            if (this.UpdateAxisError != null)
                this.UpdateAxisError(intAxisID);
        }

        /// <summary>
        /// 下位机发送上来的各个轴地址
        /// </summary>
        /// <param name="byteDataMessage">数据消息</param>
        private void UpdateAxisAddressHandler(byte[] byteDataMessage)
        {
            double[] dblAxisAddress = new double[] { };
            if (this.UpdateAxisAddress != null)
                this.UpdateAxisAddress(dblAxisAddress);
        }

        /// <summary>
        /// 下位机发送上来的各个轴的数据
        /// </summary>
        /// <param name="byteDataMessage">数据消息</param>
        private void UpdateAxisDataHandler(byte[] byteDataMessage)
        {
            double[] dblAxisData = new double[] { };
            if (this.UpdateAxisData != null)
                this.UpdateAxisData(dblAxisData);
        }

        /// <summary>
        /// 下位机发送上来的轴地址修改是否成功
        /// </summary>
        /// <param name="byteDataMessage">数据消息</param>
        private void UpdateAxisModifiedIsSuccessHandler(byte[] byteDataMessage)
        {
            int intAxisNum = 0;
            if (this.UpdateAxisModifiedIsSuccess != null)
                this.UpdateAxisModifiedIsSuccess(intAxisNum);
        }

        /// <summary>
        /// 下位机发送上来的频率设置是否成功
        /// </summary>
        /// <param name="byteDataMessage">数据消息</param>
        private void UpdateSetFrequentIsSuccessHandler(byte[] byteDataMessage)
        {
            double dblFrequent = 0.0;
            bool bolIsSuccess = false;
            if (this.UpdateSetFrequentIsSuccess != null)
                this.UpdateSetFrequentIsSuccess(bolIsSuccess, dblFrequent);
        }

        #endregion

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="byteDataMessage">传进来的数据</param>
        private void ReceiveDataMessage(byte[] byteDataMessage)
        {
            this._queueReceiveDataMessage.Enqueue(byteDataMessage);
            this._autoEvent.Set();                                      // 将事件状态设置为终止状态，允许一个或多个等待线程继续
        }

        private void SendDataMessage()
        {

        }
    }
}
