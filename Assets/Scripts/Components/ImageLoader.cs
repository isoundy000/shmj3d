
using UnityEngine;  
using System.Collections;  
using System.IO;
  
public class ImageLoader :MonoBehaviour {

	public static ImageLoader _instance = null;
	public static ImageLoader GetInstance() { return Instance; }
	string path;

	public static ImageLoader Instance {
		get {
			if (_instance == null) {
				GameObject obj = new GameObject("ImageLoader");
				_instance = obj.AddComponent<ImageLoader>();
				DontDestroyOnLoad(obj);
				_instance.Init();
			}

			return _instance;
		}
	}

	public bool Init() {
		path = Application.persistentDataPath + "/ImageCache/";

		if (!Directory.Exists(path))
			Directory.CreateDirectory(path);

		return true;
	}

	public void LoadImage(string url, UITexture texture) {
        //texture.mainTexture = placeholder;  
  
        if (!File.Exists (path + url.GetHashCode())) {
			StartCoroutine (DownloadImage (url, texture));  
        } else {
			StartCoroutine(_LoadLocalImage(url,texture));
        }
    }  
  
	IEnumerator DownloadImage(string url, UITexture texture) {
        Debug.Log("downloading new image:" + path + url.GetHashCode());

        WWW www = new WWW (url);  
		yield return www;
  
		Texture2D tex2d = www.texture;

		byte[] pngData = tex2d.EncodeToPNG();
        File.WriteAllBytes(path + url.GetHashCode(), pngData);
  
		texture.mainTexture = tex2d;
    }  
  
	IEnumerator _LoadLocalImage(string url, UITexture texture) {
        string filePath = "file:///" + path + url.GetHashCode();

        Debug.Log("getting local image:" + filePath);

        WWW www = new WWW (filePath);
        yield return www;

        texture.mainTexture = www.texture;
    }

	public void LoadLocalImage(string file, UITexture texture) {
		StartCoroutine(_LoadLocal(file, texture));
	}

	IEnumerator _LoadLocal(string url, UITexture texture) {
		string filePath = "file:///" + url;

		Debug.Log("getting local image:" + filePath);

		WWW www = new WWW (filePath);
		yield return www;

		texture.mainTexture = www.texture;
	}
}



