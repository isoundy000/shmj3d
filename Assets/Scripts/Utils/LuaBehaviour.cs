
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XLua;
using System;

[System.Serializable]
public class Injection
{
	public string name;
	public GameObject value;
}

[LuaCallCSharp]
public class LuaBehaviour : MonoBehaviour {
	public Injection[] injections;

	private Action luaStart;
	private Action luaUpdate;
	private Action luaOnDestroy;

	private LuaTable scriptEnv;

	void Awake()
	{
		LuaEnv luaEnv = LuaMgr.luaenv;

		scriptEnv = luaEnv.NewTable();

		LuaTable meta = luaEnv.NewTable();
		meta.Set("__index", luaEnv.Global);
		scriptEnv.SetMetaTable(meta);
		meta.Dispose();

		scriptEnv.Set("self", this);
		scriptEnv.Set("transform", transform);
		scriptEnv.Set("gameObject", gameObject);
		foreach (var injection in injections)
		{
			scriptEnv.Set(injection.name, injection.value);
		}

		string name = gameObject.name;

		LuaMgr.loadLua (name, text => {
			if (text == null || text.Length == 0)
				return;

			luaEnv.DoString (text, name, scriptEnv);

			Action luaAwake = scriptEnv.Get<Action> ("awake");
			scriptEnv.Get ("start", out luaStart);
			scriptEnv.Get ("update", out luaUpdate);
			scriptEnv.Get ("ondestroy", out luaOnDestroy);

			if (luaAwake != null)
				luaAwake ();
		});
	}

	void Start () {
		if (luaStart != null)
			luaStart();
	}

	void Update () {
		if (luaUpdate != null)
			luaUpdate();
	}

	void OnDestroy() {
		if (luaOnDestroy != null)
			luaOnDestroy();

		luaOnDestroy = null;
		luaUpdate = null;
		luaStart = null;
		scriptEnv.Dispose();
		injections = null;
	}

	public void setIntValue(string name, int val) {
		scriptEnv.Set(name, val);
	}
}