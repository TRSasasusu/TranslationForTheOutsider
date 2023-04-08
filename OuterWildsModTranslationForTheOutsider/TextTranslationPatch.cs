using HarmonyLib;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OWML.ModHelper;
using System.Xml;
using UnityEngine;

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
        public static void TextTranslation_Translate_Prefix(string key, TextTranslation __instance) {
            if(__instance.m_table == null) {
                return;
            }
            var text = __instance.m_table.Get(key);
            if(text == null) {
                TranslationForTheOutsider.Instance.ModHelper.Console.WriteLine($"key not contained in m_table: {key}");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TextTranslation), nameof(TextTranslation._Translate))]
        public static void TextTranslation_Translate_Postfix(string key, TextTranslation __instance, ref string __result) {
            if(__instance.m_language != TextTranslation.Language.JAPANESE) { // After adding othre languages, change this.
                return;
            }

            var text = __result;
            if (__instance.m_language == TextTranslation.Language.JAPANESE) {
                text = text.Replace("宇宙の眼", "<color=lightblue>宇宙の眼</color>");
                text = text.Replace("眼", "<color=lightblue>眼</color>");

                text = text.Replace("脆い空洞", "<color=lightblue>脆い空洞</color>");
                text = text.Replace("燃え盛る双子星", "<color=lightblue>燃え盛る双子星</color>");

                text = text.Replace("闇のイバラ", "<color=lightblue>闇のイバラ</color>");
                text = text.Replace("`船`", "<color=lightblue>船</color>");

                text = text.Replace("DATURA", "<color=lightblue>DATURA</color>");
                text = text.Replace("Datura", "<color=lightblue>Datura</color>");
                text = text.Replace("FRIEND", "<color=lime>FRIEND</color>");
                text = text.Replace("Friend", "<color=lime>Friend</color>");
            }

            if (text.Contains("#####")) { // this code is from https://github.com/StreetlightsBehindTheTrees/Outer-Wilds-The-Outsider/blob/17149bad3786f9aa68aed9eaf8ec94e62ee5ba7e/TheOutsider/OuterWildsHandling/OWPatches.cs#L136
                int errorLength = Random.Range(5, 11);
                string hash = "";
                for (int i = 0; i < errorLength; i++) hash += '#';

                text = text.Replace("#####", $"[<color=red>{hash}</color>]");

                string l = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!?";
                string newText = "";
                for (int i = 0; i < text.Length; i++) {
                    char c = text[i];
                    if (c == '#') newText += l[Random.Range(0, l.Length)];
                    else newText += c;
                }

                text = newText;
            }

            __result = text;
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
