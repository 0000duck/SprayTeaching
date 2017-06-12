using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SprayTeaching.BaseClassLib;
using System.Threading;
using SprayTeaching.Model;

namespace SprayTeaching.MyAllClass
{
    public class MyConfigFileINI
    {
        #region  外部引入的函数
        // 声明INI文件的写操作函数 WritePrivateProfileString()
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        // 声明INI文件的读操作函数 GetPrivateProfileString()
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, System.Text.StringBuilder retVal, int size, string filePath);
        #endregion

        private string _strConfigFileAddress = string.Empty;        // 配置文件的地址

        /// <summary>
        /// 标定轴的角度值
        /// </summary>
        private double[] _dblCalibrateAxisAngles = new double[6];

        public double[] CalibrateAxisAngles
        {
            get { return _dblCalibrateAxisAngles; }
            set { _dblCalibrateAxisAngles = value; }
        }

        /// <summary>
        /// 标定轴的方向值
        /// </summary>
        private double[] _dblCalibrateAxisDirections = new double[6];

        public double[] CalibrateAxisDirections
        {
            get { return _dblCalibrateAxisDirections; }
            set { _dblCalibrateAxisDirections = value; }
        }

        public event UpdateLogContentEventHandler UpdateLogContent;                 // 更新日志文件  
        public event UpdateConfigFileParameterEventHandler UpdateConfigParameter;   // 更新配置文件的参数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="strAddress">配置文件的路径</param>
        public MyConfigFileINI(string strAddress)
        {
            this._strConfigFileAddress = strAddress;
            Thread thrd = new Thread(ThrdInitConfigFileParameter);  // 采用简单的线程来初始化配置文件参数  
            thrd.IsBackground = true;                               // 设置成后台线程，在前台线程结束时，所有剩余的后台线程都会停止且不会完成    
            thrd.Name = "InitConfigFileParameter";                  // 设置线程的名字
            thrd.Start();                                           // 启动线程
        }

        private void ThrdInitConfigFileParameter( )
        {
            //Thread.Sleep(100);                        // 保证对象已经创建完成后再初始化参数
            this.CheckConfigFile();                     // 初始化的时候检查有无配置文件
            this.ReadFileParameter();                   // 初始化的时候获取配置文件中的参数
            this.WriteLogHandler("已获取配置文件中机器人标定参数.");
        }

        #region 关闭资源
        /// <summary>
        /// 关闭资源
        /// </summary>
        public void Close(DataModel dm = null)
        {
            //this.WirteFileParameter(dm);
            this.WriteFileCommunicateInformation(dm);
            this.CloseAllVariable();
        }

        /// <summary>
        /// 关闭所有变量，使它们都invalidition
        /// </summary>
        private void CloseAllVariable( )
        {
            this._strConfigFileAddress = null;
            this._dblCalibrateAxisAngles = null;
            this._dblCalibrateAxisDirections = null;
        }
        #endregion

        #region 读写INI的方法
        /// <summary>
        /// 写入INI的方法
        /// </summary>
        /// <param name="section">配置节</param>
        /// <param name="key">键名</param>
        /// <param name="value">键值</param>
        /// <param name="path">路径</param>
        private void INIWrite(string section, string key, string value, string path)
        {
            // section=配置节，key=键名，value=键值，path=路径
            WritePrivateProfileString(section, key, value, path);
        }

        /// <summary>
        /// 读取INI的方法
        /// </summary>
        /// <param name="section">配置节</param>
        /// <param name="key">键名</param>
        /// <param name="path">路径</param>
        /// <returns></returns>
        private string INIRead(string section, string key, string path)
        {
            // 每次从ini中读取多少字节
            System.Text.StringBuilder temp = new System.Text.StringBuilder(255);

            // section=配置节，key=键名，temp=上面，path=路径
            GetPrivateProfileString(section, key, "", temp, 255, path);
            return temp.ToString();
        }
        #endregion

        //删除一个INI文件
        public void INIDelete(string FilePath)
        {
            File.Delete(FilePath);
        }

        /// <summary>
        /// 检查配置文件中是否有数据
        /// </summary>
        private void CheckConfigFile( )
        {
            if (!File.Exists(this._strConfigFileAddress))
            {
                this.INIWrite("CalibrateAngles", "Angle1", "234.932", this._strConfigFileAddress);
                this.INIWrite("CalibrateAngles", "Angle2", "296.565", this._strConfigFileAddress);
                this.INIWrite("CalibrateAngles", "Angle3", "307.551", this._strConfigFileAddress);
                this.INIWrite("CalibrateAngles", "Angle4", "357.561", this._strConfigFileAddress);
                this.INIWrite("CalibrateAngles", "Angle5", "324.734", this._strConfigFileAddress);
                this.INIWrite("CalibrateAngles", "Angle6", "339.719", this._strConfigFileAddress);

                this.INIWrite("CalibrateDirection", "Direction1", "1", this._strConfigFileAddress);
                this.INIWrite("CalibrateDirection", "Direction2", "-1", this._strConfigFileAddress);
                this.INIWrite("CalibrateDirection", "Direction3", "-1", this._strConfigFileAddress);
                this.INIWrite("CalibrateDirection", "Direction4", "1", this._strConfigFileAddress);
                this.INIWrite("CalibrateDirection", "Direction5", "1", this._strConfigFileAddress);
                this.INIWrite("CalibrateDirection", "Direction6", "1", this._strConfigFileAddress);

                this.INIWrite("CommunicateInformation", "IPAddress", "10.8.193.177", this._strConfigFileAddress);
                this.INIWrite("CommunicateInformation", "PortNum", "12000", this._strConfigFileAddress);
            }
        }

