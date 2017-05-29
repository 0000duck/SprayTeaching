using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SprayTeaching.BaseClassLib;
using SprayTeaching.Model;

namespace SprayTeaching.MyAllClass
{
    public class MyErrorOperate
    {
        private Queue<string> _queueStoreErrorOperateMessage;                       // 临时存放错误操作消息的队列 
        private Thread _thrdErrorOperateMessageHandler;                             // 数据错误操作消息处理的线程
        private bool _bolIsThreadAlive = true;                                      // 控制线程活着，true为活着，false为死亡
        private string _strCurrentErrorOperateMessage = string.Empty;               // 当前的错误操作消息

        private AutoResetEvent _autoEvent = new AutoResetEvent(false);              // 控制数据消息处理的线程，控制它的睡眠和唤醒

        public event UpdateLogContentEventHandler UpdateLogContent;                 // 更新日志文件 
        public event Action<object> UpdateErrorOperateMessage;

        public MyErrorOperate( )
        {
            this._queueStoreErrorOperateMessage = new Queue<string>();
            this._thrdErrorOperateMessageHandler = new Thread(this.ThrdErrorOperateMessageHandler);
            this._thrdErrorOperateMessageHandler.IsBackground = true;
            this._thrdErrorOperateMessageHandler.Name = "ErrorOperateMessage";
            this._thrdErrorOperateMessageHandler.Start();
        }

        #region 关闭资源
        /// <summary>
        /// 关闭MyLog的所有资源,先关闭线程，再使变量无效
        /// </summary>
        public void Close( )
        {
            this.CloseThreadDataMessageHandler();
            this.CloseAllVariable();
        }

        /// <summary>
        /// 关闭所有变量，使它们都invalidition
        /// </summary>
        private void CloseAllVariable( )
        {
            this._thrdErrorOperateMessageHandler = null;
            this._bolIsThreadAlive = false;
            this._autoEvent = null;
            this._queueStoreErrorOperateMessage = null;
        }

        /// <summary>
        /// 关闭写日志到文件的线程
        /// </summary>
        private void CloseThreadDataMessageHandler( )
        {
            if (this._thrdErrorOperateMessageHandler != null)
            {
                this._bolIsThreadAlive = false;             // 关闭线程
                Thread.Sleep(110);                          // 等待线程关闭
                this._thrdErrorOperateMessageHandler.Abort();
                this._thrdErrorOperateMessageHandler = null;
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

        /// <summary>
        /// 处理错误操作消息的线程
        /// </summary>
        private void ThrdErrorOperateMessageHandler( )
        {
            Thread.Sleep(1);        // 延迟启动，为避免初始化时候出现问题
            this.WriteLogHandler("已开启错误操作消息线程.");
            while (this._bolIsThreadAlive)
            {
                if (this._queueStoreErrorOperateMessage.Count != 0)
                {
                    this.ErrorOperateMessageHandler();
                }
                this._autoEvent.WaitOne();                      // 阻止当前线程，直到当前 WaitHandle 收到信号为止
            }
        }

        /// <summary>
        /// 对错误操作消息具体处理
        /// </summary>
        private void ErrorOperateMessageHandler( )
        {
            while (this._queueStoreErrorOperateMessage.Count != 0)
            {
                string strMessage = this._queueStoreErrorOperateMessage.Dequeue();
                string strTmp = string.Empty;

                // 为了实现错误消息的闪烁效果，闪烁4次，共花费8s
                for (int i = 0; i < 3; i++)
                {
                    strTmp = strMessage;
                    this.UpdateErrorOperateMessageHandler(strTmp);          // 赋值错误消息，相当于显示消息
                    Thread.Sleep(1000);
                    strTmp = string.Empty;
                    this.UpdateErrorOperateMessageHandler(strTmp);          // 赋值空字符串，相当于隐藏消息
                    Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// 添加一条错误消息
        /// </summary>
        /// <param name="strMessage">消息</param>
        public void AddErrorOperateMessage(string strMessage)
        {
            // 如果这条消息和上条消息相同，则不进行入队操作，即不进行重复显示提示
            //if (this._strCurrentErrorOperateMessage != strMessage)
            //{
                this._queueStoreErrorOperateMessage.Enqueue(strMessage);
                this._autoEvent.Set();                                      // 将事件状态设置为终止状态，允许一个或多个等待线程继续
            //}
            this._strCurrentErrorOperateMessage = strMessage;               // 更新当前传入的错误操作的消息

        }

        /// <summary>
        /// 更新界面中的错误消息的内容
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateErrorOperateMessageHandler(object obj)
        {
            if (this.UpdateErrorOperateMessage != null)
                this.UpdateErrorOperateMessage(obj);
        }

    }
}
