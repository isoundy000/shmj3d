﻿using SimpleJson;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Pomelo.DotNetClient
{
    /// <summary>
    /// network state enum
    /// </summary>
    public enum NetWorkState
    {
        [Description("initial state")]
        CLOSED,

        [Description("connecting server")]
        CONNECTING,

        [Description("server connected")]
        CONNECTED,

        [Description("disconnected with server")]
        DISCONNECTED,

        [Description("connect timeout")]
        TIMEOUT,

        [Description("netwrok error")]
        ERROR
    }

    public class PomeloClient : IDisposable
    {
        /// <summary>
        /// netwrok changed event
        /// </summary>
        public event Action<NetWorkState> NetWorkStateChangedEvent;


        private NetWorkState netWorkState = NetWorkState.CLOSED;   //current network state

		private List<Message> msgQueue = new List<Message>();
		private object guard = new object();

        private EventManager eventManager;
        private Socket socket;
        private Protocol protocol;
        private bool disposed = false;
        private uint reqId = 1;

        private ManualResetEvent timeoutEvent = new ManualResetEvent(false);
        private int timeoutMSec = 8000;    //connect timeout count in millisecond

        public PomeloClient()
        {
			eventManager = new EventManager();
        }

        public void initClient(string host, int port, Action callback = null)
        {
            timeoutEvent.Reset();
            NetWorkChanged(NetWorkState.CONNECTING);

			IPAddress ipAddress = new IPAddress(0);
			if (!IPAddress.TryParse(host, out ipAddress))
			{
				ipAddress = null;
			}

			if (ipAddress == null) {
				try {
					IPAddress[] addresses = Dns.GetHostEntry (host).AddressList;
					foreach (var item in addresses) {
						if (item.AddressFamily == AddressFamily.InterNetwork) {
							ipAddress = item;
							break;
						}
					}
				} catch (Exception e) {
					NetWorkChanged (NetWorkState.ERROR);
					return;
				}
			}

            if (ipAddress == null)
            {
                throw new Exception("can not parse host : " + host);
            }

            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ie = new IPEndPoint(ipAddress, port);

            socket.BeginConnect(ie, new AsyncCallback((result) =>
            {
                try
                {
                    this.socket.EndConnect(result);
                    this.protocol = new Protocol(this, this.socket);
                    NetWorkChanged(NetWorkState.CONNECTED);

                    if (callback != null)
                    {
                        callback();
                    }
                }
                catch (SocketException e)
                {
                    if (netWorkState != NetWorkState.TIMEOUT)
                    {
                        NetWorkChanged(NetWorkState.ERROR);
                    }
                    Dispose();
                }
                finally
                {
                    timeoutEvent.Set();
                }
            }), this.socket);

            if (timeoutEvent.WaitOne(timeoutMSec, false))
            {
                if (netWorkState != NetWorkState.CONNECTED && netWorkState != NetWorkState.ERROR)
                {
                    NetWorkChanged(NetWorkState.TIMEOUT);
                    Dispose();
                }
            }
        }

		public NetWorkState GetNetworkState() {
			return netWorkState;
		}


		private void NetWorkChanged(NetWorkState state)
        {
			NetWorkState old = netWorkState;
            netWorkState = state;

			if (old != state && NetWorkStateChangedEvent != null)
                NetWorkStateChangedEvent(state);
        }

        public void connect()
        {
            connect(null, null);
        }

        public void connect(JsonObject user)
        {
            connect(user, null);
        }

        public void connect(Action<JsonObject> handshakeCallback)
        {
            connect(null, handshakeCallback);
        }

        public bool connect(JsonObject user, Action<JsonObject> handshakeCallback)
        {
            try
            {
                protocol.start(user, handshakeCallback);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        private JsonObject emptyMsg = new JsonObject();
        public void request(string route, Action<JsonObject> action)
        {
            this.request(route, emptyMsg, action);
        }

        public void request(string route, JsonObject msg, Action<JsonObject> action)
        {
            this.eventManager.AddCallBack(reqId, action);
            protocol.send(route, reqId, msg);

            reqId++;
        }

        public void notify(string route, JsonObject msg)
        {
            protocol.send(route, msg);
        }

        public void on(string eventName, Action<JsonObject> action)
        {
            eventManager.AddOnEvent(eventName, action);
        }

        internal void processMessage(Message msg)
        {
			lock (guard)
			{				
				msgQueue.Add(msg);
			}
        }

		public void poll()
		{
			lock (guard)
			{
				foreach (Message msg in msgQueue)
				{
					if (msg.type == MessageType.MSG_RESPONSE)
					{   
						eventManager.InvokeCallBack(msg.id, msg.data);
					}
					else if (msg.type == MessageType.MSG_PUSH)
					{
						eventManager.InvokeOnEvent(msg.route, msg.data);
					}
				}

				msgQueue.Clear();
			}
		}

        public void disconnect()
        {
            Dispose();
            NetWorkChanged(NetWorkState.DISCONNECTED);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // The bulk of the clean-up code
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
                return;

            if (disposing)
            {
                // free managed resources
                if (this.protocol != null)
                {
                    this.protocol.close();
                }

                if (this.eventManager != null)
                {
                    this.eventManager.Dispose();
                }

                try
                {
                    this.socket.Shutdown(SocketShutdown.Both);
                    this.socket.Close();
                    this.socket = null;
                }
                catch (Exception)
                {
                    //todo : 有待确定这里是否会出现异常，这里是参考之前官方github上pull request。emptyMsg
                }

                this.disposed = true;
            }
        }
    }
}