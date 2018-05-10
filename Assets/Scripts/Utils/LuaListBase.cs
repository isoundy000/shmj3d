
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XLua;
using System;

[CSharpCallLua]
public class LuaListBase : ListBase {
	public Injection[] injections;

	Action luaStart;
	Action luaUpdate;
	Action luaOnDestroy;
	Action luaEnter;
	Action luaOnBack;

	[CSharpCallLua]
	public delegate void enterClubFunc(int cid, bool admin);

	enterClubFunc luaEnterClub;

	LuaTable scriptEnv;

	void Awake()
	{
		base.Awake ();

		LuaEnv luaEnv = LuaMgr.luaenv;
		scriptEnv = luaEnv.NewTable();

		LuaTable meta = luaEnv.NewTable();
		meta.Set("__index", luaEnv.Global);
		scriptEnv.SetMetaTable(meta);
		meta.Dispose();

		scriptEnv.Set("self", this);
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
			scriptEnv.Get ("enter", out luaEnter);
			scriptEnv.Get ("enterclub", out luaEnterClub);
			scriptEnv.Get ("onback", out luaOnBack);

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
		luaEnter = null;
		luaEnterClub = null;
		luaOnBack = null;
		scriptEnv.Dispose();
		injections = null;
	}

	public void enter() {
		Debug.Log ("LuaListBase enter");

		if (luaEnter != null)
			luaEnter ();
		else
			base.enter ();
	}

	public void enter(int club_id, bool admin = false) {
		if (luaEnterClub != null)
			luaEnterClub (club_id, admin);
	}

	protected void onBack() {
		if (luaOnBack != null)
			luaOnBack ();
		else
			base.onBack ();
	}
}