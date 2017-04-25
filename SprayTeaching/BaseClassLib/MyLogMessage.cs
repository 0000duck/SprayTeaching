﻿using System;
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
        public const string IMG_SERIAL_PORT_CONNECT = "../MyImage/imgSerialPortConnect.png";                // 串口断开的图形
        public const string IMG_SERIAL_PORT_DISCONNECT = "../MyImage/imgSerialPortDisconnect.png";          // 串口连通的图形
        public const string IMG_SOCKET_CONNECT = "../MyImage/imgSocketConnect.png";                         // socket连通图标
        public const string IMG_SOCKET_DISCONNECT = "../MyImage/imgSocketDisconnect.png";                   // socket断开图标

        public const string ROBOTDK_ROBOT_FILE_NAME_ABB_IRB_1200_7_0_7 = "E:/install/RoboDK/Library/ABB-IRB-1200-7-0-7.robot";          // 机器人模型ABB-IRB-1200-7-0-7
        public const string ROBOTDK_ROBOT_FILE_NAME_ABB_IRB_120_3_0_6 = "E:/install/RoboDK/Library/ABB_IRB_120_3_0_6.robot";            // 机器人模型ABB_IRB_120_3_0_6
        public const string ROBOTDK_ROBOT_FILE_NAME_ABB_IRB_6700_155_2_85 = "E:/install/RoboDK/Library/ABB_IRB_6700_155_2_85.robot";    // 机器人模型ABB_IRB_6700_155_2_85
        public const string ROBOTDK_ROBOT_FILE_NAME_UR10 = "E:/install/RoboDK/Library/UR10.robot";                                      // 机器人模型UR10
        public const string ROBOTDK_ROBOT_FILE_NAME_UR3 = "E:/install/RoboDK/Library/UR3.robot";                                        // 机器人模型UR3

        public const string ROBOTDK_ROBOT_NAME_ABB_IRB_1200_7_0_7 = "ABB IRB 1200-7/0.7";                  // 机器人模型ABB-IRB-1200-7-0-7的名字
        public const string ROBOTDK_ROBOT_NAME_ABB_IRB_120_3_0_6 = "ABB IRB 120-3/0.6";                    // 机器人模型ABB_IRB_120_3_0_6的名字
        public const string ROBOTDK_ROBOT_NAME_ABB_IRB_6700_155_2_85 = "ABB IRB 6700-155/2.85";            // 机器人模型ABB_IRB_6700_155_2_85的名字
        public const string ROBOTDK_ROBOT_NAME_UR10 = "UR10";                                               // 机器人模型UR10的名字
        public const string ROBOTDK_ROBOT_NAME_UR3 = "UR3";                                                 // 机器人模型UR3的名字
    }
}
