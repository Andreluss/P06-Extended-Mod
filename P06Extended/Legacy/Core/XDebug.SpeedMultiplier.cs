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
        private struct ColorConstants
        {
            public static Color SmoothSwitchOverride = new Color(0.7216338f, 0.1462264f, 1f, 0.5f) * 1.75f;
        }

        public struct CustomSpeedMultiplier
        {
            public enum LUASpeedType { Ground, Air, Spindash, Flying, Climb, Homing }
            public static ArrayByEnum<LUASpeedType, float> ByType = new ArrayByEnum<LUASpeedType, float>(1f);
            public static float HomingAttackTimeShortener = 1f;

            public static float AfterHomingRotationSpeed = 1f;

            public static Dictionary<(string, string, LUASpeedType), float> OriginalSpeedLUAs = new Dictionary<(string, string, LUASpeedType), float>();

            public static void InitializeOriginalLUA()
            {
                var luas = new (string, string, LUASpeedType)[] {
                    ("Sonic_New_Lua", "c_run_speed_max", LUASpeedType.Ground),
                    ("Sonic_New_Lua", "c_jump_run", LUASpeedType.Air),
                    ("Sonic_New_Lua", "c_spindash_spd", LUASpeedType.Spindash),
                    ("Sonic_New_Lua", "c_speedup_speed_max", LUASpeedType.Ground),
                    ("Sonic_Fast_Lua", "c_walk_speed_max", LUASpeedType.Ground),
                    ("Sonic_Fast_Lua", "c_run_speed_max", LUASpeedType.Ground),
                    ("Sonic_Fast_Lua", "c_lightdash_speed", LUASpeedType.Air),
                    ("Sonic_Fast_Lua", "c_lightdash_mid_speed", LUASpeedType.Air),
                    ("Sonic_Fast_Lua", "c_lightdash_mid_speed_super", LUASpeedType.Air),
                    ("Tails_Lua", "c_run_speed_max", LUASpeedType.Ground),
                    ("Tails_Lua", "c_jump_run", LUASpeedType.Air),
                    ("Tails_Lua", "c_flight_speed_max", LUASpeedType.Flying),
                    ("Tails_Lua", "c_speedup_speed_max", LUASpeedType.Ground),
                    ("Shadow_Lua", "c_run_speed_max", LUASpeedType.Ground),
                    ("Shadow_Lua", "c_speedup_speed_max", LUASpeedType.Ground),
                    ("Shadow_Lua", "c_jump_run", LUASpeedType.Air),
                    ("Shadow_Lua", "c_spindash_spd", LUASpeedType.Spindash),
                    ("Knuckles_Lua", "c_climb_speed", LUASpeedType.Climb),
                    ("Knuckles_Lua", "c_run_speed_max", LUASpeedType.Ground),
                    ("Knuckles_Lua", "c_speedup_speed_max", LUASpeedType.Ground),
                    ("Knuckles_Lua", "c_jump_run", LUASpeedType.Air),
                    ("Knuckles_Lua", "c_flight_speed_max", LUASpeedType.Flying),
                    ("Omega_Lua", "c_run_speed_max", LUASpeedType.Ground),
                    ("Omega_Lua", "l_jump_run", LUASpeedType.Air),
                    ("Omega_Lua", "c_speedup_speed_max", LUASpeedType.Ground),
                    ("Princess_Lua", "c_run_speed_max", LUASpeedType.Ground),
                    ("Princess_Lua", "c_speedup_speed_max", LUASpeedType.Ground),
                    ("Princess_Lua", "c_jump_run", LUASpeedType.Air),
                    ("Rouge_Lua", "c_run_speed_max", LUASpeedType.Ground),
                    ("Rouge_Lua", "c_speedup_speed_max", LUASpeedType.Ground),
                    ("Rouge_Lua", "c_jump_run", LUASpeedType.Air),
                    ("Rouge_Lua", "c_flight_speed_max", LUASpeedType.Flying),
                    ("Rouge_Lua", "c_climb_speed", LUASpeedType.Climb),
                    ("Silver_Lua", "c_run_speed_max", LUASpeedType.Ground),
                    ("Silver_Lua", "c_speedup_speed_max", LUASpeedType.Ground),
                    ("Silver_Lua", "c_jump_run", LUASpeedType.Air),
                    ("Silver_Lua", "c_float_walk_speed", LUASpeedType.Ground),
                    ("Shadow_Lua", "c_homing_spd", LUASpeedType.Homing),
                    ("Sonic_New_Lua", "c_homing_spd", LUASpeedType.Homing),
                    ("Princess_Lua", "c_homing_spd", LUASpeedType.Homing)
                };

                OriginalSpeedLUAs.Clear();
                Assembly ass = Assembly.GetAssembly(typeof(SonicNew));

                foreach (var (character, lua, type) in luas)
                {
                    var originalValue = ass.GetType("STHLua." + character).Get<float>(lua);
                    OriginalSpeedLUAs[(character, lua, type)] = originalValue;
                }
            }

            public static void ResetLUA()
            {
                Assembly ass = Assembly.GetAssembly(typeof(SonicNew));
                foreach (var kv in OriginalSpeedLUAs)
                {
                    var (character, lua, _) = kv.Key;
                    var originalValue = kv.Value;
                    ass.GetType("STHLua." + character).Set<float>(lua, originalValue);
                }
            }

            public static void UpdateLUA(bool recalc_lua)
            {
                // TODO: unfinished
                Assembly ass = Assembly.GetAssembly(typeof(SonicNew));
                foreach (var kv in OriginalSpeedLUAs)
                {
                    var (character, lua, speedType) = kv.Key;
                    var originalValue = kv.Value;
                    ass.GetType("STHLua." + character).Set<float>(lua, originalValue * XDebug.CustomSpeedMultiplier.ByType[speedType]);
                }
                if (recalc_lua)
                {
                    RecalcLua();
                }
                /*
                this.ResetLUA();
                Sonic_New_Lua.c_run_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
                Sonic_New_Lua.c_speedup_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
                Sonic_Fast_Lua.c_walk_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
                Sonic_Fast_Lua.c_run_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
                Tails_Lua.c_run_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
                Tails_Lua.c_speedup_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
                Shadow_Lua.c_run_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
                Shadow_Lua.c_speedup_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
                Knuckles_Lua.c_run_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
                Knuckles_Lua.c_speedup_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
                Omega_Lua.c_run_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
                Omega_Lua.c_speedup_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
                Princess_Lua.c_run_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
                Princess_Lua.c_speedup_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
                Rouge_Lua.c_run_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
                Rouge_Lua.c_speedup_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
                Silver_Lua.c_run_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
                Silver_Lua.c_speedup_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
                Silver_Lua.c_float_walk_speed *= XDebug.CustomSpeedMultiplier.Ground;
                Sonic_New_Lua.c_jump_run *= XDebug.CustomSpeedMultiplier.Air;
                Sonic_Fast_Lua.c_lightdash_speed *= XDebug.CustomSpeedMultiplier.Air;
                Sonic_Fast_Lua.c_lightdash_mid_speed *= XDebug.CustomSpeedMultiplier.Air;
                Sonic_Fast_Lua.c_lightdash_mid_speed_super *= XDebug.CustomSpeedMultiplier.Air;
                Tails_Lua.c_jump_run *= XDebug.CustomSpeedMultiplier.Air;
                Shadow_Lua.c_jump_run *= XDebug.CustomSpeedMultiplier.Air;
                Knuckles_Lua.c_jump_run *= XDebug.CustomSpeedMultiplier.Air;
                Omega_Lua.l_jump_run *= XDebug.CustomSpeedMultiplier.Air;
                Princess_Lua.c_jump_run *= XDebug.CustomSpeedMultiplier.Air;
                Rouge_Lua.c_jump_run *= XDebug.CustomSpeedMultiplier.Air;
                Silver_Lua.c_jump_run *= XDebug.CustomSpeedMultiplier.Air;
                Sonic_New_Lua.c_spindash_spd *= XDebug.CustomSpeedMultiplier.Spindash;
                Shadow_Lua.c_spindash_spd *= XDebug.CustomSpeedMultiplier.Spindash;
                Tails_Lua.c_flight_speed_max *= XDebug.CustomSpeedMultiplier.Flying;
                Shadow_Lua.c_spindash_spd *= XDebug.CustomSpeedMultiplier.Flying;
                Knuckles_Lua.c_flight_speed_max *= XDebug.CustomSpeedMultiplier.Flying;
                Rouge_Lua.c_flight_speed_max *= XDebug.CustomSpeedMultiplier.Flying;
                Knuckles_Lua.c_climb_speed *= XDebug.CustomSpeedMultiplier.Climbing;
                Rouge_Lua.c_climb_speed *= XDebug.CustomSpeedMultiplier.Climbing;
                Sonic_New_Lua.c_homing_spd *= XDebug.CustomSpeedMultiplier.Homing;
                Shadow_Lua.c_homing_spd *= XDebug.CustomSpeedMultiplier.Homing;
                Princess_Lua.c_homing_spd *= XDebug.CustomSpeedMultiplier.Homing;
                */
            }

            class LUAWrapper
            {
                float Get(string character, string lua)
                {
                    Assembly assembly = Assembly.GetAssembly(typeof(SonicNew));
                    return assembly.GetType("STHLua." + character).Get<float>(lua);
                }
                void Set(string character, string lua, float value)
                {
                    Assembly assembly = Assembly.GetAssembly(typeof(SonicNew));
                    assembly.GetType("STHLua." + character).Set<float>(lua, value);
                }

                // add support for invoking methods:
                public float Invoke(string character, string lua, object[] args)
                {
                    Assembly assembly = Assembly.GetAssembly(typeof(SonicNew));

                    return (float)assembly.GetType("STHLua." + character).InvokeStaticMethod<float>(lua, args);
                }

                // implement [] operator for cleaner code
                public float this[string character, string lua] { get => Get(character, lua); set => Set(character, lua, value); }
            }

            private static void RecalcLua()
            {
                /* Similar way as in above functions, use reflection to recalculate values as given below:
                Sonic_New_Lua.c_run_acc = (Sonic_New_Lua.c_run_speed_max - Sonic_New_Lua.c_walk_speed_max) / Sonic_New_Lua.l_run_acc;
                Sonic_New_Lua.c_speedup_acc = (Sonic_New_Lua.c_speedup_speed_max - Sonic_New_Lua.c_walk_speed_max) / Sonic_New_Lua.l_speedup_acc;
                Sonic_New_Lua.c_bound_jump_spd_0 = Common_Lua.HeightToSpeed(Sonic_New_Lua.l_bound_jump_height0);
                Sonic_New_Lua.c_bound_jump_spd_1 = Common_Lua.HeightToSpeed(Sonic_New_Lua.l_bound_jump_height1);
                Sonic_New_Lua.c_homing_brake = (Sonic_New_Lua.c_homing_spd - Sonic_New_Lua.c_jump_run_orig) / Sonic_New_Lua.c_homing_time;
                Sonic_Fast_Lua.c_run_acc = (Sonic_Fast_Lua.c_run_speed_max - Sonic_Fast_Lua.c_walk_speed_max) / Sonic_Fast_Lua.l_run_acc;
                Knuckles_Lua.c_run_acc = (Knuckles_Lua.c_run_speed_max - Knuckles_Lua.c_walk_speed_max) / Knuckles_Lua.l_run_acc;
                Knuckles_Lua.c_speedup_acc = (Knuckles_Lua.c_speedup_speed_max - Knuckles_Lua.c_walk_speed_max) / Knuckles_Lua.l_speedup_acc;
                Omega_Lua.c_run_acc = (Omega_Lua.c_run_speed_max - Omega_Lua.c_walk_speed_max) / Omega_Lua.l_run_acc;
                Omega_Lua.c_jump_walk = Omega_Lua.l_jump_walk / (2f * Mathf.Sqrt(2f * Omega_Lua.l_jump_hight / 9.81f));
                Omega_Lua.c_jump_run = Omega_Lua.l_jump_run / (2f * Mathf.Sqrt(2f * Omega_Lua.l_jump_hight / 9.81f));
                Omega_Lua.c_speedup_acc = (Omega_Lua.c_speedup_speed_max - Omega_Lua.c_walk_speed_max) / Omega_Lua.l_speedup_acc;
                Princess_Lua.c_run_acc = (Princess_Lua.c_run_speed_max - Princess_Lua.c_walk_speed_max) / Princess_Lua.l_run_acc;
                Princess_Lua.c_jump_walk = Common_Lua.HeightAndDistanceToSpeed(Princess_Lua.l_jump_walk, Princess_Lua.l_jump_hight);
                Princess_Lua.c_speedup_acc = (Princess_Lua.c_speedup_speed_max - Princess_Lua.c_walk_speed_max) / Princess_Lua.l_speedup_acc;
                Princess_Lua.c_homing_brake = (Princess_Lua.c_homing_spd - Princess_Lua.c_jump_run_orig) / Princess_Lua.c_homing_time;
                Rouge_Lua.c_run_acc = (Rouge_Lua.c_run_speed_max - Rouge_Lua.c_walk_speed_max) / Rouge_Lua.l_run_acc;
                Rouge_Lua.c_speedup_acc = (Rouge_Lua.c_speedup_speed_max - Rouge_Lua.c_walk_speed_max) / Rouge_Lua.l_speedup_acc;
                Shadow_Lua.c_run_acc = (Shadow_Lua.c_run_speed_max - Shadow_Lua.c_walk_speed_max) / Shadow_Lua.l_run_acc;
                Shadow_Lua.c_speedup_acc = (Shadow_Lua.c_speedup_speed_max - Shadow_Lua.c_walk_speed_max) / Shadow_Lua.l_speedup_acc;
                Shadow_Lua.c_homing_brake = (Shadow_Lua.c_homing_spd - Shadow_Lua.c_jump_run_orig) / Shadow_Lua.c_homing_time;
                Silver_Lua.c_run_acc = (Silver_Lua.c_run_speed_max - Silver_Lua.c_walk_speed_max) / Silver_Lua.l_run_acc;
                Silver_Lua.c_speedup_acc = (Silver_Lua.c_speedup_speed_max - Silver_Lua.c_walk_speed_max) / Silver_Lua.l_speedup_acc;
                Silver_Lua.c_tele_dash_speed = Silver_Lua.l_tele_dash / Silver_Lua.c_tele_dash_time;
                Silver_Lua.c_psi_gauge_catch_ride = Silver_Lua.psi_power / Silver_Lua.l_psi_gauge_catch_ride;
                Silver_Lua.c_psi_gauge_float = Silver_Lua.psi_power / (Silver_Lua.l_psi_gauge_float / (Silver_Lua.c_float_walk_speed / 1.85f));
                Silver_Lua.c_psi_gauge_teleport_dash_burn = Silver_Lua.psi_power / (Silver_Lua.l_psi_gauge_float / (Silver_Lua.c_float_walk_speed / 2f));
                Sonic_Fast_Lua.c_run_acc = (Sonic_Fast_Lua.c_run_speed_max - Sonic_Fast_Lua.c_walk_speed_max) / Sonic_Fast_Lua.l_run_acc;
                Tails_Lua.c_run_acc = (Tails_Lua.c_run_speed_max - Tails_Lua.c_walk_speed_max) / Tails_Lua.l_run_acc;
                Tails_Lua.c_speedup_acc = (Tails_Lua.c_speedup_speed_max - Tails_Lua.c_walk_speed_max) / Tails_Lua.l_speedup_acc;
                */
                {
                    var lw = new LUAWrapper();
                    lw["Sonic_New_Lua", "c_run_acc"] = (lw["Sonic_New_Lua", "c_run_speed_max"] - lw["Sonic_New_Lua", "c_walk_speed_max"]) / lw["Sonic_New_Lua", "l_run_acc"];
                    lw["Sonic_New_Lua", "c_speedup_acc"] = (lw["Sonic_New_Lua", "c_speedup_speed_max"] - lw["Sonic_New_Lua", "c_walk_speed_max"]) / lw["Sonic_New_Lua", "l_speedup_acc"];
                    lw["Sonic_New_Lua", "c_bound_jump_spd_0"] = lw.Invoke("Common_Lua", "HeightToSpeed", new object[] { lw["Sonic_New_Lua", "l_bound_jump_height0"] });
                    lw["Sonic_New_Lua", "c_bound_jump_spd_1"] = lw.Invoke("Common_Lua", "HeightToSpeed", new object[] { lw["Sonic_New_Lua", "l_bound_jump_height1"] });
                    lw["Sonic_New_Lua", "c_homing_brake"] = (lw["Sonic_New_Lua", "c_homing_spd"] - lw["Sonic_New_Lua", "c_jump_run_orig"]) / lw["Sonic_New_Lua", "c_homing_time"];
                    lw["Sonic_Fast_Lua", "c_run_acc"] = (lw["Sonic_Fast_Lua", "c_run_speed_max"] - lw["Sonic_Fast_Lua", "c_walk_speed_max"]) / lw["Sonic_Fast_Lua", "l_run_acc"];
                    lw["Knuckles_Lua", "c_run_acc"] = (lw["Knuckles_Lua", "c_run_speed_max"] - lw["Knuckles_Lua", "c_walk_speed_max"]) / lw["Knuckles_Lua", "l_run_acc"];
                    lw["Knuckles_Lua", "c_speedup_acc"] = (lw["Knuckles_Lua", "c_speedup_speed_max"] - lw["Knuckles_Lua", "c_walk_speed_max"]) / lw["Knuckles_Lua", "l_speedup_acc"];
                    lw["Omega_Lua", "c_run_acc"] = (lw["Omega_Lua", "c_run_speed_max"] - lw["Omega_Lua", "c_walk_speed_max"]) / lw["Omega_Lua", "l_run_acc"];
                    lw["Omega_Lua", "c_jump_walk"] = lw["Omega_Lua", "l_jump_walk"] / (2f * Mathf.Sqrt(2f * lw["Omega_Lua", "l_jump_hight"] / 9.81f));
                    lw["Omega_Lua", "c_jump_run"] = lw["Omega_Lua", "l_jump_run"] / (2f * Mathf.Sqrt(2f * lw["Omega_Lua", "l_jump_hight"] / 9.81f));
                    lw["Omega_Lua", "c_speedup_acc"] = (lw["Omega_Lua", "c_speedup_speed_max"] - lw["Omega_Lua", "c_walk_speed_max"]) / lw["Omega_Lua", "l_speedup_acc"];
                    lw["Princess_Lua", "c_run_acc"] = (lw["Princess_Lua", "c_run_speed_max"] - lw["Princess_Lua", "c_walk_speed_max"]) / lw["Princess_Lua", "l_run_acc"];
                    lw["Princess_Lua", "c_jump_walk"] = lw.Invoke("Common_Lua", "HeightAndDistanceToSpeed", new object[] { lw["Princess_Lua", "l_jump_walk"], lw["Princess_Lua", "l_jump_hight"] });
                    lw["Princess_Lua", "c_speedup_acc"] = (lw["Princess_Lua", "c_speedup_speed_max"] - lw["Princess_Lua", "c_walk_speed_max"]) / lw["Princess_Lua", "l_speedup_acc"];
                    lw["Princess_Lua", "c_homing_brake"] = (lw["Princess_Lua", "c_homing_spd"] - lw["Princess_Lua", "c_jump_run_orig"]) / lw["Princess_Lua", "c_homing_time"];
                    lw["Rouge_Lua", "c_run_acc"] = (lw["Rouge_Lua", "c_run_speed_max"] - lw["Rouge_Lua", "c_walk_speed_max"]) / lw["Rouge_Lua", "l_run_acc"];
                    lw["Rouge_Lua", "c_speedup_acc"] = (lw["Rouge_Lua", "c_speedup_speed_max"] - lw["Rouge_Lua", "c_walk_speed_max"]) / lw["Rouge_Lua", "l_speedup_acc"];
                    lw["Shadow_Lua", "c_run_acc"] = (lw["Shadow_Lua", "c_run_speed_max"] - lw["Shadow_Lua", "c_walk_speed_max"]) / lw["Shadow_Lua", "l_run_acc"];
                    lw["Shadow_Lua", "c_speedup_acc"] = (lw["Shadow_Lua", "c_speedup_speed_max"] - lw["Shadow_Lua", "c_walk_speed_max"]) / lw["Shadow_Lua", "l_speedup_acc"];
                    lw["Shadow_Lua", "c_homing_brake"] = (lw["Shadow_Lua", "c_homing_spd"] - lw["Shadow_Lua", "c_jump_run_orig"]) / lw["Shadow_Lua", "c_homing_time"];
                    lw["Silver_Lua", "c_run_acc"] = (lw["Silver_Lua", "c_run_speed_max"] - lw["Silver_Lua", "c_walk_speed_max"]) / lw["Silver_Lua", "l_run_acc"];
                    lw["Silver_Lua", "c_speedup_acc"] = (lw["Silver_Lua", "c_speedup_speed_max"] - lw["Silver_Lua", "c_walk_speed_max"]) / lw["Silver_Lua", "l_speedup_acc"];
                    lw["Silver_Lua", "c_tele_dash_speed"] = lw["Silver_Lua", "l_tele_dash"] / lw["Silver_Lua", "c_tele_dash_time"];
                    lw["Silver_Lua", "c_psi_gauge_catch_ride"] = lw["Silver_Lua", "psi_power"] / lw["Silver_Lua", "l_psi_gauge_catch_ride"];
                    lw["Silver_Lua", "c_psi_gauge_float"] = lw["Silver_Lua", "psi_power"] / (lw["Silver_Lua", "l_psi_gauge_float"] / (lw["Silver_Lua", "c_float_walk_speed"] / 1.85f));
                    lw["Silver_Lua", "c_psi_gauge_teleport_dash_burn"] = lw["Silver_Lua", "psi_power"] / (lw["Silver_Lua", "l_psi_gauge_float"] / (lw["Silver_Lua", "c_float_walk_speed"] / 2f));
                    lw["Sonic_Fast_Lua", "c_run_acc"] = (lw["Sonic_Fast_Lua", "c_run_speed_max"] - lw["Sonic_Fast_Lua", "c_walk_speed_max"]) / lw["Sonic_Fast_Lua", "l_run_acc"];
                    lw["Tails_Lua", "c_run_acc"] = (lw["Tails_Lua", "c_run_speed_max"] - lw["Tails_Lua", "c_walk_speed_max"]) / lw["Tails_Lua", "l_run_acc"];
                    lw["Tails_Lua", "c_speedup_acc"] = (lw["Tails_Lua", "c_speedup_speed_max"] - lw["Tails_Lua", "c_walk_speed_max"]) / lw["Tails_Lua", "l_speedup_acc"];
                }

                // xdebug log that recalc succeeded
                XSingleton<XDebug>.Instance.Log("LUA recalculated", 1.5f, 18f);
            }
        }
    }
}
