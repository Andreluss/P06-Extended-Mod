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
    public class XValue<T>
    {
        public XValue(Action<T> action, T initValue = default(T))
        {
            this.ChangeValue = action;
            this.Value = initValue;
        }

        public event Action<T> OnChangeValue;
        public T Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = value;
                this.ChangeValue(value);
                Action<T> onChangeValue = this.OnChangeValue;
                if (onChangeValue == null)
                {
                    return;
                }
                onChangeValue(value);
            }
        }

        private Action<T> ChangeValue;

        private T _value;
    }
}
