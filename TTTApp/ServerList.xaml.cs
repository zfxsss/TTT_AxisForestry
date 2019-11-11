using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TTTApp.ViewModel;
using TTTCommunication;

namespace TTTApp
{
    /// <summary>
    /// ServerList.xaml 的交互逻辑
    /// </summary>
    public partial class ServerList
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="comm"></param>
        public ServerList(Comm comm)
        {
            InitializeComponent();

            var vM = SimpleIoc.Default.GetInstance<ServerListViewModel>();
            vM.comm = comm;

            DataContext = vM;

            ContentRendered += (o, e) =>
            {
                vM.StartSearchingCmd.Execute(null);
            };
        }

        /// <summary>
        /// 
        /// </summary>
        static ServerList()
        {
            if (!SimpleIoc.Default.IsRegistered<ServerListViewModel>())
            {
                SimpleIoc.Default.Register<ServerListViewModel>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchingWnd_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var vM = (ServerListViewModel)DataContext;
            vM.CleanupTask();

            if (vM.SetReturnedIp)
            {
                if (vM.SelectedIp != null)
                {
                    vM.ReturnedIp = vM.SelectedIp;
                }
            }
            else
            {
                vM.ReturnedIp = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var vM = (ServerListViewModel)DataContext;
                vM.SelectedIp = (string)e.AddedItems[0];
                vM.ConnectEnabled = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchingWnd_Closed(object sender, EventArgs e)
        {
            var vM = (ServerListViewModel)DataContext;
            vM.SetReturnedIp = false;
        }
    }
}
