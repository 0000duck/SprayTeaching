﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using SprayTeaching.BaseClassLib;

namespace SprayTeaching.MyAllClass
{
    public class MyRoboDKExtension
    {
        #region 所有变量

        private RoboDK.Item _rdkItemRobot;                                      // RoboDK中的机器人模型对象
        private RoboDK _rdkPlatform;                                            // RoboDK的平台

        private const bool BLOCKING_MOVE = false;

        private Thread _thrdUpdateRobotParameter;                               // 更新机器人参数的线程        
        private Object _thisLock = new object();                                // 用于锁定修改部分的数据
        private bool _bolIsUpdateRbtParThrdAlive = true;                        // 控制线程活着，true为活着，false为死亡
        private bool _bolIsThreadSuspend = false;                               // 控制线程暂停，true为暂停，false为不暂停

        private Thread _thrdDriveRobotMoveHandler;                              // 驱动机器人运动的线程
        private Queue<double[]> _queueRobotMoveMessage;                         // 临时存放日志消息的队列 
        private bool _bolIsDriveRbtMoveThrdAlive = true;                        // 控制线程活着，true为活着，false为死亡
        private AutoResetEvent _autoEvent;                                      // 控制数据消息处理的线程，控制它的睡眠和唤醒
        private Object _objLockRobotMove = new object();                        // 用于锁定修改部分的数据

        private double[] _dblRobotUpperLimits = new double[6];                  // 机器人上限位
        private double[] _dblRobotLowerLimits = new double[6];                  // 机器人下限位

        private string _strProgramName = "Program";                             // RoboDK中的程序名

        #endregion

        #region 外部事件

        public event UpdateLogContentEventHandler UpdateLogContent;             // 更新日志文件        
        public event UpdateRobotParameterEventHandler UpdateRobotParameter;     // 更新机器人参数  
        public event Action<object> UpdateAddTargetPointState;                  // 更新添加目标点的状态
        public event Action<object> UpdateRobotRunningState;                    // 更新机器人的运动状态

        #endregion

        #region 构造函数，初始化

        public MyRoboDKExtension( )
        {
            Thread thrd = new Thread(InitThreadInitParameterHandler);           // 初始化类参数用的线程，执行完就结束
            thrd.IsBackground = true;                                           // 设置成后台线程，在前台线程结束时，所有剩余的后台线程都会停止且不会完成                                  
            thrd.Name = "InitParameterHandler";                                 // 设置线程的名字
            thrd.Start();                                                       // 启动线程
        }

        /// <summary>
        /// 在初始化线程中初始化类参数，由于RoboDK在未启动时，将会有一个启动的过程，这就导致软件的延迟启动
        /// 在该线程中启动更新robot机器人参数的线程
        /// 初始化完成后，线程就立即结束
        /// </summary>
        private void InitThreadInitParameterHandler( )
        {
            this.WriteLogHandler("RoboDK软件开启中....");

            // 在创建RoboDK的对象时，会对RoboDK软件进行启动和连接，若路径不对，则启动不了，需要进行提示，并且不启动更新参数线程
            try
            {
                this._rdkPlatform = new RoboDK();                                               // 创建RoboDK对象，其中会对RoboDK平台进行连接  
                this.WriteLogHandler("已开启RoboDK软件.");
            }
            catch
            {
                this._rdkPlatform = null;
                this.WriteLogHandler("RoboDK软件无法启动，可能路径有误，" + "重新设置正确路径，然后重启.", 1);
                return;
            }

            this._queueRobotMoveMessage = new Queue<double[]>();
            this._autoEvent = new AutoResetEvent(false);

            // 启动机器人参数更新的线程
            this._thrdUpdateRobotParameter = new Thread(this.ThreadUpdatRobotParameterHandler); // 初始化更新机器人参数的线程
            this._thrdUpdateRobotParameter.IsBackground = true;                                 // 设置成后台线程，在前台线程结束时，所有剩余的后台线程都会停止且不会完成
            this._thrdUpdateRobotParameter.Name = "UpdateRobotParameterHandler";                // 设置线程的名字
            this._thrdUpdateRobotParameter.Start();                                             // 启动线程

            //this._thrdDriveRobotMoveHandler = new Thread(ThrdDriveRobotMoveHandler);            // 简单线程，驱动机器人运动操作
            //this._thrdDriveRobotMoveHandler.IsBackground = true;                                // 设置成后台线程，在前台线程结束时，所有剩余的后台线程都会停止且不会完成                                  
            //this._thrdDriveRobotMoveHandler.Name = "DriveRobotMove";                            // 设置线程的名字
            //this._thrdDriveRobotMoveHandler.Start();
        }

