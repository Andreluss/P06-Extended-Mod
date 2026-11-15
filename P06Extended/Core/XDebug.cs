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
    public partial class XDebug : XSingleton<XDebug>
    {
        private void Awake()
        {
            this.CreateXCanvas();
            SceneManager.sceneLoaded += this.OnSceneLoaded;
            UnityEngine.Object.DontDestroyOnLoad(this);
            Application.runInBackground = true;
            XDebug.Comment("only here initializing XValues won't crash");
            CustomSpeedMultiplier.InitializeOriginalLUA();
            this.InitializeXValues();
        }
        private void Start()
        {
            this.RealFixedDeltaTime = Time.fixedDeltaTime;
            if (XDebug.COMMENT)
            {
                new XDebug.DebugInt("effect idx", 0, 1, KeyCode.PageUp, KeyCode.PageDown, delegate (int x)
                {
                    this._T2_ = x;
                }, () => this._T2_, XDebug.DebugInt.LogT.OnChange);
            }

            new XDebug.DebugFloat("<color=#d214fc>cstm_spd_ground</color>", 1f, 0.05f, KeyCode.Keypad2, KeyCode.Keypad1, delegate (float x)
            {
                XSingleton<XDebug>.Instance.SM[CustomSpeedMultiplier.LUASpeedType.Ground].Value = x;
            }, () => XDebug.CustomSpeedMultiplier.ByType[CustomSpeedMultiplier.LUASpeedType.Ground], DebugFloat.LogT.OnChange);
            new XDebug.DebugFloat("<color=#d214fc>cstm_spd_air</color>", 1f, 0.05f, KeyCode.Keypad5, KeyCode.Keypad4, delegate (float x)
            {
                XSingleton<XDebug>.Instance.SM[CustomSpeedMultiplier.LUASpeedType.Air].Value = x;
            }, () => XDebug.CustomSpeedMultiplier.ByType[CustomSpeedMultiplier.LUASpeedType.Air], XDebug.DebugFloat.LogT.OnChange);
            new XDebug.DebugFloat("<color=#d214fc>cstm_spd_spindash</color>", 1f, 0.05f, KeyCode.Keypad8, KeyCode.Keypad7, delegate (float x)
            {
                XSingleton<XDebug>.Instance.SM[CustomSpeedMultiplier.LUASpeedType.Spindash].Value = x;
            }, () => XDebug.CustomSpeedMultiplier.ByType[CustomSpeedMultiplier.LUASpeedType.Spindash], XDebug.DebugFloat.LogT.OnChange);
            new XDebug.DebugFloat("<color=#d214fc>cstm_spd_flying</color>", 1f, 0.05f, KeyCode.Keypad9, KeyCode.Keypad6, delegate (float x)
            {
                XSingleton<XDebug>.Instance.SM[CustomSpeedMultiplier.LUASpeedType.Flying].Value = x;
            }, () => XDebug.CustomSpeedMultiplier.ByType[CustomSpeedMultiplier.LUASpeedType.Flying], XDebug.DebugFloat.LogT.OnChange);
            new XDebug.DebugFloat("<color=#d214fc>cstm_spd_climbing</color>", 1f, 0.05f, KeyCode.Keypad3, KeyCode.KeypadPeriod, delegate (float x)
            {
                XSingleton<XDebug>.Instance.SM[CustomSpeedMultiplier.LUASpeedType.Climb].Value = x;
            }, () => XDebug.CustomSpeedMultiplier.ByType[CustomSpeedMultiplier.LUASpeedType.Climb], XDebug.DebugFloat.LogT.OnChange);
            XSingleton<XFiles>.Instance.Load();
            Debug.Log(Singleton<XModMenu>.Instance.name);
            Dictionary<string, object> settingsDict = this.GetSettingsDict();
            if (settingsDict != null && settingsDict["Load automatically"] != null && (bool)settingsDict["Load automatically"])
            {
                this.LoadSettings();
            }
            else
            {
                XDebug.Comment("to prevent random output from initalized xvalues");
                this.Box_EndTime = Time.time;
            }
            Singleton<XModMenu>.Instance.Menu.OnClose += delegate
            {
                if (XSingleton<XDebug>.Instance.Saving_AutoSave.Value)
                {
                    XSingleton<XDebug>.Instance.SaveSettings();
                }
            };
        }

        private void FixedUpdate()
        {
            if (Extra_DisplaySpeedo.Value)
                RecalculateActualSpeed();
        }

        private void Update()
        {
            if (this.CanHandleInput())
            {
                this.HandleInput();
            }
            XDebug.Comment("================== Time slowdown ==================");
            if (Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetKeyDown(KeyCode.RightControl))
                {
                    this.RealFixedDeltaTime = Time.fixedDeltaTime;
                    Time.fixedDeltaTime /= 10f;
                }
                Time.timeScale = 0.1f;
            }
            else if (Input.GetKeyUp(KeyCode.RightControl))
            {
                Time.timeScale = 1f;
                Time.fixedDeltaTime = this.RealFixedDeltaTime;
            }
            XDebug.Comment("================== buggy speedup ==================");
            if (Input.GetKey(KeyCode.Keypad0) || Input.GetKey(KeyCode.Delete) || Input.GetKey(KeyCode.Insert) || Input.GetButton("Right Stick Button"))
            {
                Time.timeScale = 8f;
            }
            else if (Input.GetKeyUp(KeyCode.Keypad0) || Input.GetKeyUp(KeyCode.Delete) || Input.GetKeyUp(KeyCode.Insert) || Input.GetButtonUp("Right Stick Button"))
            {
                Time.timeScale = 1f;
            }
            if (Singleton<GameManager>.Instance.GameState == GameManager.State.Paused)
            {
                Time.timeScale = 0f;
            }
            XDebug.Comment("================== Log Box handling ==================");
            if (this.Box_GameObject != null && this.Box_GameObject.activeSelf && Time.time > this.Box_EndTime)
            {
                this.Box_GameObject.SetActive(false);
            }
            if (this.Extra_DisplaySpeedo.Value)
            {
                this.SpeedoLog();
            }
            else if (this.Speedo_GameObject != null && this.Speedo_GameObject.activeSelf)
            {
                this.Speedo_GameObject.SetActive(false);
            }
            if (this.BoxExtra != null && this.BoxExtra.GameObject.activeSelf && this.BoxExtra.HideTime < Time.time)
            {
                this.BoxExtra.GameObject.SetActive(false);
            }
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Tab))
            {
                XDebug.DBG = !XDebug.DBG;
                if (XDebug.DBG)
                {
                    if (XSingleton<XFiles>.Instance.Particle == null)
                    {
                        XSingleton<XFiles>.Instance.Load();
                    }
                    this.Log("==DEBUG MODE <color=#00ee00>ENABLED</color>==", 6f, 18f);
                    this.LogExtra("Try pressing random keys on your kb ;P", 6f, 18f);
                    return;
                }
                this.Log("==DEBUG MODE <color=#ee0000>DISABLED</color>==", 2.5f, 18f);
            }
        }

        public XDebug()
        {
            this.SpeedMultiplier = 1f;
            this.Characters = new Tuple<string, int>[]
            {
            new Tuple<string, int>("sonic_new", 0),
            new Tuple<string, int>("shadow", 5),
            new Tuple<string, int>("silver", 1),
            new Tuple<string, int>("tails", 2),
            new Tuple<string, int>("knuckles", 3),
            new Tuple<string, int>("rouge", 7),
            new Tuple<string, int>("omega", 6),
            new Tuple<string, int>("amy", 9),
            new Tuple<string, int>("blaze", 8),
            new Tuple<string, int>("princess", 4),
            new Tuple<string, int>("snow_board", 0),
            new Tuple<string, int>("sonic_fast", 0)
            };
            this._T2_ = 11;
            this._color_ = Color.white;
            this._ae_idxs_ = new int[]
            {
            18, 26, 41, 43, 49, 50, 59, 75, 77, 80,
            107, 112, 119, 122, 125, 129
            };
            this.SpeedMultiplier = 1f;
            this.Box_EndTime = 9999f;
            this.SRs = new List<Renderer>();
            XDebug.Comment("OLD COLORS: this.LA_Color = new Color(0f, 5f, 40f, 0.4f);this.SC_Color = new Color(0f, 23f, 255f, 0.3f);");
            this.LRs = new List<LineRenderer>();
        }

        public UI HUD
        {
            get
            {
                if (this._HUD == null)
                {
                    this._HUD = UnityEngine.Object.FindObjectOfType<UI>();
                }
                return this._HUD;
            }
        }

        public PlayerBase Player
        {
            get
            {
                if (this._player == null)
                {
                    this._player = UnityEngine.Object.FindObjectOfType<PlayerBase>();
                }
                return this._player;
            }
        }

        private UnityEngine.Object RandomOf(UnityEngine.Object[] array)
        {
            if (array == null || array.Length == 0)
            {
                return null;
            }
            int num = UnityEngine.Random.Range(0, array.Length - 1);
            return array[num];
        }

        public void DrawVectorFast(Vector3 start, Vector3 end, Color color, int idx)
        {
            if (!XDebug.DBG)
            {
                return;
            }
            if (idx < 0)
            {
                return;
            }
            if (this.LRs.Count <= idx)
            {
                for (int i = this.LRs.Count; i <= idx; i++)
                {
                    LineRenderer lineRenderer = new GameObject(string.Format("LR wrapper {0}", idx)).AddComponent<LineRenderer>();
                    UnityEngine.Object.DontDestroyOnLoad(lineRenderer);
                    lineRenderer.material = new Material(Shader.Find("Standard"));
                    lineRenderer.widthMultiplier = 0.05f;
                    lineRenderer.enabled = false;
                    this.LRs.Add(lineRenderer);
                }
            }
            if (!this.LRs[idx].enabled)
            {
                this.LRs[idx].material.EnableKeyword("_EMISSION");
                this.LRs[idx].material.color = color;
                this.LRs[idx].material.SetVector("_EmissionColor", color * 2f);
                this.LRs[idx].enabled = true;
            }
            this.LRs[idx].SetPositions(new Vector3[] { start, end });
        }

        public void MessageLog(string message, string voiceName = null, float time = 4f)
        {
            if (this.HUD && this.Player)
            {
                this.HUD.StartMessageBox(new string[] { message }, new string[] { voiceName }, time);
            }
        }

        public SonicNew SonicNew
        {
            get
            {
                if (this._sonicNew == null)
                {
                    this._sonicNew = UnityEngine.Object.FindObjectOfType<SonicNew>();
                }
                return this._sonicNew;
            }
        }

        public static void Comment(string text)
        {
        }

        public void DrawSphereFast(Vector3 position, float radius, Color color, int idx)
        {
            if (!XDebug.DBG)
            {
                return;
            }
            if (idx < 0)
            {
                return;
            }
            if (this.SRs.Count <= idx)
            {
                for (int i = this.SRs.Count; i <= idx; i++)
                {
                    GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    UnityEngine.Object.Destroy(gameObject.GetComponent<Collider>());
                    Renderer component = gameObject.GetComponent<Renderer>();
                    if (component.material == null)
                    {
                        component.material = new Material(Shader.Find("Standard"));
                    }
                    component.enabled = false;
                    this.SRs.Add(component);
                }
            }
            if (!this.SRs[idx].enabled)
            {
                this.SRs[idx].material.EnableKeyword("_EMISSION");
                this.SRs[idx].material.color = color;
                if (color.a >= 0.7f)
                {
                    this.SRs[idx].material.SetVector("_EmissionColor", color * 2f);
                    this.SRs[idx].enabled = true;
                }
            }
            this.SRs[idx].transform.position = position;
            this.SRs[idx].transform.localScale = Vector3.one * radius;
        }

        public void Dump(object obj)
        {
        }

        public SonicFast SonicFast
        {
            get
            {
                if (this._sonicFast == null)
                {
                    this._sonicFast = UnityEngine.Object.FindObjectOfType<SonicFast>();
                }
                return this._sonicFast;
            }
        }

        private AudioClip MakeSubclip(AudioClip clip, float start, float stop)
        {
            int frequency = clip.frequency;
            float num = stop - start;
            int num2 = (int)((float)frequency * num);
            AudioClip audioClip = AudioClip.Create(clip.name + "-sub", num2, 2, frequency, false);
            float[] array = new float[num2];
            clip.GetData(array, (int)((float)frequency * start));
            audioClip.SetData(array, 0);
            return audioClip;
        }

        public AudioClip DodgeClip
        {
            get
            {
                if (this._DodgeClip == null)
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("Defaultprefabs/Objects/kdv/Rope") as GameObject);
                    AudioClip clipLow = gameObject.GetComponent<Rope>().ClipLow;
                    gameObject.SetActive(false);
                    this._DodgeClip = this.MakeSubclip(clipLow, 0f, 0.34f);
                }
                return this._DodgeClip;
            }
        }

        public AudioClip DodgeClipFull
        {
            get
            {
                if (this._DodgeClipFull == null)
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("Defaultprefabs/Objects/kdv/Rope") as GameObject);
                    AudioClip clipLow = gameObject.GetComponent<Rope>().ClipLow;
                    gameObject.SetActive(false);
                    this._DodgeClipFull = clipLow;
                }
                return this._DodgeClipFull;
            }
        }


        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (XDebug.DBG)
            {
                Singleton<GameData>.Instance.Sonic.Lives = 993;
            }
            Time.timeScale = 1f;
            XDebug.Comment("Recalculate some scene-dependent variables...");
            this._sonicNew = null;
            this._sonicFast = null;
            this._HUD = null;
            this._player = null;
            if (this.ULTRA_FPS)
            {
                Time.fixedDeltaTime = 1f / (float)Screen.currentResolution.refreshRate;
                Application.targetFrameRate = Screen.currentResolution.refreshRate;
            }
            else
            {
                Time.fixedDeltaTime = 0.016666668f;
                Application.targetFrameRate = 60;
            }
            XDebug.Comment("check mod files integrity");
            // no longer need to check the p06 version, since we have a plugin mod
            if (scene.name == "MainMenu")
            {
                XSingleton<XFiles>.Instance.Load();
            }
            if (scene.name == "MainMenu")
                XSingleton<XDebug>.Instance.Log("P-06<color=#00ee00>X</color>" + XDebug.P06X_VERSION + " Plugin by 4ndrelus", 20f, 13.8f);
            XDebug.Comment("custom music check and update");
            if (this.PlayCustomMusic.Value)
            {
                this.StartCustomMusic();
            }
        }

        public void LB_SetAchors(Vector2 anchor)
        {
            this.Box_GameObject.GetComponent<RectTransform>().anchorMin = anchor;
            this.Box_GameObject.GetComponent<RectTransform>().anchorMax = anchor;
        }

        public void LB_Move(Vector3 vector)
        {
            Vector3 vector2 = (this.Box_Container.GetComponent<RectTransform>().localPosition += vector);
            this.MessageLog(string.Format("pos: {0}", vector2), null, 1.5f);
            this.Box_EndTime = Time.time + 1.5f;
        }

        public void Log(string message, float duration = 1.5f, float fontSize = 18f)
        {
            if (this.Box_GameObject == null)
            {
                this.Box_GameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("Defaultprefabs/UI/MessageBox_E3") as GameObject, Vector3.zero, Quaternion.identity);
                this.Box_GameObject.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1f);
                this.Box_GameObject.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);
                this.Box_Script = this.Box_GameObject.GetComponent<MessageBox>();
                this.Box_Script.Set("Duration", 9999999f);
                XDebug.Comment("this.Box_Script.transform.SetParent(UnityEngine.Object.FindObjectOfType<Canvas>().transform, false);");
                XDebug.Comment("this.Box_Script.transform.SetParent(Singleton<XModMenu>.Instance.Canvas.transform, false);");
                this.Box_Script.transform.SetParent(this.XCanvas.transform, false);
                Vector2 vector = new Vector2(350f, 45f);
                Vector2 vector2 = new Vector2(325f, 30f);
                this.Box_Text = this.Box_GameObject.GetComponentInChildren<TextMeshProUGUI>();
                this.Box_Text.overflowMode = TextOverflowModes.Linked;
                this.Box_Text.alignment = TextAlignmentOptions.CenterGeoAligned;
                this.Box_Text.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
                this.Box_Text.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
                this.Box_Text.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                this.Box_Text.GetComponent<RectTransform>().localPosition = Vector3.zero;
                this.Box_Text.GetComponent<RectTransform>().sizeDelta = vector2;
                this.Box_Container = this.Box_GameObject.FindInChildren("Box");
                this.Box_Container.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
                this.Box_Container.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
                this.Box_Container.GetComponent<RectTransform>().sizeDelta = vector;
                if (SceneManager.GetActiveScene().name != "MainMenu")
                {
                    this.Box_Container.GetComponent<RectTransform>().localPosition = new Vector3(0f, -22.5f, 0f);
                }
                else
                {
                    this.Box_GameObject.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 0.5f);
                    this.Box_GameObject.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.5f);
                    this.Box_Container.GetComponent<RectTransform>().localPosition = new Vector3(-175f, -160f, 0f);
                }
            }
            this.Box_GameObject.SetActive(true);
            this.Box_Text.text = message;
            this.Box_Text.fontSize = fontSize;
            this.Box_EndTime = Time.time + duration;
        }

        public void XSwitchTo(string Who, int ID, PlayerBase CurrentPlayer)
        {
            Vector3 velocity = CurrentPlayer._Rigidbody.velocity;
            float curSpeed = CurrentPlayer.Get<float>("CurSpeed");
            PlayerBase component = (UnityEngine.Object.Instantiate(Resources.Load("DefaultPrefabs/Player/" + Who), CurrentPlayer.transform.position, CurrentPlayer.transform.rotation) as GameObject).GetComponent<PlayerBase>();
            component.SetPlayer(ID, Who);
            component._Rigidbody.velocity = velocity;
            component.Set("CurSpeed", curSpeed);
            component.StartPlayer(false);
            component.Get<UI>("HUD").UseCrosshair(true, false);
            if (CurrentPlayer.Get<bool>("HasShield"))
            {
                component.Set("HasShield", true);
                component.Set("ShieldObject", CurrentPlayer.Get<GameObject>("ShieldObject"));
                CurrentPlayer.Get<GameObject>("ShieldObject").transform.position = component.transform.position + component.transform.up * ((!component.GetPrefab("omega")) ? 0.25f : 0.5f);
                CurrentPlayer.Get<GameObject>("ShieldObject").transform.rotation = Quaternion.identity;
                CurrentPlayer.Get<GameObject>("ShieldObject").transform.SetParent(component.transform);
                CurrentPlayer.Get<GameObject>("ShieldObject").transform.localScale = Vector3.one * ((!component.GetPrefab("omega")) ? 1f : 1.5f);
            }
            if (XDebug.DBG && this._T2_ != 11)
            {
                if (this._ae_ == null)
                {
                    this._ae_ = Resources.LoadAll<GameObject>("defaultprefabs/effect/");
                }
                GameObject fx2 = UnityEngine.Object.Instantiate<GameObject>(this._ae_[this._ae_idxs_[this._T2_]], XDebug.Finder<PlayerBase>.Instance.transform.position + this._fx_offset_, Quaternion.identity, component.gameObject.transform);
                this.Log(string.Format("idx: {0} ({1})", this._T2_, this._ae_[this._ae_idxs_[this._T2_]].name), 2f, 14f);
                this.Invoke(delegate
                {
                    UnityEngine.Object.Destroy(fx2);
                }, 2f);
            }
            else
            {
                GameObject fx = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("defaultprefabs/effect/player/shadow/SnapDashFX"), component.gameObject.transform.position + component.gameObject.transform.up * 0.15f, component.gameObject.transform.rotation, component.gameObject.transform);
                ParticleSystem[] componentsInChildren = fx.GetComponentsInChildren<ParticleSystem>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    ParticleSystem.MainModule main = componentsInChildren[i].main;
                    main.startColor = XDebug.ColorConstants.SmoothSwitchOverride;
                    main.startColor = XDebug.ColorConstants.SmoothSwitchOverride;
                }
                this.Invoke(delegate
                {
                    UnityEngine.Object.Destroy(fx);
                }, 2f);
            }
            UnityEngine.Object.Destroy(CurrentPlayer.gameObject);

            // Update target references in amigos (if any)
            var amigos = FindObjectsOfType<AmigoAIBase>();
            foreach (var amigo in amigos)
            {
                amigo.Set<PlayerBase>("FollowTarget", component);
            }

            var enemies = FindObjectsOfType<EnemyBase>();
            foreach (var enemy in enemies)
            {
                enemy.Set<GameObject>("Target", component.gameObject);
            }
        }



        private void LateUpdate()
        {
            if (XDebug.Cfg.Cheats.InfiniteGauge && XDebug.Finder<UI>.Instance)
            {
                XDebug.Finder<UI>.Instance.Set("ActionDisplay",
                    XDebug.Finder<UI>.Instance.Get<float>("MaxActionGauge"));
                XDebug.Finder<UI>.Instance.Set("ChaosMaturityDisplay",
                    XDebug.Finder<UI>.Instance.Get<float>("MaxActionGauge"));
            }
            if (XDebug.Cfg.Cheats.Immune && XDebug.Finder<PlayerBase>.Instance)
            {
                XDebug.Finder<PlayerBase>.Instance.Set("ImmunityTime", 1E+10f);
            }
            if (XDebug.Cfg.Cheats.InfiniteRings)
            {
                XDebug.Cheats.Rings = 999999999;
            }
        }

        public bool UltraFPS
        {
            get
            {
                return this.ULTRA_FPS;
            }
            set
            {
                this.ULTRA_FPS = value;
                if (this.ULTRA_FPS)
                {
                    Time.fixedDeltaTime = 1f / (float)Screen.currentResolution.refreshRate;
                    Application.targetFrameRate = Screen.currentResolution.refreshRate;
                    this.Log(string.Format("Ultra Smooth FPS <color=#00ee00>enabled</color>\ndelta is now {0}", Time.fixedDeltaTime), 4f, 18f);
                    this.RealFixedDeltaTime = Time.fixedDeltaTime;
                    return;
                }
                Time.fixedDeltaTime = 0.016666668f;
                Application.targetFrameRate = 60;
                this.Log(string.Format("Ultra Smooth FPS <color=#ee0000>disabled</color>\ndelta is now {0}", Time.fixedDeltaTime), 4f, 18f);
                this.RealFixedDeltaTime = Time.fixedDeltaTime;
            }
        }

        private void CreateXCanvas()
        {
            GameObject gameObject = new GameObject("xdebug log and mod menu canvas");
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            XCanvas = gameObject.AddComponent<Canvas>();
            CanvasScaler canvasScaler = gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<GraphicRaycaster>();
            this.XCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1280f, 720f);
            this.XCanvas.transform.position += new Vector3(0f, 0f, -99999f);
            this.XCanvas.sortingOrder = 11;
        }

        private void StartCustomMusic()
        {
            AudioSource musicSource = this.GetMusicSource();
            if (musicSource != null && XSingleton<XFiles>.Instance.Gandalf != null)
            {
                musicSource.Stop();
                this.backupClip = musicSource.clip;
                musicSource.clip = XSingleton<XFiles>.Instance.Gandalf;
                musicSource.Play();
                return;
            }
        }

        private AudioSource GetMusicSource()
        {
            AudioSource audioSource = null;
            if (XDebug.Finder<StageManager>.Instance)
            {
                audioSource = XDebug.Finder<StageManager>.Instance.Get<AudioSource>("BGMPlayer");
            }
            else if (XDebug.Finder<MainMenu>.Instance)
            {
                audioSource = XDebug.Finder<MainMenu>.Instance.Music;
            }
            else if (XDebug.Finder<TitleScreen>.Instance)
            {
                audioSource = XDebug.Finder<TitleScreen>.Instance.MusicAudio;
            }
            return audioSource;
        }

        private void StartNormalMusic()
        {
            AudioSource musicSource = this.GetMusicSource();
            if (musicSource != null)
            {
                musicSource.Stop();
                if (musicSource.clip == XSingleton<XFiles>.Instance.Gandalf)
                {
                    musicSource.clip = this.backupClip;
                }
                musicSource.Play();
            }
        }
    }
}
