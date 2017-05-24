using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SprayTeaching.BaseClassLib;
using SprayTeaching.Model;

namespace SprayTeaching.MyAllClass
{
    public class MyDataMessage
    {
        private Queue<byte[]> _queueReceiveDataMessage = new Queue<byte[]>();       // 临时存放日志消息的队列 
        private Thread _thrdDataMessageHandler;                                     // 数据消息处理的线程
        private bool _bolIsThreadAlive = true;                                      // 控制线程活着，true为活着，false为死亡
        private AutoResetEvent _autoEvent = new AutoResetEvent(false);              // 控制数据消息处理的线程，控制它的睡眠和唤醒

        #region 属性
        /// <summary>
        /// 标定的关节方向
        /// </summary>
        private double[] _dblCalibrateDirections = null;

        public double[] CalibrateDirections
        {
            get { return _dblCalibrateDirections; }
            set { _dblCalibrateDirections = value; }
        }

        /// <summary>
        /// 标定的关节角度
        /// </summary>
        private double[] _dblCalibrateAngles = null;

        public double[] CalibrateAngles
        {
            get { return _dblCalibrateAngles; }
            set { _dblCalibrateAngles = value; }
        }
        #endregion

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

        public event UpdateLogContentEventHandler UpdateLogContent;                                 // 更新日志文件 
        public event Action<object> UpdateAbsoluteAxisAngle;                                        // 更新轴角度的绝对值

        public MyDataMessage( )
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
        public void Close( )
        {
            this.CloseThreadDataMessageHandler();
            this.CloseAllVariable();
        }

        /// <summary>
        /// 关闭所有变量，使它们都invalidition
        /// </summary>
        private void CloseAllVariable( )
        {
            this._thrdDataMessageHandler = null;
            this._bolIsThreadAlive = false;
            this._autoEvent = null;
            this._queueReceiveDataMessage = null;
        }

        /// <summary>
        /// 关闭写日志到文件的线程
        /// </summary>
        private void CloseThreadDataMessageHandler( )
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

        #region 写入日志
        /// <summary>
        /// 将消息写入日志
        /// </summary>
        /// <param name="strMessage">消息内容</param>
        private void WriteLogHandler(string strMessage, int intType = 0)
        {
            if (this.UpdateLogContent != null)
            {
                this.UpdateLogContent(strMessage, intType);
            }
        }
        #endregion

