using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace LipottyTroops
{
    public class SoldierLimitBehavior : CampaignBehaviorBase
    {
        // 使用预设的兵种集合
        private List<TroopLimit> troopLimits = new List<TroopLimit>
        {
            new TroopLimit(
                "{=LRM_MOD_002}Regulars",
                TroopCollections.CommonTroops,
                SubModule.ModSettings.RegularsTownProsperity,
                SubModule.ModSettings.RegularsCastleProsperity,
                SubModule.ModSettings.RegularsVillageHouseholds
            ),
            new TroopLimit(
                "{=LRM_MOD_003}Nobles",
                TroopCollections.NobleTroops,
                SubModule.ModSettings.NoblesTownProsperity,
                SubModule.ModSettings.NoblesCastleProsperity,
                SubModule.ModSettings.NoblesVillageHouseholds
            ),
            new TroopLimit(
                "{=LRM_MOD_004}Elites",
                TroopCollections.EliteTroops,
                SubModule.ModSettings.ElitesTownProsperity,
                SubModule.ModSettings.ElitesCastleProsperity,
                SubModule.ModSettings.ElitesVillageHouseholds
            )
        };

        // 保存每个兵种集合的上限值
        private Dictionary<string, int> troopLimitValues = new Dictionary<string, int>();

        // 延迟移除任务队列
        private Queue<DelayedRemovalTask> delayedRemovalTasks = new Queue<DelayedRemovalTask>();

        public override void RegisterEvents()
        {
            // 注册每日事件，检查士兵数量是否超过上限
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, new Action(CheckSoldierLimits));
            // 注册每日事件，处理延迟移除任务
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, new Action(ProcessDelayedRemovalTasks));
        }

        public override void SyncData(IDataStore dataStore)
        {
            // 由于移除了动态保存功能，这里可以留空或移除
        }

        private void CheckSoldierLimits()
        {
            Clan playerClan = Clan.PlayerClan; // 获取玩家氏族
            if (playerClan == null)
            {
                return; // 如果玩家没有氏族，则跳过计算
            }

            // 获取玩家氏族的定居点和部队列表
            var settlements = playerClan.Settlements;
            var parties = GetClanParties(playerClan); // 获取氏族部队

            // 遍历每个兵种集合，检查是否超过上限
            foreach (var troopLimit in troopLimits)
            {
                // 强制更新 troopLimitValues
                int limit = CalculateTroopLimit(troopLimit, settlements);
                troopLimitValues[troopLimit.Name] = limit; // 更新上限值
                Debug.Print($"Updated troop limit for {troopLimit.Name}: {limit}");

                // 计算当前兵种集合的总士兵数量
                int totalSoldiers = CalculateTotalSoldiers(troopLimit.TroopIds, settlements, parties);
                Debug.Print($"Total soldiers for {troopLimit.Name}: {totalSoldiers}, Limit: {troopLimitValues[troopLimit.Name]}");

                // 如果士兵数量超过上限，则记录移除任务
                if (totalSoldiers > limit)
                {
                    int excessSoldiers = totalSoldiers - limit;
                    delayedRemovalTasks.Enqueue(new DelayedRemovalTask
                    {
                        TroopLimit = troopLimit,
                        ExcessSoldiers = excessSoldiers
                    });

                    // 显示警告信息
                    TextObject warningMessage = new TextObject("{=LRM_MOD_005}Your lands are unable to sustain the number of {TROOP_NAME}! Current: {TOTAL_SOLDIERS}, Limit: {LIMIT}. The excess troops will leave your forces by tomorrow.");
                    warningMessage.SetTextVariable("TROOP_NAME", troopLimit.Name);
                    warningMessage.SetTextVariable("TOTAL_SOLDIERS", totalSoldiers);
                    warningMessage.SetTextVariable("LIMIT", limit);
                    InformationManager.DisplayMessage(new InformationMessage(warningMessage.ToString(), new Color(1f, 0f, 0f)));
                    MBInformationManager.AddQuickInformation(warningMessage, 5, null, "");
                }
            }
        }

        private void ProcessDelayedRemovalTasks()
        {
            Clan playerClan = Clan.PlayerClan; // 获取玩家氏族
            if (playerClan == null)
            {
                return; // 如果玩家没有氏族，则跳过
            }

            // 获取玩家氏族的定居点和部队列表
            var settlements = playerClan.Settlements;
            var parties = GetClanParties(playerClan); // 获取氏族部队

            // 处理所有延迟移除任务
            while (delayedRemovalTasks.Count > 0)
            {
                var task = delayedRemovalTasks.Dequeue();
                int totalSoldiers = CalculateTotalSoldiers(task.TroopLimit.TroopIds, settlements, parties);
                int limit = troopLimitValues[task.TroopLimit.Name];

                // 如果士兵数量仍然超出上限，则执行移除操作
                if (totalSoldiers > limit)
                {
                    int excessSoldiers = totalSoldiers - limit;
                    RemoveExcessSoldiers(task.TroopLimit.TroopIds, excessSoldiers, task.TroopLimit);
                }
            }
        }

        private int CalculateTroopLimit(TroopLimit troopLimit, IEnumerable<Settlement> settlements)
        {
            int totalLimit = 0; // 初始化上限值为0

            // 遍历玩家氏族的每个定居点
            foreach (var settlement in settlements)
            {
                if (settlement == null)
                {
                    Debug.Print("Invalid settlement!"); // 跳过无效的定居点
                    continue;
                }

                if (settlement.IsTown)
                {
                    if (settlement.Town == null)
                    {
                        Debug.Print($"Invalid town for settlement: {settlement.Name}"); // 跳过无效的城镇
                        continue;
                    }

                    // 如果是城镇，按城镇繁荣度计算上限
                    int limit = (int)(settlement.Town.Prosperity / troopLimit.TownProsperityPerLimit);
                    Debug.Print($"Settlement: {settlement.Name}, Town Prosperity: {settlement.Town.Prosperity}, Limit Contribution: {limit}");
                    totalLimit += limit;
                }
                else if (settlement.IsCastle)
                {
                    if (settlement.Town == null)
                    {
                        Debug.Print($"Invalid town for settlement: {settlement.Name}"); // 跳过无效的城堡
                        continue;
                    }

                    // 如果是城堡，按城堡繁荣度计算上限
                    int limit = (int)(settlement.Town.Prosperity / troopLimit.CastleProsperityPerLimit);
                    Debug.Print($"Settlement: {settlement.Name}, Castle Prosperity: {settlement.Town.Prosperity}, Limit Contribution: {limit}");
                    totalLimit += limit;
                }
                else if (settlement.IsVillage)
                {
                    if (settlement.Village == null)
                    {
                        Debug.Print($"Invalid village for settlement: {settlement.Name}"); // 跳过无效的村庄
                        continue;
                    }

                    // 如果是村庄，按户数计算上限
                    int limit = (int)(settlement.Village.Hearth / troopLimit.HearthPerLimit);
                    Debug.Print($"Settlement: {settlement.Name}, Village Hearth: {settlement.Village.Hearth}, Limit Contribution: {limit}");
                    totalLimit += limit;
                }
            }

            Debug.Print($"Total limit: {totalLimit}");
            return totalLimit; // 返回计算后的上限值
        }

        private int CalculateTotalSoldiers(List<string> troopIds, IEnumerable<Settlement> settlements, IEnumerable<MobileParty> parties)
        {
            int total = 0; // 初始化士兵数量

            // 1. 计算玩家主部队中的士兵数量
            foreach (var troopId in troopIds)
            {
                CharacterObject troop = MBObjectManager.Instance.GetObject<CharacterObject>(troopId);
                if (troop == null)
                {
                    Debug.Print($"Troop ID {troopId} not found!"); // 跳过无效的兵种
                    continue;
                }

                int count = MobileParty.MainParty.MemberRoster.GetTroopCount(troop);
                Debug.Print($"Troop: {troop.Name}, Count in Main Party: {count}");
                total += count;
            }

            // 2. 计算氏族部队中的士兵数量
            foreach (var party in parties)
            {
                if (party == null || party.MemberRoster == null)
                {
                    Debug.Print("Invalid party or roster!"); // 跳过无效的部队
                    continue;
                }

                foreach (var troopId in troopIds)
                {
                    CharacterObject troop = MBObjectManager.Instance.GetObject<CharacterObject>(troopId);
                    if (troop == null)
                    {
                        Debug.Print($"Troop ID {troopId} not found!"); // 跳过无效的兵种
                        continue;
                    }

                    int count = party.MemberRoster.GetTroopCount(troop);
                    Debug.Print($"Troop: {troop.Name}, Count in Clan Party: {count}");
                    total += count;
                }
            }

            // 3. 计算氏族定居点驻军中的士兵数量
            foreach (var settlement in settlements)
            {
                if (settlement == null)
                {
                    Debug.Print("Invalid settlement!"); // 跳过无效的定居点
                    continue;
                }

                if (settlement.IsTown || settlement.IsCastle)
                {
                    if (settlement.Town == null || settlement.Town.GarrisonParty == null)
                    {
                        Debug.Print($"Invalid town or garrison party for settlement: {settlement.Name}"); // 跳过无效的驻军
                        continue;
                    }

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
            }

            Debug.Print($"Total soldiers: {total}");
            return total; // 返回士兵数量
        }

        private void RemoveExcessSoldiers(List<string> troopIds, int excessSoldiers, TroopLimit troopLimit)
        {
            // 按优先级排序兵种
            List<CharacterObject> sortedTroops = SortTroopsByPriority(troopIds);

            // 记录离队的士兵数量
            int totalRemoved = 0;

            // 按照优先级移除士兵
            while (excessSoldiers > 0)
            {
                // 1. 移除定居点驻兵
                int removedFromSettlements = RemoveTroopsFromSettlements(sortedTroops, ref excessSoldiers);
                totalRemoved += removedFromSettlements;
                Debug.Print($"Removed {removedFromSettlements} troops from settlements.");
                if (excessSoldiers <= 0) break;

                // 2. 移除玩家氏族部队
                int removedFromClanParties = RemoveTroopsFromClanParties(sortedTroops, ref excessSoldiers);
                totalRemoved += removedFromClanParties;
                Debug.Print($"Removed {removedFromClanParties} troops from clan parties.");
                if (excessSoldiers <= 0) break;

                // 3. 移除玩家主部队
                int removedFromMainParty = RemoveTroopsFromMainParty(sortedTroops, ref excessSoldiers);
                totalRemoved += removedFromMainParty;
                Debug.Print($"Removed {removedFromMainParty} troops from main party.");
                if (excessSoldiers <= 0) break;

                break; // 如果所有优先级都处理完毕，则退出循环
            }

            Debug.Print($"Total removed: {totalRemoved}");
            if (totalRemoved > 0)
            {
                // 加载本地化文本
                TextObject message = new TextObject("{=LRM_MOD_006}Your domain cannot sustain so many {TROOP_NAME}, and {REMOVED_COUNT} {TROOP_NAME} have left your forces.");

                // 替换占位符
                message.SetTextVariable("TROOP_NAME", troopLimit.Name);    // 替换 {TROOP_NAME}
                message.SetTextVariable("REMOVED_COUNT", totalRemoved);    // 替换 {REMOVED_COUNT}

                // 显示消息（确保只显示一次）
                InformationManager.DisplayMessage(new InformationMessage(message.ToString(), new Color(1f, 0f, 0f)));
                MBInformationManager.AddQuickInformation(message, 5, null, "");
            }
        }

        private int RemoveTroopsFromSettlements(List<CharacterObject> sortedTroops, ref int excessSoldiers)
        {
            int removedCount = 0; // 初始化移除的士兵数量
            Clan playerClan = Clan.PlayerClan; // 获取玩家氏族
            if (playerClan != null)
            {
                // 遍历玩家氏族的每个定居点
                foreach (var settlement in playerClan.Settlements)
                {
                    if (settlement == null || settlement.Town == null || settlement.Town.GarrisonParty == null)
                    {
                        Debug.Print("Invalid settlement or garrison party!"); // 跳过无效的定居点
                        continue;
                    }

                    // 遍历排序后的兵种列表
                    foreach (var troop in sortedTroops)
                    {
                        // 如果定居点驻兵中有该兵种，则移除
                        while (excessSoldiers > 0 && settlement.Town.GarrisonParty.MemberRoster.GetTroopCount(troop) > 0)
                        {
                            settlement.Town.GarrisonParty.MemberRoster.RemoveTroop(troop, 1); // 移除1个士兵
                            excessSoldiers--; // 减少超出部分的士兵数量
                            removedCount++; // 增加移除的士兵数量
                        }
                        if (excessSoldiers <= 0) break; // 如果超出部分的士兵数量为0，则退出循环
                    }
                    if (excessSoldiers <= 0) break; // 如果超出部分的士兵数量为0，则退出循环
                }
            }
            return removedCount; // 返回移除的士兵数量
        }

        private int RemoveTroopsFromClanParties(List<CharacterObject> sortedTroops, ref int excessSoldiers)
        {
            int removedCount = 0; // 初始化移除的士兵数量
            Clan playerClan = Clan.PlayerClan; // 获取玩家氏族
            if (playerClan != null)
            {
                // 遍历玩家氏族的每个部队
                foreach (var party in GetClanParties(playerClan))
                {
                    if (party == null || party.MemberRoster == null)
                    {
                        Debug.Print("Invalid party or roster!"); // 跳过无效的部队
                        continue;
                    }

                    // 遍历排序后的兵种列表
                    foreach (var troop in sortedTroops)
                    {
                        // 如果部队中有该兵种，则移除
                        while (excessSoldiers > 0 && party.MemberRoster.GetTroopCount(troop) > 0)
                        {
                            party.MemberRoster.RemoveTroop(troop, 1); // 移除1个士兵
                            excessSoldiers--; // 减少超出部分的士兵数量
                            removedCount++; // 增加移除的士兵数量
                        }
                        if (excessSoldiers <= 0) break; // 如果超出部分的士兵数量为0，则退出循环
                    }
                    if (excessSoldiers <= 0) break; // 如果超出部分的士兵数量为0，则退出循环
                }
            }
            return removedCount; // 返回移除的士兵数量
        }

        private int RemoveTroopsFromMainParty(List<CharacterObject> sortedTroops, ref int excessSoldiers)
        {
            int removedCount = 0; // 初始化移除的士兵数量
            // 遍历排序后的兵种列表
            foreach (var troop in sortedTroops)
            {
                // 如果玩家主部队中有该兵种，则移除
                while (excessSoldiers > 0 && MobileParty.MainParty.MemberRoster.GetTroopCount(troop) > 0)
                {
                    MobileParty.MainParty.MemberRoster.RemoveTroop(troop, 1); // 移除1个士兵
                    excessSoldiers--; // 减少超出部分的士兵数量
                    removedCount++; // 增加移除的士兵数量
                }
                if (excessSoldiers <= 0) break; // 如果超出部分的士兵数量为0，则退出循环
            }
            return removedCount; // 返回移除的士兵数量
        }

        private List<CharacterObject> SortTroopsByPriority(List<string> troopIds)
        {
            // 获取所有兵种对象
            List<CharacterObject> troops = new List<CharacterObject>();
            foreach (var troopId in troopIds)
            {
                CharacterObject troop = MBObjectManager.Instance.GetObject<CharacterObject>(troopId);
                if (troop != null)
                {
                    troops.Add(troop); // 将兵种对象添加到列表中
                }
            }

            // 排序逻辑：兵种类型 > 等级
            troops.Sort((a, b) =>
            {
                // 兵种类型优先级：步兵 > 射手 > 骑射手 > 骑兵
                int priorityA = GetTroopPriority(a);
                int priorityB = GetTroopPriority(b);
                if (priorityA != priorityB)
                {
                    return priorityA.CompareTo(priorityB); // 按兵种类型排序
                }

                // 等级优先级：低级 > 高级
                return a.Tier.CompareTo(b.Tier); // 按等级排序
            });

            return troops; // 返回排序后的兵种列表
        }

        private int GetTroopPriority(CharacterObject troop)
        {
            // 根据兵种类型返回优先级
            switch (troop.DefaultFormationClass)
            {
                case FormationClass.Infantry:
                    return 1; // 步兵优先级最高
                case FormationClass.Ranged:
                    return 2; // 射手
                case FormationClass.HorseArcher:
                    return 3; // 骑射手
                case FormationClass.Cavalry:
                    return 4; // 骑兵
                default:
                    return 5; // 其他
            }
        }

        private IEnumerable<MobileParty> GetClanParties(Clan clan)
        {
            List<MobileParty> parties = new List<MobileParty>();

            // 遍历氏族的领主，获取他们的部队
            foreach (var lord in clan.Lords)
            {
                if (lord.PartyBelongedTo != null && lord.PartyBelongedTo != MobileParty.MainParty)
                {
                    parties.Add(lord.PartyBelongedTo);
                }
            }

            // 获取玩家氏族的其他部队（如巡逻队、商队等）
            foreach (var party in clan.WarPartyComponents)
            {
                if (party != null && party.MobileParty != null && party.MobileParty != MobileParty.MainParty)
                {
                    parties.Add(party.MobileParty);
                }
            }

            return parties;
        }
    }

    // 延迟移除任务类
    public class DelayedRemovalTask
    {
        public TroopLimit TroopLimit { get; set; } // 需要移除的兵种集合
        public int ExcessSoldiers { get; set; }    // 超出上限的士兵数量
    }
}
