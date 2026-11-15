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
        public Dictionary<string, object> GetSettingsDict()
        {
            if (!File.Exists(XCommon.ModFilesPath + "user_settings.xml"))
            {
                return null;
            }
            List<ValueTuple<string, object>> list = XUtility.DeserializeXmlToObject<List<ValueTuple<string, object>>>(XCommon.ModFilesPath + "user_settings.xml");
            if (list != null)
            {
                this.Dump(list);
                Dictionary<string, object> dictionary = list.ToDictionary((ValueTuple<string, object> x) => x.Item1, (ValueTuple<string, object> x) => x.Item2);
                this.Dump(dictionary);
                return dictionary;
            }
            return null;
        }

        public bool CanHandleInput()
        {
            GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
            return currentSelectedGameObject == null || !currentSelectedGameObject.active || currentSelectedGameObject.GetComponent<TMP_InputField>() == null;
        }

        public void SpeedoLog()
        {
            if (this.Speedo_GameObject == null)
            {
                this.Speedo_GameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("Defaultprefabs/UI/MessageBox_E3") as GameObject, Vector3.zero, Quaternion.identity);
                this.Speedo_GameObject_Rect = this.Speedo_GameObject.GetComponent<RectTransform>();
                MessageBox component = this.Speedo_GameObject.GetComponent<MessageBox>();
                component.Set("Duration", 9999999f);
                component.transform.SetParent(this.XCanvas.transform, false);
                Vector2 vector = new Vector2(175f, 45f);
                Vector2 vector2 = new Vector2(150f, 30f);
                this.Speedo_Text = this.Speedo_GameObject.GetComponentInChildren<TextMeshProUGUI>();
                this.Speedo_Text.overflowMode = TextOverflowModes.Linked;
                this.Speedo_Text.alignment = TextAlignmentOptions.MidlineRight;
                this.Speedo_Text.fontSize = 16f;
                this.Speedo_Text_Rect = this.Speedo_Text.GetComponent<RectTransform>();
                this.Speedo_Text_Rect.anchorMin = new Vector2(0.5f, 0.5f);
                this.Speedo_Text_Rect.anchorMax = new Vector2(0.5f, 0.5f);
                this.Speedo_Text_Rect.pivot = new Vector2(0.5f, 0.5f);
                this.Speedo_Text_Rect.localPosition = Vector3.zero;
                this.Speedo_Text_Rect.sizeDelta = vector2;
                this.Speedo_GameObject_Rect.anchorMin = new Vector2(1f, 0f);
                this.Speedo_GameObject_Rect.anchorMax = new Vector2(1f, 0f);
                this.Speedo_Container_Rect = this.Speedo_GameObject.FindInChildren("Box").GetComponent<RectTransform>();
                this.Speedo_Container_Rect.anchorMin = new Vector2(1f, 0f);
                this.Speedo_Container_Rect.anchorMax = new Vector2(1f, 0f);
                this.Speedo_Container_Rect.pivot = new Vector2(1f, 0f);
                this.Speedo_Container_Rect.sizeDelta = vector;
                this.Speedo_Container_Rect.localPosition = new Vector3(0f, 0f, 0f);
            }
            if (XDebug.Finder<PlayerBase>.Instance)
            {
                this.Speedo_GameObject.SetActive(true);
                float num = X_GetActualPlayerSpeedForward();
                float num2 = num / 100f;
                string text = ColorUtility.ToHtmlStringRGB(new Color(2f * (1f - num2), 2f * num2, 0f));
                string text2 = ((num <= XDebug.Cfg.Speedo.MaxDisplayableSpeed) ? num.ToString("0.00") : (XDebug.Cfg.Speedo.MaxDisplayableSpeed.ToString("0.0") + "+"));
                this.Speedo_Text.text = string.Concat(new string[] { "Speed: <color=#", text, ">", text2, "</color>" });
            }
            else
            {
                this.Speedo_GameObject.SetActive(false);
            }
        }

        private float ActualSpeed = 0f;
        private static readonly int n_positions = 10;
        private Queue<Vector3> player_prev_position = new Queue<Vector3>(n_positions);
        private void RecalculateActualSpeed()
        {
            PlayerBase player = XDebug.Finder<PlayerBase>.Instance;
            if (!player) return;
            if (Time.fixedDeltaTime <= 0) return;

            // add new position
            player_prev_position.Enqueue(player.transform.position);

            var distance = player.transform.position - player_prev_position.Peek();
            // optionally (dot product): 
            //distance = Vector3.Project(distance, player.transform.forward);

            float d = distance.magnitude;
            float t = Time.fixedDeltaTime * Mathf.Max(1, player_prev_position.Count - 1);
            ActualSpeed = d / t;

            // remove the n+1-th last position
            if (player_prev_position.Count > n_positions) player_prev_position.Dequeue();
        }

        public float X_GetActualPlayerSpeedForward()
        {
            return Mathf.Floor(ActualSpeed * 100) / 100;
        }


        public void LogExtra(string message, float duration = 1.5f, float fontOverride = 0f)
        {
            if (this.BoxExtra == null)
            {
                Vector2 vector = new Vector2(350f, 100f);
                Vector2 vector2 = new Vector2(325f, 80f);
                Vector2 vector3 = new Vector2(0.5f, 1f);
                Vector3 vector4 = new Vector3(0f, -50f, 0f);
                this.BoxExtra = new XLogBox(vector, vector2, vector3, vector4);
            }
            this.BoxExtra.GameObject.SetActive(true);
            this.BoxExtra.HideTime = Time.time + duration;
            this.BoxExtra.Text.text = message;
            this.BoxExtra.Font = fontOverride;
        }

        public bool BlockTriggerActions()
        {
            return XDebug.Finder<PlayerBase>.Instance != null && X_CanPlayerStartBoosting();
        }

        private Rewired.Player GetRInput()
        {
            return Singleton<RInput>.Instance.Get<Rewired.Player>("P");
        }
    }
}
