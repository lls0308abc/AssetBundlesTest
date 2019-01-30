using UnityEngine;
using System.Collections;
using System;

namespace LWUtilities
{
    /// <summary>
    /// Static Coroutine. Use this to invoke Coroutine so it wont be effect by where it invoked
    /// ex: StaCor.ins.StartCo
    /// Need to be attached to an GameObject at the beginning of the game
    /// </summary>
    public class StaticCoroutine : MonoBehaviour
    {
        /// <summary>
        /// instance
        /// </summary>
        public static StaticCoroutine SingleTon
        {
            get
            {
                if (instance == null)
                {
                    obj = new GameObject("Static Coroutine");
                    instance = obj.AddComponent<StaticCoroutine>();
                    DontDestroyOnLoad(obj);
                }
                return instance;
            }
        }

        /// <summary>
        /// GameObject with this script attached
        /// </summary>
        private static GameObject obj = null;
        private static StaticCoroutine instance = null;

        // Use this for initialization
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    public static class CoroutineUtilities
    {
        /// <summary>
        /// Invoke a method after certain time
        /// </summary>
        /// <param name="invokeAfter"></param>
        public static Coroutine InvokeMethodAfter(float invokeAfter, System.Action method)
        {
            return StaticCoroutine.SingleTon.StartCoroutine(invokeMethodAfter(invokeAfter, method));
        }

        private static IEnumerator invokeMethodAfter(float invokeAfter, System.Action method)
        {
            yield return new WaitForSeconds(invokeAfter);
            if (method != null)
            {
                try
                {
                    method();
                }
                catch(Exception exp)
                {
                    Debug.Log("InvokeMethodAfter:" + exp);
                }
            }
        }

        public static void StopCoroutine(Coroutine coroutine)
        {
            if (coroutine != null)
                StaticCoroutine.SingleTon.StopCoroutine(coroutine);
        }
    }
}