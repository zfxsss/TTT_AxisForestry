using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TTTCommunication;

namespace TTTApp.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class ServerListViewModel : ViewModelBase
    {
        /// <summary>
        /// 
        /// </summary>
        public Comm comm { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public RelayCommand StartSearchingCmd { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public RelayCommand StopSearchingCmd { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public RelayCommand<MetroWindow> ConnectCmd { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string SelectedIp { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ReturnedIp { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool SetReturnedIp { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        private bool startSearchingEnabled;

        /// <summary>
        /// 
        /// </summary>
        public bool StartSearchingEnabled
        {
            get
            {
                return startSearchingEnabled;
            }
            set
            {
                startSearchingEnabled = value;
                RaisePropertyChanged("StartSearchingEnabled");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool stopSearchingEnabled;

        /// <summary>
        /// 
        /// </summary>
        public bool StopSearchingEnabled
        {
            get
            {
                return stopSearchingEnabled;
            }
            set
            {
                stopSearchingEnabled = value;
                RaisePropertyChanged("StopSearchingEnabled");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool connectEnabled;

        /// <summary>
        /// 
        /// </summary>
        public bool ConnectEnabled
        {
            get
            {
                return connectEnabled;
            }
            set
            {
                connectEnabled = value;
                RaisePropertyChanged("ConnectEnabled");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool dataGridEnabled;

        /// <summary>
        /// 
        /// </summary>
        public bool DataGridEnabled
        {
            get
            {
                return dataGridEnabled;
            }
            set
            {
                dataGridEnabled = value;
                RaisePropertyChanged("DataGridEnabled");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool isIndeterminate;

        /// <summary>
        /// 
        /// </summary>
        public bool IsIndeterminate
        {
            get
            {
                return isIndeterminate;
            }
            set
            {
                isIndeterminate = value;
                RaisePropertyChanged("IsIndeterminate");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private CancellationTokenSource tokenSrc;

        /// <summary>
        /// 
        /// </summary>
        private IDialogCoordinator dialogCoordinator = DialogCoordinator.Instance;

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<string> Ips { get; private set; } = new ObservableCollection<string>();

        /// <summary>
        /// 
        /// </summary>
        public ServerListViewModel()
        {
            StartSearchingCmd = new RelayCommand(async () =>
            {
                try
                {
                    StartSearchingEnabled = false;
                    StopSearchingEnabled = true;
                    ConnectEnabled = false;
                    DataGridEnabled = false;
                    IsIndeterminate = true;

                    Ips.Clear();

                    tokenSrc = new CancellationTokenSource();
                    comm.BroadcastReceived += AddIp;

                    await comm.ReceiveBroadcast(tokenSrc.Token);
                }
                catch
                {

                    IsIndeterminate = false;

                    try
                    {
                        dialogCoordinator.ShowModalMessageExternal(this,
                              "Searching Stopped",
                              "",
                              MessageDialogStyle.Affirmative,
                              new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });
                    }
                    catch { }
                }
                finally
                {
                    tokenSrc.Dispose();
                    tokenSrc = null;

                    StopSearchingEnabled = false;
                    StartSearchingEnabled = true;
                    DataGridEnabled = true;

                    comm.BroadcastReceived -= AddIp;
                }
            });

            StopSearchingCmd = new RelayCommand(() =>
            {
                tokenSrc.Cancel();
            });

            ConnectCmd = new RelayCommand<MetroWindow>((wnd) =>
            {
                SetReturnedIp = true;
                wnd.Close();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void CleanupTask()
        {
            if (tokenSrc != null)
            {
                tokenSrc.Cancel();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        private void AddIp(string ip)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!Ips.Contains(ip))
                    Ips.Add(ip);
            });
        }

    }
}
