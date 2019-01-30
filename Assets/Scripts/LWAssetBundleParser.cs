using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using LWAssetBundle;

namespace LWContent
{
    public static class LWAssetBundleParser
    {
        private static string LastestLoadedSkinBundleName;

        public static void LoadAssetBundle(string itemId, Hash128 version, Action<bool> callback)
        {
            Debug.Log("Load Asset: " + itemId);
            AssetBundleRef skinRef = new AssetBundleRef(TransferItemIdToBundleName(itemId), version);
            AssetBundleManager.DownloadAssetBundle(skinRef, callback);
            LastestLoadedSkinBundleName = skinRef.name;
        }

        public static string TransferItemIdToBundleName(string itemId)
        {
            string[] str = SplitsToSingleItemString(itemId);
            if (str.Length == 2)
                return str[0] + "/" + str[1];
            else
                return str[0];
        }

        public static string[] SplitsToSingleItemString(string itemId)
        {
            char[] delimiterChars = { '.' };
            return itemId.Split(delimiterChars);
        }

        public static void UnloadWeapon()
        {
            if (!string.IsNullOrEmpty(LastestLoadedSkinBundleName))
            {
                AssetBundleManager.Unload(LastestLoadedSkinBundleName, true);
            }
        }
    }
}
