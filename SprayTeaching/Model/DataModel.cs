using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using SprayTeaching.BaseClassLib;
using SprayTeaching.MyAllClass;

namespace SprayTeaching.Model
{
    public class DataModel : BaseNotifyPropertyChanged
    {
        #region RoboDK相关参数
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

        /// <summary>
        /// 机器人的运动速度
        /// </summary>
        private double _dblRobotMoveSpeed = 0.0;

        public double RobotMoveSpeed
        {
            get { return _dblRobotMoveSpeed; }
            set { _dblRobotMoveSpeed = value; RaisePropertyChanged("RobotMoveSpeed"); }
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
            set { _strSerialPortName = value; RaisePropertyChanged("SerialPortName"); }
        }

        /// <summary>
        /// 串口的波特率，默认是115200
        /// </summary>
        private int _brSerialPortBaudRate = (int)SerialPortBaudRates.BaudRate_115200;

        public int SerialPortBaudRate
        {
            get { return _brSerialPortBaudRate; }
            set { _brSerialPortBaudRate = value; RaisePropertyChanged("SerialPortBaudRate"); }
        }

        /// <summary>
        /// 串口的奇偶校验位，默认是NONE
        /// </summary>
        private int _prtSerialPortParityBit = (int)Parity.None;

        public int SerialPortParityBit
        {
            get { return _prtSerialPortParityBit; }
            set { _prtSerialPortParityBit = value; RaisePropertyChanged("SerialPortParityBit"); }
        }

        /// <summary>
        /// 串口的停止位，默认是1
        /// </summary>
        private int _sbSerialPortStopBit = (int)StopBits.One;

        public int SerialPortStopBit
        {
            get { return _sbSerialPortStopBit; }
            set { _sbSerialPortStopBit = value; RaisePropertyChanged("SerialPortStopBit"); }
        }

        /// <summary>
        /// 串口的数据位，默认为8
        /// </summary>
        private int _spdSerialPortDataBit = (int)SerialPortDataBits.EightBits;

        public int SerialPortDataBit
        {
            get { return _spdSerialPortDataBit; }
            set { _spdSerialPortDataBit = value; RaisePropertyChanged("SerialPortDataBit"); }
        }

        /// <summary>
        /// 串口是否打开，如果串行端口已打开，则为 true；否则为 false; 默认值为 false;
        /// </summary>
        private bool _bolSerialPortIsOpened = false;

        public bool SerialPortIsOpened
        {
            get { return _bolSerialPortIsOpened; }
            set { _bolSerialPortIsOpened = value; RaisePropertyChanged("SerialPortIsOpened"); }
        }

        private string _strSerialPortIsOpenedImage = MyConstString.IMG_SERIAL_PORT_DISCONNECT;

