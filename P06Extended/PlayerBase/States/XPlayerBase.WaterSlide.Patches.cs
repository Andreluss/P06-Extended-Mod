namespace P06X
{
    using HarmonyLib;
    using UnityEngine;
    using Helpers;
    using UnityEngine.Assertions;
    using System;

    public partial class XPlayerBase : MonoBehaviour
    {
        [HarmonyPatch(typeof(WaterslideBooster), "OnTriggerEnter")]
        public class WaterslideBooster_OnTriggerEnter
        {
            public static bool Prefix(WaterslideBooster __instance, Collider collider)
            {
                PlayerBase player = __instance.GetPlayer(collider);
                if (__instance.Get<bool>("IsTriggered") || !player || player.Get<bool>("IsDead"))
                {
                    return false;
                }
                __instance.GetMethod("Swoosh").Invoke(__instance, null);
                player.OnWaterSlideEnter("", false, __instance.Get<float>("Speed"));
                return false;
            }
        }

        [HarmonyPatch(typeof(WaterSlider), "OnTriggerEnter")]
        public class WaterSlider_OnTriggerEnter
        {
            public static bool Prefix(WaterSlider __instance, Collider collider)
            {
                PlayerBase player = __instance.GetPlayer(collider);
                // The game only lets sonic_new and metal_sonic onto water sliders; this lets every character on.
                if (!player || player.Get<bool>("IsDead"))
                {
                    return false;
                }
                player.OnWaterSlideEnter(__instance.Path, true, 0f);
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerBase), nameof(PlayerBase.OnWaterSlideEnter))]
        public class PlayerBase_OnWaterSlideEnter
        {
            public static void Postfix(PlayerBase __instance, string Spline, bool TriggerState, float Speed)
            {
                if (Speed != 0f)
                {
                    if (XI.WaterSlideState.active)
                    {
                        XI.WaterSlideState.WSpeed = Speed;
                    }
                    else
                    {
                        __instance.Set("CurSpeed", Speed);
                    }
                }
                if (TriggerState)
                {
                    XI.WaterSlideState.LaunchMode = Spline;
                    __instance.StateMachine.ChangeState(XI.StateWaterSlide);
                }
            }
        }

        [HarmonyPatch(typeof(SonicNew), nameof(SonicNew.OnWaterSlideEnter))]
        public class SonicNew_OnWaterSlideEnter
        {
            public static void Postfix(SonicNew __instance, string Spline, bool TriggerState, float Speed)
            {
                Debug.Log("SonicNew onwaterslideenter Postfix fired!");
            }
        }
    }
}
