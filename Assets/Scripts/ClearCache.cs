using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCache : MonoBehaviour {

	// Use this for initialization
	void Start () {
        LWAssetBundle.AssetBundleManager.ClearCache();
	}
}
