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
using static LipottyTroops.MercenaryLimit;
using Helpers;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Encounters;

namespace LipottyTroops
{
    public class SoldierLimitBehavior : CampaignBehaviorBase
    {
        // 使用预设的兵种集合
        private List<TroopLimit> troopLimits = new List<TroopLimit>
        {
            new TroopLimit(
                "{=LRM_MOD_002}Regulars",
                TroopCollections.RegularTroops,
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

        // 添加佣兵、强盗上限实例
        private readonly MercenaryLimit _mercenaryLimit;
        private readonly BanditLimit _banditLimit;
        private bool _banditExceededYesterday = false;
        private bool _banditExceededToday = false;
        private int _todayBanditCount;
        private int _todayBanditLimit;


        public SoldierLimitBehavior()
        {
            // 使用SubModule.ModSettings传入设置
            _mercenaryLimit = new MercenaryLimit(
                TroopCollections.MercenaryTroops,
                SubModule.ModSettings
            );
            _banditLimit = new BanditLimit(
                TroopCollections.BanditTroops,
                SubModule.ModSettings
            );
        }

        // 保存每个兵种集合的上限值
        private Dictionary<string, int> troopLimitValues = new Dictionary<string, int>();

        // 延迟移除任务队列
        private Queue<DelayedRemovalTask> delayedRemovalTasks = new Queue<DelayedRemovalTask>();

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, () =>
            {
                CheckSoldierLimits();
                ShowDailyTroopStatus(); // 新增每日状态报告
                CheckMercenaryLimits(); // 新增佣兵检查
                CheckBanditLimits(); // 新增强盗检查
                ProcessBanditEvents(); // 处理强盗事件盒
            });
            // // 注册每日事件，处理延迟移除任务
            // CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, new Action(ProcessDelayedRemovalTasks));
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
            //整合两个事件
            var currentLength = delayedRemovalTasks.Count;
            for (var i = 0; i < currentLength; i++)
            {
                var task = delayedRemovalTasks.Dequeue();
                int totalSoldiers = CalculateTotalSoldiers(task.TroopLimit.TroopIds, settlements, parties);
                int limit = troopLimitValues[task.TroopLimit.Name];
                if (totalSoldiers > limit)
                {
                    int excessSoldiers = totalSoldiers - limit;
                    RemoveExcessSoldiers(task.TroopLimit.TroopIds, excessSoldiers, task.TroopLimit);
                }
            }
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
                    MBInformationManager.AddQuickInformation(warningMessage, 5, null, null, "");
                }
            }
        }

        // 佣兵检查方法
        private void CheckMercenaryLimits()
        {
            if (Clan.PlayerClan == null) return;

            int playerTier = Clan.PlayerClan.Tier;
            int leadershipSkill = Hero.MainHero.GetSkillValue(DefaultSkills.Leadership);

            // 传入家族等级和统御技能值
            int mercenaryLimit = _mercenaryLimit.GetLimitForTier(
                playerTier,
                leadershipSkill
            );

            var settlements = Clan.PlayerClan.Settlements;
            var parties = GetClanParties(Clan.PlayerClan);
            int totalMercenaries = CalculateTotalSoldiers(_mercenaryLimit.TroopIds, settlements, parties);

            if (totalMercenaries <= mercenaryLimit) return;

            // 计算超额数量
            int excess = totalMercenaries - mercenaryLimit;
            // 累进计算维护费: 1+2+3+...+n = n(n+1)/2
            int maintenanceFee = excess * (excess + 1) / 2;

            int playerGold = Hero.MainHero.Gold;

            // 计算实际能扣除的金额（不超过玩家当前金币数）
            int actualPayment = Math.Min(maintenanceFee, playerGold);

            // 扣钱
            Hero.MainHero.ChangeHeroGold(-maintenanceFee);

            // 如果维护费大于玩家金钱，应用士气惩罚
            if (maintenanceFee > playerGold)
            {
                // 计算未支付金额比例
                float unpaidRatio = 1f - (float)actualPayment / (float)maintenanceFee;

                // 获取每日无工资士气惩罚模型
                var moraleModel = Campaign.Current.Models.PartyMoraleModel;

                // 计算士气惩罚
                float moralePenalty = unpaidRatio * moraleModel.GetDailyNoWageMoralePenalty(MobileParty.MainParty);

                // 应用士气惩罚
                MobileParty.MainParty.RecentEventsMorale += moralePenalty;

                // 显示士气惩罚信息
                MBInformationManager.AddQuickInformation(GameTexts.FindText("str_party_loses_moral_due_to_insufficent_funds", null), 0, null, null, "");

                // 设置未支付工资比例（模拟原版逻辑）
                MobileParty.MainParty.HasUnpaidWages = unpaidRatio;
            }
            else
            {
                // 如果全额支付，清除未支付标记
                MobileParty.MainParty.HasUnpaidWages = 0f;
            }

            // 显示扣费信息
            TextObject message = new TextObject("{=LRM_MOD_012}You have {EXCESS} mercenaries exceeding the limit {LIMIT}, and {FINE} denars has been deducted as maintenance fees.(calculated using the formula: n(n+1)/2, where n = {EXCESS})");
            message.SetTextVariable("EXCESS", excess);
            message.SetTextVariable("LIMIT", mercenaryLimit);
            message.SetTextVariable("FINE", maintenanceFee);
            InformationManager.DisplayMessage(new InformationMessage(message.ToString(), new Color(1f, 0f, 0f)));
        }

        // 新增强盗检查方法
        private void CheckBanditLimits()
        {
            if (Clan.PlayerClan == null) return;

            int leadershipSkill = Hero.MainHero.GetSkillValue(DefaultSkills.Leadership);
            _todayBanditLimit = _banditLimit.GetLimit(leadershipSkill);

            // 仅计算玩家主部队中的强盗数量
            _todayBanditCount = 0;
            foreach (var troopId in _banditLimit.TroopIds)
            {
                CharacterObject troop = MBObjectManager.Instance.GetObject<CharacterObject>(troopId);
                if (troop != null)
                {
                    _todayBanditCount += MobileParty.MainParty.MemberRoster.GetTroopCount(troop);
                }
            }

            // 每日状态报告（绿色）
            TextObject status = new TextObject("{=LRM_MOD_014}Bandit Limit: {LIMIT}, Current: {CURRENT}.");
            status.SetTextVariable("LIMIT", _todayBanditLimit);
            status.SetTextVariable("CURRENT", _todayBanditCount);
            InformationManager.DisplayMessage(new InformationMessage(status.ToString(), new Color(0f, 1f, 0f)));

            // 更新今日状态标志
            if (_todayBanditCount > _todayBanditLimit)
            {
                // 新增：计算事件触发概率
                Settings settings = Settings.Instance;
                int roguerySkill = Hero.MainHero.GetSkillValue(DefaultSkills.Roguery);
                float eventChance = Math.Max(
                    0.01f,
                    settings.BanditBaseEventChance - (roguerySkill * settings.BanditRogueryReduction)
                );
                string chancePercent = (eventChance * 100).ToString("0.0"); // 格式化为百分比

                // 超过上限的警告
                TextObject warning = new TextObject("{=LRM_MOD_015}You cannot control so many bandits! Limit: {LIMIT}, Current: {CURRENT}.According to your roguery skill, from now on there will be a {CHANCE}% chance of triggering a villain event every day!");
                warning.SetTextVariable("LIMIT", _todayBanditLimit);
                warning.SetTextVariable("CURRENT", _todayBanditCount);
                warning.SetTextVariable("CHANCE", chancePercent);
                MBInformationManager.AddQuickInformation(warning, 5, null, null, "");
                InformationManager.DisplayMessage(new InformationMessage(warning.ToString(), new Color(1f, 0f, 0f)));

                _banditExceededToday = true;  // 标记今日超出
            }
            else
            {
                _banditExceededToday = false; // 标记今日未超出
            }
        }

        // 处理事件盒
        private void ProcessBanditEvents()
        {
            // 获取配置实例
            Settings settings = Settings.Instance;
            // 检查昨日是否超出
            if (_banditExceededYesterday)
            {
                // 检查今日是否仍然超出
                if (_banditExceededToday && _todayBanditCount > _todayBanditLimit)
                {
                    int roguerySkill = Hero.MainHero.GetSkillValue(DefaultSkills.Roguery);
                    // 使用配置值
                    float eventChance = Math.Max(
                        0.01f,
                        settings.BanditBaseEventChance - (roguerySkill * settings.BanditRogueryReduction)
                    );

                    if (MBRandom.RandomFloat < eventChance)
                    {
                        // 检查叛乱条件
                        if (_todayBanditCount > 50 &&
                            _todayBanditCount > MobileParty.MainParty.MemberRoster.TotalManCount / 2 &&
                            Settlement.CurrentSettlement == null &&
                            PlayerEncounter.Current == null &&
                            !Hero.MainHero.IsWounded)
                        {
                            ShowBanditRebellionMenu();
                        }
                        else
                        {
                            TriggerBanditIncident();
                        }
                    }
                }
            }

            // 更新昨日状态
            _banditExceededYesterday = _banditExceededToday;
        }

        // 第一事件盒：强盗叛乱
        private void ShowBanditRebellionMenu()
        {
            InquiryData inquiry = new InquiryData(
                new TextObject("{=LRM_MOD_021}Bandit Rebellion!").ToString(), // 标题
                new TextObject("{=LRM_MOD_022}Your bandit troops have rebelled (bandits > 50 and > 50% of your party)! They demand to leave with your possessions (90% of your gold, up to 100,000), or they will attack you.").ToString(),
                true,
                true,
                new TextObject("{=LRM_MOD_023}Fight").ToString(), // 战斗按钮
                new TextObject("{=LRM_MOD_024}Pay to Resolve").ToString(), // 花钱消灾按钮
                () => StartBanditRebellionBattle(),  
                () => SurrenderToBandits(),          
                null
            );
            InformationManager.ShowInquiry(inquiry, true);
        }
        private void StartBanditRebellionBattle()
        {
            // 1. 创建叛军（在玩家位置）
            Settlement hideout = SettlementHelper.FindRandomSettlement(s => s.IsHideout);

            // 获取一个强盗部队模板
            PartyTemplateObject banditTemplate = MBObjectManager.Instance.GetObject<PartyTemplateObject>("looters_template"); 

            // 创建CampaignVec2位置（使用玩家当前位置）
            CampaignVec2 initialPosition = new CampaignVec2(MobileParty.MainParty.Position.ToVec2(), true);

            MobileParty rebelParty = BanditPartyComponent.CreateBanditParty(
                "rebel_bandits_" + CampaignTime.Now.ToString(),
                hideout.OwnerClan,
                hideout.Hideout,
                false, // 不是Boss队伍
                banditTemplate, // 新增：部队模板
                initialPosition // 新增：初始位置
            );

            // 2. 立即清空队伍（确保初始为空）
            rebelParty.MemberRoster.Clear();

            // 2. 转移兵种
            foreach (var troopId in _banditLimit.TroopIds)
            {
                CharacterObject troop = MBObjectManager.Instance.GetObject<CharacterObject>(troopId);
                if (troop != null)
                {
                    int count = MobileParty.MainParty.MemberRoster.GetTroopCount(troop);
                    if (count > 0)
                    {
                        rebelParty.MemberRoster.AddToCounts(troop, count);
                        MobileParty.MainParty.MemberRoster.AddToCounts(troop, -count);
                    }
                }
            }

            // 3. 直接开始战斗（使用原版系统）
            PlayerEncounter.Start();
            PlayerEncounter.Current.SetupFields(rebelParty.Party, PartyBase.MainParty); // 叛军攻击玩家

            // 开始战斗
            PlayerEncounter.StartBattle();
        }

        // 投降处理
        private void SurrenderToBandits()
        {
            // 1. 移除所有强盗兵种
            foreach (var troopId in _banditLimit.TroopIds)
            {
                CharacterObject troop = MBObjectManager.Instance.GetObject<CharacterObject>(troopId);
                if (troop != null)
                {
                    int count = MobileParty.MainParty.MemberRoster.GetTroopCount(troop);
                    MobileParty.MainParty.MemberRoster.AddToCounts(troop, -count);
                }
            }

            // 2. 金钱惩罚（上限100000）
            int goldLoss = (int)Math.Min(Hero.MainHero.Gold * 0.9f, 100000);
            Hero.MainHero.ChangeHeroGold(-goldLoss);

            // 3. 显示消息
            TextObject message = new TextObject("{=LRM_MOD_025}The bandits left with your possessions! You lost {GOLD} denars.");
            message.SetTextVariable("GOLD", goldLoss);

            // 弹出顶部中央警告框（持续5秒）
            MBInformationManager.AddQuickInformation(message, 5, null, null, "");

            // 同时在底部显示红字消息
            InformationManager.DisplayMessage(new InformationMessage(
                message.ToString(),
                new Color(1f, 0f, 0f)
            ));
        }

        // 第二事件盒：随机事件
        private void TriggerBanditIncident()
        {
            int eventType = MBRandom.RandomInt(4);
            TextObject message = TextObject.GetEmpty();

            switch (eventType)
            {
                case 0: // 丢失物品
                    if (MobileParty.MainParty.ItemRoster.Count > 0)
                    {
                        int index = MBRandom.RandomInt(MobileParty.MainParty.ItemRoster.Count);
                        ItemRosterElement item = MobileParty.MainParty.ItemRoster[index];
                        MobileParty.MainParty.ItemRoster.AddToCounts(item.EquipmentElement.Item, -1);

                        RemoveRandomBandits(1, 2);
                        message = new TextObject("{=LRM_MOD_017}Bandits stole an item and escaped!");
                    }
                    break;

                case 1: // 丢失金钱
                    if (Hero.MainHero.Gold > 1000)
                    {
                        int goldLost = MBRandom.RandomInt(100, 1001);
                        Hero.MainHero.ChangeHeroGold(-goldLost);
                        RemoveRandomBandits(1, 2);
                        message = new TextObject("{=LRM_MOD_018}Bandits stole {GOLD} denars and escaped!")
                            .SetTextVariable("GOLD", goldLost);
                    }
                    break;

                case 2: // 士兵逃跑
                    RemoveRandomBandits(1, 2);
                    message = new TextObject("{=LRM_MOD_019}Some bandits have deserted!");
                    break;

                case 3: // 士气下降
                    int moraleLoss = MBRandom.RandomInt(10, 25);
                    MobileParty.MainParty.RecentEventsMorale -= moraleLoss;
                    message = new TextObject("{=LRM_MOD_020}Bandits caused trouble! Morale -{LOSS}.")
                        .SetTextVariable("LOSS", moraleLoss);
                    break;
            }

            if (message != null && !string.IsNullOrWhiteSpace(message.ToString()))
            {
                MBInformationManager.AddQuickInformation(message, 5, null, null, "");
                InformationManager.DisplayMessage(new InformationMessage(message.ToString(), new Color(1f, 0f, 0f)));
            }
        }

        // 移除随机强盗
        private void RemoveRandomBandits(int min, int max)
        {
            List<CharacterObject> bandits = new List<CharacterObject>();
            foreach (var troopId in _banditLimit.TroopIds)
            {
                CharacterObject troop = MBObjectManager.Instance.GetObject<CharacterObject>(troopId);
                if (troop != null && MobileParty.MainParty.MemberRoster.GetTroopCount(troop) > 0)
                {
                    bandits.Add(troop);
                }
            }

            if (bandits.Count == 0) return;

            int removeCount = MBRandom.RandomInt(min, max + 1);
            for (int i = 0; i < removeCount; i++)
            {
                CharacterObject bandit = bandits[MBRandom.RandomInt(bandits.Count)];
                MobileParty.MainParty.MemberRoster.AddToCounts(bandit, -1);
            }
        }
        // private void ProcessDelayedRemovalTasks()
        // {
        //     Clan playerClan = Clan.PlayerClan; // 获取玩家氏族
        //     if (playerClan == null)
        //     {
        //         return; // 如果玩家没有氏族，则跳过
        //     }
        //
        //     // 获取玩家氏族的定居点和部队列表
        //     var settlements = playerClan.Settlements;
        //     var parties = GetClanParties(playerClan); // 获取氏族部队
        //
        //     // 处理所有延迟移除任务
        //     while (delayedRemovalTasks.Count > 0)
        //     {
        //         var task = delayedRemovalTasks.Dequeue();
        //         int totalSoldiers = CalculateTotalSoldiers(task.TroopLimit.TroopIds, settlements, parties);
        //         int limit = troopLimitValues[task.TroopLimit.Name];
        //
        //         // 如果士兵数量仍然超出上限，则执行移除操作
        //         if (totalSoldiers > limit)
        //         {
        //             int excessSoldiers = totalSoldiers - limit;
        //             RemoveExcessSoldiers(task.TroopLimit.TroopIds, excessSoldiers, task.TroopLimit);
        //         }
        //     }
        // }

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

            // 2. 添加家族等级基础加成
            Clan playerClan = Clan.PlayerClan;
            if (playerClan != null)
            {
                int clanTier = playerClan.Tier;
                int clanBonus = 0;

                // 根据兵种类型获取对应的加成
                if (troopLimit.Name.Contains("{=LRM_MOD_002}Regulars") || troopLimit.Name.Contains("Regulars"))
                {
                    clanBonus = SubModule.ModSettings.BaseRegularsBonus + (clanTier * SubModule.ModSettings.RegularsPerTier);
                }
                else if (troopLimit.Name.Contains("{=LRM_MOD_003}Nobles") || troopLimit.Name.Contains("Nobles"))
                {
                    clanBonus = SubModule.ModSettings.BaseNoblesBonus + (clanTier * SubModule.ModSettings.NoblesPerTier);
                }
                else if (troopLimit.Name.Contains("{=LRM_MOD_004}Elites") || troopLimit.Name.Contains("Elites"))
                {
                    clanBonus = SubModule.ModSettings.BaseElitesBonus + (clanTier * SubModule.ModSettings.ElitesPerTier);
                }

                Debug.Print($"Clan Tier Bonus for {troopLimit.Name}: {clanBonus} (Base: {GetBaseBonus(troopLimit)}, Per Tier: {GetPerTierBonus(troopLimit)}, Tier: {clanTier})");
                totalLimit += clanBonus;
            }

            Debug.Print($"Total limit: {totalLimit}");
            return totalLimit; // 返回计算后的上限值
        }

        // 辅助方法：获取基础加成
        private int GetBaseBonus(TroopLimit troopLimit)
        {
            if (troopLimit.Name.Contains("{=LRM_MOD_002}Regulars") || troopLimit.Name.Contains("Regulars"))
                return SubModule.ModSettings.BaseRegularsBonus;
            else if (troopLimit.Name.Contains("{=LRM_MOD_003}Nobles") || troopLimit.Name.Contains("Nobles"))
                return SubModule.ModSettings.BaseNoblesBonus;
            else if (troopLimit.Name.Contains("{=LRM_MOD_004}Elites") || troopLimit.Name.Contains("Elites"))
                return SubModule.ModSettings.BaseElitesBonus;
            return 0;
        }

        // 辅助方法：获取每级加成
        private int GetPerTierBonus(TroopLimit troopLimit)
        {
            if (troopLimit.Name.Contains("{=LRM_MOD_002}Regulars") || troopLimit.Name.Contains("Regulars"))
                return SubModule.ModSettings.RegularsPerTier;
            else if (troopLimit.Name.Contains("{=LRM_MOD_003}Nobles") || troopLimit.Name.Contains("Nobles"))
                return SubModule.ModSettings.NoblesPerTier;
            else if (troopLimit.Name.Contains("{=LRM_MOD_004}Elites") || troopLimit.Name.Contains("Elites"))
                return SubModule.ModSettings.ElitesPerTier;
            return 0;
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
                // 1. 移除玩家氏族部队
                int removedFromClanParties = RemoveTroopsFromClanParties(sortedTroops, ref excessSoldiers);
                totalRemoved += removedFromClanParties;
                Debug.Print($"Removed {removedFromClanParties} troops from clan parties.");
                if (excessSoldiers <= 0) break;

                // 2. 移除定居点驻兵
                int removedFromSettlements = RemoveTroopsFromSettlements(sortedTroops, ref excessSoldiers);
                totalRemoved += removedFromSettlements;
                Debug.Print($"Removed {removedFromSettlements} troops from settlements.");
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
                MBInformationManager.AddQuickInformation(message, 5, null, null, "");
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
            HashSet<MobileParty> partiesSet = new HashSet<MobileParty>(); // 使用 HashSet 去重

            // 遍历氏族的领主，获取他们的部队
            foreach (var lord in clan.AliveLords)
            {
                if (lord.PartyBelongedTo != null && lord.PartyBelongedTo != MobileParty.MainParty)
                {
                    partiesSet.Add(lord.PartyBelongedTo);
                }
            }

            // 获取玩家氏族的其他部队（如巡逻队、商队等）
            foreach (var party in clan.WarPartyComponents)
            {
                if (party != null && party.MobileParty != null && party.MobileParty != MobileParty.MainParty)
                {
                    partiesSet.Add(party.MobileParty);
                }
            }

            return partiesSet; // 转换为列表返回
        }

        // 在现有类中添加这个新方法
        private void ShowDailyTroopStatus()
        {
            Clan playerClan = Clan.PlayerClan;
            if (playerClan == null) return;

            var settlements = playerClan.Settlements;
            var parties = GetClanParties(playerClan);

            // 获取玩家统御技能值
            int leadershipSkill = Hero.MainHero.GetSkillValue(DefaultSkills.Leadership);

            foreach (var troopLimit in troopLimits)
            {
                // 获取各区域士兵分布
                var distribution = CalculateTroopDistribution(
                    troopLimit.TroopIds,
                    settlements,
                    parties
                );

                // 计算家族等级加成
                int clanTier = playerClan.Tier;
                int clanBonus = GetBaseBonus(troopLimit) + (clanTier * GetPerTierBonus(troopLimit));

                // 构建状态消息
                TextObject status = new TextObject("{=LRM_MOD_011}" +
                    $"{{TROOP_NAME}} Limit: {{LIMIT}}, " +
                    $"Current: {{TOTAL}}. " +
                    $"(Main party: {{MAIN}}, " +
                    $"Clan: {{CLAN}}, " +
                    $"Garrison: {{GARRISON}}.)");

                status.SetTextVariable("TROOP_NAME", troopLimit.Name);
                status.SetTextVariable("LIMIT", troopLimitValues[troopLimit.Name]);
                status.SetTextVariable("TOTAL", distribution.Total);
                status.SetTextVariable("MAIN", distribution.MainParty);
                status.SetTextVariable("CLAN", distribution.ClanParties);
                status.SetTextVariable("GARRISON", distribution.Garrisons);

                // 显示绿色状态消息
                InformationManager.DisplayMessage(
                    new InformationMessage(status.ToString(), new Color(0f, 1f, 0f))
                );
            }

            // 2. 添加佣兵的状态提示
            int playerTier = playerClan.Tier;
            // 传入两个参数：家族等级和统御技能
            int mercenaryLimit = _mercenaryLimit.GetLimitForTier(
                playerTier,
                leadershipSkill
            );
            var mercDistribution = CalculateTroopDistribution(
                _mercenaryLimit.TroopIds,
                settlements,
                parties
            );

            // 佣兵提示消息（使用不同的本地化键）
            TextObject mercStatus = new TextObject("{=LRM_MOD_013}" +
                $"Mercenaries Limit: {{LIMIT}}, " +
                $"Current: {{TOTAL}}. " +
                $"(Main party: {{MAIN}}, " +
                $"Clan: {{CLAN}}, " +
                $"Garrison: {{GARRISON}}.)");

            mercStatus.SetTextVariable("LIMIT", mercenaryLimit);
            mercStatus.SetTextVariable("TOTAL", mercDistribution.Total);
            mercStatus.SetTextVariable("MAIN", mercDistribution.MainParty);
            mercStatus.SetTextVariable("CLAN", mercDistribution.ClanParties);
            mercStatus.SetTextVariable("GARRISON", mercDistribution.Garrisons);

            // 显示绿色状态消息
            InformationManager.DisplayMessage(
                new InformationMessage(mercStatus.ToString(), new Color(0f, 1f, 0f))
            );
        }

        // 新增士兵分布数据结构
        private class TroopDistribution
        {
            public int Total { get; set; }
            public int MainParty { get; set; }
            public int ClanParties { get; set; }
            public int Garrisons { get; set; }
        }

        // 新增分布统计方法
        private TroopDistribution CalculateTroopDistribution(
            List<string> troopIds,
            IEnumerable<Settlement> settlements,
            IEnumerable<MobileParty> parties)
        {
            var distribution = new TroopDistribution();

            // 统计主部队
            foreach (var troopId in troopIds)
            {
                var troop = MBObjectManager.Instance.GetObject<CharacterObject>(troopId);
                distribution.MainParty += MobileParty.MainParty.MemberRoster.GetTroopCount(troop);
            }

            // 统计氏族部队
            foreach (var party in parties)
            {
                if (party?.MemberRoster == null) continue;

                foreach (var troopId in troopIds)
                {
                    var troop = MBObjectManager.Instance.GetObject<CharacterObject>(troopId);
                    distribution.ClanParties += party.MemberRoster.GetTroopCount(troop);
                }
            }

            // 统计驻军
            foreach (var settlement in settlements)
            {
                if (settlement?.Town?.GarrisonParty?.MemberRoster == null) continue;

                foreach (var troopId in troopIds)
                {
                    var troop = MBObjectManager.Instance.GetObject<CharacterObject>(troopId);
                    distribution.Garrisons += settlement.Town.GarrisonParty.MemberRoster.GetTroopCount(troop);
                }
            }

            distribution.Total = distribution.MainParty + distribution.ClanParties + distribution.Garrisons;
            return distribution;
        }
    }

    // 延迟移除任务类
    public class DelayedRemovalTask
    {
        public TroopLimit TroopLimit { get; set; } // 需要移除的兵种集合
        public int ExcessSoldiers { get; set; }    // 超出上限的士兵数量
    }
}
