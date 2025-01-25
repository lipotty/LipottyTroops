using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TaleWorlds.CampaignSystem.CampaignBehaviors.AiBehaviors;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;

namespace LipottyTroops
{
    [HarmonyPatch(typeof(AiVisitSettlementBehavior), "ApproximateNumberOfVolunteersCanBeRecruitedFromSettlement")]
    public class EarnRecruitsPatch
    {
        private static void Postfix(ref int __result, Hero hero, Settlement settlement)
        {
            int num;

            if (settlement.Owner != null && settlement.Owner.Clan == hero.Clan)
            {
                num = 6;
            }
            else
            {
                num = (hero.MapFaction == settlement.MapFaction) ? 4 : 2;
            }

            int num2 = 0;

            var heroesWithoutParty = settlement.HeroesWithoutParty;
            var connectedFellows = (heroesWithoutParty != null)
                ? heroesWithoutParty.Where(h => h.Occupation == Occupation.Special).Concat(settlement.Notables)
                : settlement.Notables;

            foreach (var hero2 in connectedFellows)
            {
                int num3 = Campaign.Current.Models.VolunteerModel.MaximumIndexHeroCanRecruitFromHero(hero, hero2, -101);
                for (int num4 = 0; num4 < num3 && num4 < num; num4++)
                {
                    if (hero2.VolunteerTypes[num4] != null)
                    {
                        num2++;
                    }
                }
            }

            __result = num2;
        }
    }
}
