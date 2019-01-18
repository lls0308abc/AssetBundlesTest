using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace LWContent
{
    public class AssetBundlesLoader : MonoBehaviour
    {
        const string serverPath = @"https://dagkjcdmsdqis.cloudfront.net/AssetBundles/Android/w_ak47basic/";
        public Transform root;

        void Start()
        {
            StartCoroutine(GetAssetBundle(@"common", (common)=> {
                StartCoroutine(GetAssetBundle(@"ws_oem",
                (bundle) => {
                    var assetLoadRequest = bundle.LoadAssetAsync<GameObject>("ws_oem-inv");
                    GameObject gameObject = Instantiate(assetLoadRequest.asset as GameObject);
                    SetParentAndNormalize(gameObject.transform, root);
                })
            );
            }));
        }

        IEnumerator GetAssetBundle(string FileName, Action<AssetBundle> callback)
        {
            UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(serverPath + FileName, new Hash128(), 0);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
                AssetBundles.Add(bundle);
                if (callback != null)
                    callback(bundle);
            }
        }

        public List<AssetBundle> AssetBundles;

        public void SetParentAndNormalize(Transform targetTransform, Transform parent)
        {
            targetTransform.SetParent(parent);
            NormalizeTransform(targetTransform.transform);
        }

        public void NormalizeTransform(Transform _transform)
        {
            _transform.localRotation = Quaternion.identity;
            _transform.localPosition = Vector3.zero;
            _transform.localScale = new Vector3(1, 1, 1);
        }
    }
}
