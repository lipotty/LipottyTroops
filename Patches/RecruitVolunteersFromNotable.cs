using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace LipottyTroops.Patches
{
    [HarmonyPatch]
    public class RecruitVolunteersFromNotable
    {
        private static List<TroopLimit> troopLimits;
        private static Dictionary<string, int> troopLimitValues;
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("RecruitmentCampaignBehavior:RecruitVolunteersFromNotable", new Type[] { typeof(MobileParty), typeof(Settlement) }, null);
        }
        public static bool Prefix(MobileParty mobileParty, Settlement settlement)
        {
            // 检查队伍成员数量是否超过上限
            if ((mobileParty.Party.NumberOfAllMembers + 0.5f) / mobileParty.Party.PartySizeLimit <= 1f)
            {
                // 遍历定居点中的知名人物
                foreach (var hero in settlement.Notables)
                {
                    if (hero.IsAlive)
                    {
                        // 检查队伍工资是否超过限制
                        if (mobileParty.IsWageLimitExceeded())
                        {
                            break;
                        }

                        // 随机生成一个起始索引
                        int startIndex = MBRandom.RandomInt(6);
                        int maxIndex = Campaign.Current.Models.VolunteerModel.MaximumIndexHeroCanRecruitFromHero(
                            mobileParty.IsGarrison ? mobileParty.Party.Owner : mobileParty.LeaderHero,
                            hero,
                            -101
                        );

                        // 遍历6个志愿者类型
                        for (int i = startIndex; i < startIndex + 6; i++)
                        {
                            int troopIndex = i % 6;
                            if (troopIndex >= maxIndex)
                            {
                                break;
                            }

                            // 获取当前格子的兵种
                            var characterObject = hero.VolunteerTypes[troopIndex];
                            if (characterObject == null)
                            {
                                continue; // 如果兵种为空，跳过
                            }

                            // 判断兵种是否属于某个集合
                            bool isInCollection = TroopCollections.CommonTroops.Contains(characterObject.StringId) ||
                                                  TroopCollections.NobleTroops.Contains(characterObject.StringId) ||
                                                  TroopCollections.EliteTroops.Contains(characterObject.StringId);

                            // 如果兵种属于某个集合，则检查集合是否达到上限
                            if (isInCollection)
                            {
                                bool isAtLimit = false;

                                // 遍历所有兵种集合
                                foreach (var troopLimit in troopLimits)
                                {
                                    // 检查当前兵种是否属于该集合
                                    if (troopLimit.TroopIds.Contains(characterObject.StringId))
                                    {
                                        // 获取该集合的上限值
                                        if (troopLimitValues.TryGetValue(troopLimit.Name, out int limit))
                                        {
                                            // 计算当前集合的士兵总数
                                            int totalSoldiers = CalculateTotalSoldiers(troopLimit.TroopIds, mobileParty, settlement);
                                            Debug.Print($"Total soldiers for {troopLimit.Name}: {totalSoldiers}, Limit: {limit}");

                                            // 如果士兵数量超过上限，标记为达到上限
                                            if (totalSoldiers >= limit)
                                            {
                                                isAtLimit = true;
                                            }
                                        }
                                        break; // 如果找到匹配的集合，退出循环
                                    }
                                }

                                // 如果集合达到上限，跳过当前兵种
                                if (isAtLimit)
                                {
                                    continue;
                                }
                            }

                            // 计算招募概率
                            int wealthFactor = mobileParty.LeaderHero != null ? (int)MathF.Sqrt(mobileParty.LeaderHero.Gold / 10000f) : 0;
                            float recruitProbability = MBRandom.RandomFloat;

                            for (int j = 0; j < wealthFactor; j++)
                            {
                                float randomFloat = MBRandom.RandomFloat;
                                if (randomFloat > recruitProbability)
                                {
                                    recruitProbability = randomFloat;
                                }
                            }

                            // 如果队伍属于军队，调整招募概率
                            if (mobileParty.Army != null)
                            {
                                float armyFactor = mobileParty.Army.LeaderParty == mobileParty ? 0.5f : 0.67f;
                                recruitProbability = MathF.Pow(recruitProbability, armyFactor);
                            }

                            // 检查是否满足招募条件
                            float partySizeRatio = (float)mobileParty.Party.NumberOfAllMembers / mobileParty.Party.PartySizeLimit;
                            if (recruitProbability > partySizeRatio - 0.1f)
                            {
                                if (characterObject != null &&
                                    mobileParty.LeaderHero.Gold > Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(characterObject, mobileParty.LeaderHero, false) &&
                                    mobileParty.PaymentLimit >= mobileParty.TotalWage + Campaign.Current.Models.PartyWageModel.GetCharacterWage(characterObject))
                                {
                                    // 执行招募逻辑
                                    GetRecruitVolunteerFromIndividual(mobileParty, characterObject, hero, troopIndex);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            // 返回 false 以跳过原方法
            return false;
        }
        public static void GetRecruitVolunteerFromIndividual(MobileParty mobileParty, CharacterObject character, Hero hero, int troopIndex)
        {
            // 这里是招募志愿者的具体逻辑
            // 你可以根据需要实现或调用原游戏的方法
            // 例如：mobileParty.AddElementToMemberRoster(character, 1);
        }

        public static int CalculateTotalSoldiers(List<string> troopIds, MobileParty mobileParty, Settlement settlement)
        {
            int total = 0;

            // 1. 计算玩家主部队中的士兵数量
            foreach (var troopId in troopIds)
            {
                CharacterObject troop = MBObjectManager.Instance.GetObject<CharacterObject>(troopId);
                if (troop == null)
                {
                    Debug.Print($"Troop ID {troopId} not found!"); // 跳过无效的兵种
                    continue;
                }

                int count = mobileParty.MemberRoster.GetTroopCount(troop);
                Debug.Print($"Troop: {troop.Name}, Count in Main Party: {count}");
                total += count;
            }

            // 2. 计算定居点驻军中的士兵数量
            if (settlement != null && settlement.IsTown)
            {
                foreach (var troopId in troopIds)
                {
                    CharacterObject troop = MBObjectManager.Instance.GetObject<CharacterObject>(troopId);
                    if (troop == null)
                    {
                        Debug.Print($"Troop ID {troopId} not found!"); // 跳过无效的兵种
                        continue;
                    }

                    int count = settlement.Town.GarrisonParty.MemberRoster.GetTroopCount(troop);
                    Debug.Print($"Troop: {troop.Name}, Count in Garrison: {count}");
                    total += count;
                }
            }

            Debug.Print($"Total soldiers: {total}");
            return total;
        }
    }
}