using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TranslationForTheOutsider {
    [HarmonyPatch]
    public static class IssueOfTheOutsiderPatch {
        static bool _fixShipLogCardPosition = false;
        static int _fixShipLogCardPositionCount = 0;
        static bool _projecting = false;

        public static void Initialize() {
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) => {
                if (loadScene == OWScene.TitleScreen) {
                    _fixShipLogCardPosition = true; // It cannot be true when the title is loaded at first because Initialized() has not run yet, I think
                }
            };

            TranslationForTheOutsider.Instance.Log($"{nameof(IssueOfTheOutsiderPatch)} is initialized.");
        }

        // Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/7
        [HarmonyFinalizer]
        [HarmonyPatch(typeof(NomaiTranslatorProp), nameof(NomaiTranslatorProp.UpdateTimeFreeze))]
        public static Exception NomaiTranslatorProp_UpdateTimeFreeze_Finalizer(Exception __exception) {
            if(!TranslationForTheOutsider.Instance.IsFixIssuesOfTheOutsider) {
                return __exception;
            }
            return null;
        }

        // Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/8
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipLogManager), nameof(ShipLogManager.Awake))]
        [HarmonyAfter(new string[] {"SBtT.TheOutsider"})]
        public static void ShipLogManager_Awake_Prefix(ShipLogManager __instance) {
            if(!TranslationForTheOutsider.Instance.IsFixIssuesOfTheOutsider) {
                return;
            }

            if(!_fixShipLogCardPosition) {
                return;
            }
            _fixShipLogCardPosition = false; // Fixing is only done after reloading from the title screen.
            ++_fixShipLogCardPositionCount; // Shifts are accumulated, so it should be multiplied.

            var offset = new Vector2(-250f, 0);
            bool isInOutsiderShipLog = false;
            for(int i = 0; i < __instance._shipLogLibrary.entryData.Length; ++i) {
                if (__instance._shipLogLibrary.entryData[i].id == "DB_NORTHERN_OBSERVATORY") {
                    isInOutsiderShipLog = true;
                }
                if(!isInOutsiderShipLog) {
                    continue;
                }

                __instance._shipLogLibrary.entryData[i].cardPosition -= offset * _fixShipLogCardPositionCount;
                //TranslationForTheOutsider.Instance.Log($"{__instance._shipLogLibrary.entryData[i].id}'s cardPosition is fixed! ({__instance._shipLogLibrary.entryData[i].cardPosition})");
            }

            TranslationForTheOutsider.Instance.Log("ShipLog's card positions are fixed.");
        }

        // ### Start: Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/9 ###
        [HarmonyPrefix]
        [HarmonyPatch(typeof(NomaiRemoteCameraPlatform), nameof(NomaiRemoteCameraPlatform.SwitchToRemoteCamera))]
        public static bool NomaiRemoteCameraPlatform_SwitchToRemoteCamera_Prefix() {
            //TranslationForTheOutsider.Instance.Log("SwitchToRemoteCamera now.");
            _projecting = true;
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NomaiRemoteCameraPlatform), nameof(NomaiRemoteCameraPlatform.SwitchToPlayerCamera))]
        public static bool NomaiRemoteCameraPlatform_SwitchToPlayerCamera_Prefix() {
            //TranslationForTheOutsider.Instance.Log("SwitchToPlayerCamera now.");
            _projecting = false;
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FogWarpVolume), nameof(FogWarpVolume.WarpDetector))]
        public static bool FogWarpVolume_WarpDetector_Prefix() {
            //TranslationForTheOutsider.Instance.Log("Warped maybe.");
            if(!TranslationForTheOutsider.Instance.IsFixIssuesOfTheOutsider) {
                return true;
            }
            return !_projecting;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FogWarpDetector), nameof(FogWarpDetector.FixedUpdate))]
        public static bool FogWarpDetector_FixedUpdate_Prefix() {
            if(!TranslationForTheOutsider.Instance.IsFixIssuesOfTheOutsider) {
                return true;
            }
            return !_projecting;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NomaiRemoteCameraStreaming), nameof(NomaiRemoteCameraStreaming.NomaiRemoteCameraPlatformIDToSceneName))]
        public static bool NomaiRemoteCameraStreaming_NomaiRemoteCameraPlatformIDToSceneName_Prefix(NomaiRemoteCameraPlatform.ID id, ref string __result) {
            if(!TranslationForTheOutsider.Instance.IsFixIssuesOfTheOutsider) {
                return true;
            }

            // DarkBramble's SharedStone (i.e., projection stone) in Ash Twin Project has an id (i.e., _connectedPlatform), 100.
            // This code replace an empty string result as StreamingGroup of "DarkBramble" in L50 of NomaiRemoteCameraStreaming.cs (in its FixedUpdate)
            //   - _sceneName of StreamingGroup of DarkBramble is "DarkBramble", so it works.
            // The Outsider did not fix this function but fixed IDToPlanetString, see https://github.com/StreetlightsBehindTheTrees/Outer-Wilds-The-Outsider/blob/65d297e9e2a9e02c7ea40c72b1a109c440a0b2e0/TheOutsider/OuterWildsHandling/OWPatches.cs#L283
            if ((int)id == 100) {
                //TranslationForTheOutsider.Instance.Log("id to darkbramble.");
                __result = "DarkBramble";
                return false;
            }
            return true;
        }
        // ### End: Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/9 ###
    }
}
