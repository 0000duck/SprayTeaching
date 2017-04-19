using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            this._mainDataModel = new DataModel();
            this._dlgtUpdateDifThreadParam = new UpdateDifferentThreadParameterEventHandler(UpdateDifferentThreadParam);    // 用于更新不同线程的参数

            this._myLogObject = new MyLog(this._mainDataModel.LogFilePath);
            this._myLogObject.UpdateLogContent += new UpdateLogContentEventHandler(UpdateLogContentHandler);

            this._mySerialPortMain = new MySerialPort();
            this._mySerialPortMain.DataReceived += new DataReceivedEventHandler(DataReceiveHandler);

            this._myRoboDKExtension = new MyRoboDKExtension();
            this._myRoboDKExtension.UpdateLogContent += new UpdateLogContentEventHandler(UpdateLogContentHandler);
            this._myRoboDKExtension.UpdateRobotParameter += new UpdateRobotParameterEventHandler(UpdateRobotParameterHandler);

            
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
            this._mySerialPortMain.BaudRate = this._mainDataModel.SerialPortBaudRate;
            this._mySerialPortMain.ParityBits = this._mainDataModel.SerialPortParityBits;
            this._mySerialPortMain.StopBits = this._mainDataModel.SerialPortStopBits;
            this._mySerialPortMain.DataBits = this._mainDataModel.SerialPortDataBits;
            try
            {
                //若串口是打开着的，则关闭串口；若串口是关闭着的，则打开串口
                if (!this._mySerialPortMain.IsOpen)
                {
                    this._mySerialPortMain.OpenPort();
                    this._mainDataModel.SerialPortIsOpened = this._mySerialPortMain.IsOpen;         //更新串口状态，是否打开
                }
                else
                {
                    this._mySerialPortMain.ClosePort();
                    this._mainDataModel.SerialPortIsOpened = this._mySerialPortMain.IsOpen;         //更新串口状态，是否打开
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// 对串口中接收的数据进行处理
        /// </summary>
        /// <param name="strDataReceive">接收的数据</param>
        private void DataReceiveHandler(string strDataReceive)
        {
            this._mainDataModel.SerialPortDataReceived = strDataReceive;
        }
        #endregion

        #region  RoboDK相关的方法

        /// <summary>
        /// 更新机器人的参数
        /// </summary>
        /// <param name="dblJoints">6个关节角度</param>
        private void UpdateRobotParameterHandler(double[] dblJoints,double[] dblPoses)
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
            App.Current.Dispatcher.BeginInvoke(this._dlgtUpdateDifThreadParam, myLogMessage);               // 使用调度者dispatcher来异步处理多线程
        }

        /// <summary>
        /// 异步更新日志数据
        /// </summary>
        /// <param name="objParameter"></param>
        private void UpdateDifferentThreadParam(object objParameter)
        {
            MyLogMessage myLogMessage = objParameter as MyLogMessage;
            this._mainDataModel.LogDataList.Add(myLogMessage);
        }
        #endregion

        #region  关闭窗口所需要关闭的资源

        /// <summary>
        /// 关闭所有资源
        /// </summary>
        public void CloseAllResource()
        {
            this._myRoboDKExtension.Close();        // 关闭与RoboDK相关的资源
            this._mySerialPortMain.ClosePort();     // 关闭与串口相关的资源
            this._myLogObject.Close();              // 关闭日志相关的资源
        }
        #endregion

        #endregion
    }
}
