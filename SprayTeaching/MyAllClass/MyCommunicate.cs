using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using SprayTeaching.BaseClassLib;
using SprayTeaching.Model;

namespace SprayTeaching.MyAllClass
{
    public class MyCommunicate
    {
        private MySerialPort _mySerialPortCom;             // 串口的对象
        private MySocketCom _mySocketCom;                   // Socket通信对象
        private bool _bolFlagCommunicateWay = false;        // 默认是false，false为串口方式，true为wifi方式

        public event DataReceivedEventHandler DataReceived;
        public event UpdateLogContentEventHandler UpdateLogContent;
        public event UpdateSerialPortIsOpenedEventHandler UpdateSerialPortIsOpened;
        public event UpdateSocketIsConnectedEventHandler UpdateSocketIsConnected;

        public MyCommunicate( )
        {
            this._mySerialPortCom = new MySerialPort();                                                                                    // 串口的对象
            this._mySerialPortCom.DataReceived += new DataReceivedEventHandler(ReceiveDataHandler);                                        // 接收数据的处理
            this._mySerialPortCom.UpdateLogContent += new UpdateLogContentEventHandler(WriteLogHandler);                                   // 串口写日志
            this._mySerialPortCom.UpdateSerialPortIsOpened += new UpdateSerialPortIsOpenedEventHandler(UpdateSerialPortIsOpenedHandler);   // 更新串口通断状态信息

            this._mySocketCom = new MySocketCom();                                                                                          // socket通信的对象
            this._mySocketCom.UpdateLogContent += new UpdateLogContentEventHandler(WriteLogHandler);                                        // socket写日志
            this._mySocketCom.DataReceived += new DataReceivedEventHandler(ReceiveDataHandler);                                             // 接收数据的处理
            this._mySocketCom.UpdateSocketIsConnected += new UpdateSocketIsConnectedEventHandler(UpdateSocketIsConnectedHandler);           // 更新socket通断的状态信息
        }

        /// <summary>
        /// 关闭通信部分的所有资源
        /// </summary>
        public void Close( )
        {
            this._mySerialPortCom.Close();          // 关闭与串口相关的资源 
            this._mySocketCom.Close();              // 关闭与socket相关的资源
            this.CloseAllVariable();
        }

        /// <summary>
        /// 关闭所有变量，使它们都invalidition
        /// </summary>
        private void CloseAllVariable( )
        {
            this._mySerialPortCom = null;
            this._mySocketCom = null;
            this._bolFlagCommunicateWay = false;
        }


        #region 数据接收

        /// <summary>
        /// 对串口中接收的数据进行处理
        /// </summary>
        /// <param name="byteDataReceiveMessage">接收的数据</param>
        private void ReceiveDataHandler(byte[] byteDataReceiveMessage)
        {
            if (this.DataReceived != null)
                this.DataReceived(byteDataReceiveMessage);
        }
        #endregion

        #region  日志相关的方法

        /// <summary>
        /// 更新日志
        /// </summary>
        /// <param name="strMessage">日志消息</param>
        private void WriteLogHandler(string strMessage, int intType = 0)
        {
            if (this.UpdateLogContent != null)
                this.UpdateLogContent(strMessage, intType);
        }

        #endregion

        /// <summary>
        /// 更新串口的通断状态
        /// </summary>
        /// <param name="bolIsOpened">是否连接，true为连接，false为断开</param>
        private void UpdateSerialPortIsOpenedHandler(bool bolIsOpened)
        {
            if (this.UpdateSerialPortIsOpened != null)
                this.UpdateSerialPortIsOpened(bolIsOpened);
        }

        /// <summary>
        /// 更新socket的连接状态事件处理
        /// </summary>
        /// <param name="bolIsConnected"></param>
        private void UpdateSocketIsConnectedHandler(bool bolIsConnected)
        {
            if (this.UpdateSocketIsConnected != null)
                this.UpdateSocketIsConnected(bolIsConnected);
        }

        /// <summary>
        /// 打开或关闭串口
        /// </summary>
        public void OpenCloseSerialPort(DataModel dm)
        {
            //更新串口参数
            this._mySerialPortCom.PortName = dm.SerialPortName;
            this._mySerialPortCom.BaudRate = (SerialPortBaudRates)dm.SerialPortBaudRate;
            this._mySerialPortCom.ParityBit = (Parity)dm.SerialPortParityBit;
            this._mySerialPortCom.StopBit = (StopBits)dm.SerialPortStopBit;
            this._mySerialPortCom.DataBit = (SerialPortDataBits)dm.SerialPortDataBit;

            bool bolSerialPortIsOpened = dm.SerialPortIsOpened;
            this._mySerialPortCom.OpenCloseSerialPort(bolSerialPortIsOpened);
        }

        /// <summary>
        /// 打开或者关闭socket事件处理
        /// </summary>
        public void OpenCloseSocket(DataModel dm)
        {
            this._mySocketCom.SocketIPAddress = dm.SocketIPAddress;
            this._mySocketCom.SocketPortNum = dm.SocketPortNum;

            bool bolSocketIsConnected = dm.SocketIsConnected;
            this._mySocketCom.OpenCloseSocket(bolSocketIsConnected);
        }

        /// <summary>
        /// 选择通信方式
        /// </summary>
        /// <param name="obj">通信方式的标识</param>
        /// <param name="dm">所有数据的对象</param>
        public void SelectCommunicateWayHandler(object obj, DataModel dm)
        {
            string strWay = obj as string;
            switch (strWay)
            {
                case "SerialPortWay":
                    if (dm.SocketIsConnected)
                        this._mySocketCom.CloseSocket();
                    this.WriteLogHandler("选择串口方式通信.");
                    this._bolFlagCommunicateWay = false;            // 更新通信方式标识，false，为串口方式
                    break;
                case "WifiWay":
                    if (dm.SerialPortIsOpened)
                        this._mySerialPortCom.ClosePort();
                    this.WriteLogHandler("选择Wifi方式通信.");
                    this._bolFlagCommunicateWay = true;             // 更新通信方式标识，true，为wifi方式
                    break;
            }
        }

        /// <summary>
        /// 数据发送处理
        /// </summary>
        /// <param name="btData">具体数据</param>
        public bool SendDataHandler(byte[] btData)
        {
            bool bolIsSuccess = false;

            // ture为Wifi方式发送数据，false为串口方式发送数据
            if (this._bolFlagCommunicateWay)
                bolIsSuccess = this._mySocketCom.SendDataHandler(btData);
            else
                bolIsSuccess = this._mySerialPortCom.SendDataHandler(btData);
            return bolIsSuccess;
        }

        /// <summary>
        /// 通信是否连接
        /// </summary>
        /// <returns></returns>
        public bool IsCommunicateConnected()
        {
            bool bolIsConnected = false;

            // ture为Wifi方式发送数据，false为串口方式发送数据
            if (this._bolFlagCommunicateWay)
                bolIsConnected = this._mySocketCom.IsConnected;
            else
                bolIsConnected = this._mySerialPortCom.IsOpen;
            return bolIsConnected;
        }
    }
}
