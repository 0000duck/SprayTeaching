using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SprayTeaching.BaseClassLib;

namespace SprayTeaching.MyAllClass
{
    public class MyFTPClient
    {
        private MyFTPHelper _myFTPHelper;       // FTP对象

        //private string _strFtpRemotePath;
        private string _strFtpUserID;
        private string _strFtpPassword;
        private string _strFtpURI;
        private string _strFtpServerIP;

        public MyFTPClient(string FtpServerIP, string FtpUserID, string FtpPassword)
        {
            this._myFTPHelper = new MyFTPHelper(FtpServerIP, FtpUserID, FtpPassword);
            this._strFtpServerIP = this._myFTPHelper.FtpServerIP;
            this._strFtpUserID = this._myFTPHelper.FtpUserID;
            this._strFtpPassword = this._myFTPHelper.FtpPassword;
            this._strFtpURI = this._myFTPHelper.FtpURI;
        }

        public void UpLoad(string strFileName)
        {
            this._myFTPHelper.Upload(strFileName);
        }
    }
}
