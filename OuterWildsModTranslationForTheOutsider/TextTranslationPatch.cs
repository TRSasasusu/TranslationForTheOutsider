using HarmonyLib;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OWML.ModHelper;
using System.Xml;

namespace TranslationForTheOutsider {
    [HarmonyPatch]
    public static class TextTranslationPatch {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TextTranslation), nameof(TextTranslation.SetLanguage))]
        public static void TextTranslation_SetLanguage_Postfix(ref TextTranslation.Language lang, TextTranslation __instance) {
            if(lang == TextTranslation.Language.JAPANESE) {
                var path = TranslationForTheOutsider.Instance.ModHelper.Manifest.ModFolderPath + "assets/japanese.xml";
                TranslationForTheOutsider.Instance.ModHelper.Console.WriteLine($"path to xml file: {path}");

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(ReadAndRemoveByteOrderMarkFromPath(path));
                var translationTableNode = xmlDoc.SelectSingleNode("TranslationTable_XML");

//                foreach(var item in __instance.m_table.theTable) {
//                    TranslationForTheOutsider.Instance.ModHelper.Console.WriteLine($"theTable's 1st key: {item.Key}, value: {item.Value}");
//                    break;
//                }
//
//                foreach(var item in __instance.m_table.theTable) {
//                    if(item.Key.Contains("This anglerfish specimen was found attached")) {
//                        TranslationForTheOutsider.Instance.ModHelper.Console.WriteLine($"This anglerfish ... key: {item.Key}, value: {item.Value}");
//                        break;
//                    }
//                }
//
//                foreach(var item in __instance.m_table.theShipLogTable) {
//                    TranslationForTheOutsider.Instance.ModHelper.Console.WriteLine($"theShipLogTable's 1st key: {item.Key}, value: {item.Value}");
//                    break;
//                }

                foreach(XmlNode node in translationTableNode.SelectNodes("entry")) {
                    var key = node.SelectSingleNode("key").InnerText;
                    var value = node.SelectSingleNode("value").InnerText;

                    __instance.m_table.theTable[key] = value;
                    //TranslationForTheOutsider.Instance.ModHelper.Console.WriteLine($"key: {key}, value: {value}");
                }

                foreach(XmlNode node in translationTableNode.SelectNodes("table_shipLog")) {
                    var key = node.SelectSingleNode("key").InnerText;
                    var value = node.SelectSingleNode("value").InnerText;

                    __instance.m_table.theShipLogTable[key] = value;
                    //TranslationForTheOutsider.Instance.ModHelper.Console.WriteLine($"key: {key}, value: {value}");
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TextTranslation), nameof(TextTranslation._Translate))]
        public static bool TextTranslation_Translate_Prefix(string key, TextTranslation __instance) {
            if(__instance.m_table.Get(key) == null) {
                TranslationForTheOutsider.Instance.ModHelper.Console.WriteLine($"key not contained in m_table: {key}");
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TextTranslation), nameof(TextTranslation._Translate_ShipLog))]
        public static bool TextTranslation_Translate_ShipLog_Prefix(string key, TextTranslation __instance) {
            if(__instance.m_table.GetShipLog(key) == null) {
                TranslationForTheOutsider.Instance.ModHelper.Console.WriteLine($"key not contained in m_table(ship): {key}");
            }
            return true;
        }


        public static string ReadAndRemoveByteOrderMarkFromPath(string path) {
            // this code is from https://github.com/xen-42/outer-wilds-localization-utility/blob/6cf4eb784c06237820d318b4ce22ac30da4acac1/LocalizationUtility/Patches/TextTranslationPatches.cs#L198-L209
            byte[] bytes = File.ReadAllBytes(path);
            byte[] preamble1 = Encoding.UTF8.GetPreamble();
            byte[] preamble2 = Encoding.Unicode.GetPreamble();
            byte[] preamble3 = Encoding.BigEndianUnicode.GetPreamble();
            if (bytes.StartsWith(preamble1))
                return Encoding.UTF8.GetString(bytes, preamble1.Length, bytes.Length - preamble1.Length);
            if (bytes.StartsWith(preamble2))
                return Encoding.Unicode.GetString(bytes, preamble2.Length, bytes.Length - preamble2.Length);
            return bytes.StartsWith(preamble3) ? Encoding.BigEndianUnicode.GetString(bytes, preamble3.Length, bytes.Length - preamble3.Length) : Encoding.UTF8.GetString(bytes);
        }
    }


}
