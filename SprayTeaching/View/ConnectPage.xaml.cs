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

namespace SprayTeaching.View
{
    /// <summary>
    /// ConnectPage.xaml 的交互逻辑
    /// </summary>
    public partial class ConnectPage : Page
    {
        public ConnectPage()
        {
            InitializeComponent();
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            string strWay = rb.Tag.ToString();
            switch(strWay)
            {
                case "SerialPortWay":
                    this.gridCommunicate.ColumnDefinitions[0].Width = GridLength.Auto;
                    this.gridCommunicate.ColumnDefinitions[1].Width = new GridLength(0);
                    break;
                case "WifiWay":
                    this.gridCommunicate.ColumnDefinitions[0].Width = new GridLength(0);
                    this.gridCommunicate.ColumnDefinitions[1].Width = GridLength.Auto;
                    break;
            }
        }
    }
}
