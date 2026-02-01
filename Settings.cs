using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Base.Global;

namespace LipottyTroops
{
    public class Settings : AttributeGlobalSettings<Settings>
    {
        public override string Id => "LipottyTroops";
        public override string DisplayName => "LipottyTroops";
        public override string FolderName => "LipottyTroops";
        public override string FormatType => "json2";

        // 分标题：招募难度
        [SettingPropertyGroup("{=LRM_SET_001}Recruitment Difficulty Settings", GroupOrder = 0)]
        [SettingPropertyInteger(
            "{=LRM_SET_024}Recruitment Difficulty",
            0, 6, "0", Order = 0, RequireRestart = false,
            HintText = "{=LRM_SET_002}The amount of extra troops that you can recruit from notables. Default is 0."
        )]
        public int RecruitDifficulty { get; set; } = 0;

        // 分标题：家族等级基础加成
        [SettingPropertyGroup("{=LRM_SET_041}Clan Tier Base Bonuses", GroupOrder = 1)]
        [SettingPropertyInteger(
            "{=LRM_SET_042}Base Regulars Limit Bonus",
            0, 1000, "0", Order = 1, RequireRestart = false,
            HintText = "{=LRM_SET_043}Additional Regulars limit bonus at clan tier 0. Default: 0"
        )]
        public int BaseRegularsBonus { get; set; } = 0;

        [SettingPropertyGroup("{=LRM_SET_041}Clan Tier Base Bonuses", GroupOrder = 1)]
        [SettingPropertyInteger(
            "{=LRM_SET_044}Base Nobles Limit Bonus",
            0, 1000, "0", Order = 2, RequireRestart = false,
            HintText = "{=LRM_SET_045}Additional Nobles limit bonus at clan tier 0. Default: 0"
        )]
        public int BaseNoblesBonus { get; set; } = 0;

        [SettingPropertyGroup("{=LRM_SET_041}Clan Tier Base Bonuses", GroupOrder = 1)]
        [SettingPropertyInteger(
            "{=LRM_SET_046}Base Elites Limit Bonus",
            0, 1000, "0", Order = 3, RequireRestart = false,
            HintText = "{=LRM_SET_047}Additional Elites limit bonus at clan tier 0. Default: 0"
        )]
        public int BaseElitesBonus { get; set; } = 0;

        [SettingPropertyGroup("{=LRM_SET_041}Clan Tier Base Bonuses", GroupOrder = 1)]
        [SettingPropertyInteger(
            "{=LRM_SET_048}Regulars Per Clan Tier",
            0, 1000, "0", Order = 4, RequireRestart = false,
            HintText = "{=LRM_SET_049}Additional Regulars per clan tier. Default: 0"
        )]
        public int RegularsPerTier { get; set; } = 0;

        [SettingPropertyGroup("{=LRM_SET_041}Clan Tier Base Bonuses", GroupOrder = 1)]
        [SettingPropertyInteger(
            "{=LRM_SET_050}Nobles Per Clan Tier",
            0, 1000, "0", Order = 5, RequireRestart = false,
            HintText = "{=LRM_SET_051}Additional Nobles per clan tier. Default: 0"
        )]
        public int NoblesPerTier { get; set; } = 0;

        [SettingPropertyGroup("{=LRM_SET_041}Clan Tier Base Bonuses", GroupOrder = 1)]
        [SettingPropertyInteger(
            "{=LRM_SET_052}Elites Per Clan Tier",
            0, 1000, "0", Order = 6, RequireRestart = false,
            HintText = "{=LRM_SET_053}Additional Elites per clan tier. Default: 0"
        )]
        public int ElitesPerTier { get; set; } = 0;

        // 分标题：常备
        [SettingPropertyGroup("{=LRM_SET_003}Regulars Limit Settings", GroupOrder = 2)]
        [SettingPropertyInteger(
            "{=LRM_SET_004}Town Prosperity's Impact on Limit (Regulars)",
            10, 2000, "0", Order = 1, RequireRestart = false,
            HintText = "{=LRM_SET_005}Town prosperity required per Regular: 100 (default)."
        )]
        public int RegularsTownProsperity { get; set; } = 100;

        [SettingPropertyGroup("{=LRM_SET_003}Regulars Limit Settings", GroupOrder = 2)]
        [SettingPropertyInteger(
            "{=LRM_SET_006}Castle Prosperity's Impact on Limit (Regulars)",
            10, 2000, "0", Order = 2, RequireRestart = false,
            HintText = "{=LRM_SET_007}Castle prosperity required per Regular: 20 (default)."
        )]
        public int RegularsCastleProsperity { get; set; } = 20;

        [SettingPropertyGroup("{=LRM_SET_003}Regulars Limit Settings", GroupOrder = 2)]
        [SettingPropertyInteger(
            "{=LRM_SET_008}Village Hearth's Impact on Limit (Regulars)",
            10, 2000, "0", Order = 3, RequireRestart = false,
            HintText = "{=LRM_SET_009}Village hearth required per Regular: 20 (default)."
        )]
        public int RegularsVillageHouseholds { get; set; } = 20;

        // 分标题：亲随
        [SettingPropertyGroup("{=LRM_SET_010}Nobles Limit Settings", GroupOrder = 3)]
        [SettingPropertyInteger(
            "{=LRM_SET_018}Town Prosperity's Impact on Limit (Nobles)",
            10, 2000, "0", Order = 1, RequireRestart = false,
            HintText = "{=LRM_SET_011}Town prosperity required per Noble: 500 (default)."
        )]
        public int NoblesTownProsperity { get; set; } = 500;

