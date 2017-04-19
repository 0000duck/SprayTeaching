using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using SprayTeaching.BaseClassLib;
using SprayTeaching.MyAllClass;

namespace SprayTeaching.Model
{
    public class DataModel : BaseNotifyPropertyChanged
    {
        #region  机器人关节坐标系下的6个角度值
        /// <summary>
        /// 机器人关节坐标系下的关节1角度
        /// </summary>
        private double _dblRobotJoint1 = 1.0;
        
        public double RobotJoint1
        {
            get { return _dblRobotJoint1; }
            set { _dblRobotJoint1 = value; RaisePropertyChanged("RobotJoint1"); }
        }

        /// <summary>
        /// 机器人关节坐标系下的关节2角度
        /// </summary>
        private double _dblRobotJoint2 = 2.0;

        public double RobotJoint2
        {
            get { return _dblRobotJoint2; }
            set { _dblRobotJoint2 = value; RaisePropertyChanged("RobotJoint2"); }
        }

        /// <summary>
        /// 机器人关节坐标系下的关节3角度
        /// </summary>
        private double _dblRobotJoint3 = 3.0;

        public double RobotJoint3
        {
            get { return _dblRobotJoint3; }
            set { _dblRobotJoint3 = value; RaisePropertyChanged("RobotJoint3"); }
        }

        /// <summary>
        /// 机器人关节坐标系下的关节4角度
        /// </summary>
        private double _dblRobotJoint4 = 4.0;

        public double RobotJoint4
        {
            get { return _dblRobotJoint4; }
            set { _dblRobotJoint4 = value; RaisePropertyChanged("RobotJoint4"); }
        }

        /// <summary>
        /// 机器人关节坐标系下的关节5角度
        /// </summary>
        private double _dblRobotJoint5 = 5.0;

        public double RobotJoint5
        {
            get { return _dblRobotJoint5; }
            set { _dblRobotJoint5 = value; RaisePropertyChanged("RobotJoint5"); }
        }

        /// <summary>
        /// 机器人关节坐标系下的关节6角度
        /// </summary>
        private double _dblRobotJoint6 = 6.0;

        public double RobotJoint6
        {
            get { return _dblRobotJoint6; }
            set { _dblRobotJoint6 = value; RaisePropertyChanged("RobotJoint6"); }
        }
        #endregion

        #region 机器人直角坐标系下的6个坐标值

        /// <summary>
        /// 机器人直角坐标系下的X坐标值
        /// </summary>
        private double _dblRobotRectangularX = 1.0;

        public double RobotRectangularX
        {
            get { return _dblRobotRectangularX; }
            set { _dblRobotRectangularX = value; RaisePropertyChanged("RobotRectangularX"); }
        }

        /// <summary>
        /// 机器人直角坐标系下的Y坐标值
        /// </summary>
        private double _dblRobotRectangularY = 2.0;

        public double RobotRectangularY
        {
            get { return _dblRobotRectangularY; }
            set { _dblRobotRectangularY = value; RaisePropertyChanged("RobotRectangularY"); }
        }

        /// <summary>
        /// 机器人直角坐标系下的Z坐标值
        /// </summary>
        private double _dblRobotRectangularZ = 3.0;

        public double RobotRectangularZ
        {
            get { return _dblRobotRectangularZ; }
            set { _dblRobotRectangularZ = value; RaisePropertyChanged("RobotRectangularZ"); }
        }

        /// <summary>
        /// 机器人直角坐标系下的U坐标值
        /// </summary>
        private double _dblRobotRectangularU = 4.0;

        public double RobotRectangularU
        {
            get { return _dblRobotRectangularU; }
            set { _dblRobotRectangularU = value; RaisePropertyChanged("RobotRectangularU"); }
        }

        /// <summary>
        /// 机器人直角坐标系下的V坐标值
        /// </summary>
        private double _dblRobotRectangularV = 5.0;

