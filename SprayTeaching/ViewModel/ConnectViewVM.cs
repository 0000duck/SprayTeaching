using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SprayTeaching.BaseClassLib;

namespace SprayTeaching.ViewModel
{
    public class ConnectViewVM : BaseNotifyPropertyChanged
    {
        #region  构造函数
        public ConnectViewVM( )
        {
            this._modelViewModel = new ConnectModelVM();
        }
        #endregion

        #region  在界面中要绑定的数据

        /// <summary>
        /// 与数据model连接的ViewModel对象
        /// </summary>
        private ConnectModelVM _modelViewModel;

        public ConnectModelVM ModelViewModel
        {
            get { return _modelViewModel; }
            set { _modelViewModel = value; RaisePropertyChanged("MainDataModel"); }
        }

        #endregion

        #region  在界面中要绑定的命令

        /// <summary>
        /// 打开关闭串口命令
        /// </summary>
        private MyCommand _openCloseSerialPortCommand;

        public MyCommand OpenCloseSerialPortCommand
        {
            get
            {
                if (this._openCloseSerialPortCommand == null)
                    this._openCloseSerialPortCommand = new MyCommand(this.ExecuteOpenCloseSerialPort);
                return this._openCloseSerialPortCommand;
            }
        }

        /// <summary>
        /// 打开关闭socket命令
        /// </summary>
        private MyCommand _openCloseSocketCommand;

        public MyCommand OpenCloseSocketCommand
        {
            get
            {
                if (this._openCloseSocketCommand == null)
                    this._openCloseSocketCommand = new MyCommand(this.ExecuteOpenCloseSocket);
                return this._openCloseSocketCommand;
            }
        }

        /// <summary>
        /// 关闭窗口之前命令
        /// </summary>
        private MyCommand _closingWindowCommand;

        public MyCommand ClosingWindowCommand
        {
            get
            {
                if (this._closingWindowCommand == null)
                    this._closingWindowCommand = new MyCommand(this.ExecuteClosingWindow);
                return this._closingWindowCommand;
            }
        }

        /// <summary>
        /// socket发送数据命令
        /// </summary>
        private MyCommand _socketSendDataCommand;

        public MyCommand SocketSendDataCommand
        {
            get
            {
                if (this._socketSendDataCommand == null)
                    this._socketSendDataCommand = new MyCommand(this.ExecuteSocketSendData);
                return this._socketSendDataCommand;
            }
        }

        /// <summary>
        /// 通信方式的选择命令
        /// </summary>
        private MyCommand _selectCommunicateWayCommand;

        public MyCommand SelectCommunicateWayCommand
        {
            get
            {
                if (this._selectCommunicateWayCommand == null)
                    this._selectCommunicateWayCommand = new MyCommand(this.ExecuteSelectCommunicateWay);
                return this._selectCommunicateWayCommand;
            }
        }

        /// <summary>
        /// 选择机器人模型命令
        /// </summary>
        private MyCommand _selectRobotModelCommand;

        public MyCommand SelectRobotModelCommand
        {
            get
            {
                if (this._selectRobotModelCommand == null)
                    this._selectRobotModelCommand = new MyCommand(this.ExecuteSelectRobotModel);
                return this._selectRobotModelCommand;
            }
        }

        /// <summary>
        /// 通信，发送数据命令
        /// </summary>
        private MyCommand _sendDataCommunicateCommand;

        public MyCommand SendDataCommunicateCommand
        {
            get
            {
                if (this._sendDataCommunicateCommand == null)
                    this._sendDataCommunicateCommand = new MyCommand(this.ExecuteSendDataCommunicate);
                return _sendDataCommunicateCommand;
            }
        }

        /// <summary>
        /// 将轴地址清零命令
        /// </summary>
        private MyCommand _clearAllAxisAddressCommand;

