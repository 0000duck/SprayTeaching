using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SprayTeaching.BaseClassLib;


namespace SprayTeaching.MyAllClass
{
    public class MySocketCom
    {
        #region 变量
        private string _strSocketIPAddress = "10.8.193.177";                    // socket的IP地址     
        private int _intSocketPortNum = 12000;                                  // socket的端口号
        private byte[] _bytReceiveBuffer;                                       // 数据接收缓存区
        private List<byte> _lstBytReceiveData = new List<byte>();
        private StringBuilder _sbReceiveDataStorage = new StringBuilder();      // 存储所有接收的数据

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
            get { return IsSocketConnected(); }
        }

        #endregion

        #region 构造函数

        public MySocketCom()
        {
            this._strSocketIPAddress = "10.8.193.177";
            this._intSocketPortNum = 12000;

        }

        public MySocketCom(string strIPAddress, int intPortNum)
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
        /// Socket是否已经连接
        /// </summary>
        /// <returns>true为连接，false为未连接</returns>
        private bool IsSocketConnected()
        {
            if (this._sktCommunicate != null)
                return this._sktCommunicate.Connected;
            return false;
        }

        /// <summary>
        /// 与服务器进行连接
        /// </summary>
        /// <returns>是否连接成功，true为成功，false为失败</returns>
        public bool Connect()
        {
            IPAddress ip = IPAddress.Parse(this._strSocketIPAddress);
            IPEndPoint ipe = new IPEndPoint(ip, this._intSocketPortNum);

            this._sktCommunicate = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            this._sktCommunicate.ReceiveTimeout = 500;
            this._sktCommunicate.SendTimeout = 500;
            try
            {
                this._sktCommunicate.Connect(ipe);          // 连接服务器
                if (this._sktCommunicate.Connected)
                {
                    this._bytReceiveBuffer = new byte[1024];
                    this._sktCommunicate.BeginReceive(this._bytReceiveBuffer, 0, this._bytReceiveBuffer.Length, SocketFlags.None,
                        new AsyncCallback(ReceiveDataCallbackHandler), this._sktCommunicate);           // 开启异步接收
                    return true;
                }
                else
                    return false;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// 打开socket，若socket已经打开，则先关闭，再重新打开
        /// </summary>
        public void OpenSocket()
        {
            if (this._sktCommunicate != null)
            {
                this._sktCommunicate.Close();
                this._sktCommunicate = null;
            }
            try
            {
                this.Connect();
                this.UpdateSocketIsConnected(this.IsSocketConnected());
                this.WriteLog(string.Format("Wifi连接成功,主机为{0}:{1}.", this._strSocketIPAddress, this._intSocketPortNum));                
            }
            catch (Exception e)
            {
                this.WriteLog(e.Message);
            }
        }

        /// <summary>
        /// 关闭socket
        /// </summary>
        public void CloseSocket()
        {
            if (this._sktCommunicate != null)
            {
                this._sktCommunicate.Close();
                this._sktCommunicate = null;
            }
            this.UpdateSocketIsConnected(this.IsSocketConnected());
            this.WriteLog("Wifi已关闭.");
        }

        #endregion

        /// <summary>
        /// 关闭socket相关的所有资源
        /// </summary>
        public void Close()
        {
            this.CloseSocket();
        }

        #region 接收数据并处理相关方法

        /// <summary>
        /// 异步接收数据处理
        /// </summary>
        /// <param name="result"></param>
        private void ReceiveDataCallbackHandler(IAsyncResult result)
        {
            Socket ts = (Socket)result.AsyncState;      // 这里的Socket是客户端的Socket

            // 当连接断开时，则将接收数据线程关闭,此处主要是客户端主动断开
            if (!this.IsSocketConnected())
                return;

            int intByteRead = ts.EndReceive(result);    // 接收到数据的字节数
            if (intByteRead > 0)
            {
                // 存储数据
                this.SaveReceivePartData(intByteRead);

                // 判断通道中还有没有数据
                if (ts.Available > 0)
                {
                    // 接收数据未完成，继续接收数据
                    ts.BeginReceive(this._bytReceiveBuffer, 0, this._bytReceiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveDataCallbackHandler), ts);
                    return;
                }
                else
                {
                    // 接收已完成，开始处理数据，处理完后清空缓存区
                    this.DataHandler();
                }
                // 数据处理完成，则进入新的数据接收等待
                ts.BeginReceive(this._bytReceiveBuffer, 0, this._bytReceiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveDataCallbackHandler), ts);
                return;
            }
            else
            {
                // 问题：在远程服务器断开的情况下，客户端无法及时判断出socket已经断开，因此设定当接收的数据个数为0时，和服务器的连接已经断开
                ts.Close();
                this.WriteLog("远程服务器已断开,Wifi已关闭.");
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
        private void DataHandler()
        {
            string strData = ReadData();                    // 读取数据

            // 当数据不为空的时候才算是有效的
            if (!string.IsNullOrEmpty(strData))
            {
                this._sbReceiveDataStorage.Append(strData);     // 添加数据到存储的地方
                UpdateReceiveData(strData);
            }

            this._lstBytReceiveData.Clear();                // 清除list<byte>的缓存区          
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <returns>返回读取的内容</returns>
        private string ReadData()
        {
            List<byte> byteData = new List<byte>();
            byte[] bytTmp = this._lstBytReceiveData.ToArray();      // 所有数据
            byte BeginByte = 0x5B;          //string Begin = "[";   起始字节
            byte EndByte = 0x5D;            //string End = "]";     结束字节
            bool bStartFlag = false;        // 起始标志位
            bool bEndFlag = false;          // 结束标志位

            for (int i = 0; i < bytTmp.Length; i++)
            {
                // 寻找到起始位时，将开始标志位设置为true，结束标志位设置为false
                if (bytTmp[i] == BeginByte)
                {
                    bStartFlag = true;
                    bEndFlag = false;
                    byteData.Clear();
                    continue;  //检测到开始符号，后面就不执行了
                }

                // 寻找到结束符
                if (bytTmp[i] == EndByte)
                {
                    bEndFlag = true;
                    continue;  //检测到结束符号，后面就不执行了
                }

                // 只有在开始标志位为true，结束标志位为false时添加数据
                if (bStartFlag == true && bEndFlag == false)
                    byteData.Add(bytTmp[i]);
            }

            // 只有满足起始标志位为ture，结束标志位也为true，则数据是正确的数据
            if (bStartFlag == true && bEndFlag == true)
            {
                string readString = System.Text.Encoding.Default.GetString(byteData.ToArray(), 0, byteData.Count);  // 字节型数据转换为字符型数据
                return readString;
            }
            else
            {
                byteData.Clear();
                return string.Empty;
            }
        }

        #endregion

        #region 发送数据并处理的相关方法
        public void SendDataHandler()
        {
            if (this._sktCommunicate == null)
            {
                this.WriteLog("数据发送失败，连接已断开.");
                return;
            }
            if (!this.IsSocketConnected())
            {
                this.WriteLog("连接断开.");
                return;
            }
            string strData = "xingshuang\r\n";
            byte[] bytBuffer = System.Text.Encoding.Default.GetBytes(strData);
            this._sktCommunicate.BeginSend(bytBuffer, 0, bytBuffer.Length, SocketFlags.None, new AsyncCallback(SendDataCallbackHandler), this._sktCommunicate);
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
                this.WriteLog("发送成功.");
            }
            catch
            {
                this.WriteLog("发送失败.");
            }
        }

        #endregion

        #region  事件响应
        /// <summary>
        /// 更新接收的数据并处理
        /// </summary>
        /// <param name="strData"></param>
        private void UpdateReceiveData(string strData)
        {
            // 触发数据的处理
            if (DataReceived != null)
                DataReceived(strData);
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
        private void WriteLog(string strMessage)
        {
            if (this.UpdateLogContent != null)
                this.UpdateLogContent(strMessage);
        }
        #endregion

        #endregion

        #endregion
    }
}
