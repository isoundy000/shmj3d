
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ResourceType
{
    Table,
    DiTan,
    none,
    Hand,
    Audio
}

public class ResourceItem {
    public int handle = -1;
    public GameObject obj = null;
    public string path = string.Empty;
}

public class ResourcesMgr : MonoBehaviour {
	public static ResourcesMgr mInstance = null;

    Dictionary<string, GameObject> _prefabCache = new Dictionary<string, GameObject>();
    List<ResourceItem> cacheList = new List<ResourceItem>();
    int handle = 100;

    [Header("透明材质")]
    private Material m_transparent = null;
    public Material M_transparent {
        set { m_transparent = value; }
        get { return m_transparent; }
    }
    
	public void Awake() {
		m_transparent = Resources.Load("Materials/Transparent") as Material;
		mInstance = this;
	}

	public static ResourcesMgr GetInstance () {
		return mInstance;
	}

    public GameObject InstantiateGameObject(string name , ResourceType type = ResourceType.Table)
    {
        string path = GetPath(name, type);
        return (GameObject)Instantiate(GetPrefab(path));
    }

    public GameObject InstantiateGameObjectWithType(string name, ResourceType type = ResourceType.Table)
    {
        string path = GetPath(name, type);
		GameObject go = GetGameObject(path);

		if (type == ResourceType.Hand) {
			foreach (var tran in go.GetComponentsInChildren<SkinnedMeshRenderer>())
				tran.material = this.M_transparent;

			Animation anim = go.GetComponent<Animation>();
			foreach (AnimationState state in anim)
				state.speed = 2.0f;
		}

		return go;
    }

    #region  
	public GameObject GetGameObject(string path) {
        ResourceItem item = FindGameObject(path);

        item.handle = GetHandle();
        item.obj.transform.SetParent(null);
        item.obj.SetActive(true);
        return item.obj;
    }

    ResourceItem FindGameObject(string path) {
        ResourceItem item = null;
        for (int i = 0; i < cacheList.Count; i++) {
            if (cacheList[i].path == path && cacheList[i].handle == -1) {
                item = cacheList[i];
                break;
            }
        }

        if (item==null)
            item = CreateGameObject(path);

        return item;
    }

    ResourceItem CreateGameObject(string path) {
        ResourceItem item = new ResourceItem();

        item.obj = Instantiate(GetPrefab(path));
        if (item.obj == null)
            Debug.LogError(path + "路径下无法找到资源");

        item.handle = -1;
        item.path = path;
        cacheList.Add(item);
        return item;
    }

	public void StopAllHands() {
		ResourceItem item = null;
		for (int i = 0; i < cacheList.Count; i++) {
			item = cacheList[i];
			if (item.handle >= 0) {
				DHM_HandAnimationCtr ctrl = item.obj.GetComponent<DHM_HandAnimationCtr>();

				if (ctrl != null)
					ctrl.Stop();
			}
		}
	}

    public void RemoveGameObject(int handles) {
        ResourceItem item = FindGameObjectToRemove(handle);
        if (item != null) {
            item.handle = -1;
            item.obj.transform.SetParent(this.transform);
            item.obj.SetActive(false);
        }
    }

    public void RemoveGameObject(GameObject obj) {
        ResourceItem item = FindGameObjectToRemove(obj);
        if (item != null) {
            item.handle = -1;
            item.obj.transform.SetParent(this.transform);
            item.obj.SetActive(false);
        }
    }

    ResourceItem FindGameObjectToRemove(int handle) {
        ResourceItem item = null;
        for (int i = 0; i < cacheList.Count; i++) {
            if (cacheList[i].handle.Equals(handle)) {
                item = cacheList[i];
                break;
            }
        }

        return item;
    }

    ResourceItem FindGameObjectToRemove(GameObject obj) {
        ResourceItem item = null;
        for (int i = 0; i < cacheList.Count; i++) {
            if(cacheList[i].obj.Equals(obj) && cacheList[i].handle!=-1) {
                item = cacheList[i];
                break;
            }
        }

        return item;
    }

    int GetHandle() {
        return handle++;
    }

	public void release() {
		foreach (var item in cacheList) {
			GameObject ob = item.obj;
			Animation anim = ob.GetComponent<Animation> ();
			if (anim != null)
				anim.Stop();

			DestroyImmediate(ob);
		}

		cacheList.Clear ();
	}

    #endregion

	string GetPath(string name, ResourceType type = ResourceType.Table) {
        string path = string.Empty;
        switch (type)
        {
            case ResourceType.Table:
            {
                path = "Prefab/" + "table" + "/" + name;
                break;
            }
            case ResourceType.DiTan:
            {
                path = "Prefab/" + "ditan" + "/" + name;
                break;
            }
            case ResourceType.Audio:
            {
                path = "Prefab/" + "audio" + "/" + name;
                break;
            }
            case ResourceType.none:
            {
                break;
            }
            case ResourceType.Hand:
            {
                path = "Prefab/" + "hand" + "/" + name;
                break;
            }
        }

        return path;
    }

    public GameObject GetPrefab(string path) {
        if (!_prefabCache.ContainsKey(path)) {
            GameObject prefab = Resources.Load(path) as GameObject;
            _prefabCache.Add(path, prefab);
        }

        return _prefabCache[path];
    }

	public GameObject LoadMJ(int id) {
		string path = "Prefab/majiang/mj" + id;
		GameObject ob = Instantiate(Resources.Load(path)) as GameObject;

		return ob;
	}
}
