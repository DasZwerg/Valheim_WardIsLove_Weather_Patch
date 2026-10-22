using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace WardIsLoveWeatherPatch
{
    [BepInPlugin("com.devin.wardisloveweatherpatch", "Ward Is Love Weather Patch", "1.0.1")]
    [BepInDependency("Azumatt.WardIsLove", BepInDependency.DependencyFlags.SoftDependency)]
    public class WardIsLoveWeatherPatchPlugin : BaseUnityPlugin
    {
        private static ConfigEntry<bool> _allowWeatherParticles;
        private static ConfigEntry<bool> _allowWetDebuff;
        private static FieldInfo _currentPSystems;
        private static MethodInfo _setParticleArrayEnabled;
        private static Type _wardMonoscriptType;
        private static MethodInfo _isInsideShield;
        private static RaycastHit[] _hits = new RaycastHit[128];
        private static int _coverMask;
        private static readonly object[] _shieldArgs = new object[2];
        private static bool _inLocalPlayerEnvCheck;
        private static bool _vanillaShieldResult;

        private void Awake()
        {
            _allowWeatherParticles = Config.Bind("Weather", "AllowWeatherParticlesInsideWard", true,
                "Let rain/snow/ash render inside active ward bubbles. (WardIsLove hides them.)");
            _allowWetDebuff = Config.Bind("Weather", "AllowWetDebuffInsideWard", true,
                "Let the Wet debuff apply inside wards again. Real roofs and vanilla shield generators still keep you dry.");
            _coverMask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece", "terrain", "vehicle");
            _currentPSystems = AccessTools.Field(typeof(EnvMan), "m_currentPSystems");
            _setParticleArrayEnabled = AccessTools.Method(typeof(EnvMan), "SetParticleArrayEnabled");
            var wardAsm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "WardIsLove");
            if (wardAsm != null)
            {
                _wardMonoscriptType = wardAsm.GetType("WardIsLove.Util.WardMonoscript");
                _isInsideShield = AccessTools.Method(wardAsm.GetType("WardQueryManager"), "IsInsideShield", new[] { typeof(Vector3), typeof(float) });
            }
            if (_wardMonoscriptType == null || _isInsideShield == null)
            {
                Logger.LogWarning("WardIsLove not found or incompatible; patches will still apply but use fallback checks.");
            }
            new Harmony("com.devin.wardisloveweatherpatch").PatchAll();
            Logger.LogInfo("WardIsLoveWeatherPatch loaded.");
        }

        // WardIsLove's BlockWeatherInsideBubble postfix disables m_currentPSystems
        // while the player is inside a shield. Run after it and turn them back on.
        [HarmonyPatch(typeof(EnvMan), "Update")]
        [HarmonyPriority(Priority.Last)]
        private static class ReenableWeatherParticles
        {
            private static void Postfix(EnvMan __instance)
            {
                if (!_allowWeatherParticles.Value || Player.m_localPlayer == null || !InsideShield(Player.m_localPlayer.transform.position)) return;
                var psystems = (GameObject[])_currentPSystems.GetValue(__instance);
                if (psystems != null)
                {
                    _setParticleArrayEnabled.Invoke(__instance, new object[] { psystems, true });
                }
            }
        }

        // Vanilla only applies Wet when !ShieldGenerator.IsInsideShield(pos), and WardIsLove
        // makes that return true inside every ward. Scope our override to the local player's
        // env status check so structures/plants/cinders still treat wards as shields.
        [HarmonyPatch(typeof(Player), "UpdateEnvStatusEffects")]
        private static class ScopeLocalPlayerEnvCheck
        {
            private static void Prefix(Player __instance)
            {
                _inLocalPlayerEnvCheck = _allowWetDebuff.Value && __instance == Player.m_localPlayer;
            }

            private static Exception Finalizer(Exception __exception)
            {
                _inLocalPlayerEnvCheck = false;
                return __exception;
            }
        }

        [HarmonyPatch(typeof(ShieldGenerator), nameof(ShieldGenerator.IsInsideShield))]
        private static class CaptureVanillaShieldResult
        {
            [HarmonyPriority(Priority.First)]
            private static void Postfix(bool __result)
            {
                if (_inLocalPlayerEnvCheck) _vanillaShieldResult = __result;
            }
        }

        [HarmonyPatch(typeof(ShieldGenerator), nameof(ShieldGenerator.IsInsideShield))]
        private static class RestoreVanillaShieldResult
        {
            [HarmonyPriority(Priority.Last)]
            private static void Postfix(ref bool __result)
            {
                if (_inLocalPlayerEnvCheck) __result = _vanillaShieldResult;
            }
        }

        // Fallback in case the ward dome has a collider overhead: ignore ward-owned colliders
        // when the game checks whether a point is under a roof.
        [HarmonyPatch(typeof(Cover), "IsUnderRoof")]
        private static class WardDomeIsNotARoof
        {
            private static void Postfix(Vector3 startPos, ref bool __result)
            {
                if (!__result || !_allowWetDebuff.Value || _wardMonoscriptType == null) return;
                int num = Physics.SphereCastNonAlloc(startPos, 0.1f, Vector3.up, _hits, 100f, _coverMask);
                bool roofed = false;
                for (int i = 0; i < num; i++)
                {
                    var col = _hits[i].collider;
                    if (col.gameObject.CompareTag("leaky")) continue;
                    if (IsWardCollider(col)) continue;
                    roofed = true;
                    break;
                }
                __result = roofed;
            }
        }

        private static bool InsideShield(Vector3 pos)
        {
            if (_isInsideShield == null) return false;
            _shieldArgs[0] = pos;
            _shieldArgs[1] = 0f;
            return (bool)_isInsideShield.Invoke(null, _shieldArgs);
        }

        private static bool IsWardCollider(Collider col)
        {
            return col.GetComponentInParent(_wardMonoscriptType) != null;
        }
    }
}
