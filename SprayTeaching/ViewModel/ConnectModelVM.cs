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

            this._mySerialPortMain = new MySerialPort();
            this._mySerialPortMain.DataReceived += new DataReceivedEventHandler(DataReceiveHandler);

            this._myRoboDKExtension = new MyRoboDKExtension();
            this._myRoboDKExtension.UpdateLogContent += new UpdateLogContentEventHandler(UpdateLog);
            this._myRoboDKExtension.UpdateRobotParameter += new UpdateRobotParameterEventHandler(UpdateRobotParameter);

            this._myLogObject = new MyLog(this._mainDataModel.LogFilePath);
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
        private void UpdateRobotParameter(double[] dblJoints)
        {
            this._mainDataModel.RobotJoint1 = Math.Round(dblJoints[0], 3);
            this._mainDataModel.RobotJoint2 = Math.Round(dblJoints[1], 3);
            this._mainDataModel.RobotJoint3 = Math.Round(dblJoints[2], 3);
            this._mainDataModel.RobotJoint4 = Math.Round(dblJoints[3], 3);
            this._mainDataModel.RobotJoint5 = Math.Round(dblJoints[4], 3);
            this._mainDataModel.RobotJoint6 = Math.Round(dblJoints[5], 3);
        }

        #endregion

        #region  日志相关的方法

        /// <summary>
        /// 更新日志
        /// </summary>
        /// <param name="strParameter">日志消息</param>
        private void UpdateLog(string strParameter)
        {
            string strNewLog = this._myLogObject.AddLogContent(strParameter);
            this._mainDataModel.LogFileContent += strNewLog;
        }
        #endregion

        #endregion
    }
}
