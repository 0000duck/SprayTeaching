using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SprayTeaching.BaseClassLib;

namespace SprayTeaching.BaseClassLib
{

    public class MySerialPort
    {
        #region 内部私有变量
        private string _portName = "COM1";                                              // 串口号，默认COM1
        private SerialPortBaudRates _baudRate = SerialPortBaudRates.BaudRate_115200;    // 波特率，默认9600
        private Parity _parity = Parity.None;                                           // 校验位，默认NONE
        private StopBits _stopBit = StopBits.One;                                       // 停止位，默认1
        private SerialPortDataBits _dataBit = SerialPortDataBits.EightBits;             // 数据位，默认8

        private StringBuilder _sbReceiveDataStorage = new StringBuilder();              // 存储所有接收的数据

        private SerialPort _comPort = new SerialPort();                                 // 串口的对象

        //串口对象
        /// <summary>
        /// 接收事件是否有效 false表示有效
        /// </summary>
        private bool ReceiveEventFlag = false;

        /// <summary>
        /// 开始符比特
        /// </summary>
        //private byte BeginByte = 0x5B;   //string Begin = "[";
        private byte BeginByte1 = 0xAA;
        private byte BeginByte2 = 0x55;

        /// <summary>
        /// 结束符比特
        /// </summary>
        //private byte EndByte = 0x5D;     //string End = "]";
        #endregion

        #region 外部事件

        public event UpdateLogContentEventHandler UpdateLogContent;             // 更新日志文件
        public event UpdateSerialPortIsOpenedEventHandler UpdateSerialPortIsOpened; // 更新串口的通断状态

        #endregion

        #region 外部公有接口变量
        /// <summary>
        /// 完整协议的记录处理事件
        /// </summary>        
        public event DataReceivedEventHandler DataReceived;
        public event SerialErrorReceivedEventHandler Error;

        #endregion

        #region  属性
        /// <summary>
        /// 串口号
        /// </summary>
        public string PortName
        {
            get { return _portName; }
            set { _portName = value; }
        }

        /// <summary>
        /// 波特率
        /// </summary>
        public SerialPortBaudRates BaudRate
        {
            get { return _baudRate; }
            set { _baudRate = value; }
        }

        /// <summary>
        /// 奇偶校验位
        /// </summary>
        public Parity ParityBit
        {
            get { return _parity; }
            set { _parity = value; }
        }

        /// <summary>
        /// 数据位
        /// </summary>
        public SerialPortDataBits DataBit
        {
            get { return _dataBit; }
            set { _dataBit = value; }
        }

        /// <summary>
        /// 停止位
        /// </summary>
        public StopBits StopBit
        {
            get { return _stopBit; }
            set { _stopBit = value; }
        }

        /// <summary>
        /// 端口是否已经打开,如果串行端口已打开，则为 true；否则为 false; 默认值为 false;
        /// </summary>
        public bool IsOpen
        {
            get { return _comPort.IsOpen; }
        }

        /// <summary>
        /// 存储所有接收的数据
        /// </summary>
        public StringBuilder ReceiveDataStorage
        {
            get { return _sbReceiveDataStorage; }
            set { _sbReceiveDataStorage = value; }
        }
        #endregion

        #region 构造函数

        /// <summary>
        /// 参数构造函数（使用枚举参数构造）
        /// </summary>
        /// <param name="baud">波特率</param>
        /// <param name="par">奇偶校验位</param>
        /// <param name="sBits">停止位</param>
        /// <param name="dBits">数据位</param>
        /// <param name="name">串口号</param>
        public MySerialPort(string name, SerialPortBaudRates baud, Parity par, SerialPortDataBits dBits, StopBits sBits)
        {
            _portName = name;
            _baudRate = baud;
            _parity = par;
            _dataBit = dBits;
            _stopBit = sBits;

            _comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
            _comPort.ErrorReceived += new SerialErrorReceivedEventHandler(comPort_ErrorReceived);
        }