        [SettingPropertyGroup("{=LRM_SET_010}Nobles Limit Settings", GroupOrder = 3)]
        [SettingPropertyInteger(
            "{=LRM_SET_019}Castle Prosperity's Impact on Limit (Nobles)",
            10, 2000, "0", Order = 2, RequireRestart = false,
            HintText = "{=LRM_SET_012}Castle prosperity required per Noble: 100 (default)."
        )]
        public int NoblesCastleProsperity { get; set; } = 100;

        [SettingPropertyGroup("{=LRM_SET_010}Nobles Limit Settings", GroupOrder = 3)]
        [SettingPropertyInteger(
            "{=LRM_SET_020}Village Hearth's Impact on Limit (Nobles)",
            10, 2000, "0", Order = 3, RequireRestart = false,
            HintText = "{=LRM_SET_013}Village hearth required per Noble: 100 (default)."
        )]
        public int NoblesVillageHouseholds { get; set; } = 100;

        // 分标题：精锐
        [SettingPropertyGroup("{=LRM_SET_014}Elites Limit Settings", GroupOrder = 4)]
        [SettingPropertyInteger(
            "{=LRM_SET_021}Town Prosperity's Impact on Limit (Elites)",
            10, 2000, "0", Order = 1, RequireRestart = false,
            HintText = "{=LRM_SET_015}Town prosperity required per Elite: 1000 (default)."
        )]
        public int ElitesTownProsperity { get; set; } = 1000;

        [SettingPropertyGroup("{=LRM_SET_014}Elites Limit Settings", GroupOrder = 4)]
        [SettingPropertyInteger(
            "{=LRM_SET_022}Castle Prosperity's Impact on Limit (Elites)",
            10, 2000, "0", Order = 2, RequireRestart = false,
            HintText = "{=LRM_SET_016}Castle prosperity required per Elite: 200 (default)."
        )]
        public int ElitesCastleProsperity { get; set; } = 200;

        [SettingPropertyGroup("{=LRM_SET_014}Elites Limit Settings", GroupOrder = 4)]
        [SettingPropertyInteger(
            "{=LRM_SET_023}Village Hearth's Impact on Limit (Elites)",
            10, 2000, "0", Order = 3, RequireRestart = false,
            HintText = "{=LRM_SET_017}Village hearth required per Elite: 200 (default)."
        )]
        public int ElitesVillageHouseholds { get; set; } = 200;

        // 新增佣兵配置组
        [SettingPropertyGroup("{=LRM_SET_025}Mercenary Settings", GroupOrder = 5)]
        [SettingPropertyInteger(
            "{=LRM_SET_026}Base Mercenary Limit",
            0, 100, "0", Order = 1, RequireRestart = false,
            HintText = "{=LRM_SET_027}Initial mercenary limit at clan tier 0. Default: 20"
        )]
        public int MercenaryBaseLimit { get; set; } = 20;

        [SettingPropertyGroup("{=LRM_SET_025}Mercenary Settings", GroupOrder = 5)]
        [SettingPropertyInteger(
            "{=LRM_SET_028}Mercenary Per Clan Tier",
            1, 100, "0", Order = 2, RequireRestart = false,
            HintText = "{=LRM_SET_029}Additional mercenaries per clan tier. Default: +10 per tier"
        )]
        public int MercenaryPerTier { get; set; } = 10;

        [SettingPropertyGroup("{=LRM_SET_025}Mercenary Settings", GroupOrder = 5)]
        [SettingPropertyFloatingInteger(
            "{=LRM_SET_030}Leadership Bonus Factor (Mercenary)",
            0f, 1f, "0.00", Order = 3, RequireRestart = false,
            HintText = "{=LRM_SET_031}Mercenary bonus per leadership point (actual = points * factor). Default: 0.5 (1 per 2 points)"
        )]
        public float LeadershipBonusFactor { get; set; } = 0.5f;

        // 分标题：强盗设置
        [SettingPropertyGroup("{=LRM_SET_032}Bandit Settings", GroupOrder = 6)]
        [SettingPropertyInteger(
            "{=LRM_SET_033}Bandit Base Limit",  // 强盗初始上限
            0, 100, "0", Order = 1, RequireRestart = false,
            HintText = "{=LRM_SET_034}Initial bandit limit at leadership 0. Default: 0"
        )]
        public int BanditBaseLimit { get; set; } = 0;

        [SettingPropertyGroup("{=LRM_SET_032}Bandit Settings", GroupOrder = 6)]
        [SettingPropertyFloatingInteger(
            "{=LRM_SET_035}Leadership Bonus Factor (Bandits)",  // 统御影响的强盗上限
            0f, 1f, "0.00", Order = 2, RequireRestart = false,
            HintText = "{=LRM_SET_036}Bandit bonus per leadership point (actual = points * factor). Default: 0.5 (1 per 2 points)"
        )]
        public float BanditLeadershipFactor { get; set; } = 0.5f;

        [SettingPropertyGroup("{=LRM_SET_032}Bandit Settings", GroupOrder = 6)]
        [SettingPropertyFloatingInteger(
            "{=LRM_SET_037}Base Event Chance",  // 强盗事件初始触发概率
            0.01f, 1.0f, "0.00", Order = 3, RequireRestart = false,
            HintText = "{=LRM_SET_038}Base probability for bandit events (default: 1.0)"
        )]
        public float BanditBaseEventChance { get; set; } = 1.0f;

        [SettingPropertyGroup("{=LRM_SET_032}Bandit Settings", GroupOrder = 6)]
        [SettingPropertyFloatingInteger(
            "{=LRM_SET_039}Roguery Reduction Factor",  // 流氓习气影响强盗事件触发的概率
            0.001f, 0.1f, "0.000", Order = 4, RequireRestart = false,
            HintText = "{=LRM_SET_040}Roguery skill impact per point (default: 0.004)"
        )]
        public float BanditRogueryReduction { get; set; } = 0.004f;
    }
}
