using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TTTCommunication.Exceptions;

namespace TTTCommunication
{
    /// <summary>
    /// 
    /// </summary>
    public class Comm
    {
        /// <summary>
        /// 
        /// </summary>
        public TcpListener Listener { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public TcpClient CommClient { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public UdpClient BroadcastSender { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public event Action<byte> NewPositionReceived;

        /// <summary>
        /// 
        /// </summary>
        public event Action<byte, byte, byte> GameInformationReceived;

        /// <summary>
        /// 
        /// </summary>
        public event Action<byte, byte, byte, byte> OtherMessageReceived;

        /// <summary>
        /// 
        /// </summary>
        public event Action<bool> CommunicationEstablished;

        /// <summary>
        /// 
        /// </summary>
        public event Action<string> BroadcastReceived;

        /// <summary>
        /// 
        /// </summary>
        public Task TcpReceivingTask { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        private byte[] buffer = new byte[] { };

        /// <summary>
        /// 
        /// </summary>
        public async Task StartServer(CancellationToken token)
        {
            bool accepted = false;
            bool listenerReturn = false;

            try
            {
                var commEndPoint = new IPEndPoint(IPAddress.Any, 10080);
                Listener = new TcpListener(commEndPoint);
                BroadcastSender = new UdpClient();

                Listener.Start();

                var taskAccept = Task.Run(() =>
                {
                    try
                    {
                        CommClient = Listener.AcceptTcpClient();
                        accepted = true;
                    }
                    finally
                    {
                        listenerReturn = true;
                    }
                });

                var taskBroadcast = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (listenerReturn) break;

                        try
                        {
                            var remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, 10080);
                            BroadcastSender.Send(new byte[] { 0xcc, 0xdd }, 2, remoteEndPoint);
                        }
                        catch { }

                        await Task.Delay(2000);
                    }
                });

                var cancellationCheck = Task.Run(async () =>
                {
                    try
                    {
                        while (true)
                        {
                            await Task.Delay(500);

                            if (token.IsCancellationRequested)
                            {
                                if (!accepted)
                                {
                                    Listener.Stop();
                                }

                                break;
                            }

                            if (listenerReturn) break;
                        }
                    }
                    catch { }
                });

                await taskAccept;
            }
            catch
            {             
                throw;
            }
            finally
            {
                Listener.Stop();
                BroadcastSender.Close();

                if ((!accepted) && (CommClient != null))
                {
                    CommClient.Close();
                    CommClient = null;
                }

                if (accepted)
                {
                    SetUpReceivingTask();
                    CommunicationEstablished?.Invoke(true);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        public void SendPosition(byte position)
        {
            if (CommClient != null)
            {
                CommClient.Client.Send(new byte[] { 0x01, position, 0x00, 0x00 });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wins"></param>
        /// <param name="losses"></param>
        /// <param name="ties"></param>
        public void SendGameInformation(byte wins, byte losses, byte ties)
        {
            if (CommClient != null)
            {
                CommClient.Client.Send(new byte[] { 0x02, wins, losses, ties });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="byte1"></param>
        /// <param name="byte2"></param>
        /// <param name="byte3"></param>
        /// <param name="byte4"></param>
        public void SendOtherMessage(byte byte1, byte byte2, byte byte3, byte byte4)
        {
            if (CommClient != null)
            {
                CommClient.Client.Send(new byte[] { byte1, byte2, byte3, byte4 });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public async Task ReceiveBroadcast(CancellationToken token)
        {
            UdpClient client = null;
            Task t = null;

            try
            {
                client = new UdpClient(10080);

                var t1 = Task.Run(() =>
                {
                    while (true)
                    {
                        var remoteEndPoint = default(IPEndPoint);
                        var bytes = client.Receive(ref remoteEndPoint);

                        if (bytes.Length == 2 && bytes[0] == 0xcc && bytes[1] == 0xdd)
                        {
                            BroadcastReceived?.Invoke(remoteEndPoint.Address.ToString());
                        }
                    }
                });

                var t2 = Task.Run(() =>
                {
                    var count = 0;

                    while (true)
                    {
                        if (count > 30) break;

                        count++;

                        if (token.IsCancellationRequested)
                        {
                            client.Close();
                            return;
                        }

                        Thread.Sleep(500);
                    }

                    client.Close();
                });

                t = Task.WhenAll(t1, t2);
                await t;
            }
            catch
            {
                if (t != null)
                {
                    throw t.Exception;
                }

                throw;
            }
            finally
            {
                if (client != null) client.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        public async Task ConnectRemoteServer(string ip)
        {
            bool connected = false;
            try
            {
                CommClient = new TcpClient();
                await CommClient.ConnectAsync(IPAddress.Parse(ip), 10080);
                connected = true;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (!connected)
                {
                    CommClient.Close();
                    CommClient = null;
                }
                else
                {
                    SetUpReceivingTask();
                    CommunicationEstablished?.Invoke(false);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void CloseClient()
        {
            if (CommClient != null)
            {
                CommClient.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetUpReceivingTask()
        {
            TcpReceivingTask = new Task(() =>
            {
                var bytes = new byte[4];

                try
                {
                    while (true)
                    {
                        var count = CommClient.Client.Receive(bytes, 4, SocketFlags.None);

                        if (count == 0)
                        {
                            throw new Exception("Remote Endpoint could be closed");
                        }

                        buffer = buffer.Concat(bytes.Take(count)).ToArray();
                        HandleBuffer();
                    }
                }
                catch (Exception ex)
                {
                    throw new CommunicationTaskException(ex);
                }
                finally
                {
                    CommClient.Close();
                    CommClient = null;
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        private void HandleBuffer()
        {
            while (buffer.Length >= 4)
            {
                var workSet = buffer.Take(4).ToArray();

                if (workSet[0] == 0x01) // Position
                {
                    NewPositionReceived?.Invoke(workSet[1]);
                }
                else if (workSet[0] == 0x02) //Game Info
                {
                    GameInformationReceived?.Invoke(workSet[1], workSet[2], workSet[3]);
                }
                else if (workSet[0] >= 0x03) //Other Message
                {
                    OtherMessageReceived?.Invoke(workSet[0], workSet[1], workSet[2], workSet[3]);
                }

                buffer = buffer.Skip(4).ToArray();
            }
        }
    }
}
