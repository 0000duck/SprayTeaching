using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

using SprayTeaching.Model;
using SprayTeaching.BaseClassLib;
using SprayTeaching.MyAllClass;

namespace SprayTeaching.ViewModel
{
    public class ConnectModelVM : BaseNotifyPropertyChanged
    {
        #region 变量

        /// <summary>
        /// 所有数据的对象
        /// </summary>
        private DataModel _mainDataModel;                   // 所有数据的对象        
        private MyLog _myLogObject;                         // 日志文件的对象
        private MyRoboDKExtension _myRoboDKExtension;       // RoboDK的对象
        private MySerialPort _mySerialPortMain;             // 串口的对象
        private MySocketCom _mySocketCom;                   // Socket通信对象

        private delegate void UpdateDifferentThreadParameterEventHandler(object strParameter);      // 更新不同线程的参数的delegate变量声明，主要用于对绑定控件数据的更改 
        private UpdateDifferentThreadParameterEventHandler _dlgtUpdateDifThreadParam;               // 更新不同线程的数据

        public DataModel MainDataModel
        {
            get { return _mainDataModel; }
            set { _mainDataModel = value; RaisePropertyChanged("MainDataModel"); }
        }

        #endregion

        #region  构造函数
        public ConnectModelVM()
        {
            this._mainDataModel = new DataModel();                                                                              // 所有数据的对象
            this._dlgtUpdateDifThreadParam = new UpdateDifferentThreadParameterEventHandler(UpdateDifferentThreadParam);        // 用于更新不同线程的参数,目前用于更新listview的绑定的数据

            this._myLogObject = new MyLog(this._mainDataModel.LogFilePath);                                                     // 日志更新的对象
            this._myLogObject.UpdateLogContent += new UpdateLogContentEventHandler(UpdateLogContentHandler);                    // 日志中线程初始化的时候写日志，自己写自己，因为要将数据写到listview中，只能统一采用外部方式

            this._mySerialPortMain = new MySerialPort();                                                                        // 串口的对象
            this._mySerialPortMain.DataReceived += new DataReceivedEventHandler(DataReceiveHandler);                            // 接收数据的处理
            this._mySerialPortMain.UpdateLogContent += new UpdateLogContentEventHandler(UpdateLogContentHandler);               // 串口写日志
            this._mySerialPortMain.UpdateSerialPortIsOpened += new UpdateSerialPortIsOpenedEventHandler(UpdateSerialPortIsOpenedHandler);

            this._mySocketCom = new MySocketCom();                                                                              // socket通信的对象
            this._mySocketCom.UpdateLogContent += new UpdateLogContentEventHandler(UpdateLogContentHandler);                    // socket写日志
            this._mySocketCom.DataReceived += new DataReceivedEventHandler(DataReceiveHandler);                                 // 接收数据的处理
            this._mySocketCom.UpdateSocketIsConnected += new UpdateSocketIsConnectedEventHandler(UpdateSocketIsConnectedHandler);

            this._myRoboDKExtension = new MyRoboDKExtension();                                                                  // RoboDK的对象
            this._myRoboDKExtension.UpdateLogContent += new UpdateLogContentEventHandler(UpdateLogContentHandler);              // RoboDK写日志
            this._myRoboDKExtension.UpdateRobotParameter += new UpdateRobotParameterEventHandler(UpdateRobotParameterHandler);  // 更新机器人参数
        }

        #endregion

        #region 方法

        #region 串口相关的方法

        /// <summary>
        /// 打开或关闭串口
        /// </summary>
        public void OpenCloseSerialPortHandler()
        {
            //更新串口参数
            this._mySerialPortMain.PortName = this._mainDataModel.SerialPortName;
            this._mySerialPortMain.BaudRate = (SerialPortBaudRates)this._mainDataModel.SerialPortBaudRate;
            this._mySerialPortMain.ParityBit = (Parity)this._mainDataModel.SerialPortParityBit;
            this._mySerialPortMain.StopBit = (StopBits)this._mainDataModel.SerialPortStopBit;
            this._mySerialPortMain.DataBit = (SerialPortDataBits)this._mainDataModel.SerialPortDataBit;

            //// 临时存储串口是否打开，串口的通断图
            //bool bolSerialPortIsOpened = false;
            //string strSerialPortIsOpenedImage = string.Empty;

            //// 这么做的原因是由于“属性或索引器不得作为 out 或 ref 参数传递”
            //this._mySerialPortMain.OpenCloseSerialPortHandler(ref bolSerialPortIsOpened, ref strSerialPortIsOpenedImage);      

            //// 更新串口的参数
            //this._mainDataModel.SerialPortIsOpened=bolSerialPortIsOpened;
            //this._mainDataModel.SerialPortIsOpenedImage=strSerialPortIsOpenedImage;

            bool bolSerialPortIsOpened = this._mainDataModel.SerialPortIsOpened;
            this._mySerialPortMain.OpenCloseSerialPort(bolSerialPortIsOpened);
        }

