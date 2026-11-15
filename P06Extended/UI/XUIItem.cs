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
    public class XUIItem
    {
        public XUIItem()
        {
            this.gameObject = DefaultControls.CreatePanel(default(DefaultControls.Resources));
            this.gameObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.25f);
            this.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(XUIConfig.item_width, XUIConfig.item_height);
        }

        public string Name { get; set; }

        public GameObject gameObject;
    }
}