        #endregion

        #region 方法
        #region 关闭所有资源
        /// <summary>
        /// 关闭MyRoboDKExtension的所有资源,先关闭线程，再使变量无效
        /// </summary>
        public void Close( )
        {
            this.CloseThreadUpdatRobotParameter();
            this.CloseCommunicate();
            this.CloseAllVariable();
        }

        /// <summary>
        /// 关闭和RoboDK的socket通信
        /// </summary>
        private void CloseCommunicate( )
        {
            if (this._rdkPlatform != null)
                this._rdkPlatform.Close();
        }

        /// <summary>
        /// 关闭所有变量，使它们都invalidition
        /// </summary>
        private void CloseAllVariable( )
        {
            this._rdkItemRobot = null;
            this._rdkPlatform = null;

            this._thrdUpdateRobotParameter = null;
            this._thisLock = null;

            this._thrdDriveRobotMoveHandler = null;
            this._queueRobotMoveMessage = null;
            this._bolIsDriveRbtMoveThrdAlive = false;
            this._autoEvent = null;

            this._bolIsUpdateRbtParThrdAlive = false;
            this._bolIsThreadSuspend = false;
        }

        /// <summary>
        /// 关闭更新机器人参数的线程
        /// </summary>
        private void CloseThreadUpdatRobotParameter( )
        {
            if (this._thrdUpdateRobotParameter != null)
            {
                this._bolIsUpdateRbtParThrdAlive = false;           // 关闭线程
                Thread.Sleep(100);                                  // 等待线程关闭
                this._thrdUpdateRobotParameter.Abort();
                this._thrdUpdateRobotParameter = null;
            }

            if (this._thrdDriveRobotMoveHandler != null)
            {
                this._bolIsDriveRbtMoveThrdAlive = false;       // 关闭线程
                Thread.Sleep(100);                              // 等待线程关闭
                this._thrdDriveRobotMoveHandler.Abort();
                this._thrdDriveRobotMoveHandler = null;
            }
        }

        #endregion

        #region 更新机器人参数
        /// <summary>
        /// 更新机器人参数的线程，主要是6个角度，6个姿态和速度
        /// </summary>
        private void ThreadUpdatRobotParameterHandler( )
        {
            Thread.Sleep(1);        // 延迟启动，为避免初始化时候出现问题
            this.WriteLogHandler("已开启RoboDK接收机器人参数线程.");
            this.SelectRobotItem();                 //开始的时候先选取机器人对象
            while (this._bolIsUpdateRbtParThrdAlive)
            {
                // 线程是否暂停，暂停后后面不执行
                if (this._bolIsThreadSuspend)
                {
                    //Thread.Sleep(50);
                    continue;
                }

                // 检查是否连接，是否选中机器人
                if (this.CheckRobot())
                {
                    // 打开了RoboDK和选中了机器人
                    this.GetRobotParameters();
                    this.DriveRobotMove();
                    this.UpdateRobotRunningStateHandler();
                    Thread.Sleep(10);
                }
                else
                {
                    // 没有选中机器人或者没有打开，则打开并选中                    
                    Thread.Sleep(10000);
                    this.SelectRobotItem();
                }
            }
        }

        /// <summary>
        /// 暂停更新机器人相关参数线程
        /// </summary>
        private void SuspendThreadUpdateRobotParameter( )
        {
            lock (this._thisLock)
            {
                if (this._bolIsUpdateRbtParThrdAlive)
                    this._bolIsThreadSuspend = true;
            }
            // 在切换选择机器人的时候会出现异常，因此暂时采用延迟执行后续内容，防止后续操作和线程中的操作冲突，猜测是这个问题
            Thread.Sleep(100);
        }

        /// <summary>
        /// 唤醒更新机器人相关参数线程
        /// </summary>
        private void ResumeThreadUpdateRobotParameter( )
        {
            lock (this._thisLock)
            {
                if (this._bolIsUpdateRbtParThrdAlive)
                    this._bolIsThreadSuspend = false;
            }
        }

