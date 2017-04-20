using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace SprayTeaching.MyAllClass
{
    public delegate void UpdateLogContentEventHandler(string strParameter);                 // 更新日志内容的delegate变量声明
    public delegate void UpdateRobotParameterEventHandler(double[] dblParameter1, double[] dblParameter2);  // 更新机器人参数的delegate变量声明      

    public class MyRoboDKExtension
    {
        #region 所有变量

        private RoboDK.Item _rdkItemRobot;                                      // RoboDK中的机器人模型对象
        private RoboDK _rdkPlatform;                                            // RoboDK的平台

        private const bool BLOCKING_MOVE = false;

        private Thread _thrdUpdateRobotParameter;                               // 更新机器人参数的线程
        private bool _bolIsThreadAlive = true;                                  // 控制线程活着

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
                this.WriteLog("RoboDK软件无法启动，可能路径有误." + "\r\n重新设置正确路径，然后重启.");
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
            this.SelectRobot();                 //开始的时候先选取机器人对象
            while (this._bolIsThreadAlive)
            {
                // 检查是否连接，是否选中机器人
                if (this.CheckRobot())
                {
                    // 打开了RoboDK和选中了机器人
                    this.GetRobotParameter();
                    Thread.Sleep(200);
                }
                else
                {
                    // 没有选中机器人或者没有打开，则打开并选中                    
                    Thread.Sleep(10000);
                    this.SelectRobot();
                }
            }
        }

        /// <summary>
        /// 获取机器人相关的参数
        /// </summary>
        private void GetRobotParameter()
        {
            if (!this.CheckRobot()) { return; }
            try
            {
                // 获取机器人关节坐标系的角度
                double[] dblJoints = this._rdkItemRobot.Joints();

                // 获取机器人直角坐标系的角度
                Mat locMatPose = this._rdkItemRobot.SolveFK(dblJoints);
                double[] dblPoses = locMatPose.ToXYZRPW();

                // 更新机器人参数
                if (this.UpdateRobotParameter != null)
                    this.UpdateRobotParameter(dblJoints, dblPoses);
            }
            catch
            {
                this.WriteLog("选中的机器人模型无效或者不存在.");
            }
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
        /// 选中机器人
        /// </summary>
        /// <returns>是否选中机器人</returns>
        private bool SelectRobot()
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

        #endregion
    }
}
