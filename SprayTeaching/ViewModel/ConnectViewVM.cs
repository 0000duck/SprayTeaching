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
        public ConnectViewVM()
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
        /// 打开串口命令
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
        /// 关闭窗口之前命令
        /// </summary>
        private MyCommand _closingWindowCommand;

        public MyCommand ClosingWindowCommand
        {
            get 
            {
                if (this._closingWindowCommand == null)
                    this._closingWindowCommand = new MyCommand(this.ExecuteClosingWindow);
                return _closingWindowCommand; 
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
            this._modelViewModel.OpenCloseSerialPort();
        }

        /// <summary>
        /// 执行关闭窗口之前的操作
        /// </summary>
        /// <param name="objParameter"></param>
        private void ExecuteClosingWindow(object objParameter = null)
        {
            this._modelViewModel.CloseAllResource();
        }

        #endregion

    }
}
