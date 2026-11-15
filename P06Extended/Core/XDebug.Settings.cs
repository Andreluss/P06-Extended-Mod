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
        private void InitializeXValues()
        {
            for (int i = 0; i < this.dbg_toggles.Length; i++)
            {
                this.dbg_toggles[i] = new XValue<bool>(delegate (bool on)
                {
                }, false);
            }
            for (int j = 0; j < this.dbg_floats.Length; j++)
            {
                this.dbg_floats[j] = new XValue<float>(delegate (float x)
                {
                }, 1f);
            }
            this.tmp0 = new XValue<float>(delegate (float x)
            {
            }, 0.6f);
            this.tmp1 = new XValue<float>(delegate (float x)
            {
            }, -0.25f);
            this.tmp2 = new XValue<float>(delegate (float x)
            {
            }, -0.1f);
            this.Saving_AutoLoad = new XValue<bool>(delegate (bool on)
            {
            }, true);
            this.Saving_AutoSave = new XValue<bool>(delegate (bool on)
            {
            }, true);
            this.UltraSmoothFPS = new XValue<bool>(delegate (bool on)
            {
                XSingleton<XDebug>.Instance.UltraFPS = on;
            }, false);
            this.Extra_DisplaySpeedo = new XValue<bool>(delegate (bool on)
            {
                if (on)
                {
                    this.Log("Speed is now <color=#00ee00>displayed</color>", 1.5f, 18f);
                    return;
                }
                this.Log("Speed is <color=#ee0000>not displayed</color>", 1.5f, 18f);
            }, true);
            this.Invincible = new XValue<bool>(delegate (bool on)
            {
                XDebug.Cfg.Cheats.Immune = on;
                if (XDebug.Cfg.Cheats.Immune)
                {
                    this.Log("Infinite immunity <color=#00ee00>enabled</color>", 1.5f, 18f);
                    return;
                }
                this.Log("Infinite immunity <color=#ee0000>disabled</color>", 1.5f, 18f);
                if (XDebug.Finder<PlayerBase>.Instance)
                {
                    XDebug.Comment("If there was a immune player then disable its immunity");
                    XDebug.Finder<PlayerBase>.Instance.Set("ImmunityTime", Time.time);
                }
            }, false);
            this.InfiniteGauge = new XValue<bool>(delegate (bool on)
            {
                XDebug.Cfg.Cheats.InfiniteGauge = on;
                if (XDebug.Cfg.Cheats.InfiniteGauge)
                {
                    this.Log("Action gauge is now <color=#8f07f0>infinite</color>", 1.5f, 16f);
                    return;
                }
                this.Log("Action gauge is now normal", 1.5f, 16f);
            }, false);
            this.PlayCustomMusic = new XValue<bool>(delegate (bool playcustom)
            {
                if (playcustom)
                {
                    XSingleton<XDebug>.Instance.StartCustomMusic();
                    this.Log("Playing <color=#ffb536>custom</color> music...", 1.5f, 18f);
                    return;
                }
                XSingleton<XDebug>.Instance.StartNormalMusic();
                this.Log("Playing <color=#3dcfff>original</color> music...", 1.5f, 18f);
            }, false);
            this.MaxedOutGems = new XValue<bool>(delegate (bool on)
            {
                if (!on)
                {
                    if (XDebug.Finder<UI>.Instance && XDebug.Finder<UI>.Instance.Get<int[]>("ActiveGemLevel") != null)
                    {
                        if (this.MaxedOutGems_Data != null)
                        {
                            int[] agl = XDebug.Finder<UI>.Instance.Get<int[]>("ActiveGemLevel");
                            this.MaxedOutGems_Data.CopyTo(agl, 0);
                            XDebug.Comment("[investigate] ^^^^^^^^ ");
                            XDebug.Comment("XDebug.Finder<UI>.Instance.ActiveGemLevel = MaxedOutGems_Data.Clone() as int[];");
                        }
                        else
                        {
                            var agl = XDebug.Finder<UI>.Instance.Get<int[]>("ActiveGemLevel");
                            for (int i = 0; i < agl.Length; i++)
                            {
                                agl[i] = 0;
                            }
                        }
                    }
                }
                else
                {
                    if (XDebug.Finder<UI>.Instance == null || XDebug.Finder<UI>.Instance.Get<int[]>("ActiveGemLevel") == null)
                    {
                        this.Log("Error: unable to set gem levels currently", 3f, 14f);
                        XSingleton<XDebug>.Instance.MaxedOutGems.Value = false;
                        return;
                    }
                    if (!XDebug.Cfg.Cheats.MaxedOutGems)
                    {
                        XDebug.Comment("make a copy");
                        this.MaxedOutGems_Data = XDebug.Finder<UI>.Instance.Get<int[]>("ActiveGemLevel").Clone() as int[];
                    }
                    for (int k = 0; k < 9; k++)
                    {
                        XDebug.Finder<UI>.Instance.Get<int[]>("ActiveGemLevel")[k] = 2;
                    }
                    XSingleton<XDebug>.Instance.Log("All gems <color=#de921f>maxed out</color>", 1.5f, 18f);
                }
                XDebug.Cfg.Cheats.MaxedOutGems = on;
            }, false);
            this.InfiniteLives = new XValue<bool>(delegate (bool on)
            {
                int num = 9999999;
                if (on)
                {
                    XDebug.Cheats.Lives += num;
                    return;
                }
                XDebug.Cheats.Lives = Math.Max(XDebug.Cheats.Lives - num, 3);
            }, false);
            this.InfiniteRings = new XValue<bool>(delegate (bool on)
            {
                XDebug.Cfg.Cheats.InfiniteRings = on;
                if (!on)
                {
                    XDebug.Cheats.Rings = 50;
                }
            }, false);
            this.Cheat_ChainJumpZeroDelay = new XValue<bool>(delegate (bool on)
            {
                this.Log("Zero delay for chain jump " + (on ? "<color=#00ee00>enabled</color>" : "<color=#ee0000>disabled</color>"), 1.5f, 16f);
            }, false);
            this.Cheat_IgnoreWaterDeath = new XValue<bool>(delegate (bool on)
            {
                this.Log("Water immunity " + (on ? "<color=#00ee00>enabled</color>" : "<color=#ee0000>disabled</color>"), 1.5f, 16f);
            }, false);
            this.Moveset_FreeWaterSliding = new XValue<bool>(delegate (bool on)
            {
                this.Log("Free Water Sliding" + (on ? "<color=#00ee00>enabled</color>" : "<color=#ee0000>disabled</color>"), 1.5f, 16f);
            }, true);
            this.Moveset_WallJumping = new XValue<bool>(delegate (bool on)
            {
                this.Log("Wall Jump " + (on ? "<color=#00ee00>enabled</color>" : "<color=#ee0000>disabled</color>"), 1.5f, 16f);
            }, true);
            this.Moveset_ClimbAll = new XValue<bool>(delegate (bool on)
            {
                this.Log("Climb All (for K&R) " + (on ? "<color=#00ee00>enabled</color>" : "<color=#ee0000>disabled</color>"), 1.5f, 16f);
            }, true);
            this.Moveset_Boost = new XValue<bool>(delegate (bool on)
            {
                this.Log("Boost :X " + (on ? "<color=#00ee00>enabled</color>" : "<color=#ee0000>disabled</color>"), 1.5f, 16f);
            }, true);
            this.Moveset_AHMovement = new XValue<bool>(delegate (bool on)
            {
                this.Log("After Homing Movement " + (on ? "<color=#00ee00>enabled</color>" : "<color=#ee0000>disabled</color>"), 1.5f, 16f);
            }, true);
            this.Moveset_AHMovementMaxSpeed = new XValue<float>(delegate (float value)
            {
                this.Log("Max After Hom. Speed: " + value.ToString("0.00"), 1.5f, 16f);
            }, 10f);
            this.TeleportLocation = new XValue<string>(delegate (string s)
            {
            }, "test_b_sn");
            this.ASCSpinClamp = new XValue<bool>(delegate (bool on)
            {
                XDebug.IMMEDIATE_SPINDASH_CLAMP = on;
                this.Log(string.Format("<color=#fc9b14>Immediate_Spindash_Clamp</color> = {0} (default: true)", XDebug.IMMEDIATE_SPINDASH_CLAMP), 1.5f, 16f);
                this.LogExtra("It's useful if you wanna increase ground spd, but leave spindash on normal\ncause on deafult spindash spd is clamped to 2 * MaxSpeed...", 5f, 0f);
            }, true);
            this.ASCLuaRecalc = new XValue<bool>(delegate (bool on)
            {
                this.Log(string.Format("<color=#fc9b14>Lua_recalc</color> = {0} (default: true)", XDebug.IMMEDIATE_SPINDASH_CLAMP), 1.5f, 16f);
                this.LogExtra("If enabled, then other speed realated values will be updated (e.g. acceleration)", 3.14f, 0f);
            }, true);
            this.Other_OgCameraControls = new XValue<bool>(delegate (bool on)
            {
                if (on)
                {
                    this.Log("Using LB/RB as <color=#fc9b14>camera controls</color>.\nTo dodge, press the LStick and then LB/RB.", 3f, 12.5f);
                    return;
                }
                this.Log("Using LB/RB as <color=#fc9b14>dodge controls</color> (default).", 2f, 12.5f);
            }, true);
            this.Other_UltraFPSFix = new XValue<bool>(delegate (bool on)
            {
                this.Log("UltraFPS Fix " + (on ? "<color=#00ee00>enabled</color>" : "<color=#ee0000>disabled</color>"), 1.5f, 16f);
            }, true);
            this.Other_CheckP06Version = new XValue<bool>(delegate (bool on)
            {
                this.Log("Check P-06 Version " + (on ? "<color=#00ee00>enabled</color>" : "<color=#ee0000>disabled</color>"), 1.5f, 16f);
            }, true);

            foreach (var speedType in EnumUtil.GetValues<CustomSpeedMultiplier.LUASpeedType>())
            {
                SM[speedType] = new XValue<float>(delegate (float x)
                {
                    CustomSpeedMultiplier.ByType[speedType] = x;
                    USING_CUSTOM_SPEEDS = true;
                    CustomSpeedMultiplier.UpdateLUA(ASCLuaRecalc.Value);
                    this.Log(string.Format("<color=#d214fc>cstm_spd_{0}</color> = {1}", speedType.ToString().ToLower(), x), 1.25f, 16f);
                }, 1f);
            }

            this.SMHomingAttackFasterBy = new XValue<float>(delegate (float x)
            {
                XDebug.CustomSpeedMultiplier.HomingAttackTimeShortener = 1f / x;
                XDebug.USING_CUSTOM_SPEEDS = true;
                this.Log(string.Format("<color=#d214fc>homing_attack_faster_by</color> = {0}", x), 1.25f, 16f);
            }, 1f);
            this.SMAfterHomingRotation = new XValue<float>(delegate (float x)
            {
                XDebug.CustomSpeedMultiplier.AfterHomingRotationSpeed = x * 0.75f * ReflectionExtensions.GetLuaStruct("Sonic_New_Lua").Get<float>("c_rotation_speed");
                XDebug.USING_CUSTOM_SPEEDS = true;
                this.Log(string.Format("<color=#d214fc>after_homing_rotation</color> = {0}", x), 1.25f, 16f);
            }, 1f);
            this.EverySpeedMultiplier = new XValue<float>(delegate (float mul)
            {
                this.SpeedMultiplier = mul;
                foreach (var speedType in EnumUtil.GetValues<CustomSpeedMultiplier.LUASpeedType>())
                {
                    SM[speedType].Value = mul;
                }
                this.SMHomingAttackFasterBy.Value = mul;
                this.SMAfterHomingRotation.Value = mul;
                this.Log(string.Format("<color=#ff9500>Speed Multiplier</color>: {0}", this.SpeedMultiplier.ToString("0.000")), 1.5f, 18f);
                CustomSpeedMultiplier.UpdateLUA(ASCLuaRecalc.Value);
                XDebug.USING_CUSTOM_SPEEDS = false;
            }, 1f);
            this.Boost_BaseSpeed = new XValue<float>(delegate (float spd)
            {
                this.Log(string.Format("<color=#ff9500>Base Boost Speed</color>: {0}", spd.ToString("0.00")), 1.5f, 16f);
            }, 40f);
            this.Boost_NextLevelDeltaSpeed = new XValue<float>(delegate (float spd)
            {
                this.Log(string.Format("Boost levels will increase speed by: {0}", spd.ToString("0.00")), 1.5f, 14f);
            }, 20f);
            this.Boost_RotSpeed = new XValue<float>(delegate (float spd)
            {
                this.Log(string.Format("Rotation speed when boosting: {0}", spd.ToString("0.00")), 1.5f, 14f);
            }, 3f);
            this.Boost_AccelTime = new XValue<float>(delegate (float spd)
            {
                this.Log(string.Format("Boost will accelerate to target speed in {0}s", spd.ToString("0.00")), 1.5f, 14f);
            }, 0.5f);
            this.Boost_NextLevelThreshold = new XValue<float>(delegate (float spd)
            {
                this.Log(string.Format("Faster boost when speed it at most {0} less than current level target", spd.ToString("0.00")), 1.5f, 14f);
            }, 5f);
            this.BoxExtra.GameObject.SetActive(false);
        }

        public void TeleportToSection(string section)
        {
            if (section == null || section.Length <= 0)
            {
                this.Log("Couldn't teleport to: <color=#8f0000>" + section + "</color>", 5f, 18f);
                return;
            }
            section = section.Trim();
            if (SceneUtility.GetBuildIndexByScenePath(section) != -1)
            {
                this.Log("Switching to: <color=#8f07f0>" + section + "</color>", 3.5f, 18f);
                Game.ChangeArea(section, "");
                return;
            }
            this.Log("<color=#ee0000>Section</color> \"" + section + "\" <color=#ee0000>doesn't exist</color>!", 3f, 16f);
        }



        public void LoadSettings()
        {
            Dictionary<string, object> settingsDict = this.GetSettingsDict();
            if (settingsDict == null)
            {
                this.Log("user_settings.xml file <color=#ee0000>not found</color>!", 1.5f, 18f);
                return;
            }
            this.Dump(settingsDict);
            foreach (XUISection xuisection in Singleton<XModMenu>.Instance.Menu.Sections)
            {
                if (settingsDict.ContainsKey("[is_collapsed]" + xuisection.gameObject.name))
                {
                    xuisection.Toggle(!(bool)settingsDict["[is_collapsed]" + xuisection.gameObject.name]);
                }
                foreach (XUIItem xuiitem in xuisection.Items)
                {
                    if (settingsDict.ContainsKey(xuiitem.Name))
                    {
                        XUIToggleButton xuitoggleButton;
                        XUIFloatAdjuster xuifloatAdjuster;
                        XUIStringInput xuistringInput;
                        if ((xuitoggleButton = xuiitem as XUIToggleButton) != null)
                        {
                            xuitoggleButton.BindedXValue.Value = (bool)settingsDict[xuiitem.Name];
                        }
                        else if ((xuifloatAdjuster = xuiitem as XUIFloatAdjuster) != null)
                        {
                            xuifloatAdjuster.BindedXValue.Value = (float)settingsDict[xuiitem.Name];
                        }
                        else if ((xuistringInput = xuiitem as XUIStringInput) != null)
                        {
                            xuistringInput.BindedXValue.Value = (string)settingsDict[xuiitem.Name];
                        }
                    }
                }
            }
            this.Log("Settings <color=#00ee00>loaded</color>", 1.5f, 18f);
        }

        public void SaveSettings()
        {
            XUIMenu menu = Singleton<XModMenu>.Instance.Menu;
            List<ValueTuple<string, object>> list = new List<ValueTuple<string, object>>();
            foreach (XUISection xuisection in menu.Sections)
            {
                list.Add(new ValueTuple<string, object>("[is_collapsed]" + xuisection.gameObject.name, xuisection.IsCollapsed));
                foreach (XUIItem xuiitem in xuisection.Items)
                {
                    XDebug.Comment("xD i know it's bad design, but i didn't want to change xui classes cause they decompile really badly");
                    XUIToggleButton xuitoggleButton;
                    XUIFloatAdjuster xuifloatAdjuster;
                    XUIStringInput xuistringInput;
                    if ((xuitoggleButton = xuiitem as XUIToggleButton) != null)
                    {
                        list.Add(new ValueTuple<string, object>(xuiitem.Name, xuitoggleButton.State));
                    }
                    else if ((xuifloatAdjuster = xuiitem as XUIFloatAdjuster) != null)
                    {
                        list.Add(new ValueTuple<string, object>(xuiitem.Name, xuifloatAdjuster.Value));
                    }
                    else if ((xuistringInput = xuiitem as XUIStringInput) != null)
                    {
                        list.Add(new ValueTuple<string, object>(xuiitem.Name, xuistringInput.Value));
                    }
                }
            }
            XUtility.SerializeObjectToXml<List<ValueTuple<string, object>>>(list, XCommon.ModFilesPath + "user_settings.xml");
            this.Dump(list);
            this.Log("Settings <color=#00ee00>saved</color>", 1.5f, 18f);
        }
    }
}