        public double RobotRectangularV
        {
            get { return _dblRobotRectangularV; }
            set { _dblRobotRectangularV = value; RaisePropertyChanged("RobotRectangularV"); }
        }

        /// <summary>
        /// 机器人直角坐标系下的W坐标值
        /// </summary>
        private double _dblRobotRectangularW = 6.0;

        public double RobotRectangularW
        {
            get { return _dblRobotRectangularW; }
            set { _dblRobotRectangularW = value; RaisePropertyChanged("RobotRectangularW"); }
        }

        #endregion

        #region  串口相关参数

        /// <summary>
        /// 串口的端口号，默认是COM2，方便测试
        /// </summary>
        private string _strSerialPortName = "COM2";

        public string SerialPortName
        {
            get { return _strSerialPortName; }
            set { _strSerialPortName = value; RaisePropertyChanged("SerialPortNum"); }
        }

        /// <summary>
        /// 串口的波特率，默认是9600
        /// </summary>
        private SerialPortBaudRates _brSerialPortBaudRate = SerialPortBaudRates.BaudRate_9600;

        public SerialPortBaudRates SerialPortBaudRate
        {
            get { return _brSerialPortBaudRate; }
            set { _brSerialPortBaudRate = value; RaisePropertyChanged("SerialPortBaudRate"); }
        }

        /// <summary>
        /// 串口的奇偶校验位，默认是NONE
        /// </summary>
        private Parity _prtSerialPortParityBits = Parity.None;

        public Parity SerialPortParityBits
        {
            get { return _prtSerialPortParityBits; }
            set { _prtSerialPortParityBits = value; RaisePropertyChanged("SerialPortParityBits"); }
        }

        /// <summary>
        /// 串口的停止位，默认是1
        /// </summary>
        private StopBits _sbSerialPortStopBits = StopBits.One;

        public StopBits SerialPortStopBits
        {
            get { return _sbSerialPortStopBits; }
            set { _sbSerialPortStopBits = value; RaisePropertyChanged("SerialPortStopBits"); }
        }

        /// <summary>
        /// 串口的数据位，默认为8
        /// </summary>
        private SerialPortDataBits _spdSerialPortDataBits = SerialPortDataBits.EightBits;

        public SerialPortDataBits SerialPortDataBits
        {
            get { return _spdSerialPortDataBits; }
            set { _spdSerialPortDataBits = value; RaisePropertyChanged("SerialPortDataBits"); }
        }

        /// <summary>
        /// 串口是否打开，如果串行端口已打开，则为 true；否则为 false; 默认值为 false;
        /// </summary>
        private bool _bolSerialPortIsOpened = false;

        public bool SerialPortIsOpened
        {
            get { return _bolSerialPortIsOpened; }
            set { _bolSerialPortIsOpened = value; RaisePropertyChanged("SerialPortBaudRate"); }
        }

        /// <summary>
        /// 串口的接收数据
        /// </summary>
        private string _strSerialPortDataReceived = string.Empty;

        public string SerialPortDataReceived
        {
            get { return _strSerialPortDataReceived; }
            set { _strSerialPortDataReceived = value; RaisePropertyChanged("SerialPortDataReceived"); }
        }
        #endregion

        #region 日志相关参数

        /// <summary>
        /// 日志文件的路径
        /// </summary>
        private string _strLogFilePath = "./Log.txt";

        public string LogFilePath
        {
            get { return _strLogFilePath; }
            set { _strLogFilePath = value; RaisePropertyChanged("LogFilePath"); }
        }

        /// <summary>
        /// 日志文件中的内容
        /// </summary>
        private ObservableCollection<MyLogMessage> _strLogDataList = new ObservableCollection<MyLogMessage> { };

        public ObservableCollection<MyLogMessage> LogDataList
        {
            get { return _strLogDataList; }
            set { _strLogDataList = value; RaisePropertyChanged("LogDataList"); }
        }

        #endregion
    }
}
