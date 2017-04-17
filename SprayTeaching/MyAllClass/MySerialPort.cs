using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprayTeaching.MyAllClass
{
    public delegate void DataReceivedEventHandler(string strDataReceive);
    public class MySerialPort
    {
        #region 内部私有变量
        private string _portName = "COM1";                                          //串口号，默认COM1
        private SerialPortBaudRates _baudRate = SerialPortBaudRates.BaudRate_9600;  //波特率，默认9600
        private Parity _parity = Parity.None;                                       //校验位，默认NONE
        private StopBits _stopBits = StopBits.One;                                  //停止位，默认1
        private SerialPortDataBits _dataBits = SerialPortDataBits.EightBits;        //数据位，默认8

        private SerialPort comPort = new SerialPort( ); 
                                    
        //串口对象
        /// <summary>
        /// 接收事件是否有效 false表示有效
        /// </summary>
        private bool ReceiveEventFlag = false;

        /// <summary>
        /// 开始符比特
        /// </summary>
        private byte BeginByte = 0x5B;   //string Begin = "[";

        /// <summary>
        /// 结束符比特
        /// </summary>
        private byte EndByte = 0x5D;     //string End = "]";
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
        public Parity ParityBits
        {
            get { return _parity; }
            set { _parity = value; }
        }

        /// <summary>
        /// 数据位
        /// </summary>
        public SerialPortDataBits DataBits
        {
            get { return _dataBits; }
            set { _dataBits = value; }
        }

        /// <summary>
        /// 停止位
        /// </summary>
        public StopBits StopBits
        {
            get { return _stopBits; }
            set { _stopBits = value; }
        }

        /// <summary>
        /// 端口是否已经打开,如果串行端口已打开，则为 true；否则为 false; 默认值为 false;
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return comPort.IsOpen;
            }
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
        public MySerialPort( string name, SerialPortBaudRates baud, Parity par, SerialPortDataBits dBits, StopBits sBits )
        {
            _portName = name;
            _baudRate = baud;
            _parity = par;
            _dataBits = dBits;
            _stopBits = sBits;

            comPort.DataReceived += new SerialDataReceivedEventHandler( comPort_DataReceived );
            comPort.ErrorReceived += new SerialErrorReceivedEventHandler( comPort_ErrorReceived );
        }

        /// <summary>
        /// 参数构造函数（使用字符串参数构造）
        /// </summary>
        /// <param name="baud">波特率</param>
        /// <param name="par">奇偶校验位</param>
        /// <param name="sBits">停止位</param>
        /// <param name="dBits">数据位</param>
        /// <param name="name">串口号</param>
        public MySerialPort( string name, string baud, string par, string dBits, string sBits )
        {
            _portName = name;
            _baudRate = (SerialPortBaudRates)Enum.Parse( typeof( SerialPortBaudRates ), baud );
            _parity = (Parity)Enum.Parse( typeof( Parity ), par );
            _dataBits = (SerialPortDataBits)Enum.Parse( typeof( SerialPortDataBits ), dBits );
            _stopBits = (StopBits)Enum.Parse( typeof( StopBits ), sBits );

            comPort.DataReceived += new SerialDataReceivedEventHandler( comPort_DataReceived );
            comPort.ErrorReceived += new SerialErrorReceivedEventHandler( comPort_ErrorReceived );
        }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public MySerialPort( )
        {
            _portName = "COM1";
            _baudRate = SerialPortBaudRates.BaudRate_9600;
            _parity = Parity.None;
            _dataBits = SerialPortDataBits.EightBits;
            _stopBits = StopBits.One;

            comPort.DataReceived += new SerialDataReceivedEventHandler( comPort_DataReceived );
            comPort.ErrorReceived += new SerialErrorReceivedEventHandler( comPort_ErrorReceived );
        }

        #endregion

        #region  方法

        /// <summary>
        /// 打开端口
        /// </summary>
        /// <returns></returns>
        public void OpenPort( )
        {
            if (comPort.IsOpen) comPort.Close( );

            comPort.PortName = _portName;
            comPort.BaudRate = (int)_baudRate;
            comPort.Parity = _parity;
            comPort.DataBits = (int)_dataBits;
            comPort.StopBits = _stopBits;
            try
            {
                comPort.Open( );
            }
            catch (Exception e)
            {
                throw new Exception( "unable open serial port" + e.Message );
            }
        }

        /// <summary>
        /// 关闭端口
        /// </summary>
        public void ClosePort( )
        {
            if (comPort.IsOpen) comPort.Close( );
        }

        /// <summary>
        /// 数据接收处理
        /// </summary>
        private void comPort_DataReceived( object sender, SerialDataReceivedEventArgs e )
        {
            //禁止接收事件时直接退出
            if (ReceiveEventFlag) return;

            //读取数据
            List<byte> byteData = ReadData( );

            //字符转换
            string readString = System.Text.Encoding.Default.GetString( byteData.ToArray( ), 0, byteData.Count );

            //触发整条记录的处理
            if (DataReceived != null)
            {
                DataReceived( readString );
            }
        }

        /// <summary>
        /// 错误处理函数
        /// </summary>
        private void comPort_ErrorReceived( object sender, SerialErrorReceivedEventArgs e )
        {
            if (Error != null)
            {
                Error( sender, e );
            }
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <returns>读取的数据</returns>
        private List<byte> ReadData()
        {
            List<byte> byteData = new List<byte>( );
            bool bEndFlag = false;      //是否检测到结束符号
            bool bStartFlag = false;    //是否检测到开始符号
            while (comPort.BytesToRead > 0 || !bStartFlag || !bEndFlag)
            {
                byte[] readBuffer = new byte[comPort.ReadBufferSize + 1];
                int count = comPort.Read( readBuffer, 0, comPort.ReadBufferSize );
                for (int i = 0; i < count; i++)
                {
                    //寻找到起始位时，将开始标志位设置为true，结束标志位设置为false
                    if (readBuffer[i] == BeginByte)
                    {
                        bStartFlag = true;
                        bEndFlag = false;
                        byteData.Clear( );
                        continue;  //检测到开始符号，后面就不执行了
                    }

                    //寻找到结束符
                    if (readBuffer[i] == EndByte)
                    {
                        bEndFlag = true;
                        continue;  //检测到结束符号，后面就不执行了
                    }

                    //只有在开始标志位为true，结束标志位为false时添加数据
                    if (bStartFlag == true && bEndFlag == false)
                        byteData.Add( readBuffer[i] );                   
                }
            }
            return byteData;
        }

        #region 数据写入操作

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="msg"></param>
        public void WriteData(string msg)
        {
            if (!(comPort.IsOpen)) comPort.Open();

            comPort.Write(msg);
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="msg">写入端口的字节数组</param>
        public void WriteData(byte[] msg)
        {
            if (!(comPort.IsOpen)) comPort.Open();

            comPort.Write(msg, 0, msg.Length);
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="msg">包含要写入端口的字节数组</param>
        /// <param name="offset">参数从0字节开始的字节偏移量</param>
        /// <param name="count">要写入的字节数</param>
        public void WriteData(byte[] msg, int offset, int count)
        {
            if (!(comPort.IsOpen)) comPort.Open();

            comPort.Write(msg, offset, count);
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
            if (!(comPort.IsOpen)) comPort.Open();

            ReceiveEventFlag = true;        //关闭接收事件
            comPort.DiscardInBuffer();      //清空接收缓冲区                 
            comPort.Write(SendData, 0, SendData.Length);

            int num = 0, ret = 0;
            while (num++ < Overtime)
            {
                if (comPort.BytesToRead >= ReceiveData.Length) break;
                System.Threading.Thread.Sleep(1);
            }

            if (comPort.BytesToRead >= ReceiveData.Length)
            {
                ret = comPort.Read(ReceiveData, 0, ReceiveData.Length);
            }

            ReceiveEventFlag = false;       //打开事件
            return ret;
        }
        #endregion

        #endregion
    }
}
