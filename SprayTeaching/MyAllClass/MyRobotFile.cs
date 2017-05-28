using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using SprayTeaching.BaseClassLib;

namespace SprayTeaching.MyAllClass
{
    public class MyRobotFile
    {
        private StringBuilder _sbReceiveAngleMessage = new StringBuilder();         // 存放角度信息的缓存 
        private string _strFileAddress = string.Empty;                              // 有文件名的文件地址
        private const string RelativeLocation = "./RobotProgram";                   // 没有文件名的文件路径
        private int _intSampleFrequent = 0;                                         // 采样频率
        private bool _bolIsWriteFile = false;                                       // 是否执行过写入文件
        
        public event UpdateLogContentEventHandler UpdateLogContent;                                 // 更新日志文件 

        /// <summary>
        /// 文件地址
        /// </summary>
        public string RobotFileAddress
        {
            get { return _strFileAddress; }
            set { _strFileAddress = value; }
        }

        /// <summary>
        /// 是否执行写入文件
        /// </summary>
        public bool IsWriteFile
        {
            get { return _bolIsWriteFile; }
            set { _bolIsWriteFile = value; }
        }
        

        public MyRobotFile( )
        {
            this.CreateDirection();
        }

        // 关闭所有资源
        public void Close( )
        {
            this._sbReceiveAngleMessage.Clear();
            this._sbReceiveAngleMessage = null;
            this._strFileAddress = null;
        }

        private void CreateDirection( )
        {
            if (!Directory.Exists(RelativeLocation))
                Directory.CreateDirectory(RelativeLocation);
        }

        /// <summary>
        /// 开始添加角度信息，这个时候就要清除缓存中的数据
        /// </summary>
        public void StartAddAngleMessageHandler(string strFileName, int intFrequent)
        {
            this.ClearStringBuilderBuffer(strFileName,intFrequent);
        }

        /// <summary>
        /// 停止添加角度消息并写入文件
        /// </summary>
        /// <param name="bolIsSampleDataRunning">是否正在采样</param>
        /// <param name="strFileName">文件名</param>
        public void StopAddAngleMessageHandler(string strFileName, int intFrequent)
        {
            this.WriteMessageIntoFile(strFileName, intFrequent);
        }

        /// <summary>
        /// 添加角度信息
        /// </summary>
        /// <param name="dblAngles">角度</param>
        public void AddAngleMessageHandler(double[] dblAngles)
        {
            if (dblAngles.Length != 6)
                return;
            string strAngle = dblAngles[0].ToString() + " " + dblAngles[1].ToString() + " " + dblAngles[2].ToString() + " "
                + dblAngles[3].ToString() + " " + dblAngles[4].ToString() + " " + dblAngles[5].ToString() + "\r\n";
            this._sbReceiveAngleMessage.Append(strAngle);
        }

        /// <summary>
        /// 清除buffer
        /// </summary>
        private void ClearStringBuilderBuffer(string strFileName, int intFrequent)
        {
            this._sbReceiveAngleMessage.Clear();
            this._strFileAddress = RelativeLocation + "/" + strFileName + ".st";              // 更新文件地址
            this._intSampleFrequent = intFrequent;

            using (StreamWriter w = new StreamWriter(this._strFileAddress, false, System.Text.Encoding.UTF8))
            {
                w.Write("");
                w.Flush();
                w.Close();
            }
        }

        /// <summary>
        /// 将角度信息写入文件
        /// </summary>
        /// <param name="bolIsSampleDataRunning"></param>
        private void WriteMessageIntoFile(string strFileName, int intFrequent)
        {
            this._strFileAddress = RelativeLocation + "/" + strFileName + ".st";              // 更新文件地址
            this._intSampleFrequent = intFrequent;

            Thread thrd = new Thread(ThrdWriteMesageIntoFileHandler);
            thrd.IsBackground = true;
            thrd.Name = "WriteAngleMessage";
            thrd.Start();
        }

        /// <summary>
        /// 将角度信息写入文件
        /// </summary>
        private void ThrdWriteMesageIntoFileHandler( )
        {
            using (StreamWriter w = new StreamWriter(this._strFileAddress, false, System.Text.Encoding.UTF8))
            {
                string strBegin = _intSampleFrequent.ToString() + "\r\n" + "100" + "\r\n";
                w.Write(strBegin);

                int intLength = this._sbReceiveAngleMessage.Length;
                for (int i = 0; i < intLength; i++)
                {
                    w.Write(this._sbReceiveAngleMessage[i]);
                }
                w.Flush();
                w.Close();
                this.WriteLogHandler("已完成角度信息写入文件.");
                this._bolIsWriteFile = true;
            }
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
    }
}
