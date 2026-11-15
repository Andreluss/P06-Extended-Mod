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
    public class XFiles : XSingleton<XFiles>
    {
        private void Awake()
        {
            UnityEngine.Object.DontDestroyOnLoad(this);
        }

        public void Load()
        {
            base.StartCoroutine(this.LoadAll_C());
        }

        private void Start()
        {
        }

        public bool Check()
        {
            if (!Directory.Exists(XCommon.ModFilesPath))
            {
                Directory.CreateDirectory(XCommon.ModFilesPath);
            }
            string text = XCommon.ModFilesPath + "xconfig.ini";
            if (!File.Exists(text))
            {
                File.WriteAllLines(text, new string[]
                {
                "0",
                "P06X" + XDebug.P06X_VERSION
                });
            }
            long num = 0L;
            try
            {
                string[] files = Directory.GetFiles(Application.dataPath, "*.resource");
                for (int i = 0; i < files.Length; i++)
                {
                    num += new FileInfo(files[i]).Length;
                }
            }
            catch
            {
                XSingleton<XDebug>.Instance.Log("There's a problem with checking your P-06 version - couldn't access the file!", 100f, 14f);
                return false;
            }
            if (num != XFiles.VersionHash && XSingleton<XDebug>.Instance.Other_CheckP06Version.Value)
            {
                XSingleton<XDebug>.Instance.Log("P-06 eXtended " + XDebug.P06X_VERSION + " <color=#ee0000>version error</color>(most likely)\n This mod release isn't comp. with your P-06 ver.", 100f, 12.5f);
                XDebug.Comment("\n the game <color=#ee0000>may crash</color>");
                File.WriteAllLines(text, new string[]
                {
                "0",
                string.Concat(new object[]
                {
                    "P06X",
                    XDebug.P06X_VERSION,
                    " version error at ",
                    DateTime.Now
                })
                });
                return false;
            }
            XDebug.Comment("Always CheckModFiles()");
            string text2 = null;
            try
            {
                text2 = File.ReadAllLines(text)[0];
            }
            catch (Exception ex)
            {
                if (text2 == null)
                {
                    XSingleton<XDebug>.Instance.Log("P-06 eXtended " + XDebug.P06X_VERSION + " <color=#ee0000>critical file error</color>\nexception" + ex.Message, 100f, 14f);
                    return false;
                }
            }
            string text3 = null;
            foreach (string text4 in Required)
            {
                if (!File.Exists(XCommon.ModFilesPath + text4))
                {
                    text3 = text4;
                    break;
                }
            }
            XDebug.Comment("1 -> 1");
            if (text2 == "1" && text3 == null)
            {
                XSingleton<XDebug>.Instance.Log("P-06<color=#00ee00>X</color>" + XDebug.P06X_VERSION + " Plugin by 4ndrelus", 3f, 13.8f);
            }
            XDebug.Comment("0 -> 1");
            if (text2 == "0" && text3 == null)
            {
                File.WriteAllLines(text, new string[]
                {
                "1",
                string.Concat(new object[]
                {
                    "P06X",
                    XDebug.P06X_VERSION,
                    " installed ",
                    DateTime.Now
                })
                });
                XSingleton<XDebug>.Instance.Log("P-06 eXtended " + XDebug.P06X_VERSION + " <color=#00ee00>installed correctly</color>", 60f, 14f);
                XSingleton<XDebug>.Instance.LogExtra("Thanks for using the mod!\n\n1) Press F12 to open the Mod Menu\n\n2) Have fun :D", 60f, 14f);
            }
            XDebug.Comment("0/1 -> 0");
            if (text3 != null)
            {
                File.WriteAllLines(text, new string[]
                {
                "0",
                string.Concat(new object[]
                {
                    "P06X",
                    XDebug.P06X_VERSION,
                    " error at ",
                    DateTime.Now
                })
                });
                XSingleton<XDebug>.Instance.Log(string.Concat(new string[]
                {
                "P-06 eXtended ",
                XDebug.P06X_VERSION,
                " <color=#ee0000>files error</color>\n",
                text3,
                " missing in the mods folder!"
                }), 100f, 14f);
                return false;
            }
            try
            {
                this.ConfigFile = File.ReadAllLines(text);
            }
            catch (Exception ex2)
            {
                XSingleton<XDebug>.Instance.Log("P-06 eXtended " + XDebug.P06X_VERSION + " <color=#ee0000>weird file error</color>\nexception" + ex2.Message, 100f, 14f);
                return false;
            }
            return true;
        }

        public static Texture2D LoadPNG(string filePath)
        {
            Texture2D texture2D = null;
            if (File.Exists(filePath))
            {
                byte[] array = File.ReadAllBytes(filePath);
                texture2D = new Texture2D(2, 2);
                texture2D.LoadImage(array);
            }
            return texture2D;
        }

        private IEnumerator LoadOGG2(Action<AudioClip> action, string filename)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(XCommon.ModFilesPath + filename, AudioType.OGGVORBIS))
            {
                yield return www.SendWebRequest();
                action(DownloadHandlerAudioClip.GetContent(www));
            }
        }

        private IEnumerator LoadAll_C()
        {
            this.Particle = XFiles.LoadPNG(XCommon.ModFilesPath + "Particle.png");
            yield return base.StartCoroutine(this.LoadOGG2(delegate (AudioClip clip)
            {
                this.Gandalf = clip;
            }, "custom_music.ogg"));
            yield return base.StartCoroutine(this.LoadOGG2(delegate (AudioClip clip)
            {
                this.StompLand = clip;
            }, "stomp_land.ogg"));
            yield return base.StartCoroutine(this.LoadOGG2(delegate (AudioClip clip)
            {
                this.RingSound = clip;
            }, "ring.ogg"));
            yield return base.StartCoroutine(this.LoadOGG2(delegate (AudioClip clip)
            {
                this.WallLand = clip;
            }, "land_concrete.ogg"));
            if (XDebug.COMMENT)
            {
                XSingleton<XDebug>.Instance.Log("All P06<color=#00ee00>X</color>" + XDebug.P06X_VERSION + " files <color=#00ee00>loaded correctly</color>", 1f, 14f);
            }
            yield break;
        }

        public IEnumerator LoadMp3(Action<AudioClip> action, string filename)
        {
            XDebug.Comment("THIS IS BROKEN currently");
            string text = string.Format("file://{0}", XCommon.ModFilesPath + filename);
            WWW www = new WWW(text);
            yield return www;
            AudioClip audioClip = www.GetAudioClip(false, false, AudioType.MPEG);
            action(audioClip);
            yield break;
        }

        private static string CalculateMD5(string filename)
        {
            string text;
            using (MD5 md = MD5.Create())
            {
                using (FileStream fileStream = File.OpenRead(filename))
                {
                    text = BitConverter.ToString(md.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();
                }
            }
            return text;
        }

        public static string[] Required = new string[] { "Particle.png", "custom_music.ogg", "stomp_land.ogg", "ring.ogg" };

        private static XFiles _instance;

        public AudioClip Gandalf;

        public Texture2D Particle;

        public AudioClip StompLand;

        public static long VersionCode;

        public string[] ConfigFile;

        public AudioClip RingSound;

        public AudioClip WallLand;

        public static long VersionHash = 697226251L;
    }
}
