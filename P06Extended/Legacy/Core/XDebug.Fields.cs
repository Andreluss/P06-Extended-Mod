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
    }
}