        #region  读取配置文件的数据
        /// <summary>
        /// 读取配置文件中的数据
        /// </summary>
        public void ReadFileParameter( )
        {
            double[] dblAngles = this.GetCalibrateAngleParameter();
            double[] dblDirections = this.GetCalibrateDirectionParameter();
            string[] strCommunicates = this.GetCommunicateInformParameter();

            this.CalibrateAxisAngles = dblAngles;
            this.CalibrateAxisDirections = dblDirections;

            object[] objParameter = new object[] { dblAngles, dblDirections, strCommunicates };
            this.GetConfigParameterHandler(objParameter);
        }

        /// <summary>
        /// 获取标定角度的参数
        /// </summary>
        /// <returns>double类型的标定角度</returns>
        private double[] GetCalibrateAngleParameter( )
        {
            string[] strAngles = new string[6];
            double[] dblAngles = new double[6];
            strAngles[0] = this.INIRead("CalibrateAngles", "Angle1", this._strConfigFileAddress);
            strAngles[1] = this.INIRead("CalibrateAngles", "Angle2", this._strConfigFileAddress);
            strAngles[2] = this.INIRead("CalibrateAngles", "Angle3", this._strConfigFileAddress);
            strAngles[3] = this.INIRead("CalibrateAngles", "Angle4", this._strConfigFileAddress);
            strAngles[4] = this.INIRead("CalibrateAngles", "Angle5", this._strConfigFileAddress);
            strAngles[5] = this.INIRead("CalibrateAngles", "Angle6", this._strConfigFileAddress);

            // 数据以double类型传出来
            dblAngles[0] = double.Parse(strAngles[0]);
            dblAngles[1] = double.Parse(strAngles[1]);
            dblAngles[2] = double.Parse(strAngles[2]);
            dblAngles[3] = double.Parse(strAngles[3]);
            dblAngles[4] = double.Parse(strAngles[4]);
            dblAngles[5] = double.Parse(strAngles[5]);
            return dblAngles;
        }

        /// <summary>
        /// 获取标定的方向参数
        /// </summary>
        /// <returns>double类型的标定方向</returns>
        private double[] GetCalibrateDirectionParameter( )
        {
            string[] strDirections = new string[6];
            double[] dblDirections = new double[6];
            strDirections[0] = this.INIRead("CalibrateDirection", "Direction1", this._strConfigFileAddress);
            strDirections[1] = this.INIRead("CalibrateDirection", "Direction2", this._strConfigFileAddress);
            strDirections[2] = this.INIRead("CalibrateDirection", "Direction3", this._strConfigFileAddress);
            strDirections[3] = this.INIRead("CalibrateDirection", "Direction4", this._strConfigFileAddress);
            strDirections[4] = this.INIRead("CalibrateDirection", "Direction5", this._strConfigFileAddress);
            strDirections[5] = this.INIRead("CalibrateDirection", "Direction6", this._strConfigFileAddress);

            // 数据以double类型传出来
            dblDirections[0] = double.Parse(strDirections[0]);
            dblDirections[1] = double.Parse(strDirections[1]);
            dblDirections[2] = double.Parse(strDirections[2]);
            dblDirections[3] = double.Parse(strDirections[3]);
            dblDirections[4] = double.Parse(strDirections[4]);
            dblDirections[5] = double.Parse(strDirections[5]);
            return dblDirections;
        }

        /// <summary>
        /// 获取通信相关的IP地址和端口号
        /// </summary>
        /// <returns></returns>
        private string[] GetCommunicateInformParameter( )
        {
            string[] strCommunicates = new string[2];
            strCommunicates[0] = this.INIRead("CommunicateInformation", "IPAddress", this._strConfigFileAddress);
            strCommunicates[1] = this.INIRead("CommunicateInformation", "PortNum", this._strConfigFileAddress);
            return strCommunicates;
        }

        #endregion

        #region  写配置文件的数据

