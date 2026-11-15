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
    public partial class XDebug
    {
        public class Finder<T> where T : UnityEngine.Object
        {
            public static T Instance
            {
                get
                {
                    if (XDebug.Finder<T>._instance != null)
                    {
                        return XDebug.Finder<T>._instance;
                    }
                    XDebug.Finder<T>._instance = UnityEngine.Object.FindObjectOfType<T>();
                    return XDebug.Finder<T>._instance;
                }
            }

            private static T _instance;
        }

        private class DebugFloat : MonoBehaviour
        {
            private void Update()
            {
                this.Value = this.BindToVariable();
                bool flag = false;
                if (Input.GetKey(this.Plus))
                {
                    if (Input.GetKeyDown(this.Plus) || Time.time - this.prevTime > 0.2f)
                    {
                        flag = true;
                        this.prevTime = Time.time;
                        this.Value += this.Offset;
                    }
                }
                else if (Input.GetKey(this.Minus) && (Input.GetKeyDown(this.Minus) || Time.time - this.prevTime > 0.2f))
                {
                    flag = true;
                    this.prevTime = Time.time;
                    this.Value -= this.Offset / 2f;
                }
                if (flag)
                {
                    this.UpdateActualValue(this.Value);
                }
                if (this.LogType == XDebug.DebugFloat.LogT.Constant || (this.LogType == XDebug.DebugFloat.LogT.OnChange && flag))
                {
                    XSingleton<XDebug>.Instance.Log(string.Format("{0} = {1}", this.Name, this.Value.ToString("0.0000")), 1.25f, 16f);
                }
            }

            public DebugFloat(string name, float init, float offset, KeyCode plus, KeyCode minus, Action<float> copy, Func<float> bind, XDebug.DebugFloat.LogT logType = XDebug.DebugFloat.LogT.None)
            {
                XDebug.DebugFloat debugFloat = new GameObject(name + " [wrapper]").AddComponent<XDebug.DebugFloat>();
                UnityEngine.Object.DontDestroyOnLoad(debugFloat.gameObject);
                debugFloat.Name = name;
                debugFloat.Value = init;
                debugFloat.Plus = plus;
                debugFloat.Minus = minus;
                debugFloat.Offset = offset;
                debugFloat.LogType = logType;
                debugFloat.UpdateActualValue = copy;
                debugFloat.BindToVariable = bind;
            }

            private KeyCode Plus;

            private KeyCode Minus;

            private float Offset;

            private string Name;

            private float Value;

            private float prevTime;

            private Action<float> UpdateActualValue;

            private XDebug.DebugFloat.LogT LogType;

            private Func<float> BindToVariable;

            public enum LogT
            {
                None,
                Constant,
                OnChange
            }
        }

        private class DebugInt : MonoBehaviour
        {
            private void Update()
            {
                this.BindToVariable();
                bool flag = false;
                if (Input.GetKey(this.Plus))
                {
                    if (Input.GetKeyDown(this.Plus) || Time.time - this.prevTime > 0.25f)
                    {
                        flag = true;
                        this.prevTime = Time.time;
                        this.Value += this.Offset;
                    }
                }
                else if (Input.GetKey(this.Minus) && (Input.GetKeyDown(this.Minus) || Time.time - this.prevTime > 0.25f))
                {
                    flag = true;
                    this.prevTime = Time.time;
                    this.Value -= this.Offset;
                }
                if (flag)
                {
                    this.UpdateActualValue(this.Value);
                }
                if (this.LogType == XDebug.DebugInt.LogT.Constant)
                {
                    XSingleton<XDebug>.Instance.Log(string.Format("{0} = {1}", this.Name, this.Value), 1.25f, 16f);
                    return;
                }
                if (flag && this.LogType == XDebug.DebugInt.LogT.OnChange)
                {
                    XSingleton<XDebug>.Instance.Log(string.Format("{0} = {1}", this.Name, this.Value), 1.25f, 16f);
                }
            }

            public DebugInt(string name, int init, int offset, KeyCode plus, KeyCode minus, Action<int> copy, Func<int> bind, XDebug.DebugInt.LogT logType = XDebug.DebugInt.LogT.None)
            {
                XDebug.DebugInt debugInt = new GameObject(name + " [wrapper]").AddComponent<XDebug.DebugInt>();
                UnityEngine.Object.DontDestroyOnLoad(debugInt.gameObject);
                debugInt.Name = name;
                debugInt.Value = init;
                debugInt.Plus = plus;
                debugInt.Minus = minus;
                debugInt.Offset = offset;
                debugInt.LogType = logType;
                debugInt.BindToVariable = bind;
                debugInt.UpdateActualValue = copy;
            }

            private KeyCode Plus;

            private KeyCode Minus;

            private int Offset;

            private string Name;

            private int Value;

            private float prevTime;

            private Action<int> UpdateActualValue;

            private Func<int> BindToVariable;

            private XDebug.DebugInt.LogT LogType;

            public enum LogT
            {
                None,
                Constant,
                OnChange
            }
        }
    }
}
