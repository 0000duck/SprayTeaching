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

using SprayTeaching.ViewModel;

namespace SprayTeaching
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow( )
        {
            InitializeComponent( );
            this.DataContext = new ConnectViewVM( );
        }

        private void Button_PageExchange( object sender, RoutedEventArgs e )
        {
            Control btn = sender as Control;
            string strTag = (string)btn.Tag;
            switch (strTag)
            {
                case "PAGE_HOME":
                    break;
                case "PAGE_CONNECT":
                    break;
                case "PAGE_CONTROL":
                    break;
                case "PAGE_UPDATE":
                    break;
                case "PAGE_WATCH":
                    break;
                case "PAGE_CALIBRATION":
                    break;
                case "PAGE_SETUP":
                    break;
            }
        }
    }
}