        /// <summary>
        /// 将数据写入配置文件中
        /// </summary>
        private void WirteFileParameter(DataModel dm)
        {
            string strFileAddress = this._strConfigFileAddress;
            double[] dblCalibrateAngles = new double[6];
            double[] dblCalibrateDirections = new double[6];

            dblCalibrateAngles[0] = dm.RobotCalibrateAngle1;
            dblCalibrateAngles[1] = dm.RobotCalibrateAngle2;
            dblCalibrateAngles[2] = dm.RobotCalibrateAngle3;
            dblCalibrateAngles[3] = dm.RobotCalibrateAngle4;
            dblCalibrateAngles[4] = dm.RobotCalibrateAngle5;
            dblCalibrateAngles[5] = dm.RobotCalibrateAngle6;

            dblCalibrateDirections[0] = dm.RobotCalibrateDirection1;
            dblCalibrateDirections[1] = dm.RobotCalibrateDirection2;
            dblCalibrateDirections[2] = dm.RobotCalibrateDirection3;
            dblCalibrateDirections[3] = dm.RobotCalibrateDirection4;
            dblCalibrateDirections[4] = dm.RobotCalibrateDirection5;
            dblCalibrateDirections[5] = dm.RobotCalibrateDirection6;

            // 写入标定的角度值
            this.INIWrite("CalibrateAngles", "Angle1", dblCalibrateAngles[0].ToString(), strFileAddress);
            this.INIWrite("CalibrateAngles", "Angle2", dblCalibrateAngles[1].ToString(), strFileAddress);
            this.INIWrite("CalibrateAngles", "Angle3", dblCalibrateAngles[2].ToString(), strFileAddress);
            this.INIWrite("CalibrateAngles", "Angle4", dblCalibrateAngles[3].ToString(), strFileAddress);
            this.INIWrite("CalibrateAngles", "Angle5", dblCalibrateAngles[4].ToString(), strFileAddress);
            this.INIWrite("CalibrateAngles", "Angle6", dblCalibrateAngles[5].ToString(), strFileAddress);

            // 写入标定的方向值
            this.INIWrite("CalibrateDirection", "Direction1", dblCalibrateDirections[0].ToString(), strFileAddress);
            this.INIWrite("CalibrateDirection", "Direction2", dblCalibrateDirections[1].ToString(), strFileAddress);
            this.INIWrite("CalibrateDirection", "Direction3", dblCalibrateDirections[2].ToString(), strFileAddress);
            this.INIWrite("CalibrateDirection", "Direction4", dblCalibrateDirections[3].ToString(), strFileAddress);
            this.INIWrite("CalibrateDirection", "Direction5", dblCalibrateDirections[4].ToString(), strFileAddress);
            this.INIWrite("CalibrateDirection", "Direction6", dblCalibrateDirections[5].ToString(), strFileAddress);
        }

        /// <summary>
        /// 写入标定的角度值
        /// </summary>
        /// <param name="obj"></param>
        public void WriteFileCalibrateAngles(object obj)
        {
            string strFileAddress = this._strConfigFileAddress;
            double[] dblCalibrateAngles = (double[])obj;

            // 写入标定的角度值
            this.INIWrite("CalibrateAngles", "Angle1", dblCalibrateAngles[0].ToString(), strFileAddress);
            this.INIWrite("CalibrateAngles", "Angle2", dblCalibrateAngles[1].ToString(), strFileAddress);
            this.INIWrite("CalibrateAngles", "Angle3", dblCalibrateAngles[2].ToString(), strFileAddress);
            this.INIWrite("CalibrateAngles", "Angle4", dblCalibrateAngles[3].ToString(), strFileAddress);
            this.INIWrite("CalibrateAngles", "Angle5", dblCalibrateAngles[4].ToString(), strFileAddress);
            this.INIWrite("CalibrateAngles", "Angle6", dblCalibrateAngles[5].ToString(), strFileAddress);
        }

        /// <summary>
        /// 写入标定的方向值
        /// </summary>
        /// <param name="obj"></param>
        public void WriteFileCalibrateDirection(object obj)
        {
            string strFileAddress = this._strConfigFileAddress;
            double[] dblCalibrateDirections = (double[])obj;

            // 写入标定的方向值
            this.INIWrite("CalibrateDirection", "Direction1", dblCalibrateDirections[0].ToString(), strFileAddress);
            this.INIWrite("CalibrateDirection", "Direction2", dblCalibrateDirections[1].ToString(), strFileAddress);
            this.INIWrite("CalibrateDirection", "Direction3", dblCalibrateDirections[2].ToString(), strFileAddress);
            this.INIWrite("CalibrateDirection", "Direction4", dblCalibrateDirections[3].ToString(), strFileAddress);
            this.INIWrite("CalibrateDirection", "Direction5", dblCalibrateDirections[4].ToString(), strFileAddress);
            this.INIWrite("CalibrateDirection", "Direction6", dblCalibrateDirections[5].ToString(), strFileAddress);
        }

        private void WriteFileCommunicateInformation(object obj)
        {
            if (obj == null) { return; }

            DataModel dm = (DataModel)obj;
            string strIPAddress = dm.SocketIPAddress;
            string strPortNum = dm.SocketPortNum.ToString();
            this.INIWrite("CommunicateInformation", "IPAddress", strIPAddress, this._strConfigFileAddress);
            this.INIWrite("CommunicateInformation", "PortNum", strPortNum, this._strConfigFileAddress);
        }

        #endregion

        #region  更新配置文件的参数
        private void GetConfigParameterHandler(object obj)
        {
            if (this.UpdateConfigParameter != null)
            {
                this.UpdateConfigParameter(obj);
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
    }
}
