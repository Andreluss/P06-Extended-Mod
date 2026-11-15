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
        private void HandleInput()
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKey(KeyCode.F1) && Time.time - this._ringsPrevClick > 0.125f)
                {
                    if (Input.GetKeyDown(KeyCode.F1))
                    {
                        XDebug.Cheats.Rings += 50;
                        XDebug.Comment("if was just pressed then wait a little more");
                        this._ringsPrevClick = Time.time + 0.75f;
                        this.Log("50 rings <color=#00ee00>added!</color>", 0.7f, 16f);
                    }
                    else
                    {
                        XDebug.Cheats.Rings += 51;
                        this._ringsPrevClick = Time.time;
                        this.Log("Now adding moooooooooooore......", 0.5f, 16f);
                    }
                    XDebug.Finder<PlayerBase>.Instance.Audio.PlayOneShot(XSingleton<XFiles>.Instance.RingSound);
                }
                if (Input.GetKeyDown(KeyCode.F2))
                {
                    XDebug.Cheats.Lives += 100;
                    this.Log("<color=#00ee00>+100</color> Lives!", 0.5f, 16f);
                }
                if (Input.GetKeyDown(KeyCode.F3))
                {
                    this.Invincible.Value = !this.Invincible.Value;
                }
                if (Input.GetKeyDown(KeyCode.F5))
                {
                    string[] array = File.ReadAllLines(XCommon.ModFilesPath + "next_area.txt");
                    this.TeleportToSection(array[0]);
                }
                if (Input.GetKeyDown(KeyCode.F4))
                {
                    this.InfiniteGauge.Value = !this.InfiniteGauge.Value;
                }
                if (Input.GetKeyDown(KeyCode.F6))
                {
                    XDebug.Cheats.CurrentGemLevel++;
                    this.Log(string.Format("Active gem level: {0}", XDebug.Cheats.CurrentGemLevel + 1), 1.5f, 18f);
                }
                if (Input.GetKeyDown(KeyCode.F7))
                {
                    XSingleton<XDebug>.Instance.MaxedOutGems.Value = true;
                }
                if (Input.GetKeyDown(KeyCode.F8))
                {
                    XDebug.Cheats.GetAllGems();
                }
                if (Input.GetKeyDown(KeyCode.F9))
                {
                    this.LoadSettings();
                }
                else if (Input.GetKeyDown(KeyCode.F10))
                {
                    this.SaveSettings();
                }
            }
            else
            {
                XDebug.Comment("================== Ultra FPS (experimental) ==================");
                if (Input.GetKeyDown(KeyCode.F1))
                {
                    this.UltraSmoothFPS.Value = !this.UltraSmoothFPS.Value;
                }
                XDebug.Comment("================== Speed Override ==================");
                if (Input.GetKeyDown(KeyCode.F2))
                {
                    this.EverySpeedMultiplier.Value -= 0.025f;
                }
                else if (Input.GetKeyDown(KeyCode.F3))
                {
                    this.EverySpeedMultiplier.Value += 0.05f;
                }
                XDebug.Comment("================== Custom BGM ==================");
                if (Input.GetKeyDown(KeyCode.F11))
                {
                    this.PlayCustomMusic.Value = false;
                }
                if (Input.GetKeyDown(KeyCode.F10))
                {
                    this.PlayCustomMusic.Value = true;
                }
                XDebug.Comment("================== Info ==================");
                if (Input.GetKeyDown(KeyCode.F6))
                {
                    this.Log(string.Format("tgt: {0}FPS, fdt: {1}s", Application.targetFrameRate, Time.fixedDeltaTime.ToString("0.0000")), 1.5f, 18f);
                }
                if (Input.GetKeyDown(KeyCode.F7))
                {
                    this.Log("Sonic P-06<color=#00ee00>X</color> " + XDebug.P06X_VERSION + " [for Version 4.6]", 3f, 14f);
                }
                if (Input.GetKeyDown(KeyCode.F8))
                {
                    this.Log("log seems to be working", 1.5f, 18f);
                }
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                XDebug.Comment("whatever");
                this.LogExtra("sgflkdsjf dfk sjdlkfj sdf fd sdjlkjlkjl kjsdf kjlk jwlekjr", 1.5f, 0f);
            }
            XDebug.Comment("Load/Save Settings");
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                XDebug.IMMEDIATE_SPINDASH_CLAMP = !XDebug.IMMEDIATE_SPINDASH_CLAMP;
                this.Log(string.Format("<color=#fc9b14>Immediate_Spindash_Clamp</color> = {0} (default: true)", XDebug.IMMEDIATE_SPINDASH_CLAMP), 1.5f, 16f);
                this.Invoke(delegate
                {
                    this.Log("It's useful if you wanna increase ground spd, but leave spindash on normal", 3f, 14f);
                }, 1.75f);
                this.Invoke(delegate
                {
                    this.Log("cause on deafult spindash spd is clamped to 2 * MaxSpeed...", 2.5f, 14f);
                }, 4.75f);
            }
            XDebug.Comment("================== Char Switch ==================");
            if (Singleton<RInput>.Instance.Get<Rewired.Player>("P").GetButton("Left Trigger"))
            {
                bool flag = false;
                if (this.PrevDPadX == 0f && Singleton<RInput>.Instance.Get<Rewired.Player>("P").GetAxis("D-Pad X") != 0f)
                {
                    this.CurrentCharIdx = 0;
                    for (int i = 0; i < this.Characters.Length; i++)
                    {
                        if (this.Characters[i].Item1 == this.Player.PlayerPrefab.ToString())
                        {
                            this.CurrentCharIdx = i;
                            break;
                        }
                    }
                    this.CurrentCharIdx = (this.CurrentCharIdx + ((Singleton<RInput>.Instance.Get<Rewired.Player>("P").GetAxis("D-Pad X") > 0f) ? 1 : (this.Characters.Length - 1))) % this.Characters.Length;
                    flag = true;
                }
                if (flag)
                {
                    this.JustUsedLeftTrigger = true;
                    this.XSwitchTo(this.Characters[this.CurrentCharIdx].Item1, this.Characters[this.CurrentCharIdx].Item2, XDebug.Finder<PlayerBase>.Instance);
                }
            }
            else if (!Singleton<RInput>.Instance.Get<Rewired.Player>("P").GetButtonUp("Left Trigger"))
            {
                this.JustUsedLeftTrigger = false;
            }
            this.PrevDPadX = Singleton<RInput>.Instance.Get<Rewired.Player>("P").GetAxis("D-Pad X");
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                this.XSwitchTo("sonic_new", 0, XDebug.Finder<PlayerBase>.Instance);
                this.CurrentCharIdx = 0;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                this.XSwitchTo("shadow", 5, XDebug.Finder<PlayerBase>.Instance);
                this.CurrentCharIdx = 1;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                this.XSwitchTo("silver", 1, XDebug.Finder<PlayerBase>.Instance);
                this.CurrentCharIdx = 2;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                this.XSwitchTo("tails", 2, XDebug.Finder<PlayerBase>.Instance);
                this.CurrentCharIdx = 3;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                this.XSwitchTo("knuckles", 3, XDebug.Finder<PlayerBase>.Instance);
                this.CurrentCharIdx = 4;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                this.XSwitchTo("rouge", 7, XDebug.Finder<PlayerBase>.Instance);
                this.CurrentCharIdx = 5;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                this.XSwitchTo("omega", 6, XDebug.Finder<PlayerBase>.Instance);
                this.CurrentCharIdx = 6;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                this.XSwitchTo("sonic_fast", 0, XDebug.Finder<PlayerBase>.Instance);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                this.XSwitchTo("princess", 4, XDebug.Finder<PlayerBase>.Instance);
                this.CurrentCharIdx = 7;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                this.XSwitchTo("snow_board", 0, XDebug.Finder<PlayerBase>.Instance);
            }
            else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.Underscore))
            {
                this.XSwitchTo("amy", 9, XDebug.Finder<PlayerBase>.Instance);
            }
            else if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.Equals))
            {
                this.XSwitchTo("blaze", 8, XDebug.Finder<PlayerBase>.Instance);
            }
            XDebug.Comment("================== DEBUG MODE ==================");
            if (XDebug.DBG)
            {
                if (XDebug.COMMENT && Input.GetKeyDown(KeyCode.Keypad5))
                {
                    XDebug.SWITCH = !XDebug.SWITCH;
                    this.Log(string.Format("switch: {0}", XDebug.SWITCH), 1.5f, 18f);
                }
                if (XDebug.COMMENT)
                {
                    XDebug.Comment("this was controlling the position of the logbox");
                    Vector3 vector = Vector3.zero;
                    if (Input.GetKey(KeyCode.Keypad4))
                    {
                        vector += new Vector3(-5f, 0f, 0f);
                    }
                    if (Input.GetKey(KeyCode.Keypad6))
                    {
                        vector += new Vector3(5f, 0f, 0f);
                    }
                    if (Input.GetKey(KeyCode.Keypad8))
                    {
                        vector += new Vector3(0f, 2.5f, 0f);
                    }
                    if (Input.GetKey(KeyCode.Keypad2))
                    {
                        vector += new Vector3(0f, -2.5f, 0f);
                    }
                    if (vector != Vector3.zero && Time.time >= this.LB_NextMoveTime)
                    {
                        this.LB_Move(vector);
                        this.LB_NextMoveTime = Time.time + 0.02f;
                    }
                    if (Input.GetKeyDown(KeyCode.LeftShift))
                    {
                        this.Box_GameObject.transform.SetParent(null, true);
                    }
                }
                if (Input.GetKeyDown(KeyCode.Backslash))
                {
                    Game.ChangeArea("csc_b_sn", "");
                }
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    this.Dump(Directory.GetParent(Application.dataPath).GetFiles("*sonic*.exe")[0].LastWriteTimeUtc);
                    this.Log("ticks: " + Directory.GetParent(Application.dataPath).GetFiles("*sonic*.exe")[0].LastWriteTimeUtc.Ticks.ToString(), 1.5f, 18f);
                }
                if (Input.GetKeyDown(KeyCode.M))
                {
                    string text = "<color=#28DD00>BGM</color> paused...";
                    XDebug.Finder<StageManager>.Instance.Get<AudioSource>("BGMPlayer").Stop();
                    string text2 = "all01_v12_bz";
                    this.MessageLog(text, text2, 4f);
                }
                XDebug.Comment("Life hack");
                if (Input.GetKeyDown(KeyCode.Alpha9))
                {
                    Singleton<GameData>.Instance.Sonic.Lives = 993;
                    this.HUD.Set<int>("Lives", 993);
                }
                if (Input.GetKeyDown(KeyCode.BackQuote))
                {
                    this.Box_GameObject.SetActive(!this.Box_GameObject.activeSelf);
                }
                if (Input.GetKeyDown(KeyCode.Alpha8))
                {
                    if (this.SonicNew)
                    {
                        this.SonicNew.SonicEffects.PM.sonic.Set<bool>("IsSuper", !this.SonicNew.SonicEffects.PM.sonic.Get<bool>("IsSuper"));
                    }
                    if (this.SonicFast)
                    {
                        this.SonicFast.SonicFastEffects.PM.sonic_fast.Set<bool>("IsSuper", !this.SonicFast.SonicFastEffects.PM.sonic_fast.Get<bool>("IsSuper"));
                    }
                }
            }
        }
    }
}
