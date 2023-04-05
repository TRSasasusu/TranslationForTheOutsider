using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;

namespace TranslationForTheOutsider {
    public class TranslationForTheOutsider : ModBehaviour {

        public static TranslationForTheOutsider Instance;

        private void Awake() {
            Instance = this;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
        }

        private void Start() {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"My mod {nameof(TranslationForTheOutsider)} is loaded!", MessageType.Success);

            //var api = ModHelper.Interaction.TryGetModApi<ILocalizationAPI>("xen.LocalizationUtility");
            //api.RegisterLanguage(this, "Japanese", "assets/japanese.xml");

            // Example of accessing game code.
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) => {
                if (loadScene != OWScene.SolarSystem) return;
                ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);
            };
        }
    }
}