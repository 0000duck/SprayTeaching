//**************************************************************************************************//
// 此处是软件核心，主要是用于数据更新
//**************************************************************************************************//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

using SprayTeaching.Model;
using SprayTeaching.BaseClassLib;
using SprayTeaching.MyAllClass;
using System.Threading;
using Microsoft.Win32;
using System.Windows;
using System.IO;

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
        private MyDataMessage _myDataMessage;               // 数据消息的对象
        private MyConfigFileINI _myConfigFileINI;           // 配置文件的对象
        private MyCommunicate _myCommunicate;               // 数据通信对象
        private MyRoboDKExtension _myRoboDKExtension;       // RoboDK的对象
        private MyErrorOperate _myErrorOperate;             // 错误操作消息对象
        private MyRobotFile _myRobotFile;                   // 机器人文件对象

        private delegate void UpdateDifferentThreadParameterEventHandler(object strParameter);      // 更新不同线程的参数的delegate变量声明，主要用于对绑定控件数据的更改 
        private UpdateDifferentThreadParameterEventHandler _dlgtUpdateDifThreadParam;               // 更新不同线程的数据

        public DataModel MainDataModel
        {
            get { return _mainDataModel; }
            set { _mainDataModel = value; RaisePropertyChanged("MainDataModel"); }
        }

        #endregion

        #region  构造函数

        /// <summary>
        /// 构造函数中初始化顺序很有讲究，log必须最前面，DataMessage必须在ConfigFileINI的前面，ConfigFileINI要更新DataMessage中的数据
        /// _myErrorOperate和_myLogObject之间存在嵌套，已经采用条件判断来避免初始化出现问题，嵌套地点为UpdateLogContentHandler()
        /// </summary>
        public ConnectModelVM( )
        {
            this._mainDataModel = new DataModel();                                                                                      // 所有数据的对象
            this._dlgtUpdateDifThreadParam = new UpdateDifferentThreadParameterEventHandler(UpdateDifferentThreadParam);                // 用于更新不同线程的参数,目前用于更新listview的绑定的数据                      

            this._myLogObject = new MyLog(this._mainDataModel.LogFilePath);                                                             // 日志更新的对象
            this._myLogObject.UpdateLogContent += new UpdateLogContentEventHandler(UpdateLogContentHandler);                            // 日志中线程初始化的时候写日志，自己写自己，因为要将数据写到listview中，只能统一采用外部方式

            this._myErrorOperate = new MyErrorOperate();                                                                                // 错误操作的对象
            this._myErrorOperate.UpdateLogContent += new UpdateLogContentEventHandler(UpdateLogContentHandler);                         // 错误操作写日志
            this._myErrorOperate.UpdateErrorOperateMessage += new Action<object>(UpdateErrorOperateMessageHandler);                     // 错误操作消息更新

            this._myDataMessage = new MyDataMessage();                                                                                  // DataMessage的对象，存放接收数据和发送数据
            this._myDataMessage.UpdateLogContent += new UpdateLogContentEventHandler(UpdateLogContentHandler);
            this._myDataMessage.UpdateSampleInform += new UpdateMessageSampleInformEventHandler(UpdateMessageSampleInformHandler);      // 接收采样频率和周期的处理
            this._myDataMessage.UpdateAxisAddress += new UpdateMessageAxisAddressEventHandler(UpdateMessageAxisAddressHandler);         // 接收查询所有轴地址的处理
            this._myDataMessage.UpdateMcuIsConnected += new UpdateMessageStateInformEventHandler(UpdateMessageMcuIsConnectedHandler);   // 接收查询设备是否连接的处理
            this._myDataMessage.UpdateMcuIsReady += new UpdateMessageStateInformEventHandler(UpdateMessageMcuSampleIsReadyHandler);     // 接收查询设备采样是否准备就绪的处理
            this._myDataMessage.UpdateAxisData += new UpdateMessageAxisDataEventHandler(UpdateMessageAxisDataHandler);                  // 接收采集的数据
            this._myDataMessage.UpdateAbsoluteAxisAngle += new Action<object>(UpdateAbsoluteAxisAngleHandler);                          // 

            this._myConfigFileINI = new MyConfigFileINI(this._mainDataModel.ConfigFileAddress);                                         // 配置文件的对象
            this._myConfigFileINI.UpdateLogContent += new UpdateLogContentEventHandler(UpdateLogContentHandler);                        // 配置文件写日志
            this._myConfigFileINI.UpdateConfigParameter += new UpdateConfigFileParameterEventHandler(GetConfigParameterHandler);        // 配置文件更新参数

            this._myCommunicate = new MyCommunicate();                                                                                  // 数据通信的对象
            this._myCommunicate.DataReceived += new DataReceivedEventHandler(ReceiveDataHandler);                                       // 数据通信接收数据的处理
            this._myCommunicate.UpdateLogContent += new UpdateLogContentEventHandler(UpdateLogContentHandler);                          // 数据通信写日志
            this._myCommunicate.UpdateSerialPortIsOpened += new UpdateSerialPortIsOpenedEventHandler(UpdateSerialPortIsOpenedHandler);  // 更新串口通断状态信息
            this._myCommunicate.UpdateSocketIsConnected += new UpdateSocketIsConnectedEventHandler(UpdateSocketIsConnectedHandler);     // 更新socket通断的状态信息

            this._myRoboDKExtension = new MyRoboDKExtension();                                                                          // RoboDK的对象
            this._myRoboDKExtension.UpdateLogContent += new UpdateLogContentEventHandler(UpdateLogContentHandler);                      // RoboDK写日志
            this._myRoboDKExtension.UpdateRobotParameter += new UpdateRobotParameterEventHandler(UpdateRobotParameterHandler);          // 更新机器人参数
            this._myRoboDKExtension.UpdateAddTargetPointState += new Action<object>(UpdateAddTargetPointStateHandler);                  // 向RoboDK中添加程序的运行状态
            this._myRoboDKExtension.UpdateRobotRunningState += new Action<object>(UpdateRobotRunningStateHandler);                      // 更新机器人的运动状态

            this._myRobotFile = new MyRobotFile();                                                                                      // 机器人文件的对象
            this._myRobotFile.UpdateLogContent += new UpdateLogContentEventHandler(UpdateLogContentHandler);                            // 机器人文件写日志
        }

        #endregion

        #region 方法

        #region 数据通信部分

        #region 串口相关的方法

        /// <summary>
        /// 打开或关闭串口
        /// </summary>
        public void OpenCloseSerialPortHandler( )
        {
            //更新串口参数
            this._myCommunicate.OpenCloseSerialPort(this._mainDataModel);
        }

        /// <summary>
        /// 更新串口的通断状态
        /// </summary>
        /// <param name="bolIsOpened">是否连接，true为连接，false为断开</param>
        private void UpdateSerialPortIsOpenedHandler(bool bolIsOpened)
        {
            if (bolIsOpened)
                this._mainDataModel.SerialPortIsOpenedImage = MyConstString.IMG_SERIAL_PORT_CONNECT;
            else
                this._mainDataModel.SerialPortIsOpenedImage = MyConstString.IMG_SERIAL_PORT_DISCONNECT;

            this._mainDataModel.SerialPortIsOpened = bolIsOpened;
        }

        #endregion

        #region Scoket相关的方法

        /// <summary>
        /// 打开或者关闭socket事件处理
        /// </summary>
        public void OpenCloseSocketHandler( )
        {
            this._myCommunicate.OpenCloseSocket(this._mainDataModel);
        }

        /// <summary>
        /// 更新socket的连接状态事件处理
        /// </summary>
        /// <param name="bolIsConnected"></param>
        private void UpdateSocketIsConnectedHandler(bool bolIsConnected)
        {
            if (bolIsConnected)
                this._mainDataModel.SocketIsConnectedImage = MyConstString.IMG_SOCKET_CONNECT;
            else
                this._mainDataModel.SocketIsConnectedImage = MyConstString.IMG_SOCKET_DISCONNECT;

            this._mainDataModel.SocketIsConnected = bolIsConnected;
        }

        /// <summary>
        /// socket发送数据事件处理
        /// </summary>
        public void SocketSendDataHandler( )
        {
            //this._mySocketCom.SendDataHandler();
        }
        #endregion

        #region 数据发送部分
        /// <summary>
        /// 发送数据处理部分
        /// </summary>
        /// <param name="obj">发送什么数据的命令</param>
        public void SendDataHandler(object obj)
        {
            Thread thrd = new Thread(ThrdSendDataHandler);          // 采用简单的线程来发送数据，首先判断是否有设备
            thrd.IsBackground = true;                               // 设置成后台线程，在前台线程结束时，所有剩余的后台线程都会停止且不会完成    
            thrd.Name = "CommunicateSendData";                      // 设置线程的名字
            thrd.Start(obj);
        }

        /// <summary>
        /// 发送数据的线程
        /// </summary>
        /// <param name="obj">命令参数</param>
        private void ThrdSendDataHandler(object obj)
        {
            string strCommand = (string)obj;

            // 任何指令在发送前先确定数据采集是否正在进行，若是，则提示要执行其他操作，先停止采样
            // 只有停止命令才能停止数据采集
            if (this._mainDataModel.IsSampleDataRunning && strCommand != "StopSampleData")
            {
                this.UpdateLogContentHandler("正在进行数据采样，若要执行其他操作，先停止数据采样", 1);
                return;
            }

            // 发送实际的命令            
            byte[] byteData = this._myDataMessage.SendDataMessage(strCommand, this._mainDataModel);
            bool bolIsSuccess = this._myCommunicate.SendDataHandler(byteData);       // 返回是否发送成功

            if (bolIsSuccess)
            {
                this._myDataMessage.TipMessageOperate(strCommand);          // 对完成的操作进行提示
                this.UpdateSampleState(strCommand);     // 更新采样的状态，只对开始采样和结束采样起作用
            }
                
        }

        /// <summary>
        /// 更新采样状态，只对开始采样和结束采样起作用
        /// </summary>
        /// <param name="strCommand">命令</param>
        private void UpdateSampleState(string strCommand)
        {
            if (strCommand == "StartSampleData")
            {
                this._mainDataModel.IsSampleDataRunning = true;                 // 更新是否正在运行数据采样
                // 开启添加角度信息，用于将运动轨迹写入文件
                this._myRobotFile.StartAddAngleMessageHandler(this._mainDataModel.LocationRobotMoveFileName, this._mainDataModel.CurrentSampleFrequent);
                return;
            }
            if (strCommand == "StopSampleData")
            {
                this._mainDataModel.DeviceIsConnected = false;              // 每次结束采集的时候都将状态信息回复到初始状态，下次采集的时候重新检查
                this._mainDataModel.DeviceSampleIsReady = false;            // 每次结束采集的时候都将状态信息回复到初始状态，下次采集的时候重新检查
                this._mainDataModel.IsSampleDataRunning = false;            // 更新是否正在运行数据采样
                // 结束添加角度信息，用于将运动轨迹写入文件
                this._myRobotFile.StopAddAngleMessageHandler(this._mainDataModel.LocationRobotMoveFileName, this._mainDataModel.CurrentSampleFrequent);
                return;
            }
        }

        /// <summary>
        /// 查询硬件设备是否连接
        /// </summary>
        /// <returns>连接状态，true为连接，false为断开</returns>
        private bool QueryDeviceIsConnected( )
        {
            bool bolIsConnected = this._mainDataModel.DeviceIsConnected;
            int intCount = 0;

            // 检查硬件设备是否连接，检查时间为1s            
            while (!bolIsConnected)
            {
                if (intCount >= 10)
                    break;

                // 查询命令只发送一次
                if (intCount == 0)
                {
                    byte[] byteData = this._myDataMessage.SendDataMessage("QueryDeviceConnect", this._mainDataModel);
                    if (!this._myCommunicate.SendDataHandler(byteData))
                        return false;
                }
                Thread.Sleep(10);
                bolIsConnected = this._mainDataModel.DeviceIsConnected;
                intCount++;
            }

            if (!bolIsConnected)
                UpdateLogContentHandler("外部设备无法连接，请检查.", 1);

            return bolIsConnected;
        }

        /// <summary>
        /// 查询设备采样是否准备就绪
        /// </summary>
        /// <returns>状态，true为准备就绪，false为没有准备好</returns>
        private bool QueryDeviceSampleIsReady( )
        {
            bool bolIsReady = this._mainDataModel.DeviceSampleIsReady;
            int intCount = 0;

            // 检查数据采样是否准备就绪，检查时间为1s            
            while (!bolIsReady)
            {
                if (intCount >= 10)
                    break;

                // 查询命令只发送一次
                if (intCount == 0)
                {
                    byte[] byteData = this._myDataMessage.SendDataMessage("QueryDeviceSampleReady", this._mainDataModel);
                    this._myCommunicate.SendDataHandler(byteData);
                }
                Thread.Sleep(100);
                bolIsReady = this._mainDataModel.DeviceSampleIsReady;
                intCount++;
            }

            if (!bolIsReady)
                UpdateLogContentHandler("外部设备数据采集还未准备好.", 1);

            return bolIsReady;
        }


        #region 轴地址清零

        /// <summary>
        /// 将轴地址清零
        /// </summary>
        public void ClearAllAxisAddressHandler( )
        {
            Thread thrd = new Thread(ThrdClearAllAxisAddressHandler);           // 采用简单的线程,将轴地址清零
            thrd.IsBackground = true;                                           // 设置成后台线程，在前台线程结束时，所有剩余的后台线程都会停止且不会完成    
            thrd.Name = "ClearAllAxisAddress";                                  // 设置线程的名字
            thrd.Start();
        }

        /// <summary>
        /// 将轴地址清零的线程
        /// </summary>
        private void ThrdClearAllAxisAddressHandler( )
        {
            // 查询硬件设备是否连接
            if (!this.QueryDeviceIsConnected())
                return;
            System.Windows.MessageBoxResult dr = System.Windows.MessageBox.Show("是否期望将所有地址清零？", "提示",
                System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Question);
            if (dr == System.Windows.MessageBoxResult.Cancel)
                return;

            this._mainDataModel.SetAxis1Address = 0x00;
            this._mainDataModel.SetAxis2Address = 0x00;
            this._mainDataModel.SetAxis3Address = 0x00;
            this._mainDataModel.SetAxis4Address = 0x00;
            this._mainDataModel.SetAxis5Address = 0x00;
            this._mainDataModel.SetAxis6Address = 0x00;

            for (int i = 0; i < 6; i++)
            {
                // 发送实际的命令            
                byte[] byteData = this._myDataMessage.SendDataMessage("ModifyAxis" + (i + 1).ToString() + "Address", this._mainDataModel);
                this._myCommunicate.SendDataHandler(byteData);
                Thread.Sleep(100);
            }
        }

        #endregion

        /// <summary>
        /// 开始进行数据采样
        /// </summary>
        public void StartSampleDataHandler( )
        {
            // 查询硬件设备是否连接,数据采样是否准备就绪，若没有，则先检查
            if (!this._mainDataModel.DeviceIsConnected || !this._mainDataModel.DeviceSampleIsReady)
                if (!this.QueryDeviceIsConnected())
                    return;

            // 在开始采样之前先获取下采样频率
            this.SendDataHandler("QuerySampleFrequent");
            Thread.Sleep(100);
            this.SendDataHandler("StartSampleData");
        }

        /// <summary>
        /// 停止数据采样
        /// </summary>
        public void StopSampleDataHandler( )
        {
            this.SendDataHandler("StopSampleData");
        }

        #endregion

        #region 数据接收部分

        /// <summary>
        /// 对串口中接收的数据进行处理
        /// </summary>
        /// <param name="byteDataReceiveMessage">接收的数据</param>
        private void ReceiveDataHandler(byte[] byteDataReceiveMessage)
        {
            this._myDataMessage.ReceiveDataMessage(byteDataReceiveMessage);             // 将接收的数据传入DataMessage对象中
            string readString = BitConverter.ToString(byteDataReceiveMessage);          // 将字节型数组转换为字符串型
            this._mainDataModel.SerialPortDataReceived = readString + "\r\n";
        }
        #endregion

        #region 通信方式选择
        public void SelectCommunicateWayHandler(object obj)
        {
            this._myCommunicate.SelectCommunicateWayHandler(obj, this._mainDataModel);
        }
        #endregion

        #endregion

        #region  RoboDK相关的方法

        #region RoboDK接收机器人参数部分
        /// <summary>
        /// 更新机器人的参数
        /// </summary>
        /// <param name="dblJoints">6个关节角度</param>
        private void UpdateRobotParameterHandler(Dictionary<string, object> dicData)
        {
            this.UpdateRobotJoints(dicData["RobotJoint1"], dicData["RobotJoint2"], dicData["RobotJoint3"], dicData["RobotJoint4"], dicData["RobotJoint5"], dicData["RobotJoint6"]);
            this.UpdateRobotPoses(dicData["RobotPoseX"], dicData["RobotPoseY"], dicData["RobotPoseZ"], dicData["RobotPoseU"], dicData["RobotPoseV"], dicData["RobotPoseW"]);
            this.UpdateRobotMoveSpeed(dicData["RobotMoveSpeed"]);
        }

        /// <summary>
        /// 更新机器人关节角度
        /// </summary>
        /// <param name="objJoint1">关节1</param>
        /// <param name="objJoint2">关节2</param>
        /// <param name="objJoint3">关节3</param>
        /// <param name="objJoint4">关节4</param>
        /// <param name="objJoint5">关节5</param>
        /// <param name="objJoint6">关节6</param>
        private void UpdateRobotJoints(object objJoint1, object objJoint2, object objJoint3, object objJoint4, object objJoint5, object objJoint6)
        {
            double dblJoint1 = (double)objJoint1;
            double dblJoint2 = (double)objJoint2;
            double dblJoint3 = (double)objJoint3;
            double dblJoint4 = (double)objJoint4;
            double dblJoint5 = (double)objJoint5;
            double dblJoint6 = (double)objJoint6;

            this._mainDataModel.RobotJoint1 = Math.Round(dblJoint1, 2);
            this._mainDataModel.RobotJoint2 = Math.Round(dblJoint2, 2);
            this._mainDataModel.RobotJoint3 = Math.Round(dblJoint3, 2);
            this._mainDataModel.RobotJoint4 = Math.Round(dblJoint4, 2);
            this._mainDataModel.RobotJoint5 = Math.Round(dblJoint5, 2);
            this._mainDataModel.RobotJoint6 = Math.Round(dblJoint6, 2);
        }

        /// <summary>
        /// 更新机器人6个直角坐标系的值
        /// </summary>
        /// <param name="objPoseX">X轴</param>
        /// <param name="objPoseY">Y轴</param>
        /// <param name="objPoseZ">Z轴</param>
        /// <param name="objPoseU">U轴</param>
        /// <param name="objPoseV">V轴</param>
        /// <param name="objPoseW">W轴</param>
        private void UpdateRobotPoses(object objPoseX, object objPoseY, object objPoseZ, object objPoseU, object objPoseV, object objPoseW)
        {
            double dblPoseX = (double)objPoseX;
            double dblPoseY = (double)objPoseY;
            double dblPoseZ = (double)objPoseZ;
            double dblPoseU = (double)objPoseU;
            double dblPoseV = (double)objPoseV;
            double dblPoseW = (double)objPoseW;

            this._mainDataModel.RobotRectangularX = Math.Round(dblPoseX, 2);
            this._mainDataModel.RobotRectangularY = Math.Round(dblPoseY, 2);
            this._mainDataModel.RobotRectangularZ = Math.Round(dblPoseZ, 2);
            this._mainDataModel.RobotRectangularU = Math.Round(dblPoseU, 2);
            this._mainDataModel.RobotRectangularV = Math.Round(dblPoseV, 2);
            this._mainDataModel.RobotRectangularW = Math.Round(dblPoseW, 2);
        }

        /// <summary>
        /// 机器人的运动速度
        /// </summary>
        /// <param name="dblMoveSpeed">速度</param>
        private void UpdateRobotMoveSpeed(object objMoveSpeed)
        {
            double dblMoveSpeed = (double)objMoveSpeed;
            this._mainDataModel.RobotMoveSpeed = dblMoveSpeed;
        }
        #endregion

        #region 机器人模型切换
        /// <summary>
        /// 选择机器人模型
        /// </summary>
        /// <param name="objParameter"></param>
        public void SelectRobotModelHandler(object objParameter)
        {
            if (objParameter == null)
                return;
            this._myRoboDKExtension.SelectRobotModelHandler(objParameter);
        }
        #endregion

        #region 向添加RoboDK添加程序，运行程序，停止程序，生成机器人程序
        /// <summary>
        /// 生成机器人程序
        /// </summary>
        /// <param name="obj"></param>
        public void CreateRoboDKProgramHandler(object obj = null)
        {
            OpenFileDialog ofdDlg = new OpenFileDialog();
            ofdDlg.Title = "请选择指定文件";
            ofdDlg.Filter = "所有文件(*.st)|*.st";
            ofdDlg.InitialDirectory = System.IO.Directory.GetCurrentDirectory() + ".\\RobotProgram";
            if (ofdDlg.ShowDialog() == true)
            {
                string strFileAddress = ofdDlg.FileName;
                this._myRoboDKExtension.CreateRoboDKProgram(strFileAddress);
            }
        }

        /// <summary>
        /// 更新向RoboDK中添加目标点的状态
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateAddTargetPointStateHandler(object obj)
        {
            int intState = (int)obj;
            this._mainDataModel.RunningAddTargetState = intState;

            // 如果是0和100，则不显示进程条
            if (this._mainDataModel.RunningAddTargetState == 0)
            {
                this._mainDataModel.IsRunningAddTarget = false;
            }
            else if (this._mainDataModel.RunningAddTargetState == 100)
            {
                this._mainDataModel.IsRunningAddTarget = false;
                this._mainDataModel.RunningAddTargetState = 0;
            }
            else
                this._mainDataModel.IsRunningAddTarget = true;
        }

        /// <summary>
        /// 运行RoboDK中的机器人程序
        /// </summary>
        /// <param name="obj"></param>
        public void RunRoboDKProgramHandler(object obj = null)
        {
            this._myRoboDKExtension.RunRoboDKProgramHandler();
        }

        /// <summary>
        /// 停止RoboDK中的机器人程序
        /// </summary>
        /// <param name="obj"></param>
        public void StopRoboDKProgramHandler(object obj=null)
        {
            this._myRoboDKExtension.StopRoboDKProgramHandler();
        }

        /// <summary>
        /// 生成实际机器人程序
        /// </summary>
        public void MakeRobotProgramHandler()
        {
            string strFileAddress = this._myRobotFile.RobotFileAddress;
            strFileAddress = System.IO.Directory.GetCurrentDirectory() + ".\\RobotProgram" + "\\" + Path.GetFileNameWithoutExtension(strFileAddress) + ".tid";
            this._myRoboDKExtension.MakeRobotProgramHandler(strFileAddress);
        }

        #endregion

        #region 机器人的运动状态

        /// <summary>
        /// 更新机器人的运动状态
        /// </summary>
        /// <param name="obj">运动状态</param>
        public void UpdateRobotRunningStateHandler(object obj)
        {
            string strState = (string)obj;
            this._mainDataModel.RobotRunningState = strState;
            if (strState == "运行")
                this._mainDataModel.IsRobotRunning = true;
            else
                this._mainDataModel.IsRobotRunning = false;

        }
        #endregion

        #endregion

        #region  日志相关的方法

        /// <summary>
        /// 更新日志
        /// </summary>
        /// <param name="strMessage">日志消息</param>
        /// <param name="intType">消息类型，默认为0，为1错误操作消息</param>
        private void UpdateLogContentHandler(string strMessage, int intType = 0)
        {
            // intType为1表示这条消息是一条错误操作消息
            // 由于_myErrorOperate和_myLogObject之间存在嵌套，为避免初始化的时候出现文件，用条件判断_myErrorOperate是否被初始化来避免
            if (this._myErrorOperate != null && intType == 1)
            {
                this._myErrorOperate.AddErrorOperateMessage(strMessage);                                    // 识别错误操作的消息，并进行另外处理
                strMessage = "--" + strMessage;
            }

            string strTime = this._myLogObject.AddLogMessage(strMessage);                                   // 在日志中添加消息
            MyLogMessage myLogMessage = new MyLogMessage() { LogTime = strTime, LogMessage = strMessage };
            App.Current.Dispatcher.BeginInvoke(this._dlgtUpdateDifThreadParam, myLogMessage);               // 使用调度者dispatcher来异步处理多线程，目前用于更新listview绑定的数据
        }

        /// <summary>
        /// 异步更新日志数据，目前是用于更新listview绑定的数据
        /// </summary>
        /// <param name="objParameter"></param>
        private void UpdateDifferentThreadParam(object objParameter)
        {
            MyLogMessage myLogMessage = objParameter as MyLogMessage;
            this._mainDataModel.LogDataList.Insert(0, myLogMessage);
        }
        #endregion

        #region  关闭窗口所需要的资源

        /// <summary>
        /// 关闭所有资源
        /// </summary>
        public void CloseAllResourceHandler( )
        {
            this.CloseStopSampleData();             // 先关闭数据采集，避免关掉软件后，硬件设备还在发送数据
            this._myLogObject.Close();              // 关闭日志相关的资源
            this._myCommunicate.Close();            // 关闭通信的资源
            this._myRoboDKExtension.Close();        // 关闭与RoboDK相关的资源
            this._myDataMessage.Close();            // 关闭与数据消息相关的资源
            this._myConfigFileINI.Close(this._mainDataModel);          // 关闭配置文件的资源
            this._myErrorOperate.Close();           // 关闭错误操作的资源
            this._myRobotFile.Close();              // 关闭机器人文件操作的所有资源
        }

        /// <summary>
        /// 在关闭之前，先将采集数据停止掉
        /// </summary>
        private void CloseStopSampleData( )
        {
            if (this._myCommunicate.IsCommunicateConnected())
            {
                this.StopSampleDataHandler();
                Thread.Sleep(10);                   // 延迟10ms，避免在发送数据过程中通信通道断开了
            }

        }
        #endregion

        #region 配置文件相关的方法

        /// <summary>
        /// 获取配置文件中的参数
        /// </summary>
        /// <param name="obj">传入的参数</param>
        private void GetConfigParameterHandler(object obj)
        {
            object[] objPar = (object[])obj;
            double[] dblCalibrateAngles = (double[])objPar[0];
            double[] dblCalibrateDirectioins = (double[])objPar[1];
            string[] strCommunicates = (string[])objPar[2];

            // 更新DataMessage对象中的标定角度
            this._myDataMessage.CalibrateAngles = dblCalibrateAngles;
            this._myDataMessage.CalibrateDirections = dblCalibrateDirectioins;

            // 更新数据区中的标定轴角度
            this._mainDataModel.RobotCalibrateAngle1 = dblCalibrateAngles[0];
            this._mainDataModel.RobotCalibrateAngle2 = dblCalibrateAngles[1];
            this._mainDataModel.RobotCalibrateAngle3 = dblCalibrateAngles[2];
            this._mainDataModel.RobotCalibrateAngle4 = dblCalibrateAngles[3];
            this._mainDataModel.RobotCalibrateAngle5 = dblCalibrateAngles[4];
            this._mainDataModel.RobotCalibrateAngle6 = dblCalibrateAngles[5];

            // 初始化待设定的标定轴角度
            this._mainDataModel.SetAbsoluteAngle1 = dblCalibrateAngles[0];
            this._mainDataModel.SetAbsoluteAngle2 = dblCalibrateAngles[1];
            this._mainDataModel.SetAbsoluteAngle3 = dblCalibrateAngles[2];
            this._mainDataModel.SetAbsoluteAngle4 = dblCalibrateAngles[3];
            this._mainDataModel.SetAbsoluteAngle5 = dblCalibrateAngles[4];
            this._mainDataModel.SetAbsoluteAngle6 = dblCalibrateAngles[5];

            // 更新数据区中的标定关节方向
            this._mainDataModel.RobotCalibrateDirection1 = dblCalibrateDirectioins[0];
            this._mainDataModel.RobotCalibrateDirection2 = dblCalibrateDirectioins[1];
            this._mainDataModel.RobotCalibrateDirection3 = dblCalibrateDirectioins[2];
            this._mainDataModel.RobotCalibrateDirection4 = dblCalibrateDirectioins[3];
            this._mainDataModel.RobotCalibrateDirection5 = dblCalibrateDirectioins[4];
            this._mainDataModel.RobotCalibrateDirection6 = dblCalibrateDirectioins[5];

            // 初始化待设定的标定轴方向
            this._mainDataModel.SetCalibrateAxis1Direction = dblCalibrateDirectioins[0] > 0 ? true : false;
            this._mainDataModel.SetCalibrateAxis2Direction = dblCalibrateDirectioins[1] > 0 ? true : false;
            this._mainDataModel.SetCalibrateAxis3Direction = dblCalibrateDirectioins[2] > 0 ? true : false;
            this._mainDataModel.SetCalibrateAxis4Direction = dblCalibrateDirectioins[3] > 0 ? true : false;
            this._mainDataModel.SetCalibrateAxis5Direction = dblCalibrateDirectioins[4] > 0 ? true : false;
            this._mainDataModel.SetCalibrateAxis6Direction = dblCalibrateDirectioins[5] > 0 ? true : false;

            // 初始化通信相关的IP地址和端口号
            this._mainDataModel.SocketIPAddress = strCommunicates[0];
            this._mainDataModel.SocketPortNum = int.Parse(strCommunicates[1]);
        }

        #endregion

        #region  DataMessage数据处理部分

        /// <summary>
        /// 更新采样的信息，采样频率和采样周期
        /// </summary>
        /// <param name="intSampleFrequent">采样频率</param>
        /// <param name="intSampleCycle">采样周期</param>
        private void UpdateMessageSampleInformHandler(int intSampleFrequent, int intSampleCycle)
        {
            this._mainDataModel.CurrentSampleFrequent = intSampleFrequent;
            this._mainDataModel.CurrentSampleCycle = intSampleCycle;
        }

        /// <summary>
        /// 更新轴地址信息
        /// </summary>
        /// <param name="byteAxisAddress">轴地址</param>
        private void UpdateMessageAxisAddressHandler(byte[] byteAxisAddress)
        {
            this._mainDataModel.CurrentAxis1Address = byteAxisAddress[0];
            this._mainDataModel.CurrentAxis2Address = byteAxisAddress[1];
            this._mainDataModel.CurrentAxis3Address = byteAxisAddress[2];
            this._mainDataModel.CurrentAxis4Address = byteAxisAddress[3];
            this._mainDataModel.CurrentAxis5Address = byteAxisAddress[4];
            this._mainDataModel.CurrentAxis6Address = byteAxisAddress[5];
        }

        /// <summary>
        /// 查询设备是否存在，设备是否连接
        /// </summary>
        /// <param name="bolIsConnected"></param>
        private void UpdateMessageMcuIsConnectedHandler(bool bolIsConnected)
        {
            this._mainDataModel.DeviceIsConnected = bolIsConnected;
        }

        /// <summary>
        /// 查询设备数据采样是否准备就绪
        /// </summary>
        /// <param name="bolIsReady"></param>
        private void UpdateMessageMcuSampleIsReadyHandler(bool bolIsReady)
        {
            this._mainDataModel.DeviceSampleIsReady = bolIsReady;
        }

        /// <summary>
        /// 更新各个轴的相对角度值，也是需要发送给roboDK的关节角度值，同时将这些角度值保存并写入文件中
        /// </summary>
        /// <param name="dblAxisAngles"></param>
        private void UpdateMessageAxisDataHandler(double[] dblAxisAngles)
        {

            this._myRoboDKExtension.AddRobotMoveMessage(dblAxisAngles);
            this._myRobotFile.AddAngleMessageHandler(dblAxisAngles);
        }

        /// <summary>
        /// 更新各轴的绝对角度值，也是编码器采集的实际数据
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateAbsoluteAxisAngleHandler(object obj)
        {
            double[] dblAbsoluteAngles = (double[])obj;
            this._mainDataModel.CurrentAbsoluteAngle1 = dblAbsoluteAngles[0];
            this._mainDataModel.CurrentAbsoluteAngle2 = dblAbsoluteAngles[1];
            this._mainDataModel.CurrentAbsoluteAngle3 = dblAbsoluteAngles[2];
            this._mainDataModel.CurrentAbsoluteAngle4 = dblAbsoluteAngles[3];
            this._mainDataModel.CurrentAbsoluteAngle5 = dblAbsoluteAngles[4];
            this._mainDataModel.CurrentAbsoluteAngle6 = dblAbsoluteAngles[5];
        }

        #endregion

        #region 错误操作消息

        /// <summary>
        /// 更新错误操作消息
        /// </summary>
        /// <param name="obj">错误消息</param>
        private void UpdateErrorOperateMessageHandler(object obj)
        {
            this._mainDataModel.ErrorOperateMessage = (string)obj;
        }

        #endregion

        #region 原点标定部分

        /// <summary>
        /// 标定原点角度的预处理，但并未标定完成
        /// </summary>
        /// <param name="obj">指令</param>
        public void PreCalibrateOriginAngleHandler(object obj)
        {
            // 将当前的绝对角度值赋值给待设定的绝对角度值
            string strCommand = (string)obj;
            switch (strCommand)
            {
                case "Calibration1AxisOriginAngle":
                    this._mainDataModel.SetAbsoluteAngle1 = this._mainDataModel.CurrentAbsoluteAngle1;
                    break;
                case "Calibration2AxisOriginAngle":
                    this._mainDataModel.SetAbsoluteAngle2 = this._mainDataModel.CurrentAbsoluteAngle2;
                    break;
                case "Calibration3AxisOriginAngle":
                    this._mainDataModel.SetAbsoluteAngle3 = this._mainDataModel.CurrentAbsoluteAngle3;
                    break;
                case "Calibration4AxisOriginAngle":
                    this._mainDataModel.SetAbsoluteAngle4 = this._mainDataModel.CurrentAbsoluteAngle4;
                    break;
                case "Calibration5AxisOriginAngle":
                    this._mainDataModel.SetAbsoluteAngle5 = this._mainDataModel.CurrentAbsoluteAngle5;
                    break;
                case "Calibration6AxisOriginAngle":
                    this._mainDataModel.SetAbsoluteAngle6 = this._mainDataModel.CurrentAbsoluteAngle6;
                    break;
            }
        }

        /// <summary>
        /// 标定原点方向的预处理，但并未标定完成，true为正向，false为反向
        /// </summary>
        /// <param name="obj"></param>
        public void PreCalibrateOriginDirectionHandler(object obj)
        {
            // 修改各轴的标定方向，true为正向，false为反向
            string strCommand = (string)obj;
            switch (strCommand)
            {
                case "Calibration1AxisDirectoinP":
                    this._mainDataModel.SetCalibrateAxis1Direction = true;
                    break;
                case "Calibration1AxisDirectoinN":
                    this._mainDataModel.SetCalibrateAxis1Direction = false;
                    break;
                case "Calibration2AxisDirectoinP":
                    this._mainDataModel.SetCalibrateAxis2Direction = true;
                    break;
                case "Calibration2AxisDirectoinN":
                    this._mainDataModel.SetCalibrateAxis2Direction = false;
                    break;
                case "Calibration3AxisDirectoinP":
                    this._mainDataModel.SetCalibrateAxis3Direction = true;
                    break;
                case "Calibration3AxisDirectoinN":
                    this._mainDataModel.SetCalibrateAxis3Direction = false;
                    break;
                case "Calibration4AxisDirectoinP":
                    this._mainDataModel.SetCalibrateAxis4Direction = true;
                    break;
                case "Calibration4AxisDirectoinN":
                    this._mainDataModel.SetCalibrateAxis4Direction = false;
                    break;
                case "Calibration5AxisDirectoinP":
                    this._mainDataModel.SetCalibrateAxis5Direction = true;
                    break;
                case "Calibration5AxisDirectoinN":
                    this._mainDataModel.SetCalibrateAxis5Direction = false;
                    break;
                case "Calibration6AxisDirectoinP":
                    this._mainDataModel.SetCalibrateAxis6Direction = true;
                    break;
                case "Calibration6AxisDirectoinN":
                    this._mainDataModel.SetCalibrateAxis6Direction = false;
                    break;
            }
        }

        /// <summary>
        /// 标定机器人角度
        /// </summary>
        public void CalibrateRobotAngleHandler( )
        {
            // 更新数据中的标定角度值
            this._mainDataModel.RobotCalibrateAngle1 = this._mainDataModel.SetAbsoluteAngle1;
            this._mainDataModel.RobotCalibrateAngle2 = this._mainDataModel.SetAbsoluteAngle2;
            this._mainDataModel.RobotCalibrateAngle3 = this._mainDataModel.SetAbsoluteAngle3;
            this._mainDataModel.RobotCalibrateAngle4 = this._mainDataModel.SetAbsoluteAngle4;
            this._mainDataModel.RobotCalibrateAngle5 = this._mainDataModel.SetAbsoluteAngle5;
            this._mainDataModel.RobotCalibrateAngle6 = this._mainDataModel.SetAbsoluteAngle6;

            // 更新_myDataMessage的标定角度值
            double[] dblAngles = new double[6];
            dblAngles[0] = this._mainDataModel.RobotCalibrateAngle1;
            dblAngles[1] = this._mainDataModel.RobotCalibrateAngle2;
            dblAngles[2] = this._mainDataModel.RobotCalibrateAngle3;
            dblAngles[3] = this._mainDataModel.RobotCalibrateAngle4;
            dblAngles[4] = this._mainDataModel.RobotCalibrateAngle5;
            dblAngles[5] = this._mainDataModel.RobotCalibrateAngle6;
            this._myDataMessage.CalibrateAngles = dblAngles;

            // 更新配置文件
            this._myConfigFileINI.WriteFileCalibrateAngles(dblAngles);
            this.UpdateLogContentHandler("机器人角度标定成功.");
        }

        /// <summary>
        /// 标定机器人方向
        /// </summary>
        public void CalibrateRobotDirectionHandler( )
        {
            // 更新数据中的标定方向值
            this._mainDataModel.RobotCalibrateDirection1 = this._mainDataModel.SetCalibrateAxis1Direction == true ? 1.0 : -1.0;
            this._mainDataModel.RobotCalibrateDirection2 = this._mainDataModel.SetCalibrateAxis2Direction == true ? 1.0 : -1.0;
            this._mainDataModel.RobotCalibrateDirection3 = this._mainDataModel.SetCalibrateAxis3Direction == true ? 1.0 : -1.0;
            this._mainDataModel.RobotCalibrateDirection4 = this._mainDataModel.SetCalibrateAxis4Direction == true ? 1.0 : -1.0;
            this._mainDataModel.RobotCalibrateDirection5 = this._mainDataModel.SetCalibrateAxis5Direction == true ? 1.0 : -1.0;
            this._mainDataModel.RobotCalibrateDirection6 = this._mainDataModel.SetCalibrateAxis6Direction == true ? 1.0 : -1.0;

            // 更新_myDataMessage的标定方向
            double[] dblDirections = new double[6];
            dblDirections[0] = this._mainDataModel.RobotCalibrateDirection1;
            dblDirections[1] = this._mainDataModel.RobotCalibrateDirection2;
            dblDirections[2] = this._mainDataModel.RobotCalibrateDirection3;
            dblDirections[3] = this._mainDataModel.RobotCalibrateDirection4;
            dblDirections[4] = this._mainDataModel.RobotCalibrateDirection5;
            dblDirections[5] = this._mainDataModel.RobotCalibrateDirection6;
            this._myDataMessage.CalibrateDirections = dblDirections;

            // 更新配置文件
            this._myConfigFileINI.WriteFileCalibrateDirection(dblDirections);
            this.UpdateLogContentHandler("机器人方向标定成功.");
        }

        #endregion


        #endregion
    }
}
