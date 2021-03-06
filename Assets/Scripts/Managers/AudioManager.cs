﻿
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour {
    Dictionary<string, GameObject> m_AudioCacheDic = new Dictionary<string, GameObject>();

    public Dictionary<string, AudioClip> audioClipCache = new Dictionary<string, AudioClip>();
    public class AudioItem {
        public string _path;
        public GameObject _object;
        public int _handle = -1;
        public AudioSource _audioSource;
        public float _audioLength;
    }

    protected int handle = 400;
    public List<AudioItem> _audioList = new List<AudioItem>();

    [SerializeField]
    GameObject m_AudioPrefab = null;
    [SerializeField]
    AudioItem m_bgm = null;

	float sfxVolume = 1.0f;
	float bgmVolume = 0.5f;

	static AudioManager mInstance = null;
	bool inited = false;

	public static AudioManager GetInstance () {
		return mInstance;
	}

    void Awake() {
		mInstance = this;

        m_AudioPrefab = Resources.Load("Prefab/audio/AudioPrefab") as GameObject;

		sfxVolume = PlayerPrefs.GetFloat ("sfxVolume", 1.0f);
		bgmVolume = PlayerPrefs.GetFloat ("bgmVolume", 0.5f);
    }

    public void PlayEffectAudio(string name) {
        string path = "Audios/" + name;
        PlayAudio(path, Vector3.zero);
    }

    public void PlayBackgrounfAudio(string name) {
        string path = "Audios/" + name;
        if (m_bgm == null)
            m_bgm = GetGameObjectOfPath(path, Vector3.zero);

        AudioClip clip = GetAudioClip(path);
        m_bgm._audioSource.clip = clip;
        m_bgm._audioLength = clip.length;
        m_bgm._path = path;
		m_bgm._audioSource.volume = bgmVolume;
        StartCoroutine(PlayAudioLogic(m_bgm, true));
    }

	public void StopBGM() {
		if (m_bgm == null)
			return;

		m_bgm._audioSource.Stop();
		StartCoroutine (Recycle(m_bgm._handle));
		m_bgm = null;
	}

    public void PlayHandCardAudio(int id) {
        string path = "Audios/" + id;

		Debug.Log ("play card: " + path);

        if (path != null || !path.Equals(string.Empty))
            PlayAudio(path, Vector3.zero);
    }

	public void PlayQuickChat(string audio) {
		string dialect = "putong";
		string speaker = "woman";
		string path = "Audios/qc/" + dialect + "/" + speaker + "/" + audio;

		PlayAudio(path, Vector3.zero);
	}

	public void PlayAudio(string path, Vector3 pos) {
        AudioItem item = GetGameObjectOfPath(path, pos);
        StartCoroutine(PlayAudioLogic(item));
    }

	public AudioClip GetAudioClip(string name) {
        if (!audioClipCache.ContainsKey(name)) {
            AudioClip clip = Resources.Load(name) as AudioClip;
            audioClipCache.Add(name, clip);
        }

        return audioClipCache[name];
    }

    public void Dispose()
    {
        audioClipCache.Clear();
    }

    IEnumerator PlayAudioLogic(AudioItem item, bool isBGM = false) {
        if (isBGM) {
            item._audioSource.playOnAwake = false;
            item._audioSource.loop = true;
        } else {
            item._audioSource.playOnAwake = false;
            item._audioSource.loop = false;
        }

		item._audioSource.Play();
        yield return new WaitForSeconds(item._audioLength);

		if (!isBGM)
            yield return StartCoroutine(Recycle(item._handle));
    }

    IEnumerator Recycle(int handle) {
        RemoveGameObject(handle);
        yield break;
    }

    public AudioItem GetGameObjectOfPath(string path, Vector3 position, Space space = Space.World) {
        AudioItem audioItem = CreateGameObject(path);
        audioItem._object.SetActive(false);
        if (space == Space.Self) {
            audioItem._object.transform.localPosition = position;
        } else if (space == Space.World) {
            audioItem._object.transform.position = position;
        }

        audioItem._object.SetActive(true);
		audioItem._audioSource.volume = sfxVolume;
        return audioItem;
    }

    public void RemoveGameObject(int handle, bool isDestroy = false) {
        AudioItem resItem = FindGameObjectOfHandleInList(handle);
        if (resItem == null) {
#if UNITY_EDITOR
            Debug.Log("GameObject is not exist:你想要删除的 handle" + handle);
#endif
            return;
        }

        if (isDestroy) {
            Destroy(resItem._object);
            _audioList.Remove(resItem);
        } else {
            resItem._object.transform.SetParent(transform);
            resItem._object.SetActive(false);
            resItem._handle = -1;
        }
    }

    protected AudioItem FindGameObjectOfHandleInList(int handle) {
        for (int i = 0; i < _audioList.Count; i++) {
            if (_audioList[i]._handle == handle)
                return _audioList[i];
        }
        return null;
    }

    protected AudioItem CreateGameObject(string path) {
        AudioItem audioItem = FindObjectOfPathInList(path);
        if (audioItem == null) {
            GameObject gameobject = Instantiate(m_AudioPrefab);
            AudioClip clip = GetAudioClip(path);
            audioItem = new AudioItem();
            audioItem._handle = GetHandle();
            audioItem._object = gameobject;
            audioItem._path = path;
            audioItem._audioSource = gameobject.GetComponent<AudioSource>();
            audioItem._audioLength = clip.length;
            audioItem._audioSource.clip = clip;
            _audioList.Add(audioItem);
        }

        audioItem._object.SetActive(true);
        audioItem._object.transform.SetParent(transform);
        return audioItem;
    }

    protected AudioItem FindObjectOfPathInList(string path) {
        for (int i = 0; i < _audioList.Count; i++) {
            if (_audioList[i]._path.Equals(path) && _audioList[i]._handle == -1)
            {
                _audioList[i]._handle = GetHandle();
                return _audioList[i];
            }
        }

        return null;
    }

    protected int GetHandle() {
        return ++handle;
    }

	public static void pauseAll() {
		AudioListener.pause = true;
	}

	public static void resumeAll() {
		AudioListener.pause = false;
	}

	public void setSFXVolume(float v) {
		sfxVolume = v;

		PlayerPrefs.SetFloat ("sfxVolume", v);
	}

	public void setBGMVolume(float v, bool force = false) {
		float old = bgmVolume;

		if (m_bgm != null) {
			if (old == 0 && v > 0)
				m_bgm._audioSource.Play();
			else if (old > 0 && v == 0)
				m_bgm._audioSource.Pause();
		}
			
		if (old != v || force) {
			PlayerPrefs.SetFloat ("bgmVolume", v);
			bgmVolume = v;

			if (m_bgm != null)
				m_bgm._audioSource.volume = v;
		}
	}

	public float getSFXVolume() { return sfxVolume; }
	public float getBGMVolume() { return bgmVolume; }
}