        /// <summary>
        /// 参数构造函数（使用字符串参数构造）
        /// </summary>
        /// <param name="baud">波特率</param>
        /// <param name="par">奇偶校验位</param>
        /// <param name="sBits">停止位</param>
        /// <param name="dBits">数据位</param>
        /// <param name="name">串口号</param>
        public MySerialPort(string name, string baud, string par, string dBits, string sBits)
        {
            _portName = name;
            _baudRate = (SerialPortBaudRates)Enum.Parse(typeof(SerialPortBaudRates), baud);
            _parity = (Parity)Enum.Parse(typeof(Parity), par);
            _dataBit = (SerialPortDataBits)Enum.Parse(typeof(SerialPortDataBits), dBits);
            _stopBit = (StopBits)Enum.Parse(typeof(StopBits), sBits);

            _comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
            _comPort.ErrorReceived += new SerialErrorReceivedEventHandler(comPort_ErrorReceived);
        }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public MySerialPort( )
        {
            _portName = "COM1";
            _baudRate = SerialPortBaudRates.BaudRate_115200;
            _parity = Parity.None;
            _dataBit = SerialPortDataBits.EightBits;
            _stopBit = StopBits.One;

            _comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
            _comPort.ErrorReceived += new SerialErrorReceivedEventHandler(comPort_ErrorReceived);
        }

        #endregion

        #region  方法

        /// <summary>
        /// 关闭串口的所有资源
        /// </summary>
        public void Close( )
        {
            this.ClosePort();       // 关闭串口
            this.CloseVariables();  // 关闭所有变量
        }

        /// <summary>
        /// 关闭所有变量，使它们都invalidition
        /// </summary>
        private void CloseVariables( )
        {
            this._comPort = null;
            this._sbReceiveDataStorage = null;
        }

        /// <summary>
        /// 打开或关闭串口
        /// </summary>
        public void OpenCloseSerialPort(ref bool bolIsOpen, ref string strIsOpenImage)
        {
            try
            {
                //若串口是打开着的，则关闭串口；若串口是关闭着的，则打开串口
                if (!this._comPort.IsOpen)
                {
                    this.OpenPort();
                    bolIsOpen = this._comPort.IsOpen;         //更新串口状态，是否打开
                    strIsOpenImage = MyConstString.IMG_SERIAL_PORT_CONNECT;
                }
                else
                {
                    this.ClosePort();
                    bolIsOpen = this._comPort.IsOpen;         //更新串口状态，是否打开
                    strIsOpenImage = MyConstString.IMG_SERIAL_PORT_DISCONNECT;
                }
            }
            catch (Exception e)
            {
                //throw new Exception(e.Message);
                this.WriteLogHandler(e.Message);
            }
        }

        /// <summary>
        /// 打开或关闭串口
        /// </summary>
        public void OpenCloseSerialPort(bool bolIsOpen)
        {
            //若串口是打开着的，则关闭串口；若串口是关闭着的，则打开串口
            if (!bolIsOpen)
            {
                this.OpenPort();
            }
            else
            {
                this.ClosePort();
            }
        }

        /// <summary>
        /// 打开端口
        /// </summary>
        /// <returns></returns>
        private void OpenPort( )
        {
            if (_comPort.IsOpen) _comPort.Close();

            _comPort.PortName = _portName;
            _comPort.BaudRate = (int)_baudRate;
            _comPort.Parity = _parity;
            _comPort.DataBits = (int)_dataBit;
            _comPort.StopBits = _stopBit;
            try
            {
                _comPort.Open();
                this.WriteLogHandler(string.Format("串口开启成功,串口号为{0}.", this._portName));
            }
            catch (Exception e)
            {
                this.WriteLogHandler("无法开启串口" + e.Message, 1);
            }
            finally
            {
                UpdateConnectState(this._comPort.IsOpen);           // 更新串口的连接状态
            }
        }

        /// <summary>
        /// 关闭端口
        /// </summary>
        public void ClosePort( )
        {
            if (_comPort.IsOpen) this._comPort.Close();
            this.WriteLogHandler("串口已关闭.");
            UpdateConnectState(this._comPort.IsOpen);               // 更新串口的连接状态
        }

