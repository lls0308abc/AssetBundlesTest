using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinList : MonoBehaviour {

    public string[] skinIDs;

    public InputField InputField;
    private void Start()
    {
        InputField.text = skinIDs[0];
    }
}