        public string SerialPortIsOpenedImage
        {
            get { return _strSerialPortIsOpenedImage; }
            set { _strSerialPortIsOpenedImage = value; RaisePropertyChanged("SerialPortIsOpenedImage"); }
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

        /// <summary>
        /// 所有可以选择的串口端口号，分别为COM1，COM2，COM3，COM4，COM5，COM6，COM7
        /// </summary>
        public List<string> ListSerialPortName
        {
            get { return new List<string> { { "COM1" }, { "COM2" }, { "COM3" }, { "COM4" }, { "COM5" }, { "COM6" }, { "COM7" } }; }
        }

        /// <summary>
        /// 所有可以选择的串口波特率，分别为75,300,600,1200,2400,4800,9600,14400,19200,115200
        /// </summary>
        public List<int> ListSerialPortBaudRate
        {
            get { return new List<int> { { 75 }, { 300 }, { 600 }, { 1200 }, { 2400 }, { 4800 }, { 9600 }, { 14400 }, { 19200 }, { 115200 }}; }
        }

        /// <summary>
        /// 所有可以选择的串口奇偶校验位，分别为0,1,2,3,4
        /// </summary>
        public List<int> ListSerialPortParityBit
        {
            get { return new List<int> { { 0 }, { 1 }, { 2 }, { 3 }, { 4 } }; }
        }

        /// <summary>
        /// 所有可以选择的串口数据位，分别为5,6,7,8,9,10
        /// </summary>
        public List<int> ListSerialPortDataBit
        {
            get { return new List<int> { { 5 }, { 6 }, { 7 }, { 8 }, { 9 }, { 10 } }; }
        }

        /// <summary>
        /// 所有可以选择的串口停止位，分别为0,1,2,3
        /// </summary>
        public List<int> ListSerialPortStopBit
        {
            get { return new List<int> { { 0 }, { 1 }, { 2 }, { 3 } }; }
        }

        #endregion

        #region 日志相关参数

        /// <summary>
        /// 日志文件的路径
        /// </summary>
        private string _strLogFilePath = "./SprayTeachLog.log";

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

        /// <summary>
        /// 与硬件设备的通信方式，1为串口方式，2为wifi方式
        /// </summary>
        private string _strCommunicateHardwareWay = "1";

        public string CommunicateHardwareWay
        {
            get { return _strCommunicateHardwareWay; }
            set { _strCommunicateHardwareWay = value; RaisePropertyChanged("CommunicateHardwareWay"); }
        }

        #region 网络通信
        /// <summary>
        /// 网络通信，socket的IP地址
        /// </summary>
        private string _strSocketIPAddress = "10.8.193.177";

        public string SocketIPAddress
        {
            get { return _strSocketIPAddress; }
            set { _strSocketIPAddress = value; RaisePropertyChanged("SocketIPAddress"); }
        }

        /// <summary>
        /// 网络通信，socket的端口号
        /// </summary>
        private int _strSocketPortNum = 12000;

        public int SocketPortNum
        {
            get { return _strSocketPortNum; }
            set { _strSocketPortNum = value; RaisePropertyChanged("SocketPortNum"); }
        }

        /// <summary>
        /// 网络通信，socket通断标识符
        /// </summary>
        private bool _bolSocketIsConnected = false;

        public bool SocketIsConnected
        {
            get { return _bolSocketIsConnected; }
            set { _bolSocketIsConnected = value; RaisePropertyChanged("SocketIsConnected"); }
        }

        /// <summary>
        /// 网络通信，socket通断标识图片
        /// </summary>
        private string _strSocketIsConnectedImage = MyConstString.IMG_SOCKET_DISCONNECT;

        public string SocketIsConnectedImage
        {
            get { return _strSocketIsConnectedImage; }
            set { _strSocketIsConnectedImage = value; RaisePropertyChanged("SocketIsConnectedImage"); }
        }

        #endregion

        #region 配置文件的相关数据

        /// <summary>
        /// 机器人标定的关节角度1
        /// </summary>
        private double _dblRobotCalibrateAngle1 = 0.0;

        public double RobotCalibrateAngle1
        {
            get { return _dblRobotCalibrateAngle1; }
            set { _dblRobotCalibrateAngle1 = value; RaisePropertyChanged("RobotCalibrateAngle1"); }
        }

        /// <summary>
        /// 机器人标定的关节角度2
        /// </summary>
        private double _dblRobotCalibrateAngle2 = 0.0;

        public double RobotCalibrateAngle2
        {
            get { return _dblRobotCalibrateAngle2; }
            set { _dblRobotCalibrateAngle2 = value; RaisePropertyChanged("RobotCalibrateAngle2"); }
        }

        /// <summary>
        /// 机器人标定的关节角度3
        /// </summary>
        private double _dblRobotCalibrateAngle3 = 0.0;

        public double RobotCalibrateAngle3
        {
            get { return _dblRobotCalibrateAngle3; }
            set { _dblRobotCalibrateAngle3 = value; RaisePropertyChanged("RobotCalibrateAngle3"); }
        }

        /// <summary>
        /// 机器人标定的关节角度4
        /// </summary>
        private double _dblRobotCalibrateAngle4 = 0.0;

        public double RobotCalibrateAngle4
        {
            get { return _dblRobotCalibrateAngle4; }
            set { _dblRobotCalibrateAngle4 = value; RaisePropertyChanged("RobotCalibrateAngle4"); }
        }

        /// <summary>
        /// 机器人标定的关节角度5
        /// </summary>
        private double _dblRobotCalibrateAngle5 = 0.0;

        public double RobotCalibrateAngle5
        {
            get { return _dblRobotCalibrateAngle5; }
            set { _dblRobotCalibrateAngle5 = value; RaisePropertyChanged("RobotCalibrateAngle5"); }
        }

        /// <summary>
        /// 机器人标定的关节角度6
        /// </summary>
        private double _dblRobotCalibrateAngle6 = 0.0;

        public double RobotCalibrateAngle6
        {
            get { return _dblRobotCalibrateAngle6; }
            set { _dblRobotCalibrateAngle6 = value; RaisePropertyChanged("RobotCalibrateAngle6"); }
        }

        /// <summary>
        /// 配置文件的路径
        /// </summary>
        private string _strConfigFileAddress = "./RobotConfig.ini";

        public string ConfigFileAddress
        {
            get { return _strConfigFileAddress; }
            set { _strConfigFileAddress = value; RaisePropertyChanged("ConfigFileAddress"); }
        }

        /// <summary>
        /// 机器人标定关节1方向
        /// </summary>
        private double _dblRobotCalibrateDirection1 = 0.0;

        public double RobotCalibrateDirection1
        {
            get { return _dblRobotCalibrateDirection1; }
            set { _dblRobotCalibrateDirection1 = value; RaisePropertyChanged("RobotCalibrateDirection1"); }
        }

        /// <summary>
        /// 机器人标定关节2方向
        /// </summary>
        private double _dblRobotCalibrateDirection2 = 0.0;

        public double RobotCalibrateDirection2
        {
            get { return _dblRobotCalibrateDirection2; }
            set { _dblRobotCalibrateDirection2 = value; RaisePropertyChanged("RobotCalibrateDirection2"); }
        }

        /// <summary>
        /// 机器人标定关节3方向
        /// </summary>
        private double _dblRobotCalibrateDirection3 = 0.0;

        public double RobotCalibrateDirection3
        {
            get { return _dblRobotCalibrateDirection3; }
            set { _dblRobotCalibrateDirection3 = value; RaisePropertyChanged("RobotCalibrateDirection3"); }
        }

        /// <summary>
        /// 机器人标定关节4方向
        /// </summary>
        private double _dblRobotCalibrateDirection4 = 0.0;

        public double RobotCalibrateDirection4
        {
            get { return _dblRobotCalibrateDirection4; }
            set { _dblRobotCalibrateDirection4 = value; RaisePropertyChanged("RobotCalibrateDirection4"); }
        }

        /// <summary>
        /// 机器人标定关节5方向
        /// </summary>
        private double _dblRobotCalibrateDirection5 = 0.0;

        public double RobotCalibrateDirection5
        {
            get { return _dblRobotCalibrateDirection5; }
            set { _dblRobotCalibrateDirection5 = value; RaisePropertyChanged("RobotCalibrateDirection5"); }
        }

        /// <summary>
        /// 机器人标定关节6方向
        /// </summary>
        private double _dblRobotCalibrateDirection6 = 0.0;

        public double RobotCalibrateDirection6
        {
            get { return _dblRobotCalibrateDirection6; }
            set { _dblRobotCalibrateDirection6 = value; RaisePropertyChanged("RobotCalibrateDirection6"); }
        }

        #endregion

        #region 数据参数

        #region  采样频率相关
        /// <summary>
        /// 当前的采样频率
        /// </summary>
        private int _strCurrentSampleFrequent = 0;

        public int CurrentSampleFrequent
        {
            get { return _strCurrentSampleFrequent; }
            set { _strCurrentSampleFrequent = value; RaisePropertyChanged("CurrentSampleFrequent"); }
        }


        /// <summary>
        /// 当前的采样周期
        /// </summary>
        private int _strCurrentSampleCycle = 0;

        public int CurrentSampleCycle
        {
            get { return _strCurrentSampleCycle; }
            set { _strCurrentSampleCycle = value; RaisePropertyChanged("CurrentSampleCycle"); }
        }


        /// <summary>
        /// 要设置采样频率的参数
        /// </summary>
        private int _intSetSampleFrequent = 0;

        public int SetSampleFrequent
        {
            get { return _intSetSampleFrequent; }
            set { _intSetSampleFrequent = value; RaisePropertyChanged("SetSampleFrequent"); }
        }

        #endregion

        #region 轴地址相关
        /// <summary>
        /// 要设置1轴地址的参数
        /// </summary>
        private byte _byteSetAxis1Address = 0x00;

        public byte SetAxis1Address
        {
            get { return _byteSetAxis1Address; }
            set { _byteSetAxis1Address = value; RaisePropertyChanged("SetAxis1Address"); }
        }

        /// <summary>
        /// 要设置2轴地址的参数
        /// </summary>
        private byte _byteSetAxis2Address = 0x00;

        public byte SetAxis2Address
        {
            get { return _byteSetAxis2Address; }
            set { _byteSetAxis2Address = value; RaisePropertyChanged("SetAxis2Address"); }
        }

        /// <summary>
        /// 要设置3轴地址的参数
        /// </summary>
        private byte _byteSetAxis3Address = 0x00;

        public byte SetAxis3Address
        {
            get { return _byteSetAxis3Address; }
            set { _byteSetAxis3Address = value; RaisePropertyChanged("SetAxis3Address"); }
        }

        /// <summary>
        /// 要设置4轴地址的参数
        /// </summary>
        private byte _byteSetAxis4Address = 0x00;

        public byte SetAxis4Address
        {
            get { return _byteSetAxis4Address; }
            set { _byteSetAxis4Address = value; RaisePropertyChanged("SetAxis4Address"); }
        }

        /// <summary>
        /// 要设置5轴地址的参数
        /// </summary>
        private byte _byteSetAxis5Address = 0x00;

        public byte SetAxis5Address
        {
            get { return _byteSetAxis5Address; }
            set { _byteSetAxis5Address = value; RaisePropertyChanged("SetAxis5Address"); }
        }

        /// <summary>
        /// 要设置6轴地址的参数
        /// </summary>
        private byte _byteSetAxis6Address = 0x00;

        public byte SetAxis6Address
        {
            get { return _byteSetAxis6Address; }
            set { _byteSetAxis6Address = value; RaisePropertyChanged("SetAxis6Address"); }
        }

        /// <summary>
        /// 当前1轴地址的参数
        /// </summary>
        private byte _byteCurrentAxis1Address = 0x00;

        public byte CurrentAxis1Address
        {
            get { return _byteCurrentAxis1Address; }
            set { _byteCurrentAxis1Address = value; RaisePropertyChanged("CurrentAxis1Address"); }
        }

        /// <summary>
        /// 当前2轴地址的参数
        /// </summary>
        private byte _byteCurrentAxis2Address = 0x00;

        public byte CurrentAxis2Address
        {
            get { return _byteCurrentAxis2Address; }
            set { _byteCurrentAxis2Address = value; RaisePropertyChanged("CurrentAxis2Address"); }
        }

        /// <summary>
        /// 当前3轴地址的参数
        /// </summary>
        private byte _byteCurrentAxis3Address = 0x00;

        public byte CurrentAxis3Address
        {
            get { return _byteCurrentAxis3Address; }
            set { _byteCurrentAxis3Address = value; RaisePropertyChanged("CurrentAxis3Address"); }
        }

        /// <summary>
        /// 当前4轴地址的参数
        /// </summary>
        private byte _byteCurrentAxis4Address = 0x00;

        public byte CurrentAxis4Address
        {
            get { return _byteCurrentAxis4Address; }
            set { _byteCurrentAxis4Address = value; RaisePropertyChanged("CurrentAxis4Address"); }
        }

        /// <summary>
        /// 当前5轴地址的参数
        /// </summary>
        private byte _byteCurrentAxis5Address = 0x00;

        public byte CurrentAxis5Address
        {
            get { return _byteCurrentAxis5Address; }
            set { _byteCurrentAxis5Address = value; RaisePropertyChanged("CurrentAxis5Address"); }
        }

        /// <summary>
        /// 当前6轴地址的参数
        /// </summary>
        private byte _byteCurrentAxis6Address = 0x00;

        public byte CurrentAxis6Address
        {
            get { return _byteCurrentAxis6Address; }
            set { _byteCurrentAxis6Address = value; RaisePropertyChanged("CurrentAxis6Address"); }
        }
        #endregion

        #region  设备的标识信息
        /// <summary>
        /// 设备是否连接
        /// </summary>
        private bool _bolDeviceIsConnected = false;

        public bool DeviceIsConnected
        {
            get { return _bolDeviceIsConnected; }
            set { _bolDeviceIsConnected = value; RaisePropertyChanged("DeviceIsConnected"); }
        }

        /// <summary>
        /// 设备数据采样是否准备就绪
        /// </summary>
        private bool _bolDeviceSampleIsReady = false;

        public bool DeviceSampleIsReady
        {
            get { return _bolDeviceSampleIsReady; }
            set { _bolDeviceSampleIsReady = value; RaisePropertyChanged("DeviceSampleIsReady"); }
        }

        /// <summary>
        /// 采样是否正在运行标识符，正在运行为true，没有运行为false
        /// </summary>
        private bool _bolIsSampleDataRunning = false;

        public bool IsSampleDataRunning
        {
            get { return _bolIsSampleDataRunning; }
            set { _bolIsSampleDataRunning = value; RaisePropertyChanged("IsSampleDataRunning"); }
        }

        #endregion

        #region 标定设定的相关变量
        /// <summary>
        /// 当前轴1角度的绝度值
        /// </summary>
        private double _dblCurrentAbsoluteAngle1 = 0.0;

        public double CurrentAbsoluteAngle1
        {
            get { return _dblCurrentAbsoluteAngle1; }
            set { _dblCurrentAbsoluteAngle1 = value; RaisePropertyChanged("CurrentAbsoluteAngle1"); }
        }

        /// <summary>
        /// 当前轴2角度的绝度值
        /// </summary>
        private double _dblCurrentAbsoluteAngle2 = 0.0;

        public double CurrentAbsoluteAngle2
        {
            get { return _dblCurrentAbsoluteAngle2; }
            set { _dblCurrentAbsoluteAngle2 = value; RaisePropertyChanged("CurrentAbsoluteAngle2"); }
        }

        /// <summary>
        /// 当前轴3角度的绝度值
        /// </summary>
        private double _dblCurrentAbsoluteAngle3 = 0.0;

        public double CurrentAbsoluteAngle3
        {
            get { return _dblCurrentAbsoluteAngle3; }
            set { _dblCurrentAbsoluteAngle3 = value; RaisePropertyChanged("CurrentAbsoluteAngle3"); }
        }

        /// <summary>
        /// 当前轴4角度的绝度值
        /// </summary>
        private double _dblCurrentAbsoluteAngle4 = 0.0;

        public double CurrentAbsoluteAngle4
        {
            get { return _dblCurrentAbsoluteAngle4; }
            set { _dblCurrentAbsoluteAngle4 = value; RaisePropertyChanged("CurrentAbsoluteAngle4"); }
        }

        /// <summary>
        /// 当前轴5角度的绝度值
        /// </summary>
        private double _dblCurrentAbsoluteAngle5 = 0.0;

        public double CurrentAbsoluteAngle5
        {
            get { return _dblCurrentAbsoluteAngle5; }
            set { _dblCurrentAbsoluteAngle5 = value; RaisePropertyChanged("CurrentAbsoluteAngle5"); }
        }

        /// <summary>
        /// 当前轴6角度的绝度值
        /// </summary>
        private double _dblCurrentAbsoluteAngle6 = 0.0;

        public double CurrentAbsoluteAngle6
        {
            get { return _dblCurrentAbsoluteAngle6; }
            set { _dblCurrentAbsoluteAngle6 = value; RaisePropertyChanged("CurrentAbsoluteAngle6"); }
        }

        /// <summary>
        /// 设定轴1绝对的角度值
        /// </summary>
        private double _dblSetAbsoluteAngle1 = 1.0;

        public double SetAbsoluteAngle1
        {
            get { return _dblSetAbsoluteAngle1; }
            set { _dblSetAbsoluteAngle1 = value; RaisePropertyChanged("SetAbsoluteAngle1"); }
        }

        /// <summary>
        /// 设定轴2绝对的角度值
        /// </summary>
        private double _dblSetAbsoluteAngle2 = 2.0;

        public double SetAbsoluteAngle2
        {
            get { return _dblSetAbsoluteAngle2; }
            set { _dblSetAbsoluteAngle2 = value; RaisePropertyChanged("SetAbsoluteAngle2"); }
        }

        /// <summary>
        /// 设定轴3绝对的角度值
        /// </summary>
        private double _dblSetAbsoluteAngle3 = 3.0;

        public double SetAbsoluteAngle3
        {
            get { return _dblSetAbsoluteAngle3; }
            set { _dblSetAbsoluteAngle3 = value; RaisePropertyChanged("SetAbsoluteAngle3"); }
        }

        /// <summary>
        /// 设定轴4绝对的角度值
        /// </summary>
        private double _dblSetAbsoluteAngle4 = 4.0;

        public double SetAbsoluteAngle4
        {
            get { return _dblSetAbsoluteAngle4; }
            set { _dblSetAbsoluteAngle4 = value; RaisePropertyChanged("SetAbsoluteAngle4"); }
        }

        /// <summary>
        /// 设定轴5绝对的角度值
        /// </summary>
        private double _dblSetAbsoluteAngle5 = 5.0;

        public double SetAbsoluteAngle5
        {
            get { return _dblSetAbsoluteAngle5; }
            set { _dblSetAbsoluteAngle5 = value; RaisePropertyChanged("SetAbsoluteAngle5"); }
        }

        /// <summary>
        /// 设定轴6绝对的角度值
        /// </summary>
        private double _dblSetAbsoluteAngle6 = 6.0;

        public double SetAbsoluteAngle6
        {
            get { return _dblSetAbsoluteAngle6; }
            set { _dblSetAbsoluteAngle6 = value; RaisePropertyChanged("SetAbsoluteAngle6"); }
        }

        /// <summary>
        /// 设定标定轴1的方向,true为正向,false为反向
        /// </summary>
        private bool _bolSetCalibrateAxis1Direction = false;

        public bool SetCalibrateAxis1Direction
        {
            get { return _bolSetCalibrateAxis1Direction; }
            set { _bolSetCalibrateAxis1Direction = value; RaisePropertyChanged("SetCalibrateAxis1Direction"); }
        }

        /// <summary>
        /// 设定标定轴2的方向,true为正向,false为反向
        /// </summary>
        private bool _bolSetCalibrateAxis2Direction = false;

        public bool SetCalibrateAxis2Direction
        {
            get { return _bolSetCalibrateAxis2Direction; }
            set { _bolSetCalibrateAxis2Direction = value; RaisePropertyChanged("SetCalibrateAxis2Direction"); }
        }

        /// <summary>
        /// 设定标定轴3的方向,true为正向,false为反向
        /// </summary>
        private bool _bolSetCalibrateAxis3Direction = false;

        public bool SetCalibrateAxis3Direction
        {
            get { return _bolSetCalibrateAxis3Direction; }
            set { _bolSetCalibrateAxis3Direction = value; RaisePropertyChanged("SetCalibrateAxis3Direction"); }
        }

        /// <summary>
        /// 设定标定轴4的方向,true为正向,false为反向
        /// </summary>
        private bool _bolSetCalibrateAxis4Direction = false;

        public bool SetCalibrateAxis4Direction
        {
            get { return _bolSetCalibrateAxis4Direction; }
            set { _bolSetCalibrateAxis4Direction = value; RaisePropertyChanged("SetCalibrateAxis4Direction"); }
        }

        /// <summary>
        /// 设定标定轴5的方向,true为正向,false为反向
        /// </summary>
        private bool _bolSetCalibrateAxis5Direction = false;

        public bool SetCalibrateAxis5Direction
        {
            get { return _bolSetCalibrateAxis5Direction; }
            set { _bolSetCalibrateAxis5Direction = value; RaisePropertyChanged("SetCalibrateAxis5Direction"); }
        }

        /// <summary>
        /// 设定标定轴6的方向,true为正向,false为反向
        /// </summary>
        private bool _bolSetCalibrateAxis6Direction = false;

        public bool SetCalibrateAxis6Direction
        {
            get { return _bolSetCalibrateAxis6Direction; }
            set { _bolSetCalibrateAxis6Direction = value; RaisePropertyChanged("SetCalibrateAxis6Direction"); }
        }

        #endregion

        #region 机器人运动轨迹文件名

        /// <summary>
        /// 机器人本地运动轨迹文件的文件名
        /// </summary>
        private string _strLocationRobotMoveFileName = "RobotAngle";

        public string LocationRobotMoveFileName
        {
            get { return _strLocationRobotMoveFileName; }
            set { _strLocationRobotMoveFileName = value; RaisePropertyChanged("LocationRobotMoveFileName"); }
        }

        #endregion

        #endregion

        #region 界面底栏状态显示

        /// <summary>
        /// 错误操作信息提示
        /// </summary>
        private string _strErrorOperateMessage = "";

        public string ErrorOperateMessage
        {
            get { return _strErrorOperateMessage; }
            set { _strErrorOperateMessage = value; RaisePropertyChanged("ErrorOperateMessage"); }
        }

        /// <summary>
        /// 是否正在运行添加目标点
        /// </summary>
        private bool _bolIsRunningAddTarget = false;

        public bool IsRunningAddTarget
        {
            get { return _bolIsRunningAddTarget; }
            set { _bolIsRunningAddTarget = value; RaisePropertyChanged("IsRunningAddTarget"); }
        }

        /// <summary>
        /// 运行添加目标点的执行情况
        /// </summary>
        private int _intRunningAddTargetState = 0;

        public int RunningAddTargetState
        {
            get { return _intRunningAddTargetState; }
            set { _intRunningAddTargetState = value; RaisePropertyChanged("RunningAddTargetState"); }
        }

        /// <summary>
        /// 机器人的运动状态
        /// </summary>
        private string _strRobotRunningState = "停止";

        public string RobotRunningState
        {
            get { return _strRobotRunningState; }
            set { _strRobotRunningState = value; RaisePropertyChanged("RobotRunningState"); }
        }

        /// <summary>
        /// 机器人的运动状态标识符
        /// </summary>
        private bool _bolIsRobotRunning = false;

        public bool IsRobotRunning
        {
            get { return _bolIsRobotRunning; }
            set { _bolIsRobotRunning = value; RaisePropertyChanged("IsRobotRunning"); }
        }


        #endregion

        #region  FTP相关

        /// <summary>
        /// FTP服务器的IP地址
        /// </summary>
        private string _strFTPServerIP = "10.8.193.177";

        public string FTPServerIP
        {
            get { return _strFTPServerIP; }
            set { _strFTPServerIP = value; RaisePropertyChanged("FTPServerIP"); }
        }

        /// <summary>
        /// FTP的用户名
        /// </summary>
        private string _strFTPUserID = "xing";

        public string FTPUserID
        {
            get { return _strFTPUserID; }
            set { _strFTPUserID = value; RaisePropertyChanged("FTPUserID"); }
        }

        /// <summary>
        /// FTP的密码
        /// </summary>
        private string _strFTPPassword = "123456";

        public string FTPPassword
        {
            get { return _strFTPPassword; }
            set { _strFTPPassword = value; RaisePropertyChanged("FTPPassword"); }
        }

        #endregion
    }
}