        /// <summary>
        /// 数据接收处理
        /// </summary>
        private void comPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // 禁止接收事件时直接退出
            if (ReceiveEventFlag) return;

            // 对接收数据的处理
            byte[] byteReadData = this.DataReceivedHandler();

            // 如果接收的数据为空，将不更新后续的数据
            if (byteReadData.Length == 0)
                return;

            // 触发数据的处理
            if (DataReceived != null)
            {
                DataReceived(byteReadData);
            }
        }

        /// <summary>
        /// 对数据接收的处理，经过数据的读取，数据转换，数据存储
        /// </summary>
        /// <returns></returns>
        private byte[] DataReceivedHandler( )
        {
            List<byte> byteData = ReadData1();                       // 读取数据

            byte[] byteRead = byteData.ToArray();                       // 将list中的数据转换为字节数组

            return byteRead;
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <returns>读取的数据</returns>
        private List<byte> ReadData1( )
        {
            List<byte> byteData = new List<byte>();
            bool bEndFlag = false;      // 是否结束
            bool bStartFlag = false;    // 是否开始
            byte[] readBuffer = new byte[_comPort.ReadBufferSize];
            int count = 0;
            try
            {
                count = _comPort.Read(readBuffer, 0, _comPort.ReadBufferSize);
            }
            catch
            {
                byteData.Clear();
            }
            

            for (int i = 1; i < count; i++)
            {
                // 寻找到起始位时，将开始标志位设置为true，结束标志位设置为false
                if (readBuffer[i - 1] == BeginByte1 && readBuffer[i] == BeginByte2)
                {
                    bStartFlag = true;
                    bEndFlag = false;
                    byteData.Clear();
                    byteData.Add(readBuffer[i - 1]);
                    byteData.Add(readBuffer[i]);
                    continue;  //检测到开始符号，后面就不执行了
                }

                // 只有在开始标志位为true，结束标志位为false时添加数据
                if (bStartFlag == true && bEndFlag == false)
                    byteData.Add(readBuffer[i]);

                // 检测list中的数据个数是否为16，若是，则结束
                if (byteData.Count == 16)
                {
                    bEndFlag = true;
                    break;  //若数据个数够16个，后面就不执行了
                }
            }
            // 只有满足起始标志位为ture，结束标志位也为true，则是正确的数据
            if (!(bStartFlag == true && bEndFlag == true))
                byteData.Clear();

            // 若数据校验和错误，则清除所有数据
            if (!DataCheckSum(byteData))
                byteData.Clear();

            return byteData;
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <returns>读取的数据</returns>
        private List<byte> ReadData( )
        {
            List<byte> byteData = new List<byte>();
            bool bEndFlag = false;      // 是否结束
            bool bStartFlag = false;    // 是否开始
            while (_comPort.BytesToRead > 0 || !bStartFlag || !bEndFlag)
            {
                byte[] readBuffer = new byte[_comPort.ReadBufferSize];
                int count = _comPort.Read(readBuffer, 0, _comPort.ReadBufferSize);

                for (int i = 1; i < count; i++)
                {
                    // 寻找到起始位时，将开始标志位设置为true，结束标志位设置为false
                    if (readBuffer[i - 1] == BeginByte1 && readBuffer[i] == BeginByte2)
                    {
                        bStartFlag = true;
                        bEndFlag = false;
                        byteData.Clear();
                        byteData.Add(readBuffer[i - 1]);
                        byteData.Add(readBuffer[i]);
                        continue;  //检测到开始符号，后面就不执行了
                    }

                    // 只有在开始标志位为true，结束标志位为false时添加数据
                    if (bStartFlag == true && bEndFlag == false)
                        byteData.Add(readBuffer[i]);

                    // 检测list中的数据个数是否为16，若是，则结束
                    if (byteData.Count == 16)
                    {
                        bEndFlag = true;
                        break;  //若数据个数够16个，后面就不执行了
                    }
                }
                // 只有满足起始标志位为ture，结束标志位也为true，则是正确的数据
                if (bStartFlag == true && bEndFlag == true)
                {
                    break;
                }
            }

            // 若数据校验和错误，则清除所有数据
            if (!DataCheckSum(byteData))
                byteData.Clear();

            return byteData;
        }

        /// <summary>
        /// 对数据进行校验和
        /// </summary>
        /// <param name="byteData">接收的数据</param>
        /// <returns>数据是否正确</returns>
        private bool DataCheckSum(List<byte> byteData)
        {
            bool bolIsOk = false;
            byte byteSum = 0x00;

            // 如果数据不是16个，则数据格式错误
            if (byteData.Count != 16)
            {
                bolIsOk = false;
                return bolIsOk;
            }

            // 对前15个数据进行累加
            for (int i = 0; i < 15; i++)
            {
                byteSum += byteData[i];
            }

            // 只有最后一个数据和前15个数据之和相等，则算是数据正确，否则错误
            if (byteSum == byteData[15])
                bolIsOk = true;

            return bolIsOk;
        }

        /// <summary>
        /// 错误处理函数
        /// </summary>
        private void comPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            if (Error != null)
            {
                Error(sender, e);
            }
        }

        #region 数据写入操作

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="msg"></param>
        public void SendDataHandler(string msg)
        {
            if (!(_comPort.IsOpen)) _comPort.Open();

            _comPort.Write(msg);
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="msg">写入端口的字节数组</param>
        public bool SendDataHandler(byte[] msg)
        {
            if (!_comPort.IsOpen)
            {
                this.WriteLogHandler("未打开串口，请先打开串口.", 1);
                return false;
            }
            if (msg.Length == 0)
            {
                this.WriteLogHandler("数据有误，为空.", 1);
                return false;
            }

            _comPort.Write(msg, 0, msg.Length);
            return true;
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="msg">包含要写入端口的字节数组</param>
        /// <param name="offset">参数从0字节开始的字节偏移量</param>
        /// <param name="count">要写入的字节数</param>
        public void SendDataHandler(byte[] msg, int offset, int count)
        {
            if (!(_comPort.IsOpen)) _comPort.Open();

            _comPort.Write(msg, offset, count);
        }

        /// <summary>
        /// 发送串口命令
        /// </summary>
        /// <param name="SendData">发送数据</param>
        /// <param name="ReceiveData">接收数据</param>
        /// <param name="Overtime">重复次数</param>
        /// <returns></returns>
        public int SendCommand(byte[] SendData, ref  byte[] ReceiveData, int Overtime)
        {
            if (!(_comPort.IsOpen)) _comPort.Open();

            ReceiveEventFlag = true;        //关闭接收事件
            _comPort.DiscardInBuffer();      //清空接收缓冲区                 
            _comPort.Write(SendData, 0, SendData.Length);

            int num = 0, ret = 0;
            while (num++ < Overtime)
            {
                if (_comPort.BytesToRead >= ReceiveData.Length) break;
                System.Threading.Thread.Sleep(1);
            }

            if (_comPort.BytesToRead >= ReceiveData.Length)
            {
                ret = _comPort.Read(ReceiveData, 0, ReceiveData.Length);
            }

            ReceiveEventFlag = false;       //打开事件
            return ret;
        }
        #endregion

        /// <summary>
        /// 更新串口的连接状态
        /// </summary>
        /// <param name="bolIsOpen"></param>
        private void UpdateConnectState(bool bolIsOpen)
        {
            if (this.UpdateSerialPortIsOpened != null)
                this.UpdateSerialPortIsOpened(bolIsOpen);
        }

        #region 写入日志
        /// <summary>
        /// 将消息写入日志
        /// </summary>
        /// <param name="strMessage">消息内容</param>
        private void WriteLogHandler(string strMessage, int intType = 0)
        {
            if (this.UpdateLogContent != null)
            {
                this.UpdateLogContent(strMessage, intType);
            }
        }
        #endregion

        #endregion
    }
}