        public MyCommand ClearAllAxisAddressCommand
        {
            get
            {
                if (this._clearAllAxisAddressCommand == null)
                    this._clearAllAxisAddressCommand = new MyCommand(this.ExecuteClearAllAxisAddress);
                return _clearAllAxisAddressCommand;
            }
        }

        /// <summary>
        /// 开始数据采样命令
        /// </summary>
        private MyCommand _startSampleDataCommand;

        public MyCommand StartSampleDataCommand
        {
            get
            {
                if (this._startSampleDataCommand == null)
                    this._startSampleDataCommand = new MyCommand(this.ExecuteStartSampleData);
                return _startSampleDataCommand;
            }
        }

        /// <summary>
        /// 停止数据采样命令
        /// </summary>
        private MyCommand _stopSampleDataCommand;

        public MyCommand StopSampleDataCommand
        {
            get
            {
                if (this._stopSampleDataCommand == null)
                    this._stopSampleDataCommand = new MyCommand(this.ExecuteStopSampleData);
                return _stopSampleDataCommand;
            }
        }

        /// <summary>
        /// 标定原点的角度预处理命令
        /// </summary>
        private MyCommand _preCalibrateOriginAngleCommand;

        public MyCommand PreCalibrateOriginAngleCommand
        {
            get
            {
                if (this._preCalibrateOriginAngleCommand == null)
                    this._preCalibrateOriginAngleCommand = new MyCommand(this.ExecutePreCalibrateOriginAngle);
                return _preCalibrateOriginAngleCommand;
            }
        }

        /// <summary>
        /// 标定原点的方向预处理命令
        /// </summary>
        private MyCommand _preCalibrateOriginDirectionCommand;

        public MyCommand PreCalibrateOriginDirectionCommand
        {
            get
            {
                if (this._preCalibrateOriginDirectionCommand == null)
                    this._preCalibrateOriginDirectionCommand = new MyCommand(this.ExecutePreCalibrateOriginDirection);
                return _preCalibrateOriginDirectionCommand;
            }
        }

        /// <summary>
        /// 标定机器人的角度命令
        /// </summary>
        private MyCommand _calibrateRobotAngleCommand;

        public MyCommand CalibrateRobotAngleCommand
        {
            get
            {
                if (this._calibrateRobotAngleCommand == null)
                    this._calibrateRobotAngleCommand = new MyCommand(this.ExecuteCalibrateRobotAngle);
                return _calibrateRobotAngleCommand;
            }
        }

        /// <summary>
        /// 标定机器人的方向命令
        /// </summary>
        private MyCommand _calibrateRobotDirectionCommand;

        public MyCommand CalibrateRobotDirectionCommand
        {
            get
            {
                if (this._calibrateRobotDirectionCommand == null)
                    this._calibrateRobotDirectionCommand = new MyCommand(this.ExecuteCalibrateRobotDirection);
                return _calibrateRobotDirectionCommand;
            }
        }

        /// <summary>
        /// 生成RoboDK程序命令
        /// </summary>
        private MyCommand _createRoboDKProgramCommand;

        public MyCommand CreateRoboDKProgramCommand
        {
            get 
            {
                if (this._createRoboDKProgramCommand == null)
                    this._createRoboDKProgramCommand = new MyCommand(this.ExecuteCreateRoboDKProgram);
                return _createRoboDKProgramCommand; 
            }
        }

        /// <summary>
        /// 运行RoboDK中的机器人程序
        /// </summary>
        private MyCommand _runRoboDKProgramCommand;

        public MyCommand RunRoboDKProgramCommand
        {
            get 
            {
                if (this._runRoboDKProgramCommand == null)
                    this._runRoboDKProgramCommand = new MyCommand(this.ExecuteRunRoboDKProgram);
                return _runRoboDKProgramCommand; 
            }
        }

        #endregion

        #region  具体命令的执行事件

        /// <summary>
        /// 执行打开或关闭串口操作
        /// </summary>
        /// <param name="objParameter"></param>
        private void ExecuteOpenCloseSerialPort(object objParameter = null)
        {
            this._modelViewModel.OpenCloseSerialPortHandler();
        }

