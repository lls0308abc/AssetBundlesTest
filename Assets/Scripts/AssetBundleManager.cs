using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LWAssetBundle
{
    public static class AssetBundleManager
    {

        static AssetBundleManager()
        {
            AssetBundleLoadedDict = new Dictionary<string, AssetBundleRef>();
        }


        /// <summary>
        /// Dictionary of AssetBundles, key is itemId, value is the assetbundleRef
        /// </summary>
        private static Dictionary<string, AssetBundleRef> AssetBundleLoadedDict;
        private static string assetBundlePath = Application.streamingAssetsPath;
        private static AssetBundleManifest manifest;

        private static bool CheckAssetBundleIsLoaded(string bundleName)
        {
            return AssetBundleLoadedDict.ContainsKey(bundleName);
        }

        public static AssetBundle GetAssetBundle(string itemId)
        {
            if (AssetBundleLoadedDict.ContainsKey(itemId))
                return AssetBundleLoadedDict[itemId].assetBundle;
            else
                return null;
        }

        public static void DownloadAssetBundle(AssetBundleRef assetBundleRef, Action<bool> isSuccess)
        {
            if (CheckAssetBundleIsLoaded(assetBundleRef.name))
                Debug.Log(assetBundleRef.name + " has been loaded in dictionary.");
            else
                LWUtilities.StaticCoroutine.SingleTon.StartCoroutine(downloadAssetBundle(assetBundleRef, isSuccess));
        }

        public static void ClearCache()
        {
            if (Caching.ClearCache())
            {
                Debug.Log("Successfully cleaned the cache.");
            }
            else
            {
                Debug.Log("Cache is being used.");
            }
        }

        public static IEnumerator downloadAssetBundle(AssetBundleRef assetBundleRef, Action<bool> isSuccess)
        {
            Debug.Log("Get Assetbundle Name: " + assetBundleRef.name);
            Hash128 version = assetBundleRef.version;

            while (!Caching.ready)
                yield return null;

            if (manifest == null)
            {
                using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(System.IO.Path.Combine(assetBundlePath, "StandaloneWindows")))
                {
                    yield return www.SendWebRequest();

                    if (www.isNetworkError || www.isHttpError)
                    {
                        Debug.Log(www.error);
                        if (isSuccess != null)
                            isSuccess(false);
                    }
                    else
                    {
                        AssetBundle manifestBundle = DownloadHandlerAssetBundle.GetContent(www);
                        manifest = (AssetBundleManifest)manifestBundle.LoadAsset("AssetBundleManifest");
                        manifestBundle.Unload(false);
                    }
                    www.Dispose();
                }
            }

            string[] dependentAssetBundles = manifest.GetAllDependencies(assetBundleRef.name);
            assetBundleRef.Dependencies = dependentAssetBundles;
            Debug.Log("DependentAssetBundles Count: " + dependentAssetBundles.Length);
            AssetBundle[] abs = new AssetBundle[dependentAssetBundles.Length];
            for (int i = 0; i < dependentAssetBundles.Length; i++)
            {
                string dependentAssetBundlePath = assetBundlePath + "/" + dependentAssetBundles[i];
                Debug.Log("Get DependentAssetBundle: " + dependentAssetBundles[i] + " , Path = " + dependentAssetBundlePath);
                using (UnityWebRequest wwwDependent = UnityWebRequestAssetBundle.GetAssetBundle(dependentAssetBundlePath, version, 0))
                {
                    yield return wwwDependent.SendWebRequest();
                    abs[i] = DownloadHandlerAssetBundle.GetContent(wwwDependent);

                    if (abs[i] != null)
                    {
                        AssetBundleLoadedDict.Add(dependentAssetBundles[i], new AssetBundleRef(abs[i].name, version, abs[i]));
                        Debug.Log("Add " + abs[i].name + " to loaded dictionary");
                    }

                    wwwDependent.Dispose();
                }  
            }

            using (UnityWebRequest wwwAssetBundle = UnityWebRequestAssetBundle.GetAssetBundle(assetBundlePath + "/" + assetBundleRef.name, version, 0))
            {
                yield return wwwAssetBundle.SendWebRequest();

                if (wwwAssetBundle.isNetworkError || wwwAssetBundle.isHttpError)
                {
                    Debug.Log(wwwAssetBundle.error);
                    if (isSuccess != null)
                        isSuccess(false);
                }
                else
                {
                    AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(wwwAssetBundle);
                    assetBundleRef.assetBundle = bundle;
                    Debug.Log("Success! Add " + assetBundleRef.name + " to dictionary");
                    AssetBundleLoadedDict.Add(assetBundleRef.name, assetBundleRef);
                    if (isSuccess != null)
                        isSuccess(true);

                    wwwAssetBundle.Dispose();
                }
            }
        }

        public static void Unload(string bundleName, bool allObjects)
        {
            Debug.Log("Unload assetbundle: " + bundleName);
            AssetBundleRef assetBundleRef;
            if (CheckAssetBundleIsLoaded(bundleName))
            {
                assetBundleRef = AssetBundleLoadedDict[bundleName];
                if (assetBundleRef.assetBundle != null)
                {
                    Debug.Log("Unload assetbundle name: " + assetBundleRef.name + " success!");
                    assetBundleRef.assetBundle.Unload(allObjects);
                }
                else
                    Debug.Log("Unload assetbundle name: " + assetBundleRef.name + " failed! assetbundle is null");

                UnloadDependencies(bundleName, allObjects);
                assetBundleRef.assetBundle = null;
                AssetBundleLoadedDict.Remove(bundleName);
            }
            else
                Debug.Log("Unload assetbundle: " + bundleName + " failed! Could not find it in dictionary");
        }


        private static void UnloadDependencies(string bundleName, bool allObjects)
        {
            AssetBundleRef assetBundleRef = null;
            if (!AssetBundleLoadedDict.TryGetValue(bundleName, out assetBundleRef))
                return;

            Debug.Log("Unload Dependencies: " + bundleName + ", Count: " + assetBundleRef.Dependencies.Length);
            // Loop dependencies
            foreach (var dependency in assetBundleRef.Dependencies)
            {
                Debug.Log("Unload Dependcies: " + dependency);
                AssetBundleRef unloadAssetBundle = null;
                if (AssetBundleLoadedDict.TryGetValue(dependency, out unloadAssetBundle))
                {
                    Debug.Log("Unload Dependcies: " + unloadAssetBundle.name + " success!");
                    unloadAssetBundle.assetBundle.Unload(allObjects);
                    AssetBundleLoadedDict.Remove(dependency);
                }
                else
                    Debug.Log("Unload Dependcies: " + dependency + " failed!");
            }
        }
    }

    public class AssetBundleRef
    {
        public AssetBundle assetBundle = null;
        public Hash128 version;
        public string name;
        public string[] Dependencies;

        public AssetBundleRef(string name, Hash128 version, AssetBundle assetBundle = null)
        {
            this.name = name;
            this.version = version;
            this.assetBundle = assetBundle;
        }
    }
}
