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

        // 分标题：战兵
        [SettingPropertyGroup("{=LRM_SET_003}Regulars Limit Settings", GroupOrder = 1)]
        [SettingPropertyInteger(
            "{=LRM_SET_004}Town Prosperity's Impact on Limit (Regulars)",
            10, 2000, "0", Order = 1, RequireRestart = false,
            HintText = "{=LRM_SET_005}Town prosperity required per Regular: 100 (default)."
        )]
        public int RegularsTownProsperity { get; set; } = 100;

        [SettingPropertyGroup("{=LRM_SET_003}Regulars Limit Settings", GroupOrder = 1)]
        [SettingPropertyInteger(
            "{=LRM_SET_006}Castle Prosperity's Impact on Limit (Regulars)",
            10, 2000, "0", Order = 2, RequireRestart = false,
            HintText = "{=LRM_SET_007}Castle prosperity required per Regular: 40 (default)."
        )]
        public int RegularsCastleProsperity { get; set; } = 40;

        [SettingPropertyGroup("{=LRM_SET_003}Regulars Limit Settings", GroupOrder = 1)]
        [SettingPropertyInteger(
            "{=LRM_SET_008}Village Hearth's Impact on Limit (Regulars)",
            10, 2000, "0", Order = 3, RequireRestart = false,
            HintText = "{=LRM_SET_009}Village hearth required per Regular: 20 (default)."
        )]
        public int RegularsVillageHouseholds { get; set; } = 20;

        // 分标题：贵族兵
        [SettingPropertyGroup("{=LRM_SET_010}Nobles Limit Settings", GroupOrder = 2)]
        [SettingPropertyInteger(
            "{=LRM_SET_018}Town Prosperity's Impact on Limit (Nobles)",
            10, 2000, "0", Order = 1, RequireRestart = false,
            HintText = "{=LRM_SET_011}Town prosperity required per Noble: 500 (default)."
        )]
        public int NoblesTownProsperity { get; set; } = 500;

        [SettingPropertyGroup("{=LRM_SET_010}Nobles Limit Settings", GroupOrder = 2)]
        [SettingPropertyInteger(
            "{=LRM_SET_019}Castle Prosperity's Impact on Limit (Nobles)",
            10, 2000, "0", Order = 2, RequireRestart = false,
            HintText = "{=LRM_SET_012}Castle prosperity required per Noble: 100 (default)."
        )]
        public int NoblesCastleProsperity { get; set; } = 100;

        [SettingPropertyGroup("{=LRM_SET_010}Nobles Limit Settings", GroupOrder = 2)]
        [SettingPropertyInteger(
            "{=LRM_SET_020}Village Hearth's Impact on Limit (Nobles)",
            10, 2000, "0", Order = 3, RequireRestart = false,
            HintText = "{=LRM_SET_013}Village hearth required per Noble: 100 (default)."
        )]
        public int NoblesVillageHouseholds { get; set; } = 100;

        // 分标题：顶级兵
        [SettingPropertyGroup("{=LRM_SET_014}Elites Limit Settings", GroupOrder = 3)]
        [SettingPropertyInteger(
            "{=LRM_SET_021}Town Prosperity's Impact on Limit (Elites)",
            10, 2000, "0", Order = 1, RequireRestart = false,
            HintText = "{=LRM_SET_015}Town prosperity required per Elite: 1000 (default)."
        )]
        public int ElitesTownProsperity { get; set; } = 1000;

        [SettingPropertyGroup("{=LRM_SET_014}Elites Limit Settings", GroupOrder = 3)]
        [SettingPropertyInteger(
            "{=LRM_SET_022}Castle Prosperity's Impact on Limit (Elites)",
            10, 2000, "0", Order = 2, RequireRestart = false,
            HintText = "{=LRM_SET_016}Castle prosperity required per Elite: 200 (default)."
        )]
        public int ElitesCastleProsperity { get; set; } = 200;

        [SettingPropertyGroup("{=LRM_SET_014}Elites Limit Settings", GroupOrder = 3)]
        [SettingPropertyInteger(
            "{=LRM_SET_023}Village Hearth's Impact on Limit (Elites)",
            10, 2000, "0", Order = 3, RequireRestart = false,
            HintText = "{=LRM_SET_017}Village hearth required per Elite: 200 (default)."
        )]
        public int ElitesVillageHouseholds { get; set; } = 200;
    }
}