        /// <summary>
        /// 更新串口的通断状态
        /// </summary>
        /// <param name="bolIsOpened">是否连接，true为连接，false为断开</param>
        private void UpdateSerialPortIsOpenedHandler(bool bolIsOpened)
        {
            if (bolIsOpened)
                this._mainDataModel.SerialPortIsOpenedImage = MyConstString.IMG_SERIAL_PORT_CONNECT;
            else
                this._mainDataModel.SerialPortIsOpenedImage = MyConstString.IMG_SERIAL_PORT_DISCONNECT;

            this._mainDataModel.SerialPortIsOpened = bolIsOpened;
        }

        #endregion

        #region Scoket相关的方法

        /// <summary>
        /// 打开或者关闭socket事件处理
        /// </summary>
        public void OpenCloseSocketHandler()
        {
            this._mySocketCom.SocketIPAddress = this._mainDataModel.SocketIPAddress;
            this._mySocketCom.SocketPortNum = this._mainDataModel.SocketPortNum;

            //bool bolSocketIsConnected = this._mainDataModel.SocketIsConnected;
            //string strSocketIsConnectedImage = this._mainDataModel.SocketIsConnectedImage;
            //this._mySocketCom.OpenCloseSocketHandler(ref bolSocketIsConnected, ref strSocketIsConnectedImage);

            //this._mainDataModel.SocketIsConnected = bolSocketIsConnected;
            //this._mainDataModel.SocketIsConnectedImage = strSocketIsConnectedImage;

            bool bolSocketIsConnected = this._mainDataModel.SocketIsConnected;
            this._mySocketCom.OpenCloseSocket(bolSocketIsConnected);
        }

        /// <summary>
        /// 更新socket的连接状态事件处理
        /// </summary>
        /// <param name="bolIsConnected"></param>
        private void UpdateSocketIsConnectedHandler(bool bolIsConnected)
        {
            if (bolIsConnected)
                this._mainDataModel.SocketIsConnectedImage = MyConstString.IMG_SOCKET_CONNECT;
            else
                this._mainDataModel.SocketIsConnectedImage = MyConstString.IMG_SOCKET_DISCONNECT;

            this._mainDataModel.SocketIsConnected = bolIsConnected;
        }

        /// <summary>
        /// socket发送数据事件处理
        /// </summary>
        public void SocketSendDataHandler()
        {
            this._mySocketCom.SendDataHandler();
        }

        #endregion

        #region 数据接收

        /// <summary>
        /// 对串口中接收的数据进行处理
        /// </summary>
        /// <param name="strDataReceive">接收的数据</param>
        private void DataReceiveHandler(string strDataReceive)
        {
            this._mainDataModel.SerialPortDataReceived += strDataReceive + "\r\n";
        }

        #endregion

        #region  RoboDK相关的方法

        /// <summary>
        /// 更新机器人的参数
        /// </summary>
        /// <param name="dblJoints">6个关节角度</param>
        private void UpdateRobotParameterHandler(Dictionary<string, object> dicData)
        {
            this.UpdateRobotJoints(dicData["RobotJoint1"], dicData["RobotJoint2"], dicData["RobotJoint3"], dicData["RobotJoint4"], dicData["RobotJoint5"],dicData["RobotJoint6"]);
            this.UpdateRobotPoses(dicData["RobotPoseX"], dicData["RobotPoseY"], dicData["RobotPoseZ"], dicData["RobotPoseU"], dicData["RobotPoseV"], dicData["RobotPoseW"]);
            this.UpdateRobotMoveSpeed(dicData["RobotMoveSpeed"]);
        }

