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
        public void OpenCloseSerialPort()
        {
            //更新串口参数
            this._mySerialPortMain.PortName = this._mainDataModel.SerialPortName;
            this._mySerialPortMain.BaudRate = (SerialPortBaudRates)this._mainDataModel.SerialPortBaudRate;
            this._mySerialPortMain.ParityBit = (Parity)this._mainDataModel.SerialPortParityBit;
            this._mySerialPortMain.StopBit = (StopBits)this._mainDataModel.SerialPortStopBit;
            this._mySerialPortMain.DataBit = (SerialPortDataBits)this._mainDataModel.SerialPortDataBit;

            // 临时存储串口是否打开，串口的通断图
            bool bolSerialPortIsOpened = false;
            string strSerialPortIsOpenedImage = string.Empty;

            // 这么做的原因是由于“属性或索引器不得作为 out 或 ref 参数传递”
            this._mySerialPortMain.OpenCloseSerialPort(ref bolSerialPortIsOpened, ref strSerialPortIsOpenedImage);      

            // 更新串口的参数
            this._mainDataModel.SerialPortIsOpened=bolSerialPortIsOpened;
            this._mainDataModel.SerialPortIsOpenedImage=strSerialPortIsOpenedImage;
        }

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
        private void UpdateRobotParameterHandler(double[] dblJoints, double[] dblPoses)
        {
            this.UpdateRobotJoints(dblJoints);
            this.UpdateRobotPoses(dblPoses);
        }

        /// <summary>
        /// 更新机器人关节角度
        /// </summary>
        /// <param name="dblJoints">6个关节角度</param>
        private void UpdateRobotJoints(double[] dblJoints)
        {
            this._mainDataModel.RobotJoint1 = Math.Round(dblJoints[0], 3);
            this._mainDataModel.RobotJoint2 = Math.Round(dblJoints[1], 3);
            this._mainDataModel.RobotJoint3 = Math.Round(dblJoints[2], 3);
            this._mainDataModel.RobotJoint4 = Math.Round(dblJoints[3], 3);
            this._mainDataModel.RobotJoint5 = Math.Round(dblJoints[4], 3);
            this._mainDataModel.RobotJoint6 = Math.Round(dblJoints[5], 3);
        }

        /// <summary>
        /// 更新机器人6个直角坐标系的值
        /// </summary>
        /// <param name="dblPoses"></param>
        private void UpdateRobotPoses(double[] dblPoses)
        {
            this._mainDataModel.RobotRectangularX = Math.Round(dblPoses[0], 3);
            this._mainDataModel.RobotRectangularY = Math.Round(dblPoses[1], 3);
            this._mainDataModel.RobotRectangularZ = Math.Round(dblPoses[2], 3);
            this._mainDataModel.RobotRectangularU = Math.Round(dblPoses[3], 3);
            this._mainDataModel.RobotRectangularV = Math.Round(dblPoses[4], 3);
            this._mainDataModel.RobotRectangularW = Math.Round(dblPoses[5], 3);
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
            App.Current.Dispatcher.BeginInvoke(this._dlgtUpdateDifThreadParam, myLogMessage);               // 使用调度者dispatcher来异步处理多线程，目前用于更新listview中的数据
        }

        /// <summary>
        /// 异步更新日志数据，目前是用于更新listview中的数据
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
        public void CloseAllResource()
        {
            this._myLogObject.Close();              // 关闭日志相关的资源
            this._myRoboDKExtension.Close();        // 关闭与RoboDK相关的资源
            this._mySerialPortMain.Close();         // 关闭与串口相关的资源            
        }
        #endregion

        #endregion
    }
}
