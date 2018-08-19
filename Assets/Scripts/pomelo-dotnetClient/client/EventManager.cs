using System;
using System.Collections.Generic;
using System.Text;
using SimpleJson;
using UnityEngine;

namespace Pomelo.DotNetClient {

    public class EventManager : IDisposable
    {
        private Dictionary<uint, Action<JsonObject>> callBackMap;
        private Dictionary<string, List<Action<JsonObject>>> eventMap;

        public EventManager() {
            callBackMap = new Dictionary<uint, Action<JsonObject>>();
            eventMap = new Dictionary<string, List<Action<JsonObject>>>();
        }

        public void AddCallBack(uint id, Action<JsonObject> callback) {
            if (id > 0 && callback != null) {
				if (callBackMap.ContainsKey (id)) {
					Debug.Log ("AddCallBack dup: " + id);
					callBackMap[id] = callback;
				} else
	                callBackMap.Add(id, callback);
            }
        }

        public void InvokeCallBack(uint id, JsonObject data) {
			if (callBackMap == null) {
				Debug.LogError ("callBackMap null");
				return;
			}
			
            if (!callBackMap.ContainsKey(id))
				return;

            callBackMap[id].Invoke(data);

			callBackMap.Remove(id);
        }

		public Action<JsonObject> GetCallBack(uint id) {
			if (callBackMap == null) {
				Debug.LogError ("get callBackMap null");
				return null;
			}

			if (!callBackMap.ContainsKey(id))
				return null;

			var ret = callBackMap [id];

			callBackMap.Remove (id);

			return ret;
		}
	
        public void AddOnEvent(string eventName, Action<JsonObject> callback)
        {
            List<Action<JsonObject>> list = null;
            if (this.eventMap.TryGetValue(eventName, out list))
            {
                list.Add(callback);
            }
            else
            {
                list = new List<Action<JsonObject>>();
                list.Add(callback);
                this.eventMap.Add(eventName, list);
            }
        }

        public void InvokeOnEvent(string route, JsonObject msg) {
            if (!eventMap.ContainsKey(route))
				return;

            List<Action<JsonObject>> list = eventMap[route];
			foreach (Action<JsonObject> action in list) {
				action.Invoke (msg);
			}
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing) {
            this.callBackMap.Clear();
            this.eventMap.Clear();
        }
    }
}