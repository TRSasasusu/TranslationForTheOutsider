using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;

namespace TranslationForTheOutsider
{
	public class TranslationForTheOutsider : ModBehaviour
	{

		public static TranslationForTheOutsider Instance;

		public void Log(string text)
		{
			ModHelper.Console.WriteLine(text);
		}

		public void Log(string text, MessageType messageType)
		{
			ModHelper.Console.WriteLine(text, messageType);
		}

		public bool IsFixIssuesOfTheOutsider => true;// it is forced to be true because some people accidentally change it false //ModHelper.Config.GetSettingsValue<bool>("Fix issues of The Outsider");

		private void Awake()
		{
			Instance = this;
		}

		private void Start()
		{
			// Starting here, you'll have access to OWML's mod helper.
			ModHelper.Console.WriteLine($"{nameof(TranslationForTheOutsider)} is loaded!", MessageType.Success);

			Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
			IssueOfTheOutsiderPatch.Initialize();
			NewHorizonsCompat.Initialize();
		}
	}
}