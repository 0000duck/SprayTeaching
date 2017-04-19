using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace SprayTeaching.MyAllClass
{
    public class MyLog
    {
        private string _strFilePath;                        // 日志文件的路径
        private Queue<string> _queueTempStoreLogMessage;    // 临时存放日志消息的队列 
        private string ENDFLAG = "\r\n";                    // 换行结束符

        private Thread _thrdWriteLogIntoText;               // 将日志信息写入文件中
        private AutoResetEvent _autoEvent;                  // 控制写日志到文件的线程
        private Object _thisLock;                           // 用于锁定修改部分的数据

        public event UpdateLogContentEventHandler UpdateLogContent;             // 更新日志文件

        public MyLog(string strFilePath)
        {
            this._strFilePath = strFilePath;

            this.InitDeleteLogFileContent();                // 在获取日志文件路径之后，清空日志文件

            this._queueTempStoreLogMessage = new Queue<string>();

            this._autoEvent = new AutoResetEvent(false);
            this._thisLock = new Object();

            this._thrdWriteLogIntoText = new Thread(ThreadWriteLogIntoTextHandler);     // 初始化将日志写入text文本的事件的线程
            this._thrdWriteLogIntoText.IsBackground = true;                             // 设置成后台线程，在前台线程结束时，所有剩余的后台线程都会停止且不会完成    
            this._thrdWriteLogIntoText.Name = "WriteLogIntoText";                       // 设置线程的名字
            this._thrdWriteLogIntoText.Start();                                         // 启动线程
        }

        /// <summary>
        /// 在初始化的时候清空日志文件
        /// </summary>
        private void InitDeleteLogFileContent()
        {
            //判断文件路径是否为空，为空则不添加
            if (string.IsNullOrEmpty(this._strFilePath))
                return;

            // 清空日志文件
            using (StreamWriter w = new StreamWriter(this._strFilePath, false, System.Text.Encoding.UTF8))
            {
                w.Write(System.DateTime.Now.ToString("d") + ENDFLAG);
                w.Flush();
                w.Close();
            }
        }

        /// <summary>
        /// 关闭MyLog的所有资源,先关闭线程，再使变量无效
        /// </summary>
        public void Close()
        {
            this.CloseThreadWriteLogIntoText();
            this.CloseAllVariable();
        }

        /// <summary>
        /// 关闭所有变量，使它们都invalidition
        /// </summary>
        private void CloseAllVariable()
        {
            this._thrdWriteLogIntoText = null;
        }

        /// <summary>
        /// 关闭写日志到文件的线程
        /// </summary>
        private void CloseThreadWriteLogIntoText()
        {
            if (this._thrdWriteLogIntoText != null)
            {
                this._thrdWriteLogIntoText.Abort();
                this._thrdWriteLogIntoText = null;
            }
        }

        /// <summary>
        /// 写日志的线程
        /// </summary>
        private void ThreadWriteLogIntoTextHandler()
        {
            this.WriteLog("已开启日志线程...");
            while (Thread.CurrentThread.IsAlive)
            {
                if (this._queueTempStoreLogMessage.Count != 0)
                {
                    this.WriteFile();
                }
                this._autoEvent.WaitOne();                      // 阻止当前线程，直到当前 WaitHandle 收到信号为止
            }
        }

        /// <summary>
        /// 写文件，将日志写入文件
        /// </summary>
        private void WriteFile()
        {
            string strMessage = string.Empty;           // 用于临时存放从队列中取出的log消息

            // 将队列中的所有数据都拿出来
            lock (this._thisLock)
            {
                int intSum = this._queueTempStoreLogMessage.Count;
                for (int i = 0; i < intSum; i++)
                {
                    strMessage += this._queueTempStoreLogMessage.Dequeue();
                }
            }

            //判断文件路径是否为空，为空则不添加
            if (string.IsNullOrEmpty(this._strFilePath))
                return;

            // 将日志写到文本中
            using (StreamWriter w = new StreamWriter(this._strFilePath, true, System.Text.Encoding.UTF8))
            {
                w.Write(strMessage);
                w.Flush();
                w.Close();
            }
        }

        /// <summary>
        /// 添加一个日志消息,并且将这条消息写入日志文件中
        /// </summary>
        /// <param name="strMessage">日志消息</param>
        /// <returns>写入日志的时间</returns>
        public string AddLogMessage(string strMessage)
        {
            //判断文件路径是否为空，为空则不添加
            if (string.IsNullOrEmpty(this._strFilePath))
                return string.Empty;

            //设置日志字符串格式
            string strDataTime = System.DateTime.Now.ToLongTimeString().ToString();
            string strNewLog = strDataTime + "  " + strMessage + ENDFLAG;
            //this._sbLogFileContent.Append(strTime);
            this._queueTempStoreLogMessage.Enqueue(strNewLog);
            this._autoEvent.Set();                                  // 将事件状态设置为终止状态，允许一个或多个等待线程继续

            return strDataTime;
        }       

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

    }
}
