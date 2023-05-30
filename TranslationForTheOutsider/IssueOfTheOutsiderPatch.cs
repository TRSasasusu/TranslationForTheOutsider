using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslationForTheOutsider {
    [HarmonyPatch]
    public static class IssueOfTheOutsiderPatch {
        [HarmonyFinalizer]
        [HarmonyPatch(typeof(NomaiTranslatorProp), nameof(NomaiTranslatorProp.UpdateTimeFreeze))]
        public static Exception NomaiTranslatorProp_UpdateTimeFreeze_Finalizer(Exception __exception) {
            if(!TranslationForTheOutsider.Instance.IsFixIssuesOfTheOutsider) {
                return __exception;
            }

            // Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/7
            return null;
        }
    }
}
