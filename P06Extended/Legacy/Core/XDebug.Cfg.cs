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
