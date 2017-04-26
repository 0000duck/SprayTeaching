using System;
using System.Collections.Generic;
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
        private bool _bolIsThreadAlive = true;                                  // 控制线程活着，true为活着，false为死亡
        private bool _bolIsThreadSuspend = false;                               // 控制线程暂停，true为暂停，false为不暂停

        #endregion

        #region 外部事件

        public event UpdateLogContentEventHandler UpdateLogContent;             // 更新日志文件        
        public event UpdateRobotParameterEventHandler UpdateRobotParameter;     // 更新机器人参数  

        #endregion

        #region 构造函数，初始化

        public MyRoboDKExtension()
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
        private void InitThreadInitParameterHandler()
        {
            this.WriteLog("RoboDK软件开启中....");

            // 在创建RoboDK的对象时，会对RoboDK软件进行启动和连接，若路径不对，则启动不了，需要进行提示，并且不启动更新参数线程
            try
            {
                this._rdkPlatform = new RoboDK();                                               // 创建RoboDK对象，其中会对RoboDK平台进行连接  
                this.WriteLog("已开启RoboDK软件.");
            }
            catch
            {
                this._rdkPlatform = null;
                this.WriteLog("RoboDK软件无法启动，可能路径有误，" + "重新设置正确路径，然后重启.");
                return;
            }

            // 启动机器人参数更新的线程
            this._thrdUpdateRobotParameter = new Thread(this.ThreadUpdatRobotParameterHandler); // 初始化更新机器人参数的线程
            this._thrdUpdateRobotParameter.IsBackground = true;                                 // 设置成后台线程，在前台线程结束时，所有剩余的后台线程都会停止且不会完成
            this._thrdUpdateRobotParameter.Name = "UpdateRobotParameterHandler";                // 设置线程的名字
            this._thrdUpdateRobotParameter.Start();                                             // 启动线程
        }

        #endregion

        #region 方法
        #region 关闭所有资源
        /// <summary>
        /// 关闭MyRoboDKExtension的所有资源,先关闭线程，再使变量无效
        /// </summary>
        public void Close()
        {
            this.CloseThreadUpdatRobotParameter();
            this.CloseCommunicate();
            this.CloseAllVariable();
        }

        /// <summary>
        /// 关闭和RoboDK的socket通信
        /// </summary>
        private void CloseCommunicate()
        {
            if (this._rdkPlatform != null)
                this._rdkPlatform.Close();
        }

        /// <summary>
        /// 关闭所有变量，使它们都invalidition
        /// </summary>
        private void CloseAllVariable()
        {
            this._rdkItemRobot = null;
            this._rdkPlatform = null;
            this._thrdUpdateRobotParameter = null;
            this._thisLock = null;

            this._bolIsThreadAlive = false;
            this._bolIsThreadSuspend = false;
        }

        /// <summary>
        /// 关闭更新机器人参数的线程
        /// </summary>
        private void CloseThreadUpdatRobotParameter()
        {
            if (this._thrdUpdateRobotParameter != null)
            {
                this._bolIsThreadAlive = false;         // 关闭线程
                Thread.Sleep(150);                      // 等待线程关闭
                this._thrdUpdateRobotParameter.Abort();
                this._thrdUpdateRobotParameter = null;
            }
        }

        #endregion

        #region 更新机器人参数
        /// <summary>
        /// 更新机器人参数的线程
        /// </summary>
        private void ThreadUpdatRobotParameterHandler()
        {
            this.WriteLog("已开启RoboDK接收机器人参数线程.");
            this.SelectRobotItem();                 //开始的时候先选取机器人对象
            while (this._bolIsThreadAlive)
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
                    Thread.Sleep(100);
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
        private void SuspendThreadUpdateRobotParameter()
        {
            lock (this._thisLock)
            {
                if (this._bolIsThreadAlive)
                    this._bolIsThreadSuspend = true;
            }
            // 在切换选择机器人的时候会出现异常，因此暂时采用延迟执行后续内容，防止后续操作和线程中的操作冲突，猜测是这个问题
            Thread.Sleep(100);
        }

        /// <summary>
        /// 唤醒更新机器人相关参数线程
        /// </summary>
        private void ResumeThreadUpdateRobotParameter()
        {
            lock (this._thisLock)
            {
                if (this._bolIsThreadAlive)
                    this._bolIsThreadSuspend = false;
            }
        }

        /// <summary>
        /// 获取机器人相关的参数
        /// </summary>
        private void GetRobotParameters()
        {
            if (!this.CheckRobot()) { return; }
            try
            {
                // 获取机器人关节坐标系的角度
                double[] dblJoints = this._rdkItemRobot.Joints();

                // 获取机器人直角坐标系的角度
                Mat locMatPose = this._rdkItemRobot.SolveFK(dblJoints);
                double[] dblPoses = locMatPose.ToXYZRPW();

                // RoboDK中机器人的运动速度
                double dblRobotMoveSpeed = this._rdkPlatform.SimulationSpeed();

                // 机器人参数整合在字典中，一起传出去
                Dictionary<string, object> dicData = RobotParametersIntegrity(dblJoints, dblPoses, dblRobotMoveSpeed);

                // 更新机器人参数
                if (this.UpdateRobotParameter != null)
                    this.UpdateRobotParameter(dicData);
            }
            catch
            {
                this._rdkItemRobot = null; // 一旦在传输的过程中出现问题，都需要重新选择机器人模型，都将机器人对象置为null
                this.WriteLog("选中的机器人模型无效或者不存在，请重新选择.");
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

        #endregion

        #region 对RoboDK的检查
        /// <summary>
        /// 检查是否和RoboDK的连接状态，true为连接，false为断开
        /// </summary>
        /// <returns>连接状态</returns>
        private bool CheckRoboDK()
        {
            bool ok = this._rdkPlatform.Connected();
            if (!ok)
            {
                ok = this._rdkPlatform.Connect();
                this._rdkItemRobot = null;          // 每次重连或者重启软件之后，都将机器人对象置null，需重新选取机器人
                this.WriteLog("已重启或重连RoboDK.");
            }
            if (!ok)
            {
                this.WriteLog("未找到 RoboDK. 请先启动RoboDK.");
            }
            return ok;
        }

        /// <summary>
        /// 检查是否选中了机器人，或者添加了机器人，true为选中，false为没有选中
        /// </summary>
        /// <returns></returns>
        private bool CheckRobot()
        {
            if (!this.CheckRoboDK()) { return false; }
            if (this._rdkItemRobot == null || !this._rdkItemRobot.Valid())
            {
                this.WriteLog("未选中机器人对象，请添加机器人模型.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 选中机器人对象
        /// </summary>
        /// <returns>是否选中机器人</returns>
        private bool SelectRobotItem()
        {
            if (!this.CheckRoboDK()) { return false; }
            this._rdkItemRobot = this._rdkPlatform.ItemUserPick("Select a robot", RoboDK.ITEM_TYPE_ROBOT); // select robot among available robots
            if (this._rdkItemRobot.Valid())
            {
                this.WriteLog("已选中机器人: " + this._rdkItemRobot.Name() + ".");
                return true;
            }
            else
            {
                //this.WriteLog("没有机器人可以选取.");
                return false;
            }
        }

        #endregion

        #region 写入日志
        /// <summary>
        /// 将消息写入日志
        /// </summary>
        /// <param name="strMessage">消息内容</param>
        private void WriteLog(string strMessage)
        {
            if (this.UpdateLogContent != null)
            {
                this.UpdateLogContent(strMessage);
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
            //Thread thrd = new Thread(ThreadSelectRobotModel);
            //thrd.IsBackground = true;
            //thrd.Name = "SelectRobotModel";
            //thrd.Start(objParameter);
            // 判断是否可以执行选择机器人模型的操作
            if (!this.IsEnableReselectRobotModel(objParameter))
                return;

            string strFileName = this.GetRobotModelFileName(objParameter);  // 获取对应机器人的文件名
            this.SuspendThreadUpdateRobotParameter();                       // 线程暂停                                          
            this.DeleteAllRobotModel();                                     // 删除工作站中的所有机器人模型 
            this.LoadRobotModel(strFileName);                               // 载入机器人模型   
            this.SelectRobotItem();                                         // 选中机器人对象           
            this.ResumeThreadUpdateRobotParameter();                        // 线程继续执行
        }

        //private void ThreadSelectRobotModel(object objParameter)
        //{
        //    // 判断是否可以执行选择机器人模型的操作
        //    if (!this.IsEnableReselectRobotModel(objParameter))
        //        return;
        //    lock (this._thisLock)
        //    {
        //        string strFileName = this.GetRobotModelFileName(objParameter);  // 获取对应机器人的文件名
        //        this.SuspendThreadUpdateRobotParameter();                       // 线程暂停                                          
        //        this.DeleteAllRobotModel();                                     // 删除工作站中的所有机器人模型 
        //        Thread.Sleep(100);
        //        this.LoadRobotModel(strFileName);                               // 载入机器人模型   
        //        this.SelectRobotItem();                                         // 选中机器人对象           
        //        this.ResumeThreadUpdateRobotParameter();                        // 线程继续执行
        //    }
        //}

        /// <summary>
        /// 是否可以重新选择机器人对象，true为可以，false为不可以
        /// </summary>
        /// <param name="objParameter">选中的机器人编号</param>
        /// <returns></returns>
        private bool IsEnableReselectRobotModel(object objParameter)
        {
            if (objParameter == null)
            {
                this.WriteLog("选择机器人模型传入参数有误.");
                return false;
            }                
            if (this._rdkPlatform == null)
            {
                this.WriteLog("RoboDK尚未打开.");
                return false;
            }
                
            if ((this._rdkItemRobot == null)||(!this._rdkItemRobot.Valid()))
            {
                this.WriteLog("未选中机器人对象.");
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

        /// <summary>
        /// 删除机器人模型
        /// </summary>
        private void DeleteAllRobotModel()
        {
            if (this._rdkItemRobot == null)
                return;
            //this._rdkItemRobot.Delete();
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

        /// <summary>
        /// 关闭工作站
        /// </summary>
        private void CloseAllStations()
        {
            if (this._rdkPlatform == null)
                return;
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
                    this._rdkPlatform.AddFile(strFileName);
                }
                catch (Exception e)
                {
                    this.WriteLog(e.Message);
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

        #endregion
    }
}
