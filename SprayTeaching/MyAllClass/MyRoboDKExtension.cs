using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SprayTeaching.MyAllClass
{
    public delegate void UpdateLogContentEventHandler(string strParameter);                 //更新日志内容的delegate变量声明
    public delegate void UpdateRobotParameterEventHandler(double[] dblParameter1);          //更新机器人参数的delegate变量声明
    public class MyRoboDKExtension
    {
        private RoboDK.Item _rdkItemRobot;                                      // RoboDK中的机器人对象
        private RoboDK _rdkPlatform;                                            // RoboDK的平台
        const bool BLOCKING_MOVE = false;

        private Thread _thrdUpdateRobotParameter;                               //更新机器人参数的线程

        public event UpdateLogContentEventHandler UpdateLogContent;             //更新日志文件
        public event UpdateRobotParameterEventHandler UpdateRobotParameter;     //更新机器人参数

        public MyRoboDKExtension()
        {
            this._rdkPlatform = new RoboDK();
            SelectRobot();                                                              //选中机器人
            this._thrdUpdateRobotParameter = new Thread(ThreadUpdatRobotParameter);     //初始化更新机器人参数的线程
            this._thrdUpdateRobotParameter.Start();                                     //启动线程
        }

        /// <summary>
        /// 更新机器人参数的线程
        /// </summary>
        private void ThreadUpdatRobotParameter()
        {
            while (Thread.CurrentThread.IsAlive)
            {
                if (CheckRobot())
                {
                    //打开了RoboDK和选中了机器人
                    GetRobotParameter();
                    Thread.Sleep(100);
                }                    
                else
                {
                    //没有选中机器人或者没有打开，则打开并选中
                    SelectRobot();
                    Thread.Sleep(60000);
                }                    
            }
        }

        /// <summary>
        /// 关闭更新机器人参数的线程
        /// </summary>
        private void CloseThreadUpdatRobotParameter()
        {
            if(this._thrdUpdateRobotParameter!=null)
            {
                this._thrdUpdateRobotParameter.Abort();
                this._thrdUpdateRobotParameter = null;
            }
        }

        /// <summary>
        /// 获取机器人相关的参数
        /// </summary>
        private void GetRobotParameter()
        {
            if (!CheckRobot()) { return; }
            double[] dblJoints = this._rdkItemRobot.Joints();
            if (this.UpdateRobotParameter != null)
                this.UpdateRobotParameter(dblJoints);
        }



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
            }
            if (!ok)
            {
                WriteLog("未找到 RoboDK. 请先启动RoboDK.");
            }
            return ok;
        }

        /// <summary>
        /// 检查是否选中了机器人，或者添加了机器人，true为选中，false为没有选中
        /// </summary>
        /// <returns></returns>
        private bool CheckRobot()
        {
            if (!CheckRoboDK()) { return false; }
            if (this._rdkItemRobot == null || !this._rdkItemRobot.Valid())
            {
                WriteLog("未选中机器人对象.");
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
            if (!CheckRoboDK()) { return false; }
            this._rdkItemRobot = this._rdkPlatform.ItemUserPick("Select a robot", RoboDK.ITEM_TYPE_ROBOT); // select robot among available robots
            if (this._rdkItemRobot.Valid())
            {
                WriteLog("机器人: " + this._rdkItemRobot.Name() + ".");
                return true;
            }
            else
            {
                WriteLog("没有机器人可以选取.");
                return false;
            }
        }

        /// <summary>
        /// 将消息写入日志
        /// </summary>
        /// <param name="strParameter">消息内容</param>
        private void WriteLog(string strParameter)
        {
            if (this.UpdateLogContent != null)
            {
                this.UpdateLogContent(strParameter);
            }
        }


    }
}
