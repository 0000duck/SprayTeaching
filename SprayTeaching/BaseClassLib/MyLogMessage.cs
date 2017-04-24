using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SprayTeaching.BaseClassLib
{
    // 公共委托事件
    public delegate void DataReceivedEventHandler(string strDataReceive);        // 对接收的数据进行处理
    public delegate void UpdateLogContentEventHandler(string strParameter);      // 更新日志内容的delegate变量声明    
    public delegate void UpdateRobotParameterEventHandler(Dictionary<string, object> dblParameter);  // 更新机器人参数的delegate变量声明  
    public delegate void UpdateSocketIsConnectedEventHandler(bool bolParameter);    // 更新socket通信是否连接
    public delegate void UpdateSerialPortIsOpenedEventHandler(bool bolParameter);   // 更新串口是否打开

    public class MyLogMessage
    {
        public string LogTime { get; set; }
        public string LogMessage { get; set; }
    }

    public static class MyConstString
    {
        public const string IMG_SERIAL_PORT_CONNECT = "../MyImage/imgSerialPortConnect.png";        // 串口断开的图形
        public const string IMG_SERIAL_PORT_DISCONNECT = "../MyImage/imgSerialPortDisconnect.png";        // 串口连通的图形
        public const string IMG_SOCKET_CONNECT = "../MyImage/imgSocketConnect.png";
        public const string IMG_SOCKET_DISCONNECT = "../MyImage/imgSocketDisconnect.png";
    }
}