        #region 接收数据消息处理部分
        /// <summary>
        /// 处理数据消息的线程
        /// </summary>
        private void ThreadDataMessageHandler( )
        {
            Thread.Sleep(1);        // 延迟启动，为避免初始化时候出现问题
            while (this._bolIsThreadAlive)
            {
                if (this._queueReceiveDataMessage.Count != 0)
                {
                    DataMessageHandler();
                    Thread.Sleep(10);
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
        private void DataMessageHandler( )
        {
            if (this._queueReceiveDataMessage.Count == 0)
                return;
            byte[] byteDataMessage = this._queueReceiveDataMessage.Dequeue();           // 出队，获取数据
            this.DataRecognition(byteDataMessage);                                      // 对数据进行识别
        }


        /// <summary>
        /// 对数据进行识别
        /// </summary>
        /// <param name="byteDataMessages">传进来的数据</param>
        private void DataRecognition(byte[] byteDataMessage)
        {
            byte[] byteDataMessageTmp = byteDataMessage.Skip(2).Take(13).ToArray();
            byte byteTmp = byteDataMessageTmp[0];
            switch (byteTmp)
            {
                case 0xC0:  // 下位机发送上来的采样频率和采样周期
                    this.UpdateSampleInformHandler(byteDataMessageTmp);
                    break;
                case 0xE0:  // 下位机发送上来的哪个轴没有响应
                    this.UpdateAxisErrorHandler(byteDataMessageTmp);
                    break;
                case 0xF0:  // 下位机发送上来的连接响应
                    this.UpdateMcuIsConnectedHandler(byteDataMessageTmp);
                    break;
                case 0xF1:  // 下位机发送上来的准备就绪是否可以开始采集数据
                    this.UpdateMcuIsReadyHandler(byteDataMessageTmp);
                    break;
                case 0xF2:  // 下位机发送上来的SD卡是否读取完毕
                    this.UpdateSDIsReadedHandler(byteDataMessageTmp);
                    break;
                case 0xF3:  // 下位机发送上来的频率设置成功
                case 0xFE:  // 下位机发送上来的频率设置失败
                    this.UpdateSetFrequentIsSuccessHandler(byteDataMessageTmp);
                    break;
                case 0xF4:  // 下位机发送上来的采集数据是否停止
                    this.UpdateSampleIsStoppedHandler(byteDataMessageTmp);
                    break;
                case 0xF5:  // 下位机发送上来的喷枪1打开
                case 0xF6:  // 下位机发送上来的喷枪1关闭
                    this.UpdateLance1IsOpenedHandler(byteDataMessageTmp);
                    break;
                case 0xF7:  // 下位机发送上来的电机是否开转
                    this.UpdateMotorIsRanHandler(byteDataMessageTmp);
                    break;
                case 0xF8:  // 下位机发送上来的喷枪2打开
                case 0xF9:  // 下位机发送上来的喷枪2关闭
                    this.UpdateLance2IsOpenedHandler(byteDataMessageTmp);
                    break;
                case 0xFA:  // 下位机发送上来的各个轴地址
                    this.UpdateAxisAddressHandler(byteDataMessageTmp);
                    break;
                case 0xFB:  // 下位机发送上来的轴地址修改成功
                case 0xFC:  // 下位机发送上来的轴地址修改失败
                    this.UpdateAxisModifiedIsSuccessHandler(byteDataMessageTmp);
                    break;
                case 0xFD:  // 下位机发送上来的各个轴的数据
                    this.UpdateAxisDataHandler(byteDataMessageTmp);
                    break;
            }
        }

        /// <summary>
        /// 下位机发送上来的连接响应
        /// </summary>
        /// <param name="byteDataMessages">数据消息</param>
        private void UpdateMcuIsConnectedHandler(byte[] byteDataMessage)
        {
            bool bolIsConnected = false;

            if (byteDataMessage[0] == 0xF0)
                bolIsConnected = true;

            if (this.UpdateMcuIsConnected != null)
                this.UpdateMcuIsConnected(bolIsConnected);
        }

        /// <summary>
        /// 下位机发送上来的准备就绪是否可以开始采集数据
        /// </summary>
        /// <param name="byteDataMessages">数据消息</param>
        private void UpdateMcuIsReadyHandler(byte[] byteDataMessage)
        {
            bool bolIsReady = false;

            if (byteDataMessage[0] == 0xF1)
                bolIsReady = true;

            if (this.UpdateMcuIsReady != null)
                this.UpdateMcuIsReady(bolIsReady);
        }

        /// <summary>
        /// 下位机发送上来的SD卡是否读取完毕
        /// </summary>
        /// <param name="byteDataMessages">数据消息</param>
        private void UpdateSDIsReadedHandler(byte[] byteDataMessage)
        {
            bool bolIsReaded = false;

            if (byteDataMessage[1] == 0xF2)
                bolIsReaded = true;

            if (this.UpdateSDIsReaded != null)
                this.UpdateSDIsReaded(bolIsReaded);
        }

        /// <summary>
        /// 下位机发送上来的采集数据是否停止
        /// </summary>
        /// <param name="byteDataMessages">数据消息</param>
        private void UpdateSampleIsStoppedHandler(byte[] byteDataMessage)
        {
            bool bolIsStopped = false;

            if (byteDataMessage[0] == 0xF4)
                bolIsStopped = true;

            if (this.UpdateSampleIsStopped != null)
                this.UpdateSampleIsStopped(bolIsStopped);
        }

        /// <summary>
        /// 下位机发送上来的喷枪1是否打开
        /// </summary>
        /// <param name="byteDataMessages">数据消息</param>
        private void UpdateLance1IsOpenedHandler(byte[] byteDataMessage)
        {
            bool bolIsOpened = false;

            if (byteDataMessage[0] == 0xF5)
                bolIsOpened = true;
            else
                bolIsOpened = false;

            if (this.UpdateLance1IsOpened != null)
                this.UpdateSampleIsStopped(bolIsOpened);
        }

        /// <summary>
        /// 下位机发送上来的电机是否开转
        /// </summary>
        /// <param name="byteDataMessages">数据消息</param>
        private void UpdateMotorIsRanHandler(byte[] byteDataMessage)
        {
            bool bolIsRan = false;

            if (byteDataMessage[0] == 0xF7)
                bolIsRan = true;

            if (this.UpdateMotorIsRan != null)
                this.UpdateMotorIsRan(bolIsRan);
        }

        /// <summary>
        /// 下位机发送上来的喷枪2是否打开
        /// </summary>
        /// <param name="byteDataMessages">数据消息</param>
        private void UpdateLance2IsOpenedHandler(byte[] byteDataMessage)
        {
            bool bolIsOpened = false;

            if (byteDataMessage[0] == 0xF8)
                bolIsOpened = true;
            else
                bolIsOpened = false;

            if (this.UpdateLance2IsOpened != null)
                this.UpdateLance2IsOpened(bolIsOpened);
        }

        /// <summary>
        /// 下位机发送上来的采样频率和采样周期
        /// </summary>
        /// <param name="byteDataMessages">数据消息</param>
        private void UpdateSampleInformHandler(byte[] byteDataMessage)
        {
            int intSampleFrequent = 0;
            int intSampleCycle = 0;

            if (byteDataMessage[0] != 0xC0)
                return;

            intSampleFrequent = byteDataMessage[1];
            intSampleCycle = byteDataMessage[2];

            if (this.UpdateSampleInform != null)
                this.UpdateSampleInform(intSampleFrequent, intSampleCycle);
        }

        /// <summary>
        /// 下位机发送上来的哪个轴没有响应
        /// </summary>
        /// <param name="byteDataMessages">数据消息</param>
        private void UpdateAxisErrorHandler(byte[] byteDataMessage)
        {
            int intAxisID = 0;

            if (byteDataMessage[0] != 0xE0)
                return;

            intAxisID = byteDataMessage[1];

            if (this.UpdateAxisError != null)
                this.UpdateAxisError(intAxisID);
        }

        /// <summary>
        /// 下位机发送上来的各个轴地址
        /// </summary>
        /// <param name="byteDataMessages">数据消息</param>
        private void UpdateAxisAddressHandler(byte[] byteDataMessage)
        {
            byte[] byteAxisAddress = new byte[6];

            if (byteDataMessage[0] != 0xFA)
                return;

            byteAxisAddress[0] = byteDataMessage[1];
            byteAxisAddress[1] = byteDataMessage[2];
            byteAxisAddress[2] = byteDataMessage[3];
            byteAxisAddress[3] = byteDataMessage[4];
            byteAxisAddress[4] = byteDataMessage[5];
            byteAxisAddress[5] = byteDataMessage[6];

            if (this.UpdateAxisAddress != null)
                this.UpdateAxisAddress(byteAxisAddress);
        }

        /// <summary>
        /// 下位机发送上来的各个轴的数据
        /// </summary>
        /// <param name="byteDataMessages">数据消息</param>
        private void UpdateAxisDataHandler(byte[] byteDataMessages)
        {
            double[] dblRelativeAxisAngles = new double[6];
            double[] dblAbsoluteAxisAngles = new double[6];

            if (byteDataMessages[0] != 0xFD)
                return;

            TransferAxisData2Angle(byteDataMessages, ref dblRelativeAxisAngles, ref dblAbsoluteAxisAngles);         // 将轴数据转换为轴的角度

            if (this.UpdateAxisData != null)
                this.UpdateAxisData(dblRelativeAxisAngles);

            if (this.UpdateAbsoluteAxisAngle != null)
                this.UpdateAbsoluteAxisAngle(dblAbsoluteAxisAngles);
        }

        /// <summary>
        /// 将接收每个轴数据转换为实际的角度值
        /// </summary>
        /// <param name="byteDataMessages">接收的数据</param>
        /// <returns>角度值</returns>
        private void TransferAxisData2Angle(byte[] byteDataMessages, ref double[] dblRelativeAngles, ref double[] dblAbsoluteAngles)
        {
            //double[] dblRelativeAngles = new double[6];
            double dblScaleRate = 0.0219726525;           // 编码器中的值转换到实际角度值的比例关系，360/2^14=360/16384=0.0219726525
            double dblMaxAngle = 360;

            for (int i = 0; i < 6; i++)
            {
                double dblTmpAngle = (byteDataMessages[i * 2 + 1] * 256 + byteDataMessages[(i + 1) * 2]) * dblScaleRate;      // 计算出编码器转换成角度的理论值
                dblAbsoluteAngles[i] = Math.Round(dblTmpAngle, 3);

                // 判别计算出的理论角度值是否大于标定角度值
                if (dblTmpAngle >= this._dblCalibrateAngles[i])
                    dblTmpAngle = dblTmpAngle - this._dblCalibrateAngles[i];
                else
                    dblTmpAngle = dblTmpAngle + dblMaxAngle - this._dblCalibrateAngles[i];

                if (dblTmpAngle > dblMaxAngle / 2)
                    dblTmpAngle = (dblTmpAngle - dblMaxAngle) * this._dblCalibrateDirections[i];
                else
                    dblTmpAngle = dblTmpAngle * this._dblCalibrateDirections[i];
                dblRelativeAngles[i] = Math.Round(dblTmpAngle, 3);
            }

            //return dblRelativeAngles;
        }

        /// <summary>
        /// 下位机发送上来的轴地址修改是否成功
        /// </summary>
        /// <param name="byteDataMessages">数据消息</param>
        private void UpdateAxisModifiedIsSuccessHandler(byte[] byteDataMessage)
        {
            int intAxisNum = -1;
            bool bolIsSuccess = false;
            string strAxisAddress = string.Empty;

            if (byteDataMessage[0] == 0xFB)
            {
                bolIsSuccess = true;
                intAxisNum = byteDataMessage[1];
                strAxisAddress = byteDataMessage[2].ToString("X2");
            }
            else
            {
                bolIsSuccess = false;
            }

            if (this.UpdateAxisModifiedIsSuccess != null)
                this.UpdateAxisModifiedIsSuccess(bolIsSuccess, intAxisNum, strAxisAddress);
        }

        /// <summary>
        /// 下位机发送上来的频率设置是否成功
        /// </summary>
        /// <param name="byteDataMessages">数据消息</param>
        private void UpdateSetFrequentIsSuccessHandler(byte[] byteDataMessage)
        {
            int intFrequent = -1;
            bool bolIsSuccess = false;

            if (byteDataMessage[0] == 0xF3)
                bolIsSuccess = true;
            else
                bolIsSuccess = false;

            intFrequent = byteDataMessage[1];

            if (this.UpdateSetFrequentIsSuccess != null)
                this.UpdateSetFrequentIsSuccess(bolIsSuccess, intFrequent);
        }

        #endregion

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="byteDataMessages">传进来的数据</param>
        public void ReceiveDataMessage(byte[] byteDataMessage)
        {
            this._queueReceiveDataMessage.Enqueue(byteDataMessage);
            this._autoEvent.Set();                                      // 将事件状态设置为终止状态，允许一个或多个等待线程继续
        }

        /// <summary>
        /// 发送的数据message
        /// </summary>
        /// <param name="strCommand">传入的命令</param>
        /// <returns>待发送的message</returns>
        public byte[] SendDataMessage(string strCommand, DataModel dm = null)
        {
            byte[] byteSendBuffer = RecognizeCommand(strCommand, dm);
            return byteSendBuffer;
        }

        #region  发送消息的处理部分

        /// <summary>
        /// 识别指令
        /// </summary>
        /// <param name="strCommand"></param>
        /// <returns></returns>
        private byte[] RecognizeCommand(string strCommand, DataModel dm = null)
        {
            byte[] byteSendBuffer = null;
            switch (strCommand)
            {
                case "QuerySampleFrequent":     // 查询采样频率
                    byteSendBuffer = this.SendMessageQuerySampleFrequentHandler();
                    this.WriteLogHandler("执行查询采样频率操作.");
                    break;
                case "SetSampleFrequent":       // 设定采样频率
                    byteSendBuffer = this.SendMessageSetSampleRateHandler(dm.SetSampleFrequent);
                    this.WriteLogHandler("执行设定采样频率操作.");
                    break;
                case "ReadAllAxisAddress":      // 读取所有轴地址
                    byteSendBuffer = this.SendMessageReturnAxisAddressHandler();
                    this.WriteLogHandler("执行读取所有地址操作.");
                    break;
                case "ModifyAxis1Address":      // 修改1轴地址
                    byteSendBuffer = this.SendMessageModifyEncoderAddressHandler(0x01, dm.SetAxis1Address);
                    this.WriteLogHandler("执行修改1轴地址操作.");
                    break;
                case "ModifyAxis2Address":      // 修改2轴地址
                    byteSendBuffer = this.SendMessageModifyEncoderAddressHandler(0x02, dm.SetAxis2Address);
                    this.WriteLogHandler("执行修改2轴地址操作.");
                    break;
                case "ModifyAxis3Address":      // 修改3轴地址
                    byteSendBuffer = this.SendMessageModifyEncoderAddressHandler(0x03, dm.SetAxis3Address);
                    this.WriteLogHandler("执行修改3轴地址操作.");
                    break;
                case "ModifyAxis4Address":      // 修改4轴地址
                    byteSendBuffer = this.SendMessageModifyEncoderAddressHandler(0x04, dm.SetAxis4Address);
                    this.WriteLogHandler("执行修改4轴地址操作.");
                    break;
                case "ModifyAxis5Address":      // 修改5轴地址
                    byteSendBuffer = this.SendMessageModifyEncoderAddressHandler(0x05, dm.SetAxis5Address);
                    this.WriteLogHandler("执行修改5轴地址操作.");
                    break;
                case "ModifyAxis6Address":      // 修改6轴地址
                    byteSendBuffer = this.SendMessageModifyEncoderAddressHandler(0x06, dm.SetAxis6Address);
                    this.WriteLogHandler("执行修改6轴地址操作.");
                    break;
                case "QueryDeviceConnect":      // 查询设备连接状态
                    byteSendBuffer = this.SendMessageQueryDeviceConnectHandler();
                    this.WriteLogHandler("执行查询设备连接操作.");
                    break;
                case "QueryDeviceSampleReady":  // 查询设备采样是否准备就绪
                    byteSendBuffer = this.SendMessageReadDataFromSDHandler();
                    this.WriteLogHandler("执行查询设备数据采样装备就绪操作.");
                    break;
                case "StartSampleData":         // 开始采样数据
                    byteSendBuffer = this.SendMessageStartSampleHandler();
                    this.WriteLogHandler("执行开始采样数据操作.");
                    break;
                case "StopSampleData":          // 停止采样数据
                    byteSendBuffer = this.SendMessageStopSampleHandler();
                    this.WriteLogHandler("执行停止采样数据操作.");
                    break;
            }
            return byteSendBuffer;
        }

        /// <summary>
        /// 设置采样频率指令
        /// </summary>
        /// <param name="intFrequent">采样频率</param>
        /// <returns>发送的指令</returns>
        private byte[] SendMessageSetSampleRateHandler(int intFrequent)
        {
            byte[] byteSendBuffer = new byte[] { 0xFA, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xEF };
            byteSendBuffer[1] = 0xF0;
            byteSendBuffer[2] = (byte)intFrequent;
            return byteSendBuffer;
        }

        /// <summary>
        /// 从SD卡中读取数据指令
        /// </summary>
        /// <returns>发送的指令</returns>
        private byte[] SendMessageReadDataFromSDHandler( )
        {
            byte[] byteSendBuffer = new byte[] { 0xFA, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xEF };
            byteSendBuffer[1] = 0xF1;
            return byteSendBuffer;
        }

        /// <summary>
        /// 开始采集数据指令
        /// </summary>
        /// <returns>发送的指令</returns>
        private byte[] SendMessageStartSampleHandler( )
        {
            byte[] byteSendBuffer = new byte[] { 0xFA, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xEF };
            byteSendBuffer[1] = 0xF2;
            return byteSendBuffer;
        }

        /// <summary>
        /// 停止采集数据指令
        /// </summary>
        /// <returns>发送的指令</returns>
        private byte[] SendMessageStopSampleHandler( )
        {
            byte[] byteSendBuffer = new byte[] { 0xFA, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xEF };
            byteSendBuffer[1] = 0xF3;
            return byteSendBuffer;
        }

        /// <summary>
        /// 查询设备存在与否指令,设备是否连接
        /// </summary>
        /// <returns>发送的指令</returns>
        private byte[] SendMessageQueryDeviceConnectHandler( )
        {
            byte[] byteSendBuffer = new byte[] { 0xFA, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xEF };
            byteSendBuffer[1] = 0xF4;
            return byteSendBuffer;
        }

        /// <summary>
        /// 查询采样频率的指令
        /// </summary>
        /// <returns>发送的指令</returns>
        private byte[] SendMessageQuerySampleFrequentHandler( )
        {
            byte[] byteSendBuffer = new byte[] { 0xFA, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xEF };
            byteSendBuffer[1] = 0xF5;
            return byteSendBuffer;
        }

        /// <summary>
        /// 允许测试喷枪及电机
        /// </summary>
        /// <returns>发送的指令</returns>
        private byte[] SendMessageEnableTestLanceMotorHandler( )
        {
            byte[] byteSendBuffer = new byte[] { 0xFA, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xEF };
            byteSendBuffer[1] = 0xF6;
            return byteSendBuffer;
        }

        /// <summary>
        /// 修改编码器地址
        /// </summary>
        /// <returns>发送的指令</returns>
        private byte[] SendMessageModifyEncoderAddressHandler(byte byteAxisNum, byte byteAxisAddress)
        {
            byte[] byteSendBuffer = new byte[] { 0xFA, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xEF };
            byteSendBuffer[1] = 0xF7;
            byteSendBuffer[2] = byteAxisNum;
            byteSendBuffer[3] = byteAxisAddress;
            byteSendBuffer[4] = (byte)(byteAxisNum ^ byteAxisAddress);      // 轴信息和地址信息进行异或
            return byteSendBuffer;
        }

        /// <summary>
        /// 要求返回各个轴的地址
        /// </summary>
        /// <returns>发送的指令</returns>
        private byte[] SendMessageReturnAxisAddressHandler( )
        {
            byte[] byteSendBuffer = new byte[] { 0xFA, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xEF };
            byteSendBuffer[1] = 0xF8;
            return byteSendBuffer;
        }

        /// <summary>
        /// 开启直通模式
        /// </summary>
        /// <returns>发送的指令</returns>
        private byte[] SendMessageStartRouterModeHandler( )
        {
            byte[] byteSendBuffer = new byte[] { 0xFA, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xEF };
            byteSendBuffer[1] = 0xF9;
            return byteSendBuffer;
        }

        #endregion

    }
}
