using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using LWContent;

namespace LWAssetBundle
{
    public class AssetBundlesLoader : MonoBehaviour
    {
        public Transform root;
        public InputField Skin;
        public Text Console;
        public string currentLoadedBundleName;

        public void OnPreviewClicked()
        {
            if (!string.IsNullOrEmpty(currentLoadedBundleName))
                AssetBundleManager.Unload(currentLoadedBundleName, true);

            foreach (Transform child in root)
            {
                Destroy(child.gameObject);
            }
            string skinId = Skin.text;

            GetAssetBundle(skinId, (result) =>
            {
                if (result)
                {
                    currentLoadedBundleName = LWAssetBundleParser.TransferItemIdToBundleName(skinId);
                    string skinName = LWAssetBundleParser.SplitsToSingleItemString(skinId)[1];
                    AssetBundle ab = AssetBundleManager.GetAssetBundle(currentLoadedBundleName);
                    if (ab != null)
                    {
                        GameObject obj = Instantiate(ab.LoadAsset<GameObject>(skinName + "-inv"));
                        SetParentAndNormalize(obj.transform, root);
                        Debug.Log("Get AssetBundle success!");
                    }
                    else
                        Debug.Log("Get AssetBundle failed!");
                }
                else
                    Debug.Log("Get AssetBundle failed!");

            });
        }

        public void OnCleanClicked()
        {
            Console.text = "";
            if (!string.IsNullOrEmpty(currentLoadedBundleName))
                AssetBundleManager.Unload(currentLoadedBundleName, true);
        }

        private void GetAssetBundle(string bundleName, Action<bool> callback)
        {
            Hash128 hash = new Hash128();

            Debug.Log(bundleName);
            string bundleNameTransfered = LWAssetBundleParser.TransferItemIdToBundleName(bundleName);
            Console.text += bundleName + "\r\n";
            AssetBundleRef requestAB = new AssetBundleRef(bundleNameTransfered, hash);
            AssetBundleManager.DownloadAssetBundle(requestAB, callback);
        }

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
