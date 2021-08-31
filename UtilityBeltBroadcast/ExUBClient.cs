using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UBNetworking.Lib;

namespace UtilityBeltBroadcast
{
    public class ExUBClient : ExClientBase
    {
        public event EventHandler<RemoteClientConnectionEventArgs> OnRemoteClientConnected;
        public event EventHandler<RemoteClientConnectionEventArgs> OnRemoteClientDisconnected;

        public string Host { get; }
        public int Port { get; }
        public DateTime WorkerStartedAt { get; private set; }

        private BackgroundWorker worker;

        private double retrySeconds = 1;
        private DateTime lastRetry = DateTime.MinValue;

        public ExUBClient(string host, int port, Action<string> log, Action<Action> runOnMainThread, SerializationBinder binder)
            : base(0, "local", log, runOnMainThread, binder)
        {
            Host = host;
            Port = port;

            OnMessageReceived += BaseClient_OnMessageReceived;
            StartBackgroundWorker();
        }

        private void BaseClient_OnMessageReceived(object sender, OnMessageEventArgs e)
        {
            switch (e.Header.Type)
            {
                case MessageHeaderType.RemoteClientConnected:
                    RunOnMainThread(() => {
                        OnRemoteClientConnected?.Invoke(this, new RemoteClientConnectionEventArgs(e.Header.SendingClientId, RemoteClientConnectionEventArgs.ConnectionType.Connected));
                    });
                    break;
                case MessageHeaderType.RemoteClientDisconnected:
                    RunOnMainThread(() => {
                        OnRemoteClientDisconnected?.Invoke(this, new RemoteClientConnectionEventArgs(e.Header.SendingClientId, RemoteClientConnectionEventArgs.ConnectionType.Disconnected));
                    });
                    break;
                case MessageHeaderType.ClientInit:
                    ClientId = e.Header.SendingClientId;
                    break;
                case MessageHeaderType.Serialized:
                    DeserializeMessage(e.Header, e.Body);
                    break;
            }
        }

        #region BackgroundWorker
        private void StartBackgroundWorker()
        {
            Logger.Debug("Starting background worker");
            //worker = new BackgroundWorker();
            //worker.WorkerSupportsCancellation = true;
            //worker.WorkerReportsProgress = false;
            //worker.DoWork += BackgroundWorker_DoWork;
            //worker.RunWorkerAsync();
            Task.Factory.StartNew(() =>
            {
                BackgroundWorker_DoWork(null, null);
            });
            WorkerStartedAt = DateTime.UtcNow;
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Logger.Debug("Background worker doing work");
            //BackgroundWorker worker = sender as BackgroundWorker;
            //while (worker.CancellationPending != true)
            while (true)
            {
                try
                {
                    if (TcpClient == null && DateTime.UtcNow - lastRetry > TimeSpan.FromSeconds(retrySeconds))
                    {
                        retrySeconds *= 2;
                        Logger.Debug($"Attempting to connect to {Host}:{Port}");
                        lastKeepAliveRecv = DateTime.UtcNow;
                        lastKeepAliveSent = DateTime.UtcNow;
                        var client = new TcpClient(Host, Port);
                        ConnectionId = client.Client.LocalEndPoint.ToString();
                        SetClient(client);
                        retrySeconds = 1; // reset reconnection delay
                    }
                    if (TcpClient != null && TcpClient.Connected && DateTime.UtcNow - WorkerStartedAt > TimeSpan.FromSeconds(1))
                    {
                        if (DateTime.UtcNow - lastKeepAliveSent > TimeSpan.FromMilliseconds(3000))
                        {
                            Logger.Debug($"Sending keepalive from {ClientId}");
                            SendMessageBytes(new MessageHeader()
                            {
                                SendingClientId = ClientId,
                                Type = MessageHeaderType.KeepAlive,                                
                            }, new byte[] { });
                            lastKeepAliveSent = DateTime.UtcNow;
                        }
                        ReadIncoming();
                        WriteOutgoing();
                    }
                }
                catch (SocketException ex)
                {
                    //Server is not launcher
                    Logger.LogException(ex);
                }
                catch (Exception ex) {
                    //LogAction?.Invoke(ex.ToString());
                    Logger.LogException(ex);
                }
                Thread.Sleep(15);
            }
        }
        #endregion BackgroundWorker

         protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            OnMessageReceived -= BaseClient_OnMessageReceived;
            if (worker != null)
            {
                worker.DoWork -= BackgroundWorker_DoWork;
                worker.CancelAsync();
                worker.Dispose();
            }
        }
    }

}
