using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace AssetBundles
{
    public class AssetBundlesMenuItems
    {
        const string kSimulationMode = "Assets/AssetBundles/Simulation Mode";

        [MenuItem(kSimulationMode)]
        public static void ToggleSimulationMode()
        {
            AssetBundleManager.SimulateAssetBundleInEditor = !AssetBundleManager.SimulateAssetBundleInEditor;
        }

        [MenuItem(kSimulationMode, true)]
        public static bool ToggleSimulationModeValidate()
        {
            Menu.SetChecked(kSimulationMode, AssetBundleManager.SimulateAssetBundleInEditor);
            return true;
        }

        [MenuItem("Assets/AssetBundles/Build AssetBundles")]
        static public void BuildAssetBundles()
        {
            BuildScript.BuildAssetBundles();
        }

        [MenuItem ("Assets/AssetBundles/Build Player (for use with engine code stripping)")]
        static public void BuildPlayer ()
        {
            BuildScript.BuildPlayer();
        }

        [MenuItem("Assets/AssetBundles/Build AssetBundles from Selection")]
        private static void BuildBundlesFromSelection()
        {
            // Get all selected *assets*
            var assets = Selection.objects.Where(o => !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(o))).ToArray();
            
            List<AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>();
            HashSet<string> processedBundles = new HashSet<string>();

            // Get asset bundle names from selection
            foreach (var o in assets)
            {
                var assetPath = AssetDatabase.GetAssetPath(o);
                var importer = AssetImporter.GetAtPath(assetPath);

                if (importer == null)
                {
                    continue;
                }

                // Get asset bundle name & variant
                var assetBundleName = importer.assetBundleName;
                var assetBundleVariant = importer.assetBundleVariant;
                var assetBundleFullName = string.IsNullOrEmpty(assetBundleVariant) ? assetBundleName : assetBundleName + "." + assetBundleVariant;
                
                // Only process assetBundleFullName once. No need to add it again.
                if (processedBundles.Contains(assetBundleFullName))
                {
                    continue;
                }

                processedBundles.Add(assetBundleFullName);
                
                AssetBundleBuild build = new AssetBundleBuild();

                build.assetBundleName = assetBundleName;
                build.assetBundleVariant = assetBundleVariant;
                build.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleFullName);
                
                assetBundleBuilds.Add(build);
            }

            BuildScript.BuildAssetBundles(assetBundleBuilds.ToArray());
        }

		private static string _dirName = "";  
		/// <summary>  
		/// 批量命名所选文件夹下资源的AssetBundleName.  
		/// </summary>  
		[MenuItem("Assets/AssetBundles/Set Asset Bundle Name")]  
		static void SetSelectFolderFileBundleName()  
		{  
			UnityEngine.Object[] selObj = Selection.GetFiltered(typeof(Object), SelectionMode.Unfiltered);  

			Debug.Log ("count=" + selObj.Length);

			foreach (Object item in selObj)  
			{  
				string objPath = AssetDatabase.GetAssetPath(item);  
				DirectoryInfo dirInfo = new DirectoryInfo(objPath);  
				if (dirInfo == null)  
				{  
					Debug.LogError("******请检查，是否选中了非文件夹对象******");  
					return;  
				}  
				_dirName = dirInfo.Name;  

				Debug.Log ("objPath="  +objPath);

				string filePath = dirInfo.FullName.Replace('\\', '/');  
				filePath = filePath.Replace(Application.dataPath, "Assets");  
				AssetImporter ai = AssetImporter.GetAtPath(filePath);  

				//ai.assetBundleName = "ab_" + objPath.Replace('/', '_').Substring(7);
				ai.assetBundleName = null;

 				SetAssetBundleName(dirInfo);  
			}  
			AssetDatabase.Refresh();  
			Debug.Log("******批量设置AssetBundle名称成功******");  
		}  
		static void SetAssetBundleName(DirectoryInfo dirInfo)  
		{  
			FileSystemInfo[] files = dirInfo.GetFileSystemInfos();  
			foreach (FileSystemInfo file in files)  
			{  
				if (file is FileInfo && file.Extension != ".meta")  
				{  
					string filePath = file.FullName.Replace('\\', '/');  
					filePath = filePath.Replace(Application.dataPath, "Assets");  
					AssetImporter ai = AssetImporter.GetAtPath(filePath);  

					filePath = filePath.Replace('/', '_');

					if (ai != null)
						ai.assetBundleName = _dirName == null ? null : "ab_" + filePath.Substring (7, filePath.LastIndexOf (".") - 7);
					else
						Debug.Log ("file invalid: " + filePath);
				}  
				else if (file is DirectoryInfo)  
				{  
					string filePath = file.FullName.Replace('\\', '/');  
					filePath = filePath.Replace(Application.dataPath, "Assets");  
					AssetImporter ai = AssetImporter.GetAtPath(filePath);  
					filePath = filePath.Replace('/', '_');
					//ai.assetBundleName = _dirName == null ? null : "ab_" + filePath.Substring(7);  
					ai.assetBundleName = null;
					SetAssetBundleName(file as DirectoryInfo);  
				}  
			}  
		}  
		/// <summary>  
		/// 批量清空所选文件夹下资源的AssetBundleName.  
		/// </summary>  
		[MenuItem("Assets/AssetBundles/Reset Asset Bundle Name")]  
		static void ResetSelectFolderFileBundleName()  
		{  
			UnityEngine.Object[] selObj = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Unfiltered);  
			foreach (UnityEngine.Object item in selObj)  
			{  
				string objPath = AssetDatabase.GetAssetPath(item);  
				DirectoryInfo dirInfo = new DirectoryInfo(objPath);  
				if (dirInfo == null)  
				{  
					Debug.LogError("******请检查，是否选中了非文件夹对象******");  
					return;  
				}  
				_dirName = null;  

				string filePath = dirInfo.FullName.Replace('\\', '/');  
				filePath = filePath.Replace(Application.dataPath, "Assets");  
				AssetImporter ai = AssetImporter.GetAtPath(filePath);  
				ai.assetBundleName = _dirName;  

				SetAssetBundleName(dirInfo);  
			}  
			AssetDatabase.Refresh();  
			Debug.Log("******批量清除AssetBundle名称成功******");  

		}  
    }
}