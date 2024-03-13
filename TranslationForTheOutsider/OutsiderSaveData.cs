// Downloaded from https://github.com/Outer-Wilds-New-Horizons/new-horizons/blob/cb08f4710d184b778bb4d92e995ba264186744bb/NewHorizons/External/NewHorizonsData.cs and edited.

using System;
using System.Collections.Generic;

namespace TranslationForTheOutsider
{
    public static class OutsiderSaveData
    {
        private static OutsiderSaveFile _saveFile;
        private static OutsiderProfile _activeProfile;
        private static string _activeProfileName;
        private static readonly string FileName = "save.json";

        private static object _lock = new();

        public static string GetProfileName() => StandaloneProfileManager.SharedInstance?.currentProfile?.profileName;

        public static void Load()
        {
            lock (_lock)
            {
                _activeProfileName = GetProfileName();
                if (_activeProfileName == null)
                {
                    TranslationForTheOutsider.Instance.Log("Couldn't find active profile, are you on Gamepass?", OWML.Common.MessageType.Warning);
                    _activeProfileName = "XboxGamepassDefaultProfile";
                }

                try
                {
                    _saveFile = TranslationForTheOutsider.Instance.ModHelper.Storage.Load<OutsiderSaveFile>(FileName, false);
                    if (!_saveFile.Profiles.ContainsKey(_activeProfileName))
                        _saveFile.Profiles.Add(_activeProfileName, new OutsiderProfile());
                    _activeProfile = _saveFile.Profiles[_activeProfileName];
                    TranslationForTheOutsider.Instance.Log($"Loaded save data for {_activeProfileName}");
                }
                catch (Exception)
                {
                    try
                    {
                        TranslationForTheOutsider.Instance.Log($"Couldn't load save data from {FileName}, creating a new file");
                        _saveFile = new OutsiderSaveFile();
                        _saveFile.Profiles.Add(_activeProfileName, new OutsiderProfile());
                        _activeProfile = _saveFile.Profiles[_activeProfileName];
                        TranslationForTheOutsider.Instance.ModHelper.Storage.Save(_saveFile, FileName);
                        TranslationForTheOutsider.Instance.Log($"Loaded save data for {_activeProfileName}");
                    }
                    catch (Exception e)
                    {
                        TranslationForTheOutsider.Instance.Log($"Couldn't create save data:\n{e}", OWML.Common.MessageType.Error);
                    }
                }
            }
        }

        public static void Save()
        {
            if (_saveFile == null) return;

            // Threads exist
            lock (_lock)
            {
                try
                {
                    TranslationForTheOutsider.Instance.ModHelper.Storage.Save(_saveFile, FileName);
                }
                catch (Exception ex)
                {
                    TranslationForTheOutsider.Instance.Log($"Couldn't save data:\n{ex}", OWML.Common.MessageType.Error);
                }
            }
        }

        public static void Reset()
        {
            if (_saveFile == null || _activeProfile == null) Load();
            TranslationForTheOutsider.Instance.Log($"Resetting save data for {_activeProfileName}");
            _activeProfile = new OutsiderProfile();
            _saveFile.Profiles[_activeProfileName] = _activeProfile;

            Save();
        }

        private class OutsiderSaveFile
        {
            public OutsiderSaveFile()
            {
                Profiles = new Dictionary<string, OutsiderProfile>();
            }

            public Dictionary<string, OutsiderProfile> Profiles { get; }
        }

        private class OutsiderProfile
        {
            public OutsiderProfile()
            {
                NewlyRevealedFactIDs = new List<string>();
            }

            public List<string> NewlyRevealedFactIDs { get; }
        }

        public static void AddNewlyRevealedFactID(string id)
        {
            _activeProfile?.NewlyRevealedFactIDs.Add(id);
            Save();
        }

        public static List<string> GetNewlyRevealedFactIDs()
        {
            return _activeProfile?.NewlyRevealedFactIDs;
        }

        public static void ClearNewlyRevealedFactIDs()
        {
            _activeProfile?.NewlyRevealedFactIDs.Clear();
            Save();
        }
    }
}
