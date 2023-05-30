using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;

namespace TranslationForTheOutsider {
    public class TranslationForTheOutsider : ModBehaviour {

        public static TranslationForTheOutsider Instance;

        public void Log(string text) {
            ModHelper.Console.WriteLine(text);
        }

        public bool IsFixIssuesOfTheOutsider => ModHelper.Config.GetSettingsValue<bool>("Fix issues of The Outsider");

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
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) => {
                if (loadScene != OWScene.SolarSystem) return;
                ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);
            };
        }
    }
}