        /// <summary>
        /// 获取机器人相关的参数
        /// </summary>
        private void GetRobotParameters( )
        {
            if (!this.CheckRobot()) { return; }
            try
            {
                lock (this._thisLock)
                {
                    // 获取机器人关节坐标系的角度
                    double[] dblJoints = this._rdkItemRobot.Joints();

                    // 获取机器人直角坐标系的角度
                    Mat locMatPose = this._rdkItemRobot.SolveFK(dblJoints);
                    double[] dblPoses = locMatPose.ToXYZRPW();

                    // RoboDK中机器人的运动速度
                    double dblRobotMoveSpeed = this._rdkPlatform.SimulationSpeed();

                    this._rdkItemRobot.JointLimits(ref this._dblRobotLowerLimits, ref this._dblRobotUpperLimits);


                    // 机器人参数整合在字典中，一起传出去
                    Dictionary<string, object> dicData = RobotParametersIntegrity(dblJoints, dblPoses, dblRobotMoveSpeed);

                    // 更新机器人参数
                    if (this.UpdateRobotParameter != null)
                        this.UpdateRobotParameter(dicData);
                }
            }
            catch
            {
                this._rdkItemRobot = null; // 一旦在传输的过程中出现问题，都需要重新选择机器人模型，都将机器人对象置为null
                this.WriteLogHandler("选中的机器人模型无效或者不存在，请重新选择.", 1);
            }
        }

        /// <summary>
        /// 将关于机器人的所有参数都整合在一起，然后在界面中显示
        /// </summary>
        /// <param name="dblJoints">关节角度</param>
        /// <param name="dblPoses">姿态坐标</param>
        /// <param name="dblMoveSpeed">机器人运动速度</param>
        /// <returns>所有数据整合到一起的字典</returns>
        private Dictionary<string, object> RobotParametersIntegrity(double[] dblJoints, double[] dblPoses, double dblMoveSpeed)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();

            // 6个关节坐标系的角度
            dic.Add("RobotJoint1", dblJoints[0]);
            dic.Add("RobotJoint2", dblJoints[1]);
            dic.Add("RobotJoint3", dblJoints[2]);
            dic.Add("RobotJoint4", dblJoints[3]);
            dic.Add("RobotJoint5", dblJoints[4]);
            dic.Add("RobotJoint6", dblJoints[5]);

            // 6个直角坐标系的坐标
            dic.Add("RobotPoseX", dblPoses[0]);
            dic.Add("RobotPoseY", dblPoses[1]);
            dic.Add("RobotPoseZ", dblPoses[2]);
            dic.Add("RobotPoseU", dblPoses[3]);
            dic.Add("RobotPoseV", dblPoses[4]);
            dic.Add("RobotPoseW", dblPoses[5]);

            // 机器人的运动速度
            dic.Add("RobotMoveSpeed", dblMoveSpeed);

            return dic;
        }

        /// <summary>
        /// 更新机器人的运动状态
        /// </summary>
        private void UpdateRobotRunningStateHandler( )
        {
            string strState = string.Empty;
            if (this.CheckIsRobotBusy())
                strState = "运行";
            else
                strState = "停止";
            if (this.UpdateRobotRunningState != null)
                this.UpdateRobotRunningState(strState);
        }

        #endregion

        #region 对RoboDK的检查
        /// <summary>
        /// 检查是否和RoboDK的连接状态，true为连接，false为断开
        /// </summary>
        /// <returns>连接状态</returns>
        private bool CheckRoboDK( )
        {
            lock (this._thisLock)
            {
                bool ok = this._rdkPlatform.Connected();
                if (!ok)
                {
                    ok = this._rdkPlatform.Connect();
                    this._rdkItemRobot = null;          // 每次重连或者重启软件之后，都将机器人对象置null，需重新选取机器人
                    this.WriteLogHandler("已重启或重连RoboDK.");
                }
                if (!ok)
                {
                    this.WriteLogHandler("未找到 RoboDK. 请先启动RoboDK.", 1);
                }
                return ok;
            }
        }

