using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SprayTeaching.BaseClassLib
{
    public class MyCommand : ICommand
    {
        #region 构造函数的重载
        /// <summary>
        /// 创建一个命令
        /// </summary>
        /// <param name="execute">命令要执行的方法</param>
        public MyCommand( Action<object> execute ) : this( execute, null )
        {
        }

        /// <summary>
        /// 创建一个命令
        /// </summary>
        /// <param name="execute">命令要执行的方法</param>
        /// <param name="canExecute">判断命令是否能够执行的方法</param>
        public MyCommand( Action<object> execute, Func<object, bool> canExecute )
        {
            _execute = execute;
            _canExecute = canExecute;
        }
        #endregion

        #region 重要的两个参数，一个是能否执行判断，一个是执行
        /// <summary>
        /// 判断命令是否可以执行的方法
        /// </summary>
        private Func<object, bool> _canExecute;

        /// <summary>
        /// 命令需要执行的方法
        /// </summary>
        private Action<object> _execute;
        #endregion

        #region  对ICommand接口中的内容进行实现

        /// <summary>
        /// 检查命令是否可以执行的事件，在UI事件发生导致控件状态或数据发生变化时触发
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (this._canExecute != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }
            remove
            {
                if (this._canExecute != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        /// <summary>
        /// 判断命令是否可以执行
        /// </summary>
        /// <param name="parameter">外界传入的参数</param>
        /// <returns>是否可以执行</returns>
        public bool CanExecute( object parameter )
        {
            if (this._canExecute == null) return true;
            return this._canExecute( parameter );
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameter">外界传入的参数</param>
        public void Execute( object parameter )
        {
            if (this.CanExecute(parameter))
            {
                this._execute( parameter );
            }
        }
        #endregion

       
       
    }
}
