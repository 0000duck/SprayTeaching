//**************************************************************************************************//
// 此处是软件核心，主要是用于数据更新
//**************************************************************************************************//
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
        //private MySerialPort _mySerialPortCom;             // 串口的对象
        //private MySocketCom _mySocketCom;                   // Socket通信对象
        private MyDataMessage _myDataMessage;               // 数据消息的对象
        private MyConfigFileINI _myConfigFileINI;           // 配置文件的对象
        private MyCommunicate _myCommunicate;               // 数据通信对象
        private MyRoboDKExtension _myRoboDKExtension;       // RoboDK的对象

        private delegate void UpdateDifferentThreadParameterEventHandler(object strParameter);      // 更新不同线程的参数的delegate变量声明，主要用于对绑定控件数据的更改 
        private UpdateDifferentThreadParameterEventHandler _dlgtUpdateDifThreadParam;               // 更新不同线程的数据

        public DataModel MainDataModel
        {
            get { return _mainDataModel; }
            set { _mainDataModel = value; RaisePropertyChanged("MainDataModel"); }
        }

        #endregion

        #region  构造函数

        // 构造函数中初始化顺序很有讲究，log必须最前面，DataMessage必须在ConfigFileINI的前面，ConfigFileINI要更新DataMessage中的数据
        public ConnectModelVM( )
        {
            this._mainDataModel = new DataModel();                                                                              // 所有数据的对象
            this._dlgtUpdateDifThreadParam = new UpdateDifferentThreadParameterEventHandler(UpdateDifferentThreadParam);        // 用于更新不同线程的参数,目前用于更新listview的绑定的数据

            this._myLogObject = new MyLog(this._mainDataModel.LogFilePath);                                                     // 日志更新的对象
            this._myLogObject.UpdateLogContent += new UpdateLogContentEventHandler(UpdateLogContentHandler);                    // 日志中线程初始化的时候写日志，自己写自己，因为要将数据写到listview中，只能统一采用外部方式

            this._myDataMessage = new MyDataMessage();                                                                          // DataMessage的对象，存放接收数据和发送数据
            this._myDataMessage.UpdateSampleInform += new UpdateMessageSampleInformEventHandler(UpdateMessageSampleInformHandler);

            this._myConfigFileINI = new MyConfigFileINI(this._mainDataModel.ConfigFileAddress);                                         // 配置文件的对象
            this._myConfigFileINI.UpdateLogContent += new UpdateLogContentEventHandler(UpdateLogContentHandler);                        // 配置文件写日志
            this._myConfigFileINI.UpdateConfigParameter += new UpdateConfigFileParameterEventHandler(GetConfigParameterHandler);        // 配置文件更新参数

            this._myCommunicate = new MyCommunicate();                                                                                  // 数据通信的对象
            this._myCommunicate.DataReceived += new DataReceivedEventHandler(ReceiveDataHandler);                                       // 数据通信接收数据的处理
            this._myCommunicate.UpdateLogContent += new UpdateLogContentEventHandler(UpdateLogContentHandler);                          // 数据通信写日志
            this._myCommunicate.UpdateSerialPortIsOpened += new UpdateSerialPortIsOpenedEventHandler(UpdateSerialPortIsOpenedHandler);  // 更新串口通断状态信息
            this._myCommunicate.UpdateSocketIsConnected += new UpdateSocketIsConnectedEventHandler(UpdateSocketIsConnectedHandler);     // 更新socket通断的状态信息

            //this._mySerialPortCom = new MySerialPort();                                                                                    // 串口的对象
            //this._mySerialPortCom.DataReceived += new DataReceivedEventHandler(ReceiveDataHandler);                                        // 接收数据的处理
            //this._mySerialPortCom.UpdateLogContent += new UpdateLogContentEventHandler(WriteLogHandler);                           // 串口写日志
            //this._mySerialPortCom.UpdateSerialPortIsOpened += new UpdateSerialPortIsOpenedEventHandler(UpdateSerialPortIsOpenedHandler);   // 更新串口通断状态信息

            //this._mySocketCom = new MySocketCom();                                                                                          // socket通信的对象
            //this._mySocketCom.UpdateLogContent += new UpdateLogContentEventHandler(WriteLogHandler);                                // socket写日志
            //this._mySocketCom.DataReceived += new DataReceivedEventHandler(ReceiveDataHandler);                                             // 接收数据的处理
            //this._mySocketCom.UpdateSocketIsConnected += new UpdateSocketIsConnectedEventHandler(UpdateSocketIsConnectedHandler);           // 更新socket通断的状态信息

            this._myRoboDKExtension = new MyRoboDKExtension();                                                                  // RoboDK的对象
            this._myRoboDKExtension.UpdateLogContent += new UpdateLogContentEventHandler(UpdateLogContentHandler);              // RoboDK写日志
            this._myRoboDKExtension.UpdateRobotParameter += new UpdateRobotParameterEventHandler(UpdateRobotParameterHandler);  // 更新机器人参数


        }

        #endregion

        #region 方法

        #region 数据通信部分

        #region 串口相关的方法

        /// <summary>
        /// 打开或关闭串口
        /// </summary>
        public void OpenCloseSerialPortHandler( )
        {
            //更新串口参数
            //this._mySerialPortCom.PortName = this._mainDataModel.SerialPortName;
            //this._mySerialPortCom.BaudRate = (SerialPortBaudRates)this._mainDataModel.SerialPortBaudRate;
            //this._mySerialPortCom.ParityBit = (Parity)this._mainDataModel.SerialPortParityBit;
            //this._mySerialPortCom.StopBit = (StopBits)this._mainDataModel.SerialPortStopBit;
            //this._mySerialPortCom.DataBit = (SerialPortDataBits)this._mainDataModel.SerialPortDataBit;

            //bool bolSerialPortIsOpened = this._mainDataModel.SerialPortIsOpened;
            //this._mySerialPortCom.OpenCloseSerialPort(bolSerialPortIsOpened);
            this._myCommunicate.OpenCloseSerialPort(this._mainDataModel);
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
        public void OpenCloseSocketHandler( )
        {
            //this._mySocketCom.SocketIPAddress = this._mainDataModel.SocketIPAddress;
            //this._mySocketCom.SocketPortNum = this._mainDataModel.SocketPortNum;

            //bool bolSocketIsConnected = this._mainDataModel.SocketIsConnected;
            //this._mySocketCom.OpenCloseSocket(bolSocketIsConnected);
            this._myCommunicate.OpenCloseSocket(this._mainDataModel);
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
        public void SocketSendDataHandler( )
        {
            //this._mySocketCom.SendDataHandler();
        }

        /// <summary>
        /// 发送数据处理部分
        /// </summary>
        /// <param name="obj">发送什么数据的命令</param>
        public void SendDataHandler(object obj)
        {
            string strCommand = (string)obj;
            byte[] byteData = this._myDataMessage.SendDataMessage(strCommand,this._mainDataModel);
            this._myCommunicate.SendDataHandler(byteData);
        }

        #endregion

        #region 数据接收

        /// <summary>
        /// 对串口中接收的数据进行处理
        /// </summary>
        /// <param name="byteDataReceiveMessage">接收的数据</param>
        private void ReceiveDataHandler(byte[] byteDataReceiveMessage)
        {
            this._myDataMessage.ReceiveDataMessage(byteDataReceiveMessage);             // 将接收的数据传入DataMessage对象中
            string readString = BitConverter.ToString(byteDataReceiveMessage);          // 将字节型数组转换为字符串型
            this._mainDataModel.SerialPortDataReceived += readString + "\r\n";
        }
        #endregion

        #region 通信方式选择
        public void SelectCommunicateWayHandler(object obj)
        {
            //string strWay = obj as string;
            //switch(strWay)
            //{
            //    case "SerialPortWay":
            //        if (this._mainDataModel.SocketIsConnected)
            //            this._mySocketCom.CloseSocket();
            //        this.UpdateLogContentHandler("选择串口方式通信.");
            //        break;
            //    case "WifiWay":
            //        if (this._mainDataModel.SerialPortIsOpened)
            //            this._mySerialPortCom.ClosePort();
            //        this.UpdateLogContentHandler("选择Wifi方式通信.");
            //        break;
            //}
            this._myCommunicate.SelectCommunicateWayHandler(obj, this._mainDataModel);
        }
        #endregion

        #endregion

        #region  RoboDK相关的方法

        #region RoboDK接收机器人参数部分
        /// <summary>
        /// 更新机器人的参数
        /// </summary>
        /// <param name="dblJoints">6个关节角度</param>
        private void UpdateRobotParameterHandler(Dictionary<string, object> dicData)
        {
            this.UpdateRobotJoints(dicData["RobotJoint1"], dicData["RobotJoint2"], dicData["RobotJoint3"], dicData["RobotJoint4"], dicData["RobotJoint5"], dicData["RobotJoint6"]);
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

        public void SelectRobotModelHandler(object objParameter)
        {
            if (objParameter == null)
                return;
            this._myRoboDKExtension.SelectRobotModelHandler(objParameter);
        }

        #endregion

        #region  日志相关的方法

        /// <summary>
        /// 更新日志
        /// </summary>
        /// <param name="strMessage">日志消息</param>
        private void UpdateLogContentHandler(string strMessage)
        {
            string strTime = this._myLogObject.AddLogMessage(strMessage);                                   // 在日志中添加消息
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
        public void CloseAllResourceHandler( )
        {
            this._myLogObject.Close();              // 关闭日志相关的资源
            //this._mySerialPortCom.Close();         // 关闭与串口相关的资源 
            //this._mySocketCom.Close();              // 关闭与socket相关的资源
            this._myCommunicate.Close();
            this._myRoboDKExtension.Close();        // 关闭与RoboDK相关的资源
            this._myDataMessage.Close();            // 关闭与数据消息相关的资源
        }
        #endregion



        #region 配置文件相关的方法

        /// <summary>
        /// 获取配置文件中的参数
        /// </summary>
        /// <param name="obj">传入的参数</param>
        private void GetConfigParameterHandler(object obj)
        {
            object[] objPar = (object[])obj;
            double[] dblCalibrateAngles = (double[])objPar[0];
            double[] dblCalibrateDirectioins = (double[])objPar[1];

            // 更新DataMessage对象中的标定角度
            this._myDataMessage.CalibrateAngles = dblCalibrateAngles;
            this._myDataMessage.CalibrateDirections = dblCalibrateDirectioins;

            // 更新数据区中的标定关节角度
            this._mainDataModel.RobotCalibrateAngle1 = dblCalibrateAngles[0];
            this._mainDataModel.RobotCalibrateAngle2 = dblCalibrateAngles[1];
            this._mainDataModel.RobotCalibrateAngle3 = dblCalibrateAngles[2];
            this._mainDataModel.RobotCalibrateAngle4 = dblCalibrateAngles[3];
            this._mainDataModel.RobotCalibrateAngle5 = dblCalibrateAngles[4];
            this._mainDataModel.RobotCalibrateAngle6 = dblCalibrateAngles[5];

            // 更新数据区中的标定关节方向
            this._mainDataModel.RobotCalibrateDirection1 = dblCalibrateDirectioins[0];
            this._mainDataModel.RobotCalibrateDirection2 = dblCalibrateDirectioins[1];
            this._mainDataModel.RobotCalibrateDirection3 = dblCalibrateDirectioins[2];
            this._mainDataModel.RobotCalibrateDirection4 = dblCalibrateDirectioins[3];
            this._mainDataModel.RobotCalibrateDirection5 = dblCalibrateDirectioins[4];
            this._mainDataModel.RobotCalibrateDirection6 = dblCalibrateDirectioins[5];
        }

        #endregion

        #region  DataMessage数据处理部分
        private void UpdateMessageSampleInformHandler(int intSampleFrequent, int intSampleCycle)
        {
            this._mainDataModel.CurrentSampleFrequent = intSampleFrequent;
            this._mainDataModel.CurrentSampleCycle = intSampleCycle;
        }

        #endregion


        #endregion
    }
}
