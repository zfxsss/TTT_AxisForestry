using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TTTCommunication;
using TTTCommunication.Exceptions;
using TTTHttpClient;

namespace TTTApp.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            comm = new Comm();

            ButtonCmd = new RelayCommand<string>(async num =>
            {
                if (comm.CommClient == null)
                {
                    try
                    {
                        if (apiWaiting)
                        {
                            return;
                        }

                        apiWaiting = true;

                        var numInt = int.Parse(num);
                        if (btnStatus[numInt] != null) return;

                        btnStatus[numInt] = "O";
                        UpdateButtonStatus(numInt);

                        if (await WinCheck("O") != false)
                        {
                            await Reset();
                            return;
                        }

                        await APICall();
                    }
                    catch (Exception ex)
                    {
                        dialogCoordinator.ShowModalMessageExternal(this,
                              "Message",
                              ex.Message,
                              MessageDialogStyle.Affirmative,
                              new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });
                    }
                    finally
                    {
                        apiWaiting = false;
                    }
                }
                else
                {
                    try
                    {
                        if (myTurn == false)
                        {
                            var controller = await dialogCoordinator.ShowProgressAsync(this,
                                "Waiting for your opponent...",
                                "",
                                false,
                                new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });
                            controller.SetIndeterminate();

                            await Task.Delay(1500);
                            await controller.CloseAsync();
                            return;
                        }

                        byte pos = byte.Parse(num);
                        if (btnStatus[pos] != null) return;

                        comm.SendPosition(pos);

                        btnStatus[pos] = "O";
                        UpdateButtonStatus(pos);

                        if (await WinCheck("O") != false)
                        {
                            await Reset();
                            return;
                        }

                        SetTurn(false);
                    }
                    catch (Exception ex)
                    {
                        dialogCoordinator.ShowModalMessageExternal(this,
                             "Message",
                             ex.Message,
                             MessageDialogStyle.Affirmative,
                             new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });
                    }
                    finally
                    {
                        if (comm.CommClient == null)
                        {
                            dialogCoordinator.ShowModalMessageExternal(this,
                             "Message",
                             "Connection Lost",
                             MessageDialogStyle.Affirmative,
                             new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });
                        }

                        SearchListenerCmd.RaiseCanExecuteChanged();
                        StartListenerCmd.RaiseCanExecuteChanged();
                        DisconnectCmd.RaiseCanExecuteChanged();
                    }
                }
            });

            StartListenerCmd = new RelayCommand(async () =>
            {
                var cancellationTokenSrc = new CancellationTokenSource();
                ProgressDialogController controller = null;
                try
                {
                    controller = await dialogCoordinator.ShowProgressAsync(this,
                        "Waiting Incoming Connection...",
                        "",
                        true,
                        new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });

                    controller.SetIndeterminate();

                    controller.Canceled += (o, e) =>
                    {
                        cancellationTokenSrc.Cancel();
                    };

                    await comm.StartServer(cancellationTokenSrc.Token);

                    //await Task.Run(async () =>
                    //{
                    //    await comm.StartServer(cancellationTokenSrc.Token);
                    //});
                }
                catch
                {
                    if (comm.CommClient == null)
                    {
                        dialogCoordinator.ShowModalMessageExternal(this,
                              "Message",
                              "Waiting Cancelled",
                              MessageDialogStyle.Affirmative,
                              new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });
                    }
                }
                finally
                {
                    cancellationTokenSrc.Dispose();
                    await controller.CloseAsync();
                    SearchListenerCmd.RaiseCanExecuteChanged();
                    StartListenerCmd.RaiseCanExecuteChanged();
                    DisconnectCmd.RaiseCanExecuteChanged();
                }
            },
            () =>
            {
                return comm.CommClient == null;
            });

            comm.CommunicationEstablished += async (isServer) =>
            {
                try
                {
                    comm.TcpReceivingTask.Start();

                    await Application.Current.Dispatcher.InvokeAsync(async () =>
                    {
                        await Task.Delay(300);
                        var controller = await dialogCoordinator.ShowProgressAsync(this,
                            "Communication Established!",
                            "",
                            false,
                            new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });
                        controller.SetIndeterminate();
                        await Task.Delay(1500).ContinueWith(async (t) => { await controller.CloseAsync(); });
                    });

                    await Reset();

                    if (isServer)
                    {
                        SetTurn(false);
                        meFirst = false;
                    }
                    else
                    {
                        SetTurn(true);
                        meFirst = true;
                    }

                    await comm.TcpReceivingTask;
                }
                catch (Exception ex)
                {
                    if (ex is CommunicationTaskException) //Communication Ended
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            dialogCoordinator.ShowModalMessageExternal(this,
                                  "Message",
                                  "Communication Ended",
                                  MessageDialogStyle.Affirmative,
                                  new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });
                        });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            dialogCoordinator.ShowModalMessageExternal(this,
                                  "Message",
                                  ex.Message,
                                  MessageDialogStyle.Affirmative,
                                  new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });
                        });
                    }
                }
                finally
                {
                    await Application.Current.Dispatcher.Invoke(async () =>
                    {
                        await Reset();
                        SearchListenerCmd.RaiseCanExecuteChanged();
                        StartListenerCmd.RaiseCanExecuteChanged();
                        DisconnectCmd.RaiseCanExecuteChanged();
                    });
                }
            };

            SearchListenerCmd = new RelayCommand(async () =>
            {
                try
                {
                    new ServerList(comm).ShowDialog();
                    var vM = SimpleIoc.Default.GetInstance<ServerListViewModel>();

                    if (vM.ReturnedIp != null)
                    {
                        await comm.ConnectRemoteServer(vM.ReturnedIp);
                    }
                }
                catch (Exception ex)
                {
                    dialogCoordinator.ShowModalMessageExternal(this,
                              "Message",
                              ex.Message,
                              MessageDialogStyle.Affirmative,
                              new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });
                }
                finally
                {
                    SearchListenerCmd.RaiseCanExecuteChanged();
                    StartListenerCmd.RaiseCanExecuteChanged();
                    DisconnectCmd.RaiseCanExecuteChanged();
                }
            },
            () =>
            {
                return comm.CommClient == null;
            });

            comm.NewPositionReceived += (pos) =>
            {
                Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    btnStatus[pos] = "X";
                    UpdateButtonStatus(pos);

                    if (await WinCheck("X") != false)
                    {
                        await Reset();
                        return;
                    }

                    SetTurn(true);
                });
            };

            DisconnectCmd = new RelayCommand(() =>
            {
                try
                {
                    comm.CloseClient();
                }
                catch { }
                finally
                {
                    SearchListenerCmd.RaiseCanExecuteChanged();
                    StartListenerCmd.RaiseCanExecuteChanged();
                    DisconnectCmd.RaiseCanExecuteChanged();
                }
            },
            () =>
            {
                return comm.CommClient != null;
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public RelayCommand<string> ButtonCmd { get; }

        /// <summary>
        /// 
        /// </summary>
        public RelayCommand StartListenerCmd { get; }

        /// <summary>
        /// 
        /// </summary>
        public RelayCommand DisconnectCmd { get; }

        /// <summary>
        /// 
        /// </summary>
        public RelayCommand SearchListenerCmd { get; }

        /// <summary>
        /// 
        /// </summary>
        string[] btnStatus = new string[] { null, null, null, null, null, null, null, null, null };

        /// <summary>
        /// 
        /// </summary>
        private bool apiWaiting = false;

        /// <summary>
        /// 
        /// </summary>
        private IDialogCoordinator dialogCoordinator = DialogCoordinator.Instance;

        /// <summary>
        /// 
        /// </summary>
        private bool meFirst = true;

        /// <summary>
        /// 
        /// </summary>
        private Comm comm;

        /// <summary>
        /// 
        /// </summary>
        private bool? myTurn;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isMyTurn"></param>
        private void SetTurn(bool? isMyTurn)
        {
            myTurn = isMyTurn;
            RaisePropertyChanged("Prompt");
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateButtonStatus(int num)
        {
            if (num == 0)
            {
                Button0Symbol = btnStatus[0];
            }
            else if (num == 1)
            {
                Button1Symbol = btnStatus[1];
            }
            else if (num == 2)
            {
                Button2Symbol = btnStatus[2];
            }
            else if (num == 3)
            {
                Button3Symbol = btnStatus[3];
            }
            else if (num == 4)
            {
                Button4Symbol = btnStatus[4];
            }
            else if (num == 5)
            {
                Button5Symbol = btnStatus[5];
            }
            else if (num == 6)
            {
                Button6Symbol = btnStatus[6];
            }
            else if (num == 7)
            {
                Button7Symbol = btnStatus[7];
            }
            else if (num == 8)
            {
                Button8Symbol = btnStatus[8];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Prompt
        {
            get
            {
                if (myTurn == true)
                {
                    return "It's you turn";
                }
                else if (myTurn == false)
                {
                    return "Waiting...";
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string button0Symbol;

        /// <summary>
        /// 
        /// </summary>
        public string Button0Symbol
        {
            get
            {
                return button0Symbol;
            }
            set
            {
                button0Symbol = value;
                RaisePropertyChanged("Button0Symbol");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string button1Symbol;

        /// <summary>
        /// 
        /// </summary>
        public string Button1Symbol
        {
            get
            {
                return button1Symbol;
            }
            set
            {
                button1Symbol = value;
                RaisePropertyChanged("Button1Symbol");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string button2Symbol;

        /// <summary>
        /// 
        /// </summary>
        public string Button2Symbol
        {
            get
            {
                return button2Symbol;
            }
            set
            {
                button2Symbol = value;
                RaisePropertyChanged("Button2Symbol");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string button3Symbol;

        /// <summary>
        /// 
        /// </summary>
        public string Button3Symbol
        {
            get
            {
                return button3Symbol;
            }
            set
            {
                button3Symbol = value;
                RaisePropertyChanged("Button3Symbol");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string button4Symbol;

        /// <summary>
        /// 
        /// </summary>
        public string Button4Symbol
        {
            get
            {
                return button4Symbol;
            }
            set
            {
                button4Symbol = value;
                RaisePropertyChanged("Button4Symbol");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private string button5Symbol;

        /// <summary>
        /// 
        /// </summary>
        public string Button5Symbol
        {
            get
            {
                return button5Symbol;
            }
            set
            {
                button5Symbol = value;
                RaisePropertyChanged("Button5Symbol");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string button6Symbol;

        /// <summary>
        /// 
        /// </summary>
        public string Button6Symbol
        {
            get
            {
                return button6Symbol;
            }
            set
            {
                button6Symbol = value;
                RaisePropertyChanged("Button6Symbol");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string button7Symbol;

        /// <summary>
        /// 
        /// </summary>
        public string Button7Symbol
        {
            get
            {
                return button7Symbol;
            }
            set
            {
                button7Symbol = value;
                RaisePropertyChanged("Button7Symbol");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string button8Symbol;

        /// <summary>
        /// 
        /// </summary>
        public string Button8Symbol
        {
            get
            {
                return button8Symbol;
            }
            set
            {
                button8Symbol = value;
                RaisePropertyChanged("Button8Symbol");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool gameBoardEnabled = true;

        /// <summary>
        /// 
        /// </summary>
        public bool GameBoardEnabled
        {
            get
            {
                return gameBoardEnabled;
            }
            set
            {
                gameBoardEnabled = value;
                RaisePropertyChanged("GameBoardEnabled");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task<bool?> WinCheck(string player)
        {
            bool? check = false;

            if (btnStatus.Count(x => x == null) <= 4)
            {
                if (btnStatus[0] == btnStatus[1] && btnStatus[1] == btnStatus[2] && (btnStatus[0] == "O" || btnStatus[0] == "X"))
                {
                    check = true;
                }

                if (btnStatus[3] == btnStatus[4] && btnStatus[4] == btnStatus[5] && (btnStatus[3] == "O" || btnStatus[3] == "X"))
                {
                    check = true;
                }

                if (btnStatus[6] == btnStatus[7] && btnStatus[7] == btnStatus[8] && (btnStatus[6] == "O" || btnStatus[6] == "X"))
                {
                    check = true;
                }

                if (btnStatus[0] == btnStatus[3] && btnStatus[3] == btnStatus[6] && (btnStatus[0] == "O" || btnStatus[0] == "X"))
                {
                    check = true;
                }

                if (btnStatus[1] == btnStatus[4] && btnStatus[4] == btnStatus[7] && (btnStatus[1] == "O" || btnStatus[1] == "X"))
                {
                    check = true;
                }

                if (btnStatus[2] == btnStatus[5] && btnStatus[5] == btnStatus[8] && (btnStatus[2] == "O" || btnStatus[2] == "X"))
                {
                    check = true;
                }

                if (btnStatus[0] == btnStatus[4] && btnStatus[4] == btnStatus[8] && (btnStatus[0] == "O" || btnStatus[0] == "X"))
                {
                    check = true;
                }

                if (btnStatus[2] == btnStatus[4] && btnStatus[4] == btnStatus[6] && (btnStatus[2] == "O" || btnStatus[2] == "X"))
                {
                    check = true;
                }
            }

            if (check == true)
            {
                GameBoardEnabled = false;
                await Task.Delay(1000);

                dialogCoordinator.ShowModalMessageExternal(this,
                          player == "O" ? "You Won!" : "You Lost!",
                          "",
                          MessageDialogStyle.Affirmative,
                          new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });

                return true;
            }

            if (btnStatus.Count(x => x == null) == 0)
            {
                GameBoardEnabled = false;
                await Task.Delay(1000);

                dialogCoordinator.ShowModalMessageExternal(this,
                           "It's a tie!",
                           "",
                           MessageDialogStyle.Affirmative,
                           new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });

                return null;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task APICall()
        {
            var situation = "";

            foreach (var s in btnStatus)
            {
                if (s == null)
                {
                    situation += "-";
                }
                else
                {
                    situation += s;
                }
            }

            var d = await SmartClient.GetMyRecommendation(situation, "X");

            if (d != null)
            {
                btnStatus[(int)d.recommendation] = "X";
                UpdateButtonStatus((int)d.recommendation);

                if (await WinCheck("X") != false)
                {
                    await Reset();
                    return;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private async Task Reset()
        {
            btnStatus = new string[] { null, null, null, null, null, null, null, null, null };

            for (var i = 0; i < 9; i++)
            {
                UpdateButtonStatus(i);
            }

            GameBoardEnabled = true;
            meFirst = !meFirst;
            SetTurn(null);

            if (comm.CommClient == null)
            {
                if (!meFirst)
                {
                    try
                    {
                        apiWaiting = true;

                        await APICall();
                    }
                    catch (Exception ex)
                    {
                        dialogCoordinator.ShowModalMessageExternal(this,
                              "Message",
                              ex.Message,
                              MessageDialogStyle.Affirmative,
                              new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });
                    }
                    finally
                    {
                        apiWaiting = false;
                    }
                }
            }
            else
            {
                if (!meFirst)
                    SetTurn(false);
                else
                    SetTurn(true);
            }
        }
    }
}