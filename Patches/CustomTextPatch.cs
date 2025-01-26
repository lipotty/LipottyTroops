using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace LipottyTroops.Patches
{
    [HarmonyPatch(typeof(GameTexts), "FindText")]
    public class CustomTextPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref TextObject __result, string id, string variation)
        {
            if (id == "str_recruit_volunteers_not_enough_relation")
            {
                __result = new TextObject("{=LRM_MOD_001}The number of recruits depends on your relationship with the local Notables, their liege, as well as your reputation and standing within the Kingdom.\nThere are many ways to unlock more recruitment slots.");
            }
        }
    }
}
