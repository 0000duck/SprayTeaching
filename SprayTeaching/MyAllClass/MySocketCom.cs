using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SprayTeaching.MyAllClass
{
    public class MySocketCom
    {
        private string _strSocketIPAddress = "10.8.193.177";                    // socket的IP地址     
        private int _intSocketPortNum = 12000;                                  // socket的端口号
        public byte[] _bytReceiveBuffer;                                        // 数据接收缓存区

        private Socket _sktCommunicate;                                         // socket的对象

        #region 外部事件

        public event UpdateLogContentEventHandler UpdateLogContent;             // 更新日志文件

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
            get { return this._sktCommunicate.Connected; }
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

        public bool IsSocketConnected()
        {
            if (this._sktCommunicate != null)
                return this._sktCommunicate.Connected;
            return false;
        }

        public bool Connect()
        {
            IPAddress ip = IPAddress.Parse(this._strSocketIPAddress);
            IPEndPoint ipe = new IPEndPoint(ip, this._intSocketPortNum);

            this._sktCommunicate = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            this._sktCommunicate.ReceiveTimeout = 1000;
            this._sktCommunicate.SendTimeout = 1000;
            try
            {
                this._sktCommunicate.Connect(ipe);
                if (this._sktCommunicate.Connected)
                {
                    this._bytReceiveBuffer = new byte[1024];
                    this._sktCommunicate.BeginReceive(this._bytReceiveBuffer, 0, this._bytReceiveBuffer.Length, SocketFlags.None, 
                        new AsyncCallback(ReceiveDataCallbackHandler), this._sktCommunicate);
                return true;
                }
                    
                else
                    return false;
            }
            catch(Exception e)
            {
                this.WriteLog(e.Message);
                return false;
            }            
        }

        public void CloseSocket()
        {
            if (this._sktCommunicate!=null)
            {
                this._sktCommunicate.Close();
            }
        }

        public void Close()
        {
            this.CloseSocket();
        }

        private void ReceiveDataCallbackHandler(IAsyncResult result)
        {
            Socket ts = (Socket)result.AsyncState;      //这里的Socket是客户端的Socket
            //结束当前的数据接收线程，并关闭线程资源
            try
            {
                int intByteRead = ts.EndReceive(result);
                if (intByteRead > 0)
                {
                    string strBuffer = System.Text.Encoding.Default.GetString(this._bytReceiveBuffer).TrimEnd('\0');
                    
                    //清空buffer
                    for (int i = 0; i < this._bytReceiveBuffer.Length; i++)
                    {
                        this._bytReceiveBuffer[i] = 0;
                    }
                    ts.BeginReceive(this._bytReceiveBuffer, 0, this._bytReceiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveDataCallbackHandler), ts);
                }
                else
                {
                    ts.Close();
                }
            }
            catch
            {
                ts.Close();
                return;
            }
        }

        #region 写入日志
        /// <summary>
        /// 将消息写入日志
        /// </summary>
        /// <param name="strMessage">消息内容</param>
        private void WriteLog(string strMessage)
        {
            if (this.UpdateLogContent != null)
            {
                this.UpdateLogContent(strMessage);
            }
        }
        #endregion
    }
}