        /// <summary>
        /// 执行打开或关闭socket操作
        /// </summary>
        /// <param name="objParameter"></param>
        private void ExecuteOpenCloseSocket(object objParameter = null)
        {
            this._modelViewModel.OpenCloseSocketHandler();
        }

        /// <summary>
        /// 执行关闭窗口之前的操作
        /// </summary>
        /// <param name="objParameter"></param>
        private void ExecuteClosingWindow(object objParameter = null)
        {
            this._modelViewModel.CloseAllResourceHandler();
        }

        /// <summary>
        /// 执行socket发送数据
        /// </summary>
        /// <param name="objParameter"></param>
        private void ExecuteSocketSendData(object objParameter = null)
        {
            this._modelViewModel.SocketSendDataHandler();
        }

        /// <summary>
        /// 执行选择通信方式，是串口还是wifi
        /// </summary>
        /// <param name="objParameter">通信方式的字符串，“SerialPortWay”和“WifiWay”</param>
        private void ExecuteSelectCommunicateWay(object objParameter = null)
        {
            this._modelViewModel.SelectCommunicateWayHandler(objParameter);
        }

        /// <summary>
        /// 执行选择机器人模型
        /// </summary>
        /// <param name="objParameter">机器人模型编号，radio的编号：1,2,3,4,5</param>
        private void ExecuteSelectRobotModel(object objParameter = null)
        {
            this._modelViewModel.SelectRobotModelHandler(objParameter);
        }

        /// <summary>
        /// 执行通信，发送数据命令
        /// </summary>
        /// <param name="objParameter">发送的命令</param>
        private void ExecuteSendDataCommunicate(object objParameter = null)
        {
            this._modelViewModel.SendDataHandler(objParameter);
        }

        /// <summary>
        /// 执行将轴地址清零
        /// </summary>
        private void ExecuteClearAllAxisAddress(object objParameter = null)
        {
            this._modelViewModel.ClearAllAxisAddressHandler();
        }

        /// <summary>
        /// 执行开始数据采样
        /// </summary>
        /// <param name="objParameter"></param>
        private void ExecuteStartSampleData(object objParameter = null)
        {
            this._modelViewModel.StartSampleDataHandler();
        }

        /// <summary>
        /// 执行停止数据采样
        /// </summary>
        /// <param name="objParameter"></param>
        private void ExecuteStopSampleData(object objParameter = null)
        {
            this._modelViewModel.StopSampleDataHandler();
        }

        /// <summary>
        /// 执行标定原点角度预处理
        /// </summary>
        /// <param name="obj"></param>
        private void ExecutePreCalibrateOriginAngle(object obj = null)
        {
            this._modelViewModel.PreCalibrateOriginAngleHandler(obj);
        }

        /// <summary>
        /// 执行标定原点方向预处理
        /// </summary>
        /// <param name="obj"></param>
        private void ExecutePreCalibrateOriginDirection(object obj = null)
        {
            this._modelViewModel.PreCalibrateOriginDirectionHandler(obj);
        }

        /// <summary>
        /// 执行标定机器人角度
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteCalibrateRobotAngle(object obj = null)
        {
            this._modelViewModel.CalibrateRobotAngleHandler();
        }

        /// <summary>
        /// 执行标定机器人方向
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteCalibrateRobotDirection(object obj = null)
        {
            this._modelViewModel.CalibrateRobotDirectionHandler();
        }

        /// <summary>
        /// 创建机器人程序
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteCreateRoboDKProgram(object obj = null)
        {
            this._modelViewModel.CreateRoboDKProgramHandler();
        }

        /// <summary>
        /// 运行RoboDK中的机器人程序
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteRunRoboDKProgram(object obj=null)
        {
            this._modelViewModel.RunRoboDKProgramHandler();
        }

        #endregion

    }
}
