using SimpleJson;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

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
        public event Action<NetWorkState> NetWorkStateChangedEvent;


        private NetWorkState netWorkState = NetWorkState.CLOSED;

		private List<Message> msgQueue = new List<Message>();
		private object guard = new object();

        private EventManager eventManager = null;
        private Socket socket = null;
        private Protocol protocol = null;
        private bool disposed = false;
        private uint reqId = 1;

        private ManualResetEvent timeoutEvent = new ManualResetEvent(false);
        private int timeoutMSec = 8000;    //connect timeout count in millisecond

		bool isKicked = false;
		bool ipv6 = false;

        public PomeloClient()
        {
			
        }

        public void initClient(string host, int port, Action<bool> callback = null)
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
						var family = item.AddressFamily;
						if (family == AddressFamily.InterNetworkV6) {
							ipAddress = item;
							ipv6 = true;
							break;
						} else if (family == AddressFamily.InterNetwork) {
							ipAddress = item;
							break;
						}
					}
				} catch (Exception e) {
					NetWorkChanged (NetWorkState.ERROR);
					if (callback != null)
						callback (false);
					
					return;
				}
			}

            if (ipAddress == null)
            {
                throw new Exception("can not parse host : " + host);
            }

			Debug.Log ("ipAddress=" + ipAddress + " ipv6=" + ipv6);

			var fam = ipv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;

			this.socket = new Socket(fam, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ie = new IPEndPoint(ipAddress, port);

            socket.BeginConnect(ie, new AsyncCallback((result) =>
            {
                try
                {
                    this.socket.EndConnect(result);
                    this.protocol = new Protocol(this, this.socket);
					this.eventManager = new EventManager();
                    NetWorkChanged(NetWorkState.CONNECTED);

					disposed = false;

                    if (callback != null)
                        callback(true);
                }
                catch (SocketException e)
                {
                    if (netWorkState != NetWorkState.TIMEOUT)
                    {
                        NetWorkChanged(NetWorkState.ERROR);
                    }

                    Dispose();
					if (callback != null)
						callback (false);
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
					if (callback != null)
						callback (false);
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
			uint id = reqId++;
			if (eventManager == null || protocol == null)
				return;

			try {
	            eventManager.AddCallBack(id, action);
	            protocol.send(route, id, msg);
			} catch (Exception e) {
				Debug.LogError("request err: " + e.ToString());
			}
        }

        public void notify(string route, JsonObject msg)
        {
			if (protocol == null)
				return;

			try {
	            protocol.send(route, msg);
			} catch (Exception e) {
				Debug.LogError("notify err: " + e.ToString());
			}
        }

        public void on(string eventName, Action<JsonObject> action)
        {
			if (eventManager == null)
				return;

			try {
	            eventManager.AddOnEvent(eventName, action);
			} catch (Exception e) {
				Debug.LogError("on err: " + e.ToString());
			}
        }

        internal void processMessage(Message msg)
        {
			lock (guard)
			{
				msgQueue.Add(msg);
			}
        }

		public void pseudo(string route, JsonObject data) {
			if (!data.ContainsKey("pseudo"))
				data.Add("pseudo", true);

			Message msg = new Message (MessageType.MSG_PUSH, 0, route, data);

			lock (guard) {
				msgQueue.Add(msg);
			}
		}

		public void flush() {
			lock (guard) {
				msgQueue.Clear();
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
						try {
							eventManager.InvokeCallBack(msg.id, msg.data);
						} catch (Exception e) {
							Debug.Log ("InvokeCallback exception: " + e.ToString());
							Debug.Log ("id=" + msg.id + " data: " + msg.data.ToString());
						}
					}
					else if (msg.type == MessageType.MSG_PUSH)
					{
						try {
							eventManager.InvokeOnEvent(msg.route, msg.data);
						} catch (Exception e) {
							Debug.Log ("InvokeOnEvent exception: " + e.ToString());
							Debug.Log ("route=" + msg.route + " data: " + msg.data.ToString());
						}
					}
				}

				msgQueue.Clear();
			}
		}

		public void disconnect(bool kicked = false)
        {
			Debug.Log ("pc disconnect");

			isKicked = kicked;

            Dispose();
            NetWorkChanged(NetWorkState.DISCONNECTED);
        }

		public bool getKicked() {
			bool ret = isKicked;
			if (isKicked)
				isKicked = false;

			return ret;
		}

		public void setKicked(bool kicked = true) {
			isKicked = kicked;
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
					this.protocol = null;
                }

                if (this.eventManager != null)
                {
                    this.eventManager.Dispose();
					this.eventManager = null;
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
					Debug.Log("socket shutdown exception");

                }

                this.disposed = true;
            }
        }
    }
}