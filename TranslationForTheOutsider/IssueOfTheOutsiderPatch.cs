using HarmonyLib;
using HarmonyLib.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NewHorizons.External;

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
                if(loadScene == OWScene.SolarSystem || loadScene == OWScene.EyeOfTheUniverse) {
                    NewHorizonsData.Load();
                    AddNewlyRevealedFactIDsFromBaseGameSaveFile();
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

        // Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/11
        [HarmonyPostfix]
        [HarmonyPatch(typeof(NomaiWallText), nameof(NomaiWallText.Show))]
        public static void NomaiWallText_Show_Postfix(NomaiWallText __instance) {
            if(!TranslationForTheOutsider.Instance.IsFixIssuesOfTheOutsider) {
                return;
            }

            if(__instance.name == "Text_YarrowOtherSide") {
                __instance.GetComponent<BoxCollider>().enabled = true;
                TranslationForTheOutsider.Instance.Log($"Fixed the collider of {__instance.name}.");
            }
        }

        // ### Start: Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/12 ###
        static bool _gettingEntries = false;
        static List<string> _outsiderFactIDPrefixes; // {"PS_POWER_STATION", "DB_NORTHERN_OBSERVATORY", ...}
        static List<string> _newlyRevealedFactIDsFromBaseGameSaveFile = null;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipLogManager), nameof(ShipLogManager.Awake))]
        [HarmonyAfter(new string[] { "SBtT.TheOutsider" })]
        public static void ShipLogManager_Awake_Prefix_Issue12(ShipLogManager __instance) {
            if(!TranslationForTheOutsider.Instance.IsFixIssuesOfTheOutsider) {
                return;
            }

            _newlyRevealedFactIDsFromBaseGameSaveFile = null;

            var length = __instance._shipLogXmlAssets.Length;

            _outsiderFactIDPrefixes = new List<string>();
            _gettingEntries = true;
            try {
                for(int i = 1; i <= 4; ++i) {
                    __instance.GenerateEntriesFromXml(__instance._shipLogXmlAssets[length - i]);
                }
            }
            catch(Exception e) {
                TranslationForTheOutsider.Instance.Log(e.ToString(), OWML.Common.MessageType.Error);
            }
            finally {
                _gettingEntries = false;
            }

            // If the user played The Outsider with a previous version of Translation and Patches for The Outsider, the save file may have newly revealed facts of The Outsider.
            // So, we check it, and if they exist, move the newly revealed facts of The Outsider into our New Horizons based save file.
            bool save = false;
            for(int i = PlayerData._currentGameSave.newlyRevealedFactIDs.Count - 1; i >= 0; --i) {
                //TranslationForTheOutsider.Instance.Log($"saved newlyRevealedFactIDs in base game save file: {id}");
                var id = PlayerData._currentGameSave.newlyRevealedFactIDs[i];
                if (IsModdedFact(id)) {
                    TranslationForTheOutsider.Instance.Log($"newly revealed fact mod id: {id} is found in the base game save file, so it is moved to the mod save file.");
                    if(_newlyRevealedFactIDsFromBaseGameSaveFile == null) {
                        _newlyRevealedFactIDsFromBaseGameSaveFile = new List<string>();
                    }
                    _newlyRevealedFactIDsFromBaseGameSaveFile.Add(id);
                    //NewHorizonsData.AddNewlyRevealedFactID(id); // it cannot work because Load() has not run yet.
                    PlayerData._currentGameSave.newlyRevealedFactIDs.RemoveAt(i);
                    save = true;
                }
            }
            if(save) {
                PlayerData.SaveCurrentGame();
            }
        }

        static void AddNewlyRevealedFactIDsFromBaseGameSaveFile() {
            if(_newlyRevealedFactIDsFromBaseGameSaveFile != null) {
                foreach(var id in _newlyRevealedFactIDsFromBaseGameSaveFile) {
                    NewHorizonsData.AddNewlyRevealedFactID(id);
                }
                _newlyRevealedFactIDsFromBaseGameSaveFile = null;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipLogManager), nameof(ShipLogManager.AddEntry))]
        public static bool ShipLogManager_AddEntry_Prefix(ShipLogEntry entry) {
            if(_gettingEntries) {
                //TranslationForTheOutsider.Instance.Log($"{entry._id}, {entry._name}");
                _outsiderFactIDPrefixes.Add(entry._id);
                return false;
            }
            return true;
        }

        static bool IsModdedFact(string id) {
            foreach(var idPrefixes in _outsiderFactIDPrefixes) {
                if(id.StartsWith(idPrefixes)) {
                    TranslationForTheOutsider.Instance.Log($"{id} is modded fact");
                    return true;
                }
            }
            return false;
        }

        // ###### See https://github.com/Outer-Wilds-New-Horizons/new-horizons/blob/e2a07d33106f52667a30fec85c5fc2e957086dad/NewHorizons/Patches/PlayerPatches/PlayerDataPatches.cs#L96-L142 ######
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.AddNewlyRevealedFactID))]
        public static bool PlayerData_AddNewlyRevealedFactID(string id) {
            if (IsModdedFact(id)) {
                NewHorizonsData.AddNewlyRevealedFactID(id);
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.GetNewlyRevealedFactIDs))]
        public static bool PlayerData_GetNewlyRevealedFactIDs_Prefix(ref List<string> __result) {
            var newHorizonsNewlyRevealedFactIDs = NewHorizonsData.GetNewlyRevealedFactIDs();
            if (newHorizonsNewlyRevealedFactIDs != null) {
                __result = PlayerData._currentGameSave.newlyRevealedFactIDs.Concat(newHorizonsNewlyRevealedFactIDs).ToList();
                return false;
            }
            TranslationForTheOutsider.Instance.Log("Newly Revealed Fact IDs is null!", OWML.Common.MessageType.Error);
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.GetNewlyRevealedFactIDs))]
        public static void PlayerData_GetNewlyRevealedFactIDs_Postfix(ref List<string> __result) {
            var manager = Locator.GetShipLogManager();
            __result = __result.Where(id => manager.GetFact(id) != null).ToList();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.ClearNewlyRevealedFactIDs))]
        public static void PlayerData_ClearNewlyRevealedFactIDs() {
            NewHorizonsData.ClearNewlyRevealedFactIDs();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.ResetGame))]
        public static void PlayerData_ResetGame() {
            NewHorizonsData.Reset();
        }

        // ### End: Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/12 ###
    }
}
