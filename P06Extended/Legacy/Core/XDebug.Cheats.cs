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
        public struct Cheats
        {
            public static int Rings
            {
                get
                {
                    return Singleton<GameManager>.Instance._PlayerData.rings;
                }
                set
                {
                    Singleton<GameManager>.Instance._PlayerData.rings = value;
                }
            }

            public static int Lives
            {
                get
                {
                    return Singleton<GameManager>.Instance.GetLifeCount();
                }
                set
                {
                    GameData.StoryData storyData = Singleton<GameManager>.Instance.GetStoryData();
                    storyData.Lives = value;
                    Singleton<GameManager>.Instance.SetStoryData(storyData);
                }
            }

            public static int ActiveGemId
            {
                get
                {
                    SonicNew instance = XDebug.Finder<SonicNew>.Instance;
                    if (instance == null)
                    {
                        return -1;
                    }
                    return instance.Get<int>(" GemSelector");
                }
            }

            public static int CurrentGemLevel
            {
                get
                {
                    if (XDebug.Finder<SonicNew>.Instance && XDebug.Cheats.ActiveGemId >= 0)
                    {
                        return XDebug.Finder<UI>.Instance.Get<int[]>("ActiveGemLevel")[XDebug.Cheats.ActiveGemId];
                    }
                    return -1;
                }
                set
                {
                    if (XDebug.Finder<SonicNew>.Instance != null && XDebug.Cheats.ActiveGemId >= 0)
                    {
                        XDebug.Finder<UI>.Instance.Get<int[]>("ActiveGemLevel")[XDebug.Cheats.ActiveGemId] = Mathf.Clamp(value, 0, 2);
                        return;
                    }
                }
            }

            public static void GetAllGems()
            {
                SonicNew instance = XDebug.Finder<SonicNew>.Instance;
                if (instance == null)
                {
                    return;
                }
                GameData.GlobalData gameData = Singleton<GameManager>.Instance.GetGameData();
                gameData.ObtainedGems.Clear();
                for (int i = 0; i <= 8; i++)
                {
                    gameData.ObtainedGems.Add(i);
                }
                Singleton<GameManager>.Instance.SetGameData(gameData);
                instance.Set("GemData", gameData);
                instance.Set("ObtainedGemIndex", 8);
                instance.Set("GemSelector", 8);
                instance.Set("ActiveGem", SonicNew.Gem.Rainbow);
                instance.Get<UI>("HUD").UpdateGemPanel(gameData);
                // TODO: check this
                for (int j = 0; j < gameData.ObtainedGems.Count - 1; j++)
                {
                    instance.Get<UI>("HUD").GemSlots[j].GetComponent<Image>().sprite = instance.Get<UI>("HUD").GemImages[j + 1];
                }
                XSingleton<XDebug>.Instance.Log("All gems <color=#ff9a00>are yours ;)</color>", 1.5f, 18f);
            }

            public static void MaxOutAllGems()
            {
                XDebug.Cfg.Cheats.MaxedOutGems = true;
                int[] activeGemLevel = XDebug.Finder<UI>.Instance.Get<int[]>("ActiveGemLevel");
                if (activeGemLevel == null)
                {
                    return;
                }
                for (int i = 0; i < 9; i++)
                {
                    activeGemLevel[i] = 2;
                }
                XSingleton<XDebug>.Instance.Log("All gems <color=#de921f>maxed out</color>", 1.5f, 18f);
            }

            public static void GetLightMemoryShard()
            {
                global::Shadow instance = XDebug.Finder<global::Shadow>.Instance;
                if (instance == null)
                {
                    XSingleton<XDebug>.Instance.Log("You're not Shadow!", 1.5f, 18f);
                    return;
                }
                instance.Set("HasLightMemoryShard", true);
                XSingleton<XDebug>.Instance.Log("Light Memory Shard <color=#ff9a00>collected</color>", 1.5f, 18f);
            }

            public static void GetSigil()
            {
                Silver instance = XDebug.Finder<Silver>.Instance;
                if (instance == null)
                {
                    XSingleton<XDebug>.Instance.Log("You're not Silver!", 1.5f, 18f);
                    return;
                }
                instance.Set("HasSigilOfAwakening", true);
                XSingleton<XDebug>.Instance.Log("Sigil of Awakening <color=#ff9a00>collected</color>", 1.5f, 18f);
            }

            public static void GetFlame()
            {
                Silver instance = XDebug.Finder<Silver>.Instance;
                if (instance == null)
                {
                    XSingleton<XDebug>.Instance.Log("You're not Silver!", 1.5f, 18f);
                    return;
                }
                instance.Set("HasFlameOfControl", true);
                XSingleton<XDebug>.Instance.Log("Flame of Control <color=#ff9a00>collected</color>", 1.5f, 18f);
            }

            public static void GetLotus()
            {
                Silver instance = XDebug.Finder<Silver>.Instance;
                if (instance == null)
                {
                    XSingleton<XDebug>.Instance.Log("You're not Silver!", 1.5f, 18f);
                    return;
                }
                instance.Set("HasLotusOfResilience", true);
                instance.Animator.SetBool("Has Lotus", instance.Get<bool>("HasLotusOfResilience"));
                XSingleton<XDebug>.Instance.Log("Lotus of Resilience <color=#ff9a00>collected</color>", 1.5f, 18f);
            }
        }
    }
}
