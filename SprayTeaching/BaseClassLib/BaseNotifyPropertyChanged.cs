using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprayTeaching.BaseClassLib
{
    public class BaseNotifyPropertyChanged : INotifyPropertyChanged
    {
        /// <summary>
        /// 继承接口INotifyPropertyChanged对应的属性改变事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 属性发生改变时调用该方法发出通知
        /// </summary>
        /// <param name="strPropertyName">属性名</param>
        public void RaisePropertyChanged( string strPropertyName )
        {
            if (PropertyChanged != null)
            {
                PropertyChanged( this, new PropertyChangedEventArgs( strPropertyName ) );
            }
        }
    }
}
