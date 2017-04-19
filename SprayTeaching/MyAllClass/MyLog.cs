using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SprayTeaching.MyAllClass
{
    public class MyLog
    {
        private string _strFilePath;                //日志文件的路径
        private StringBuilder _sbLogFileContent;    //日志文件的内容
        private string ENDFLAG = "\r\n";            //换行结束符

        public MyLog(string strFilePath)
        {
            this._strFilePath = strFilePath;
            this._sbLogFileContent = new StringBuilder();
        }

        /// <summary>
        /// 添加一个日志消息,并且将这条消息写入日志文件中
        /// </summary>
        /// <param name="strMessage">日志消息</param>
        /// <returns>经过转换过的日志消息</returns>
        public string AddLogContent(string strMessage)
        {
            //判断文件路径是否为空，为空则不添加
            if (string.IsNullOrEmpty(this._strFilePath))
                return string.Empty;

            //设置日志字符串格式
            string strData = System.DateTime.Now.ToString() + ENDFLAG;
            string strNewLog = strData + strMessage + ENDFLAG;
            this._sbLogFileContent.Append(strNewLog);

            //将日志写到文本中
            FileStream fileStream = new FileStream(this._strFilePath, FileMode.Append);
            using (StreamWriter w = new StreamWriter(this._strFilePath, true, System.Text.Encoding.UTF8))
            {
                w.Write(strNewLog);
                w.Flush();
                w.Close();
            }
            return strNewLog;
        }
    }
}
