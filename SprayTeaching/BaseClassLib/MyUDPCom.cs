using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SprayTeaching.BaseClassLib
{
    public class MyUDPCom
    {
        #region 变量
        private string _strSocketIPAddress = "10.8.193.177";                    // socket的IP地址     
        private int _intSocketPortNum = 12000;                                  // socket的端口号
        private bool _bolIsConnected = false;                                   // 是否连接，UDP没有连接不连接的概念，这里为了有个指示，设立一个连接标识
        private byte[] _bytReceiveBuffer;                                       // 数据接收缓存区
        private List<byte> _lstBytReceiveData = new List<byte>();
        private StringBuilder _sbReceiveDataStorage = new StringBuilder();      // 存储所有接收的数据
        private EndPoint _epServer;                                             // 终端地址

        private Socket _sktCommunicate;                                         // socket的对象
        #endregion

        #region 外部事件

        public event UpdateLogContentEventHandler UpdateLogContent;             // 更新日志文件
        public event DataReceivedEventHandler DataReceived;                     // 接收数据的处理事件
        public event UpdateSocketIsConnectedEventHandler UpdateSocketIsConnected;   // 更新socket是否断开

        #endregion

        #region 属性

        /// <summary>
        /// IP地址
        /// </summary>
        public string SocketIPAddress
        {
            get { return _strSocketIPAddress; }
            set { _strSocketIPAddress = value; }
        }

        /// <summary>
        /// 端口号
        /// </summary>
        public int SocketPortNum
        {
            get { return _intSocketPortNum; }
            set { _intSocketPortNum = value; }
        }

        /// <summary>
        /// 是否已经连接
        /// </summary>
        public bool IsConnected
        {
            get { return this._bolIsConnected; }
        }

        #endregion

        #region 构造函数

        public MyUDPCom( )
        {
            this._strSocketIPAddress = "10.8.193.177";
            this._intSocketPortNum = 12000;
        }

        public MyUDPCom(string strIPAddress, int intPortNum)
        {
            this._strSocketIPAddress = strIPAddress;
            this._intSocketPortNum = intPortNum;
        }

        #endregion

        #region 方法

        #region 打开关闭socket相关方法

        /// <summary>
        /// 打开或关闭socket
        /// </summary>
        /// <param name="bolSocketIsConnected">连接标识符</param>
        /// <param name="strSocketIsConnectedImage">连接标识图</param>
        public void OpenCloseSocket(ref bool bolSocketIsConnected, ref string strSocketIsConnectedImage)
        {
            if (!bolSocketIsConnected)
                this.OpenSocket();
            else
                this.CloseSocket();

            // 更新状态信息
            if (this.IsSocketConnected())
            {
                bolSocketIsConnected = true;
                strSocketIsConnectedImage = MyConstString.IMG_SOCKET_CONNECT;
            }
            else
            {
                bolSocketIsConnected = false;
                strSocketIsConnectedImage = MyConstString.IMG_SOCKET_DISCONNECT;
            }
        }

        /// <summary>
        /// 打开或关闭socket
        /// </summary>
        /// <param name="bolSocketIsConnected">连接标识符</param>
        public void OpenCloseSocket(bool bolSocketIsConnected)
        {
            if (!bolSocketIsConnected)
                this.OpenSocket();
            else
                this.CloseSocket();
        }

        /// <summary>
        /// 打开socket，若socket已经打开，则先关闭，再重新打开
        /// </summary>
        public void OpenSocket( )
        {
            if (this._sktCommunicate != null)
            {
                this._sktCommunicate.Close();
                this._sktCommunicate = null;
            }
            try
            {
                this.Connect();
                this.WriteLogHandler(string.Format("Wifi连接成功,主机为{0}:{1}.", this._strSocketIPAddress, this._intSocketPortNum));
                this._bolIsConnected = true;                    // 在这里就说是UDP已经连接，因为UDP不存在连接不连接的概念，这里指示连接为了给操作者看的
            }
            catch (Exception e)
            {
                this.WriteLogHandler(e.Message, 1);
            }
            finally
            {
                this.UpdateConnectState(this.IsSocketConnected());
            }
        }

        /// <summary>
        /// 关闭socket
        /// </summary>
        public void CloseSocket( )
        {
            if (this._sktCommunicate != null)
            {
                this._sktCommunicate.Close();
                this._sktCommunicate = null;
            }
            this._bolIsConnected = false;
            this.UpdateSocketIsConnected(this.IsSocketConnected());
            this.WriteLogHandler("Wifi已关闭.");
        }

        /// <summary>
        /// Socket是否已经连接
        /// </summary>
        /// <returns>true为连接，false为未连接</returns>
        private bool IsSocketConnected( )
        {
            if (this._sktCommunicate != null)
                return this._bolIsConnected;
            return false;
        }

        /// <summary>
        /// 与服务器进行连接
        /// </summary>
        /// <returns>是否连接成功，true为成功，false为失败</returns>
        public bool Connect( )
        {
            IPAddress ip = IPAddress.Parse(this._strSocketIPAddress);
            IPEndPoint ipe = new IPEndPoint(ip, this._intSocketPortNum);
            this._epServer = (EndPoint)ipe;
            this._sktCommunicate = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                // 给Udp绑定一个地址和端口号
                IPEndPoint localEp = new IPEndPoint(IPAddress.Any, 12001);
                this._sktCommunicate.Bind((EndPoint)localEp);                

                // 开启异步接收
                this._bytReceiveBuffer = new byte[1024];
                this._sktCommunicate.BeginReceiveFrom(this._bytReceiveBuffer, 0, this._bytReceiveBuffer.Length, SocketFlags.None,
                    ref this._epServer, new AsyncCallback(ReceiveDataCallbackHandler), this._sktCommunicate);           
                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }           
        #endregion

        #region 关闭资源方法

        /// <summary>
        /// 关闭所有变量，使它们都invalidition
        /// </summary>
        private void CloseVariables( )
        {
            this._sktCommunicate = null;
            this._sbReceiveDataStorage = null;
            this._lstBytReceiveData = null;
        }

        /// <summary>
        /// 关闭socket相关的所有资源
        /// </summary>
        public void Close( )
        {
            this.CloseSocket();
            this.CloseVariables();
        }

        #endregion

        #region 接收数据并处理相关方法

        /// <summary>
        /// 异步接收数据处理
        /// </summary>
        /// <param name="result"></param>
        private void ReceiveDataCallbackHandler(IAsyncResult result)
        {
            Socket ts = (Socket)result.AsyncState;      // 这里的Socket是客户端的Socket

            // 当连接断开时，则将接收数据线程关闭,此处主要是客户端主动断开
            //if (!this.IsSocketConnected())
            //    return;
            int intByteRead = 0;
            try
            {
                intByteRead = ts.EndReceive(result);    // 接收到数据的字节数
            }
            catch
            {
                ts.Close();                
                return;
            }
            //int intByteRead = ts.EndReceive(result);    // 接收到数据的字节数
            if (intByteRead > 0)
            {
                // 存储数据
                this.SaveReceivePartData(intByteRead);

                // 接收已完成，开始处理数据，处理完后清空缓存区
                this.DataHandler();

                // 数据处理完成，则进入新的数据接收等待
                this._bytReceiveBuffer = new byte[1024];
                this._sktCommunicate.BeginReceiveFrom(this._bytReceiveBuffer, 0, this._bytReceiveBuffer.Length, SocketFlags.None,
                    ref this._epServer, new AsyncCallback(ReceiveDataCallbackHandler), this._sktCommunicate);           // 开启异步接收
                return;
            }
            else
            {
                // 问题：在远程服务器断开的情况下，客户端无法及时判断出socket已经断开，因此设定当接收的数据个数为0时，和服务器的连接已经断开
                ts.Close();
                this.WriteLogHandler("远程服务器已断开,Wifi已关闭.", 1);
                this.UpdateConnectState(false);         //通知连接断开
                return;
            }
        }

        /// <summary>
        /// 存储接收的部分数据，都放在list<byte>中，清空1024字节数组的缓存区
        /// </summary>
        /// <param name="intReceiveByteNum">接收byte的个数</param>
        private void SaveReceivePartData(int intReceiveByteNum)
        {
            // 将数据存储到list<byte>中
            for (int i = 0; i < intReceiveByteNum; i++)
            {
                this._lstBytReceiveData.Add(this._bytReceiveBuffer[i]);
            }

            // 清空缓存区
            for (int i = 0; i < this._bytReceiveBuffer.Length; i++)
            {
                this._bytReceiveBuffer[i] = 0;
            }
        }

        /// <summary>
        /// 处理接收的数据，并清除list<byte>的缓存区
        /// </summary>
        private void DataHandler( )
        {
            List<byte> lstByteData = ReadData();                    // 读取数据

            // 当数据个数为16的时候才算是有效的，若为空，则不更新后续的数据
            if (lstByteData.Count == 0)
                return;

            //this._sbReceiveDataStorage.Append(lstByteData);     // 添加数据到存储的地方
            byte[] byteData = lstByteData.ToArray();                // 将list中的数据转换为byte数组
            UpdateReceiveData(byteData);
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <returns>返回读取的内容</returns>
        private List<byte> ReadData( )
        {
            List<byte> byteData = new List<byte>();                 // 用于存储实际所需的数据
            byte[] bytTmp = this._lstBytReceiveData.ToArray();      // 所有数据
            byte BeginByte1 = 0xAA;          //string Begin = "[";   起始字节
            byte BeginByte2 = 0x55;          //string Begin = "[";   起始字节
            bool bStartFlag = false;        // 起始标志位
            bool bEndFlag = false;          // 结束标志位

            for (int i = 1; i < bytTmp.Length; i++)
            {
                // 寻找到起始位时，将开始标志位设置为true，结束标志位设置为false
                if (bytTmp[i - 1] == BeginByte1 && bytTmp[i] == BeginByte2)
                {
                    bStartFlag = true;
                    bEndFlag = false;
                    byteData.Clear();
                    byteData.Add(bytTmp[i - 1]);
                    byteData.Add(bytTmp[i]);
                    continue;               // 检测到开始符号，后面就不执行了
                }

                // 只有在开始标志位为true，结束标志位为false时添加数据
                if (bStartFlag == true && bEndFlag == false)
                    byteData.Add(bytTmp[i]);

                // 检测list中的数据个数是否为16，若是则结束
                if (byteData.Count == 16)
                {
                    bEndFlag = true;
                    break;                  // 若数据个数够16个，后面就不执行了
                }
            }

            this._lstBytReceiveData.Clear();                // 在这组数据读取完毕后，清除list<byte>的缓存区   

            // 只有满足起始标志位为ture，结束标志位也为true，则是正确的数据
            if (bStartFlag == true && bEndFlag == true)
            {
                // 若数据校验和错误，则清除所有数据
                if (!DataCheckSum(byteData))
                    byteData.Clear();
                return byteData;
            }
            else
            {
                byteData.Clear();
                return byteData;
            }
        }

        /// <summary>
        /// 对数据进行校验和
        /// </summary>
        /// <param name="byteData">接收的数据</param>
        /// <returns>数据是否正确</returns>
        private bool DataCheckSum(List<byte> byteData)
        {
            bool bolIsOk = false;
            byte byteSum = 0x00;

            // 如果数据不是16个，则数据格式错误
            if (byteData.Count != 16)
            {
                bolIsOk = false;
                return bolIsOk;
            }

            // 对前15个数据进行累加
            for (int i = 0; i < 15; i++)
            {
                byteSum += byteData[i];
            }

            // 只有最后一个数据和前15个数据之和相等，则算是数据正确，否则错误
            if (byteSum == byteData[15])
                bolIsOk = true;

            return bolIsOk;
        }

        #endregion

        #region 发送数据并处理的相关方法
        public bool SendDataHandler(byte[] btData = null)
        {
            if (this._sktCommunicate == null)
            {
                this.WriteLogHandler("数据发送失败，Wifi连接已断开.", 1);
                return false;
            }
            if (!this.IsSocketConnected())
            {
                this.WriteLogHandler("Wifi连接断开.", 1);
                return false;
            }
            if (btData.Length == 0)
            {
                this.WriteLogHandler("数据有误，为空.", 1);
                return false;
            }

            this._sktCommunicate.BeginSendTo(btData, 0, btData.Length, SocketFlags.None,
                this._epServer, new AsyncCallback(SendDataCallbackHandler), this._sktCommunicate);
            return true;
        }

        /// <summary>
        /// 异步发送数据处理
        /// </summary>
        /// <param name="result">socket对象</param>
        private void SendDataCallbackHandler(IAsyncResult result)
        {
            Socket ts = (Socket)result.AsyncState;      // 这里的Socket是客户端的Socket
            try
            {
                ts.EndSend(result);
                //this.WriteLogHandler("发送成功.");
            }
            catch
            {
                //this.WriteLogHandler("发送失败.", 1);
            }
        }

        #endregion

        #region  事件响应
        /// <summary>
        /// 更新接收的数据并处理
        /// </summary>
        /// <param name="lstByteData"></param>
        private void UpdateReceiveData(byte[] byteData)
        {
            // 触发数据的处理
            if (DataReceived != null)
                DataReceived(byteData);
        }

        /// <summary>
        /// 更新socket的连接状态
        /// </summary>
        /// <param name="bolIsOpened"></param>
        private void UpdateConnectState(bool bolIsConnected)
        {
            if (this.UpdateSocketIsConnected != null)
                this.UpdateSocketIsConnected(bolIsConnected);
        }

        #region 写入日志
        /// <summary>
        /// 将消息写入日志
        /// </summary>
        /// <param name="strMessage">消息内容</param>
        private void WriteLogHandler(string strMessage, int intType = 0)
        {
            if (this.UpdateLogContent != null)
                this.UpdateLogContent(strMessage, intType);
        }
        #endregion

        #endregion

        #endregion
    }
}
