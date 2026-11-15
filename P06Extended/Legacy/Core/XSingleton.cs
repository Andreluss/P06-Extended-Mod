using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using System.Linq;
using System.Globalization;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using System.Security.Cryptography;
using UnityEngine.SceneManagement;
using System.Reflection;
using P06X.Helpers;
using TMPro;
using System.Xml.Schema;
using Harmony;
using System.Linq.Expressions;

namespace P06X
{
    public class XSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance
        {
            get
            {
                if (XSingleton<T>._instance == null)
                {
                    XSingleton<T>._instance = UnityEngine.Object.FindObjectOfType<T>();
                    if (XSingleton<T>._instance == null)
                    {
                        XSingleton<T>._instance = new GameObject(typeof(T).ToString()).AddComponent<T>();
                    }
                }
                return XSingleton<T>._instance;
            }
        }

        private void Start()
        {
            if (this != XSingleton<T>._instance)
            {
                UnityEngine.Object.Destroy(this);
                return;
            }
        }

        private static T _instance;
    }
}
