using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Net;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace TranslationForTheOutsider {
    public class TranslationForTheOutsider : ModBehaviour {

        public static TranslationForTheOutsider Instance;

        bool _newHorizons;
        ScreenPrompt _displayWarningTagger;
        //TitleScreenManager _titleScreenManager;
        //Text _gameVersionTextDisplay;

        public void Log(string text) {
            ModHelper.Console.WriteLine(text);
        }

        public void Log(string text, MessageType messageType) {
            ModHelper.Console.WriteLine(text, messageType);
        }

        public bool IsFixIssuesOfTheOutsider => true;// it is forced to be true because some people accidentally change it false //ModHelper.Config.GetSettingsValue<bool>("Fix issues of The Outsider");

        private void Awake() {
            Instance = this;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
        }

        private void Start() {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"{nameof(TranslationForTheOutsider)} is loaded!", MessageType.Success);

            // Example of accessing game code.
            //LoadManager.OnCompleteSceneLoad += (scene, loadScene) => {
            //    if (loadScene != OWScene.SolarSystem) return;
            //    ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);
            //};

            IssueOfTheOutsiderPatch.Initialize();

            _newHorizons = ModHelper.Interaction.TryGetMod("xen.NewHorizons") != null;
            if(_newHorizons ) {
                ModHelper.Console.WriteLine($"New Horizons is found. It is recommended to be disabled when playing The Outsider.", MessageType.Success);
            }
        }

        private void OnGUI() {
            if(!_newHorizons) {
                return;
            }
            //if(SceneManager.GetActiveScene().name == "TitleScreen") {
            //    if(!_gameVersionTextDisplay) {
            //        var gameVersionTextDisplay = GameObject.Find("TitleMenu/TitleCanvas/FooterBlock/VersionBlock/VersionText");
            //        if(!gameVersionTextDisplay) {
            //            return;
            //        }
            //        _gameVersionTextDisplay = gameVersionTextDisplay.GetComponent<Text>();
            //    }
            //    if(!_gameVersionTextDisplay.enabled) {
            //        return;
            //    }
            //    //if (!_titleScreenManager) {
            //    //    var titleScreenManager = GameObject.Find("TitleMenuManagers");
            //    //    if (!titleScreenManager) {
            //    //        return;
            //    //    }
            //    //    _titleScreenManager = titleScreenManager.GetComponent<TitleScreenManager>();
            //    //}
            //    //if(!_titleScreenManager.MainMenuIsActive()) {
            //    //    return;
            //    //}
            //}
            if(SceneManager.GetActiveScene().name != "SolarSystem") {
                return;
            }

            if (_displayWarningTagger == null) {
                _displayWarningTagger = new ScreenPrompt("");
            }
            _displayWarningTagger.SetText(TextTranslation.Translate("WARNING: New Horizons Mod should be disabled when playing The Outsider!!"));
            var promptManager = Locator.GetPromptManager();
            if(promptManager != null) {
                var screenPromptList = promptManager.GetScreenPromptList(PromptPosition.LowerLeft);
                if(screenPromptList != null && !screenPromptList.Contains(_displayWarningTagger)) {
                    promptManager.AddScreenPrompt(_displayWarningTagger, PromptPosition.LowerLeft, true);
                }
            }
        }
    }
}