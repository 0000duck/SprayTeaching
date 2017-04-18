using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Navigation;
using SprayTeaching.View;
using SprayTeaching.ViewModel;

namespace SprayTeaching
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private ConnectViewVM _viewViewModel;           //数据上下文的对象
        private Dictionary<string, Page> _dicPage;      //用于存放page名和page对象的字典

        public MainWindow()
        {
            InitializeComponent();
            InitVar();
        }

        /// <summary>
        /// 初始化所有变量
        /// </summary>
        private void InitVar()
        {
            this._viewViewModel = new ConnectViewVM();
            this.DataContext = this._viewViewModel;
            InitDictionary();
        }

        /// <summary>
        /// 初始化字典，内含键值对，键为page名的字符串，名为对应的page对象
        /// </summary>
        private void InitDictionary()
        {
            this._dicPage = new Dictionary<string, Page>();

            string strPagName = "PAGE_HOME";
            Page objPage = new HomePage();
            objPage.DataContext = this._viewViewModel;
            this._dicPage.Add(strPagName, objPage);

            strPagName = "PAGE_CONNECT";
            objPage = new ConnectPage();
            objPage.DataContext = this._viewViewModel;
            this._dicPage.Add(strPagName, objPage);

            strPagName = "PAGE_CONTROL";
            objPage = new ControlPage();
            objPage.DataContext = this._viewViewModel;
            this._dicPage.Add(strPagName, objPage);

            strPagName = "PAGE_UPDATE";
            objPage = new UpdatePage();
            objPage.DataContext = this._viewViewModel;
            this._dicPage.Add(strPagName, objPage);

            strPagName = "PAGE_CALIBRATION";
            objPage = new CalibrationPage();
            objPage.DataContext = this._viewViewModel;
            this._dicPage.Add(strPagName, objPage);

            strPagName = "PAGE_SETUP";
            objPage = new SetupPage();
            objPage.DataContext = this._viewViewModel;
            this._dicPage.Add(strPagName, objPage);
        }

        private void Button_PageExchange(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string strTag = btn.Tag as string;
            this.frmPage.Content = this._dicPage[strTag];
        }

        /// <summary>
        /// 是否显示查看数据窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBoxWatch_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chkWatch = sender as CheckBox;
            if (chkWatch.IsChecked == true)
            {
                this.gridBottom.ColumnDefinitions[1].Width = GridLength.Auto;
                //this.gridBottom.ColumnDefinitions[1].Width = new GridLength(100);
            }
            else
            {
                this.gridBottom.ColumnDefinitions[1].Width = new GridLength(1);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
