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
    public class XModMenu : Singleton<XModMenu>
    {
        private void Awake()
        {
            this.Menu = new XUIMenu("P-06X " + XDebug.P06X_VERSION, KeyCode.F12);
            this.Canvas = this.Menu.gameObject.transform.parent.GetComponent<Canvas>();
            if (XDebug.DBG)
            {
                XUISection xuisection = this.Menu.AddSection(new XUISection("Debug"));
                for (int i = 0; i < XSingleton<XDebug>.Instance.dbg_toggles.Length; i++)
                {
                    xuisection.AddItem(new XUIToggleButton("dbg_toggle_" + i.ToString(), XSingleton<XDebug>.Instance.dbg_toggles[i]));
                }
                for (int j = 0; j < XSingleton<XDebug>.Instance.dbg_floats.Length; j++)
                {
                    xuisection.AddItem(new XUIFloatAdjuster("dbg_slider_" + j.ToString(), XSingleton<XDebug>.Instance.dbg_floats[j], -0.1f, 0.5f, 3));
                }
            }
            XUISection xuisection2 = this.Menu.AddSection(new XUISection("Quick Settings"));
            xuisection2.AddItem(new XUIToggleButton("Ultra Smooth FPS", XSingleton<XDebug>.Instance.UltraSmoothFPS));
            xuisection2.AddItem(new XUIToggleButton("Custom Music", XSingleton<XDebug>.Instance.PlayCustomMusic));
            xuisection2.AddItem(new XUIFloatAdjuster("Global Speed Multiplier", XSingleton<XDebug>.Instance.EverySpeedMultiplier, -0.1f, 0.2f, 3));
            xuisection2.AddItem(new XUIToggleButton("Free Water Sliding", XSingleton<XDebug>.Instance.Moveset_FreeWaterSliding));
            xuisection2.AddItem(new XUIToggleButton("Wall Jump", XSingleton<XDebug>.Instance.Moveset_WallJumping));
            xuisection2.AddItem(new XUIToggleButton("Climb All Walls", XSingleton<XDebug>.Instance.Moveset_ClimbAll));
            xuisection2.AddItem(new XUIToggleButton("Boost", XSingleton<XDebug>.Instance.Moveset_Boost));
            xuisection2.AddItem(new XUIToggleButton("Speedometer", XSingleton<XDebug>.Instance.Extra_DisplaySpeedo));
            if (XDebug.COMMENT)
            {
                xuisection2.AddItem(new XUIToggleButton("<color=#4a4a4a>A</color>fter <color=#4a4a4a>H</color>oming <color=#4a4a4a>M</color>ovement", XSingleton<XDebug>.Instance.Moveset_AHMovement));
                xuisection2.AddItem(new XUIFloatAdjuster("<color=#4a4a4a>AHM</color> Max Speed", XSingleton<XDebug>.Instance.Moveset_AHMovementMaxSpeed, -1f, 2f, 1, 0f, float.PositiveInfinity));
            }
            else
            {
                xuisection2.AddItem(new XUIToggleButton("After Homing Movement", XSingleton<XDebug>.Instance.Moveset_AHMovement));
                xuisection2.AddItem(new XUIFloatAdjuster("AHM Max Speed", XSingleton<XDebug>.Instance.Moveset_AHMovementMaxSpeed, -1f, 2f, 1, 0f, float.PositiveInfinity));
            }
            XUISection xuisection3 = this.Menu.AddSection(new XUISection("Cheats"));
            xuisection3.AddItem(new XUIToggleButton("Always Invincible", XSingleton<XDebug>.Instance.Invincible));
            xuisection3.AddItem(new XUIToggleButton("Infinite Gauge", XSingleton<XDebug>.Instance.InfiniteGauge));
            xuisection3.AddItem(new XUIToggleButton("Maxed Out Gems", XSingleton<XDebug>.Instance.MaxedOutGems));
            xuisection3.AddItem(new XUIFunctionButton("Get All Gems", delegate
            {
                XDebug.Cheats.GetAllGems();
            }));
            xuisection3.AddItem(new XUIFunctionButton("Get Light Memory Shard", delegate
            {
                XDebug.Cheats.GetLightMemoryShard();
            }));
            xuisection3.AddItem(new XUIFunctionButton("Get Lotus of Resilience", delegate
            {
                XDebug.Cheats.GetLotus();
            }));
            xuisection3.AddItem(new XUIFunctionButton("Get Flame of Control", delegate
            {
                XDebug.Cheats.GetFlame();
            }));
            xuisection3.AddItem(new XUIFunctionButton("Get Sigil of Awakening", delegate
            {
                XDebug.Cheats.GetSigil();
            }));
            xuisection3.AddItem(new XUIToggleButton("Infinite Rings", XSingleton<XDebug>.Instance.InfiniteRings));
            xuisection3.AddItem(new XUIToggleButton("Infinite Lives", XSingleton<XDebug>.Instance.InfiniteLives));
            xuisection3.AddItem(new XUIStringInput("Teleport location", XSingleton<XDebug>.Instance.TeleportLocation, null, TMP_InputField.CharacterValidation.None));
            xuisection3.AddItem(new XUIFunctionButton("TELEPORT", delegate
            {
                XSingleton<XDebug>.Instance.TeleportToSection(XSingleton<XDebug>.Instance.TeleportLocation.Value);
            }));
            xuisection3.AddItem(new XUIToggleButton("Water immunity", XSingleton<XDebug>.Instance.Cheat_IgnoreWaterDeath));
            xuisection3.AddItem(new XUIToggleButton("Faster Chain Jump", XSingleton<XDebug>.Instance.Cheat_ChainJumpZeroDelay));
            XUISection xuisection4 = this.Menu.AddSection(new XUISection("Advanced Speed Control"));
            xuisection4.AddItem(new XUIFloatAdjuster("Ground", XSingleton<XDebug>.Instance.SM[XDebug.CustomSpeedMultiplier.LUASpeedType.Ground], -0.05f, 0.1f, 3));
            xuisection4.AddItem(new XUIFloatAdjuster("Air", XSingleton<XDebug>.Instance.SM[XDebug.CustomSpeedMultiplier.LUASpeedType.Air], -0.05f, 0.1f, 3));
            xuisection4.AddItem(new XUIFloatAdjuster("Spindash", XSingleton<XDebug>.Instance.SM[XDebug.CustomSpeedMultiplier.LUASpeedType.Spindash], -0.05f, 0.1f, 3));
            xuisection4.AddItem(new XUIFloatAdjuster("Flying", XSingleton<XDebug>.Instance.SM[XDebug.CustomSpeedMultiplier.LUASpeedType.Flying], -0.05f, 0.1f, 3));
            xuisection4.AddItem(new XUIFloatAdjuster("Climbing", XSingleton<XDebug>.Instance.SM[XDebug.CustomSpeedMultiplier.LUASpeedType.Climb], -0.05f, 0.1f, 3));
            xuisection4.AddItem(new XUIFloatAdjuster("Homing", XSingleton<XDebug>.Instance.SM[XDebug.CustomSpeedMultiplier.LUASpeedType.Homing], -0.05f, 0.1f, 3));
            xuisection4.AddItem(new XUIFloatAdjuster("H. Attack", XSingleton<XDebug>.Instance.SMHomingAttackFasterBy, -0.05f, 0.1f, 3));
            xuisection4.AddItem(new XUIFloatAdjuster("After H. Rotation", XSingleton<XDebug>.Instance.SMAfterHomingRotation, -0.05f, 0.1f, 3));
            XUISection xuisection5 = this.Menu.AddSection(new XUISection("Boost"));
            xuisection5.AddItem(new XUIFloatAdjuster("Base Speed", XSingleton<XDebug>.Instance.Boost_BaseSpeed, -10f, 10f, 0, 0f, float.PositiveInfinity));
            xuisection5.AddItem(new XUIFloatAdjuster("Delta Speed", XSingleton<XDebug>.Instance.Boost_NextLevelDeltaSpeed, -10f, 10f, 0, 0f, float.PositiveInfinity));
            xuisection5.AddItem(new XUIFloatAdjuster("Turn speed", XSingleton<XDebug>.Instance.Boost_RotSpeed, -1f, 1f, 1, 0f, float.PositiveInfinity));
            xuisection5.AddItem(new XUIFloatAdjuster("Acceleration time", XSingleton<XDebug>.Instance.Boost_AccelTime, -0.1f, 0.25f, 2, 0.01f, float.PositiveInfinity));
            xuisection5.AddItem(new XUIFloatAdjuster("Level Up Threshold", XSingleton<XDebug>.Instance.Boost_NextLevelThreshold, -2f, 3f, 1, 0f, float.PositiveInfinity));
            this.Menu.AddSection(new XUISection("Other")).AddItem(new XUIToggleButton("Old Camera Controls", XSingleton<XDebug>.Instance.Other_OgCameraControls)).AddItem(new XUIToggleButton("FDT Fix (Beta)", XSingleton<XDebug>.Instance.Other_UltraFPSFix))
                .AddItem(new XUIToggleButton("Version check", XSingleton<XDebug>.Instance.Other_CheckP06Version));
            XUISection xuisection6 = this.Menu.AddSection(new XUISection("Saving"));
            xuisection6.AddItem(new XUIFunctionButton("Save Settings", delegate
            {
                XSingleton<XDebug>.Instance.SaveSettings();
            }));
            xuisection6.AddItem(new XUIFunctionButton("Load Settings", delegate
            {
                XSingleton<XDebug>.Instance.LoadSettings();
            }));
            xuisection6.AddItem(new XUIToggleButton("Load automatically", XSingleton<XDebug>.Instance.Saving_AutoLoad));
            xuisection6.AddItem(new XUIToggleButton("Save automatically", XSingleton<XDebug>.Instance.Saving_AutoSave));
        }



        private void Update()
        {
        }

        public Canvas Canvas;

        public XUIMenu Menu;
    }
}
