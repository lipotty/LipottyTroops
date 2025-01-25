using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdonnaysTroopChanger.Models;
using HarmonyLib;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;

namespace LipottyTroops
{
    [HarmonyPatch(typeof(ATCVolunteerProductionModel), "MaximumIndexHeroCanRecruitFromHero")]
    [HarmonyPriority(Priority.High)]
    public class CompaniesNumberPatch
    {
        public static bool Prefix(Hero buyerHero, Hero sellerHero, int useValueAsRelation, ref int __result)
        {
            int num = (buyerHero.IsMinorFactionHero) ? 2 : -1;
            int num2 = (buyerHero == Hero.MainHero) ? Campaign.Current.Models.DifficultyModel.GetPlayerRecruitSlotBonus() + SubModule.ModSettings.RecruitDifficulty - 1 : 0;
            int num3 = 0;
            Settlement currentSettlement = sellerHero.CurrentSettlement;
            int num4;
            if (currentSettlement != null && buyerHero.MapFaction == currentSettlement.MapFaction)
            {
                if (buyerHero == Hero.MainHero)
                {
                    int buyerClanTier = buyerHero.Clan?.Tier ?? 0;
                    num4 = (buyerClanTier <= 3) ? 2 : 3;
                }
                else
                {
                    num4 = 3;
                }
            }
            else
            {
                num4 = 0;
            }
            int num5 = (currentSettlement != null && buyerHero.MapFaction.IsAtWarWith(currentSettlement.MapFaction)) ? -6 : 0;
            if (buyerHero.IsMinorFactionHero && currentSettlement != null && currentSettlement.IsVillage)
            {
                num5 = 0;
            }
            int num6 = (useValueAsRelation < -100) ? buyerHero.GetRelation(sellerHero) : useValueAsRelation;
            int num7 = (num6 >= 90) ? 3 :
                       (num6 >= 50) ? 2 :
                       (num6 >= 20) ? 1 :
                       (num6 >= 0) ? 0 :
                       (num6 >= -20) ? -1 :
                       (num6 >= -40) ? -2 :
                       (num6 >= -60) ? -3 :
                       (num6 >= -80) ? -4 : -5;
            int num8 = 0;
            if (sellerHero.IsGangLeader && currentSettlement.OwnerClan == buyerHero.Clan)
            {
                if (currentSettlement.IsTown)
                {
                    var governor = currentSettlement.Town.Governor;
                    if (governor != null && governor.GetPerkValue(DefaultPerks.Roguery.OneOfTheFamily))
                    {
                        num8 += (int)DefaultPerks.Roguery.OneOfTheFamily.SecondaryBonus;
                    }
                }
                else if (currentSettlement.IsVillage)
                {
                    var governor = currentSettlement.Village.Bound.Town.Governor;
                    if (governor != null && governor.GetPerkValue(DefaultPerks.Roguery.OneOfTheFamily))
                    {
                        num8 += (int)DefaultPerks.Roguery.OneOfTheFamily.SecondaryBonus;
                    }
                }
            }
            if (sellerHero.IsMerchant && buyerHero.GetPerkValue(DefaultPerks.Trade.ArtisanCommunity))
            {
                num8 += (int)DefaultPerks.Trade.ArtisanCommunity.SecondaryBonus;
            }
            if (sellerHero.Culture == buyerHero.Culture && buyerHero.GetPerkValue(DefaultPerks.Leadership.CombatTips))
            {
                num8 += (int)DefaultPerks.Leadership.CombatTips.SecondaryBonus;
            }
            if (sellerHero.IsRuralNotable && buyerHero.GetPerkValue(DefaultPerks.Charm.Firebrand))
            {
                num8 += (int)DefaultPerks.Charm.Firebrand.SecondaryBonus;
            }
            if (sellerHero.IsUrbanNotable && buyerHero.GetPerkValue(DefaultPerks.Charm.FlexibleEthics))
            {
                num8 += (int)DefaultPerks.Charm.FlexibleEthics.SecondaryBonus;
            }
            if (sellerHero.IsArtisan && buyerHero.PartyBelongedTo != null && buyerHero.PartyBelongedTo.EffectiveEngineer != null && buyerHero.PartyBelongedTo.EffectiveEngineer.GetPerkValue(DefaultPerks.Engineering.EngineeringGuilds))
            {
                num8 += (int)DefaultPerks.Engineering.EngineeringGuilds.PrimaryBonus;
            }
            int num9 = (buyerHero.Clan == currentSettlement.OwnerClan) ? 5 : 0;
            int num10 = 0;
            if (buyerHero == Hero.MainHero && currentSettlement != null)
            {
                int relationWithOwner = buyerHero.GetRelation(currentSettlement.Owner);
                num10 = (relationWithOwner < -80) ? -5 :
                        (relationWithOwner < -60) ? -4 :
                        (relationWithOwner < -40) ? -3 :
                        (relationWithOwner < -20) ? -2 :
                        (relationWithOwner < -10) ? -1 :
                        (relationWithOwner < 20) ? 0 :
                        (relationWithOwner < 50) ? 1 :
                        (relationWithOwner < 90) ? 2 : 3;
            }
            __result = Math.Min(6, Math.Max(-1, num + num2 + num3 + num4 + num5 + num7 + num8 + num9 + num10));
            return false;
        }
    }
}
