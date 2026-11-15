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
    public class XDebug : XSingleton<XDebug>
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
            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                Debug.Log("default obj");
            }
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
            if (scene.name == "MainMenu" /*&& XSingleton<XFiles>.Instance.Check()*/) 
            {
                XSingleton<XFiles>.Instance.Load();
            }
            if(scene.name == "MainMenu")
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
                            //Array.Fill<int>(XDebug.Finder<UI>.Instance.Get<int[]>("ActiveGemLevel"), 0);
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

            var v_distance = player.transform.position - player_prev_position.Peek();
            // optionally (dot product): 
            //v_distance = Vector3.Project(v_distance, player.transform.forward);

            float d = v_distance.magnitude;
            float t = Time.fixedDeltaTime * Mathf.Max(1, player_prev_position.Count - 1);
            ActualSpeed = d / t;

            // remove the n+1-th last position
            if (player_prev_position.Count > n_positions) player_prev_position.Dequeue();
        }

        public float X_GetActualPlayerSpeedForward()
        {
            return Mathf.Floor(ActualSpeed * 100) / 100;
            //return Vector3.Dot(player._Rigidbody.velocity, player.transform.forward.normalized);
            //return player._Rigidbody.velocity.magnitude;
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
        public class XPlayerBase
        {
            public float X_BStopTime = 0;
            public float X_BReboostTime = 0.5f;
        };
        XPlayerBase xPlayerBase = new XPlayerBase();
        //[todo] [investigate] should be created on playerbase awake postfix or smth

        public bool X_CanPlayerStartBoosting()
        {
            bool trig = GetRInput().GetButton("Left Trigger");
            return trig && Time.time - xPlayerBase.X_BStopTime <= xPlayerBase.X_BReboostTime;
        }

        private UI _HUD;

        private PlayerBase _player;

        private static XDebug _instance;

        private List<LineRenderer> LRs;

        public static bool DBG = true;

        public static bool EXP = true;

        private SonicNew _sonicNew;

        public static bool OFF = true;

        private GameObject Box_GameObject;

        private MessageBox Box_Script;

        private TextMeshProUGUI Box_Text;

        private GameObject Box_Container;

        private List<Renderer> SRs;

        private SonicFast _sonicFast;

        public static int SNF_DDG_MODE;

        public static bool COMMENT = false;

        private AudioClip _DodgeClip;

        private AudioClip _DodgeClipFull;

        private float Box_EndTime;

        private float LB_NextMoveTime;

        private bool ULTRA_FPS;

        public static bool SWITCH = true;

        public static bool FASTER_STOMPDASH = true;

        private float RealFixedDeltaTime;

        public static readonly string P06X_VERSION = XCommon.Version;

        private float SpeedMultiplier;

        private int _T2_;

        private GameObject[] _ae_;

        private int[] _ae_idxs_;

        private Vector3 _fx_offset_ = Vector3.zero;

        private Color _color_;

        private Tuple<string, int>[] Characters;

        public int CurrentCharIdx;

        private float PrevDPadY;

        public bool JustUsedLeftTrigger;

        public static bool IMMEDIATE_SPINDASH_CLAMP = true;

        public static bool USING_CUSTOM_SPEEDS = false;

        private float _ringsPrevClick;

        public XValue<bool> UltraSmoothFPS;

        public Canvas XCanvas;

        public XValue<bool> Invincible;

        public XValue<bool> InfiniteGauge;

        public XValue<bool> PlayCustomMusic;

        public XValue<float> EverySpeedMultiplier;

        public XValue<bool> MaxedOutGems;

        public XValue<bool> InfiniteRings;

        public XValue<bool> InfiniteLives;

        private int[] MaxedOutGems_Data;

        public XValue<string> TeleportLocation;

        public ArrayByEnum<CustomSpeedMultiplier.LUASpeedType, XValue<float>> SM = new ArrayByEnum<CustomSpeedMultiplier.LUASpeedType, XValue<float>>();
        //public XValue<float> SMGround;

        //public XValue<float> SMAir;

        //public XValue<float> SMSpindash;

        //public XValue<float> SMFly;

        //public XValue<float> SMClimb;

        //public XValue<float> SMHoming;

        public XValue<bool> ASCSpinClamp;

        public XValue<bool> ASCLuaRecalc;

        public XValue<bool> Saving_AutoLoad;

        private AudioClip backupClip;

        public XValue<float> tmp0;

        public XValue<float> tmp1;

        public XValue<float> tmp2;

        public XValue<bool>[] dbg_toggles = new XValue<bool>[5];

        public XValue<float>[] dbg_floats = new XValue<float>[5];

        public XValue<bool> Other_OgCameraControls;

        public XValue<bool> Cheat_IgnoreWaterDeath;

        public XValue<bool> Moveset_FreeWaterSliding;

        private GameObject Speedo_GameObject;

        private TextMeshProUGUI Speedo_Text;

        private RectTransform Speedo_Text_Rect;

        private RectTransform Speedo_Container_Rect;

        public XValue<bool> Extra_DisplaySpeedo;

        private RectTransform Speedo_GameObject_Rect;

        public XValue<bool> Saving_AutoSave;

        public XValue<float> SMHomingAttackFasterBy;

        public XValue<float> SMAfterHomingRotation;

        public XValue<bool> Moveset_WallJumping;

        public XValue<bool> Moveset_Boost;

        public XValue<bool> Cheat_ChainJumpZeroDelay;

        public XValue<bool> Other_UltraFPSFix;

        private float PrevDPadX;

        public XValue<float> Boost_BaseSpeed;

        public XValue<float> Boost_NextLevelDeltaSpeed;

        public XValue<float> Boost_NextLevelThreshold;

        public XValue<float> Boost_RotSpeed;

        public XValue<float> Boost_AccelTime;

        public XValue<bool> Other_CheckP06Version;

        private XLogBox BoxExtra;

        public XValue<bool> Moveset_AHMovement;

        public XValue<float> Moveset_AHMovementMaxSpeed;

        public XValue<bool> Moveset_ClimbAll;

        public class Finder<T> where T : UnityEngine.Object
        {
            public static T Instance
            {
                get
                {
                    if (XDebug.Finder<T>._instance != null)
                    {
                        return XDebug.Finder<T>._instance;
                    }
                    XDebug.Finder<T>._instance = UnityEngine.Object.FindObjectOfType<T>();
                    return XDebug.Finder<T>._instance;
                }
            }

            private static T _instance;
        }

        private class DebugFloat : MonoBehaviour
        {
            private void Update()
            {
                this.Value = this.BindToVariable();
                bool flag = false;
                if (Input.GetKey(this.Plus))
                {
                    if (Input.GetKeyDown(this.Plus) || Time.time - this.prevTime > 0.2f)
                    {
                        flag = true;
                        this.prevTime = Time.time;
                        this.Value += this.Offset;
                    }
                }
                else if (Input.GetKey(this.Minus) && (Input.GetKeyDown(this.Minus) || Time.time - this.prevTime > 0.2f))
                {
                    flag = true;
                    this.prevTime = Time.time;
                    this.Value -= this.Offset / 2f;
                }
                if (flag)
                {
                    this.UpdateActualValue(this.Value);
                }
                if (this.LogType == XDebug.DebugFloat.LogT.Constant || (this.LogType == XDebug.DebugFloat.LogT.OnChange && flag))
                {
                    XSingleton<XDebug>.Instance.Log(string.Format("{0} = {1}", this.Name, this.Value.ToString("0.0000")), 1.25f, 16f);
                }
            }

            public DebugFloat(string name, float init, float offset, KeyCode plus, KeyCode minus, Action<float> copy, Func<float> bind, XDebug.DebugFloat.LogT logType = XDebug.DebugFloat.LogT.None)
            {
                XDebug.DebugFloat debugFloat = new GameObject(name + " [wrapper]").AddComponent<XDebug.DebugFloat>();
                UnityEngine.Object.DontDestroyOnLoad(debugFloat.gameObject);
                debugFloat.Name = name;
                debugFloat.Value = init;
                debugFloat.Plus = plus;
                debugFloat.Minus = minus;
                debugFloat.Offset = offset;
                debugFloat.LogType = logType;
                debugFloat.UpdateActualValue = copy;
                debugFloat.BindToVariable = bind;
            }

            private KeyCode Plus;

            private KeyCode Minus;

            private float Offset;

            private string Name;

            private float Value;

            private float prevTime;

            private Action<float> UpdateActualValue;

            private XDebug.DebugFloat.LogT LogType;

            private Func<float> BindToVariable;

            public enum LogT
            {
                None,
                Constant,
                OnChange
            }
        }

        private class DebugInt : MonoBehaviour
        {
            private void Update()
            {
                this.BindToVariable();
                bool flag = false;
                if (Input.GetKey(this.Plus))
                {
                    if (Input.GetKeyDown(this.Plus) || Time.time - this.prevTime > 0.25f)
                    {
                        flag = true;
                        this.prevTime = Time.time;
                        this.Value += this.Offset;
                    }
                }
                else if (Input.GetKey(this.Minus) && (Input.GetKeyDown(this.Minus) || Time.time - this.prevTime > 0.25f))
                {
                    flag = true;
                    this.prevTime = Time.time;
                    this.Value -= this.Offset;
                }
                if (flag)
                {
                    this.UpdateActualValue(this.Value);
                }
                if (this.LogType == XDebug.DebugInt.LogT.Constant)
                {
                    XSingleton<XDebug>.Instance.Log(string.Format("{0} = {1}", this.Name, this.Value), 1.25f, 16f);
                    return;
                }
                if (flag && this.LogType == XDebug.DebugInt.LogT.OnChange)
                {
                    XSingleton<XDebug>.Instance.Log(string.Format("{0} = {1}", this.Name, this.Value), 1.25f, 16f);
                }
            }

            public DebugInt(string name, int init, int offset, KeyCode plus, KeyCode minus, Action<int> copy, Func<int> bind, XDebug.DebugInt.LogT logType = XDebug.DebugInt.LogT.None)
            {
                XDebug.DebugInt debugInt = new GameObject(name + " [wrapper]").AddComponent<XDebug.DebugInt>();
                UnityEngine.Object.DontDestroyOnLoad(debugInt.gameObject);
                debugInt.Name = name;
                debugInt.Value = init;
                debugInt.Plus = plus;
                debugInt.Minus = minus;
                debugInt.Offset = offset;
                debugInt.LogType = logType;
                debugInt.BindToVariable = bind;
                debugInt.UpdateActualValue = copy;
            }

            private KeyCode Plus;

            private KeyCode Minus;

            private int Offset;

            private string Name;

            private int Value;

            private float prevTime;

            private Action<int> UpdateActualValue;

            private Func<int> BindToVariable;

            private XDebug.DebugInt.LogT LogType;

            public enum LogT
            {
                None,
                Constant,
                OnChange
            }
        }

        private struct ColorConstants
        {
            public static Color SmoothSwitchOverride = new Color(0.7216338f, 0.1462264f, 1f, 0.5f) * 1.75f;
        }

        public struct CustomSpeedMultiplier
        {
            public enum LUASpeedType { Ground, Air, Spindash, Flying, Climb, Homing }
            public static ArrayByEnum<LUASpeedType, float> ByType = new ArrayByEnum<LUASpeedType, float>(1f);
            //public static float Ground = 1f;

            //public static float Air = 1f;

            //public static float Spindash = 1f;

            //public static float Flying = 1f;

            //public static float Climbing = 1f;

            //public static float Homing = 1f;

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
                XDebug.Comment("[pending]");
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
                XDebug.Comment("[fix]?");
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
                XDebug.Comment("gameData.SetFlag(Game.LotusOfResilience); ???");
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

        [Serializable]
        public struct Cfg
        {
            public struct Cheats
            {
                public static bool InfiniteGauge;

                public static bool Immune;

                public static bool MaxedOutGems;

                public static bool InfiniteRings;

                public static bool InfiniteLives;
            }

            public struct FWS
            {
                public static float YWaterOffset = 0.5f;

                public static float MinActivationSpeed = 8.5f;

                public static float SpeedBoost = 1.25f;

                public static float AccelTime = 0.65f;

                public static float MinRunAnimationSpeed = 27f;

                public static float YMaxWaterRaycastDist = 0.501f;

                public static float RunningBrakeSpeed = 25f;
            }

            public struct MachSpeedSecondJump
            {
                public static float FXPlaybackScale = 0.6f;

                public static float HueShift = -0.25f;

                public static float Offset = -0.4f;
            }

            public struct WJ
            {
                public static float MaxWaitTime = 0.75f;

                public static float MinDotNormal = -0.5f;

                public static float MaxDotNormal = 0.5f;

                public static float UpOffset = -0.25f;

                public static float NormalOffset = 0.5f;

                public static Vector3 MeshRotation = new Vector3(90f, 0f, 0f);

                public static float JumpStrength = 25f;

                public static float MinHeightAboveGround = 1f;
            }

            public struct Speedo
            {
                public static float MaxDisplayableSpeed = 999.9f;
            }

            public struct Boost
            {
                public static float FirstSpeed = 40f;
            }
        }
    }
}