        /// <summary>
        /// 更新机器人关节角度
        /// </summary>
        /// <param name="objJoint1">关节1</param>
        /// <param name="objJoint2">关节2</param>
        /// <param name="objJoint3">关节3</param>
        /// <param name="objJoint4">关节4</param>
        /// <param name="objJoint5">关节5</param>
        /// <param name="objJoint6">关节6</param>
        private void UpdateRobotJoints(object objJoint1, object objJoint2, object objJoint3, object objJoint4, object objJoint5, object objJoint6)
        {
            double dblJoint1 = (double)objJoint1;
            double dblJoint2 = (double)objJoint2;
            double dblJoint3 = (double)objJoint3;
            double dblJoint4 = (double)objJoint4;
            double dblJoint5 = (double)objJoint5;
            double dblJoint6 = (double)objJoint6;

            this._mainDataModel.RobotJoint1 = Math.Round(dblJoint1, 3);
            this._mainDataModel.RobotJoint2 = Math.Round(dblJoint2, 3);
            this._mainDataModel.RobotJoint3 = Math.Round(dblJoint3, 3);
            this._mainDataModel.RobotJoint4 = Math.Round(dblJoint4, 3);
            this._mainDataModel.RobotJoint5 = Math.Round(dblJoint5, 3);
            this._mainDataModel.RobotJoint6 = Math.Round(dblJoint6, 3);
        }

        /// <summary>
        /// 更新机器人6个直角坐标系的值
        /// </summary>
        /// <param name="objPoseX">X轴</param>
        /// <param name="objPoseY">Y轴</param>
        /// <param name="objPoseZ">Z轴</param>
        /// <param name="objPoseU">U轴</param>
        /// <param name="objPoseV">V轴</param>
        /// <param name="objPoseW">W轴</param>
        private void UpdateRobotPoses(object objPoseX, object objPoseY, object objPoseZ, object objPoseU, object objPoseV, object objPoseW)
        {
            double dblPoseX = (double)objPoseX;
            double dblPoseY = (double)objPoseY;
            double dblPoseZ = (double)objPoseZ;
            double dblPoseU = (double)objPoseU;
            double dblPoseV = (double)objPoseV;
            double dblPoseW = (double)objPoseW;

            this._mainDataModel.RobotRectangularX = Math.Round(dblPoseX, 3);
            this._mainDataModel.RobotRectangularY = Math.Round(dblPoseY, 3);
            this._mainDataModel.RobotRectangularZ = Math.Round(dblPoseZ, 3);
            this._mainDataModel.RobotRectangularU = Math.Round(dblPoseU, 3);
            this._mainDataModel.RobotRectangularV = Math.Round(dblPoseV, 3);
            this._mainDataModel.RobotRectangularW = Math.Round(dblPoseW, 3);
        }

        /// <summary>
        /// 机器人的运动速度
        /// </summary>
        /// <param name="dblMoveSpeed">速度</param>
        private void UpdateRobotMoveSpeed(object objMoveSpeed)
        {
            double dblMoveSpeed = (double)objMoveSpeed;
            this._mainDataModel.RobotMoveSpeed = dblMoveSpeed;
        }

        #endregion

        #region  日志相关的方法

        /// <summary>
        /// 更新日志
        /// </summary>
        /// <param name="strMessage">日志消息</param>
        private void UpdateLogContentHandler(string strMessage)
        {
            string strTime = this._myLogObject.AddLogMessage(strMessage);
            MyLogMessage myLogMessage = new MyLogMessage() { LogTime = strTime, LogMessage = strMessage };
            App.Current.Dispatcher.BeginInvoke(this._dlgtUpdateDifThreadParam, myLogMessage);               // 使用调度者dispatcher来异步处理多线程，目前用于更新listview绑定的数据
        }

        /// <summary>
        /// 异步更新日志数据，目前是用于更新listview绑定的数据
        /// </summary>
        /// <param name="objParameter"></param>
        private void UpdateDifferentThreadParam(object objParameter)
        {
            MyLogMessage myLogMessage = objParameter as MyLogMessage;
            this._mainDataModel.LogDataList.Insert(0, myLogMessage);
        }
        #endregion

        #region  关闭窗口所需要关闭的资源

        /// <summary>
        /// 关闭所有资源
        /// </summary>
        public void CloseAllResourceHandler()
        {
            this._myLogObject.Close();              // 关闭日志相关的资源
            this._mySerialPortMain.Close();         // 关闭与串口相关的资源 
            this._mySocketCom.Close();              // 关闭与socket相关的资源
            this._myRoboDKExtension.Close();        // 关闭与RoboDK相关的资源

        }
        #endregion

        public void SelectCommunicateWayHandler(object obj)
        {
            string strWay = obj as string;
            switch(strWay)
            {
                case "SerialPortWay":
                    if (this._mainDataModel.SocketIsConnected)
                        this._mySocketCom.CloseSocket();
                    this.UpdateLogContentHandler("选择串口方式通信.");
                    break;
                case "WifiWay":
                    if (this._mainDataModel.SerialPortIsOpened)
                        this._mySerialPortMain.ClosePort();
                    this.UpdateLogContentHandler("选择Wifi方式通信.");
                    break;
            }
        }

        #endregion
    }
}