        /// <summary>
        /// 检查是否选中了机器人，或者添加了机器人，true为选中，false为没有选中
        /// </summary>
        /// <returns></returns>
        private bool CheckRobot( )
        {
            lock (this._thisLock)
            {
                if (!this.CheckRoboDK()) { return false; }
                if (this._rdkItemRobot == null || !this._rdkItemRobot.Valid())
                {
                    this.WriteLogHandler("未选中机器人对象，请添加机器人模型.", 1);
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 检查机器人是否正在运动，true为运动，false为停止
        /// </summary>
        /// <returns></returns>
        private bool CheckIsRobotBusy( )
        {
            bool bolIsBusy = false;
            int intBusy = 0;

            if (this._rdkItemRobot == null)
                return false;

            lock (this._thisLock)
            {
                intBusy = this._rdkItemRobot.Busy();
            }
            if (intBusy == 1)
            {
                bolIsBusy = true;
            }
            else
                bolIsBusy = false;
            return bolIsBusy;
        }

        /// <summary>
        /// 选中机器人对象
        /// </summary>
        /// <returns>是否选中机器人</returns>
        private bool SelectRobotItem( )
        {
            lock (this._thisLock)
            {
                if (!this.CheckRoboDK()) { return false; }
                this._rdkItemRobot = this._rdkPlatform.ItemUserPick("Select a robot", RoboDK.ITEM_TYPE_ROBOT); // select robot among available robots
                if (this._rdkItemRobot.Valid())
                {
                    this.WriteLogHandler("已选中机器人: " + this._rdkItemRobot.Name() + ".");
                    return true;
                }
                else
                {
                    //this.WriteLogHandler("没有机器人可以选取.");
                    return false;
                }
            }
        }

        #endregion

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

        #region 选择机器人模型
        /// <summary>
        /// 选择机器人模型的处理事件
        /// </summary>
        /// <param name="objParameter">机器人对应的编号</param>
        public void SelectRobotModelHandler(object objParameter)
        {
            Thread thrd = new Thread(ThreadSelectRobotModel);
            thrd.IsBackground = true;
            thrd.Name = "SelectRobotModel";
            thrd.Start(objParameter);
            // 判断是否可以执行选择机器人模型的操作
            //if (!this.IsEnableReselectRobotModel(objParameter))
            //    return;

            //string strFileName = this.GetRobotModelFileName(objParameter);  // 获取对应机器人的文件名
            //this.SuspendThreadUpdateRobotParameter();                       // 线程暂停                                          
            //this.DeleteAllRobotModel();                                     // 删除工作站中的所有机器人模型 
            //this.LoadRobotModel(strFileName);                               // 载入机器人模型   
            //this.SelectRobotItem();                                         // 选中机器人对象           
            //this.ResumeThreadUpdateRobotParameter();                        // 线程继续执行
        }

        private void ThreadSelectRobotModel(object objParameter)
        {
            // 判断是否可以执行选择机器人模型的操作
            if (!this.IsEnableReselectRobotModel(objParameter))
                return;
            lock (this._thisLock)
            {
                string strFileName = this.GetRobotModelFileName(objParameter);  // 获取对应机器人的文件名
                this.SuspendThreadUpdateRobotParameter();                       // 线程暂停                                          
                this.DeleteAllRobotModel();                                     // 删除工作站中的所有机器人模型 
                Thread.Sleep(100);
                this.LoadRobotModel(strFileName);                               // 载入机器人模型   
                this.SelectRobotItem();                                         // 选中机器人对象           
                this.ResumeThreadUpdateRobotParameter();                        // 线程继续执行
            }
        }

        /// <summary>
        /// 是否可以重新选择机器人对象，true为可以，false为不可以
        /// </summary>
        /// <param name="objParameter">选中的机器人编号</param>
        /// <returns></returns>
        private bool IsEnableReselectRobotModel(object objParameter)
        {
            if (objParameter == null)
            {
                this.WriteLogHandler("选择机器人模型传入参数有误.", 1);
                return false;
            }
            if (this._rdkPlatform == null)
            {
                this.WriteLogHandler("RoboDK尚未打开.", 1);
                return false;
            }

            lock (this._thisLock)
            {
                if ((this._rdkItemRobot == null) || (!this._rdkItemRobot.Valid()))
                {
                    this.WriteLogHandler("未选中机器人对象.", 1);
                    return true;
                }


                // 避免重复载入相同的机器人模型
                // 判断当前的机器人名字和选中的名字是否一致，一致则不需要重选，不一致则重选
                string strName = this.GetRobotModelName(objParameter);
                if (strName == this._rdkItemRobot.Name())
                    return false;       // 机器人名字一样              
                else
                    return true;        // 机器人名字不一样
            }
        }

        /// <summary>
        /// 删除机器人模型
        /// </summary>
        private void DeleteAllRobotModel( )
        {
            if (this._rdkItemRobot == null)
                return;
            //this._rdkItemRobot.Delete();
            lock (this._thisLock)
            {
                RoboDK.Item station = this._rdkPlatform.getItem("", RoboDK.ITEM_TYPE_STATION);
                RoboDK.Item[] station_childs = station.Childs();
                while (station_childs.Length > 0)
                {
                    int sum = station_childs.Length;
                    for (int i = 0; i < sum; i++)
                        station_childs[i].Delete();
                    station = this._rdkPlatform.getItem("", RoboDK.ITEM_TYPE_STATION);
                    station_childs = station.Childs();
                }
            }
        }

        /// <summary>
        /// 关闭工作站
        /// </summary>
        private void CloseAllStations( )
        {
            if (this._rdkPlatform == null)
                return;
            lock (this._thisLock)
            {
                // get the first station available:
                RoboDK.Item station = this._rdkPlatform.getItem("", RoboDK.ITEM_TYPE_STATION);
                // check if the station has any items (object, robots, reference frames, ...)
                RoboDK.Item[] station_childs = station.Childs();
                while (station_childs.Length > 0)
                {
                    this._rdkPlatform.CloseStation();
                    station = this._rdkPlatform.getItem("", RoboDK.ITEM_TYPE_STATION);
                    station_childs = station.Childs();
                }
            }
        }

        /// <summary>
        /// 载入机器人模型
        /// </summary>
        /// <param name="strName">对应的文件名</param>
        private void LoadRobotModel(string strFileName)
        {
            if (this._rdkPlatform == null)
                return;
            strFileName = System.IO.Path.GetFullPath(strFileName);
            if (strFileName.EndsWith(".robot", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    lock (this._thisLock)
                    {
                        this._rdkPlatform.AddFile(strFileName);
                    }
                }
                catch (Exception e)
                {
                    this.WriteLogHandler(e.Message, 1);
                }

            }
        }

        /// <summary>
        /// 获取机器人的文件名，也是文件路径
        /// </summary>
        /// <param name="objParameter">机器人对应的编号</param>
        /// <returns>机器人的文件名</returns>
        private string GetRobotModelFileName(object objParameter)
        {
            string strNum = (string)objParameter;
            string strFileName = string.Empty;
            switch (strNum)
            {
                case "1":
                    strFileName = MyConstString.ROBOTDK_ROBOT_FILE_NAME_ABB_IRB_1200_7_0_7;
                    break;
                case "2":
                    strFileName = MyConstString.ROBOTDK_ROBOT_FILE_NAME_ABB_IRB_120_3_0_6;
                    break;
                case "3":
                    strFileName = MyConstString.ROBOTDK_ROBOT_FILE_NAME_ABB_IRB_6700_155_2_85;
                    break;
                case "4":
                    strFileName = MyConstString.ROBOTDK_ROBOT_FILE_NAME_UR10;
                    break;
                case "5":
                    strFileName = MyConstString.ROBOTDK_ROBOT_FILE_NAME_UR3;
                    break;
            }
            return strFileName;
        }

        /// <summary>
        /// 获取机器人模型的名称
        /// </summary>
        /// <param name="objParameter">对应机器人的编号</param>
        /// <returns>机器人的名称</returns>
        private string GetRobotModelName(object objParameter)
        {
            string strNum = (string)objParameter;
            string strName = string.Empty;
            switch (strNum)
            {
                case "1":
                    strName = MyConstString.ROBOTDK_ROBOT_NAME_ABB_IRB_1200_7_0_7;
                    break;
                case "2":
                    strName = MyConstString.ROBOTDK_ROBOT_NAME_ABB_IRB_120_3_0_6;
                    break;
                case "3":
                    strName = MyConstString.ROBOTDK_ROBOT_NAME_ABB_IRB_6700_155_2_85;
                    break;
                case "4":
                    strName = MyConstString.ROBOTDK_ROBOT_NAME_UR10;
                    break;
                case "5":
                    strName = MyConstString.ROBOTDK_ROBOT_NAME_UR3;
                    break;
            }
            return strName;
        }
        #endregion

        #region 机器人运动

        //private void ThrdDriveRobotMoveHandler(object obj)
        //{
        //    Thread.Sleep(1);        // 延迟启动，为避免初始化时候出现问题
        //    this.WriteLogHandler("已开启机器人运动线程并等待.");
        //    while (this._bolIsDriveRbtMoveThrdAlive)
        //    {
        //        if (this._queueRobotMoveMessage.Count != 0)
        //            DriveRobotMoveHandler();
        //        else
        //            this._autoEvent.WaitOne();                      // 阻止当前线程，直到当前 WaitHandle 收到信号为止
        //    }
        //}

        //public void DriveRobotMoveHandler( )
        //{
        //    double[] dblJoints = this._queueRobotMoveMessage.Dequeue();
        //    lock (this._objLockRobotMove)
        //    {
        //        //this.SuspendThreadUpdateRobotParameter();
        //        this._rdkItemRobot.MoveJ(dblJoints, BLOCKING_MOVE);
        //        //this.ResumeThreadUpdateRobotParameter();
        //    }
        //    Thread.Sleep(10);           // 防止数据发送太快导致roboDK出问题
        //}


        /// <summary>
        /// 添加一条机器人运动的消息
        /// </summary>
        /// <param name="dblAngles">角度信息消息</param>
        public void AddRobotMoveMessage(double[] dblAngles)
        {
            this._queueRobotMoveMessage.Enqueue(dblAngles);

            // 使整个队列的大小保持在20个
            if (this._queueRobotMoveMessage.Count > 20)
                this._queueRobotMoveMessage.Dequeue();
            //this._autoEvent.Set();                                  // 将事件状态设置为终止状态，允许一个或多个等待线程继续
        }


        /// <summary>
        /// 驱动机器人运动
        /// </summary>
        private void DriveRobotMove( )
        {
            if (this._queueRobotMoveMessage.Count != 0)
            {
                double[] dblJoints = this._queueRobotMoveMessage.Dequeue();

                // 检查关节角度值是否在限定范围内，若不是，则使其在限定范围内
                for (int i = 0; i < 6; i++)
                {
                    if (dblJoints[i] > this._dblRobotUpperLimits[i])
                        dblJoints[i] = this._dblRobotUpperLimits[i];
                    if (dblJoints[i] < this._dblRobotLowerLimits[i])
                        dblJoints[i] = this._dblRobotLowerLimits[i];
                }
                lock (this._thisLock)
                {
                    try
                    {
                        this._rdkPlatform.setSimulationSpeed(1000);      //控制机器人的运动速度
                        this._rdkItemRobot.MoveJ(dblJoints, BLOCKING_MOVE);
                    }
                    catch
                    {
                        this._rdkItemRobot = null; // 一旦在传输的过程中出现问题，都需要重新选择机器人模型，都将机器人对象置为null
                        this.WriteLogHandler("选中的机器人模型无效或者不存在，请重新选择.", 1);
                    }
                }
            }
        }


        #endregion

        #region 添加,执行RoboDK的程序，生成机器人程序

        /// <summary>
        /// 创建RoboDK程序
        /// </summary>
        /// <param name="strFileAddress">文件地址</param>
        public void CreateRoboDKProgram(string strFileAddress)
        {
            Thread thrd = new Thread(ThrdCreateRoboDKProgram);
            thrd.IsBackground = true;
            thrd.Name = "CreateRoboDKProgram";
            thrd.Start(strFileAddress);
        }

        /// <summary>
        /// 创建RoboDK程序的线程
        /// </summary>
        /// <param name="obj">文件地址</param>
        private void ThrdCreateRoboDKProgram(object obj)
        {
            string strFileAddress = (string)obj;

            // 如果没有执行写文件，或者文件不存在
            if (!File.Exists(strFileAddress))
                return;
            try
            {
                if (!this.CheckRoboDK()) { return; }
                if (!this.CheckRobot()) { return; }
            }
            catch
            {
                this.WriteLogHandler("未选中机器人对象.");
                return;
            }

            // 读取机器人程序文件，转换成每行的内容
            List<string> lstStr = new List<string>();
            if (!this.ReadRobotFile(strFileAddress, ref lstStr))
            {
                this.WriteLogHandler("读取的文件为空.");
                return;
            }

            // 将每行的内容转换为具体的角度值
            List<double[]> lstJoints = new List<double[]>();
            if (!this.TransferData2Joints(lstStr, ref lstJoints))
            {
                this.WriteLogHandler("文件数据转换到角度值有误.");
                return;
            }

            // 若工作站中存在目标点和程序，则进行删除操作
            int intDeleteNum = 0;
            if (!this.DeleteTargetPointAndProgram(ref intDeleteNum))
            {
                this.WriteLogHandler("RoboDK目标点和程序删除失败.");
                return;
            }

            // 向RoboDK中添加关节目标节点
            if (!this.AddJointTarget(lstJoints, intDeleteNum))
            {
                this.WriteLogHandler("无法添加目标点.");
                return;
            }
        }

        /// <summary>
        /// 读取机器人程序文件
        /// </summary>
        /// <param name="strFileAddress">文件地址</param>
        /// <param name="lstStr">每行的内容</param>
        /// <returns>是否读取成功</returns>
        private bool ReadRobotFile(string strFileAddress, ref List<string> lstStr)
        {
            bool bolIsSuccess = false;
            using (StreamReader r = new StreamReader(strFileAddress, System.Text.Encoding.UTF8))
            {
                while (r.Peek() >= 0)
                {
                    string strOneLine = r.ReadLine();
                    lstStr.Add(strOneLine);
                }
                r.Close();
            }
            if (lstStr.Count != 0)
                bolIsSuccess = true;
            return bolIsSuccess;
        }

        /// <summary>
        /// 将每行的内容转换为具体的角度值
        /// </summary>
        /// <param name="lstStr">每行内容</param>
        /// <param name="lstJoints">关节角度值</param>
        /// <returns>转换是否成功</returns>
        private bool TransferData2Joints(List<string> lstStr, ref List<double[]> lstJoints)
        {
            bool bolIsSuccess = false;
            int intLength = lstStr.Count;
            for (int i = 0; i < intLength; i++)
            {
                string strTmp = lstStr[i];
                string[] strJoints = strTmp.Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (strJoints.Length == 6)
                {
                    double[] dblJoints = new double[6];
                    dblJoints[0] = double.Parse(strJoints[0]);
                    dblJoints[1] = double.Parse(strJoints[1]);
                    dblJoints[2] = double.Parse(strJoints[2]);
                    dblJoints[3] = double.Parse(strJoints[3]);
                    dblJoints[4] = double.Parse(strJoints[4]);
                    dblJoints[5] = double.Parse(strJoints[5]);
                    lstJoints.Add(dblJoints);
                }
            }
            if (lstJoints.Count != 0)
                bolIsSuccess = true;
            return bolIsSuccess;
        }

        /// <summary>
        /// 删除目标点和程序
        /// </summary>
        /// <param name="intDeleteNum">目标点的个数</param>
        /// <returns>是否删除成功</returns>
        private bool DeleteTargetPointAndProgram(ref int intDeleteNum)
        {
            bool bolIsSuccess = false;
            if (this._rdkItemRobot == null)
                return bolIsSuccess;
            //this._rdkItemRobot.Delete();
            lock (this._thisLock)
            {
                RoboDK.Item station = this._rdkPlatform.getItem("", RoboDK.ITEM_TYPE_STATION);
                RoboDK.Item[] station_childs = station.Childs();
                this.WriteLogHandler("开始删除RoboDK中的目标点.");
                int sum = station_childs.Length;
                for (int i = 0; i < sum; i++)
                {
                    // 删除所有PROGRAM
                    if (station_childs[i].Type() == RoboDK.ITEM_TYPE_PROGRAM)
                        station_childs[i].Delete();
                    // 删除FRAME下面所有TARGET
                    else if (station_childs[i].Type() == RoboDK.ITEM_TYPE_FRAME)
                    {
                        RoboDK.Item[] frame_childs = station_childs[i].Childs();
                        int intLength = frame_childs.Length;
                        intDeleteNum = intLength;
                        for (int j = 0; j < intLength; j++)
                        {
                            if (frame_childs[j].Type() == RoboDK.ITEM_TYPE_TARGET)
                            {
                                frame_childs[j].Delete();

                                // 更新添加目标点的状态
                                int intState = (int)(50 * (j + 1) / intLength);
                                this.UpdateAddTargetPointStateHandler(intState);
                            }                                
                        }
                    }
                }
            }
            this.WriteLogHandler("结束删除RoboDK中的目标点.");
            bolIsSuccess = true;
            return bolIsSuccess;
        }

        /// <summary>
        /// 向RoboDK中添加关节目标节点
        /// </summary>
        /// <param name="lstJoints">关节角度</param>
        /// <returns>是否添加成功</returns>
        private bool AddJointTarget(List<double[]> lstJoints, int intDeleteNum)
        {
            bool bolIsSuccess = false;
            RoboDK.Item itemProgram = null;
            lock (this._thisLock)
            {
                this._rdkPlatform.setSimulationSpeed(5);      //控制机器人的运动速度
                itemProgram = this._rdkPlatform.AddProgram(this._strProgramName, this._rdkItemRobot);
                this.WriteLogHandler("开始向RoboDK中添加目标点.");
                this.WriteLogHandler("正在向RoboDK中添加目标点...");
            }
            int intLength = lstJoints.Count;
            for (int i = 0; i < intLength; i++)
            {
                lock (this._thisLock)
                {
                    RoboDK.Item jointTarget = this._rdkPlatform.AddTarget("Target " + i.ToString(), null, this._rdkItemRobot);
                    jointTarget.setAsJointTarget();
                    jointTarget.setJoints(lstJoints[i]);
                    itemProgram.addMoveJ(jointTarget);
                }

                // 更新添加目标点的状态
                int intState = 0;
                if (intDeleteNum > 2)
                    intState = (int)(50 * (i + 1) / intLength + 50);
                else
                    intState = (int)(100 * (i + 1) / intLength);
                this.UpdateAddTargetPointStateHandler(intState);
            }
            this.WriteLogHandler("结束向RoboDK中添加目标点.");
            bolIsSuccess = true;
            return bolIsSuccess;
        }

        /// <summary>
        /// 更新添加目标点的执行情况，用progressbar来表示
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateAddTargetPointStateHandler(object obj)
        {
            if (this.UpdateAddTargetPointState != null)
            {
                this.UpdateAddTargetPointState(obj);
            }
        }

        /// <summary>
        /// 运行RoboDK中的程序
        /// </summary>
        public void RunRoboDKProgramHandler( )
        {
            if (!this.CheckRoboDK()) { return; }
            if (!this.CheckRobot()) { return; }
            if (this.CheckIsRobotBusy()) { this.WriteLogHandler("机器人正在运动,无法进行其他操作.", 1); return; }
            lock (this._thisLock)
            {
                RoboDK.Item itemProgram = this._rdkPlatform.getItem(this._strProgramName, RoboDK.ITEM_TYPE_PROGRAM);
                if (itemProgram.Valid())
                {
                    this.WriteLogHandler("运行RoboDK机器人程序.");
                    itemProgram.setRunType(RoboDK.PROGRAM_RUN_ON_SIMULATOR);      // force to run on robot
                    itemProgram.RunProgram();
                }
                else
                {
                    this.WriteLogHandler("不存在" + this._strProgramName + "这个程序.", 1);
                }
            }
        }

        /// <summary>
        /// 停止RoboDK中的程序
        /// </summary>
        public void StopRoboDKProgramHandler( )
        {
            if (!this.CheckRoboDK()) { return; }
            if (!this.CheckRobot()) { return; }
            if (!this.CheckIsRobotBusy()) { return; }       // 若机器人停止中，则不执行停止操作

            lock (this._thisLock)
            {
                RoboDK.Item itemProgram = this._rdkPlatform.getItem(this._strProgramName, RoboDK.ITEM_TYPE_PROGRAM);
                if (itemProgram.Valid())
                {
                    itemProgram.Stop();
                    this.WriteLogHandler("停止RoboDK机器人程序.");
                }
                //this._rdkItemRobot.Stop();
            }
        }

        /// <summary>
        /// 生成机器人程序
        /// </summary>
        /// <param name="strFileAddress"></param>
        public void MakeRobotProgramHandler( string strFileAddress)
        {
            if (!this.CheckRoboDK()) { return; }
            if (!this.CheckRobot()) { return; }
            if (this.CheckIsRobotBusy()) { return; }

            lock(this._thisLock)
            {
                RoboDK.Item itemProgram = this._rdkPlatform.getItem(this._strProgramName, RoboDK.ITEM_TYPE_PROGRAM);
                if (itemProgram.Valid())
                {
                    itemProgram.MakeProgram(strFileAddress);
                    this.WriteLogHandler("生成实际机器人程序.");
                }
            }
        }

        #endregion

        #endregion
    }
}
