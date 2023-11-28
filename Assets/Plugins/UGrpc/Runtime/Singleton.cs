using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UGrpc.Runtime
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static readonly Lazy<T> LazyInstance = new Lazy<T>(CreateSingleton);

        public static T Inst => LazyInstance.Value;

        private static T CreateSingleton()
        {
            var ownerObject = new GameObject($"{typeof(T).Name} (singleton)");
            var instance = ownerObject.AddComponent<T>();
            DontDestroyOnLoad(ownerObject);
            return instance;
        }
    }

    public abstract class SingletonBase<T> where T : SingletonBase<T>, new()
    {
        private static T _instance = new T();
        public static T Inst
        {
            get
            {
                return _instance;
            }
        }
    }
}
