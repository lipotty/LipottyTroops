using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LipottyTroops
{
    public class TroopLimit
    {
        public string Name { get; set; } // 兵种集合的名称
        public List<string> TroopIds { get; set; } // 兵种集合的ID列表
        public int TownProsperityPerLimit { get; set; } // 城镇每多少繁荣度增加1个上限
        public int CastleProsperityPerLimit { get; set; } // 城堡每多少繁荣度增加1个上限
        public int HearthPerLimit { get; set; } // 每多少户增加1个上限

        public TroopLimit(string name, List<string> troopIds, int townProsperityPerLimit, int castleProsperityPerLimit, int hearthPerLimit)
        {
            Name = name; // 初始化兵种集合名称
            TroopIds = troopIds; // 初始化兵种集合ID列表
            TownProsperityPerLimit = townProsperityPerLimit; // 初始化城镇繁荣度计算比例
            CastleProsperityPerLimit = castleProsperityPerLimit; // 初始化城堡繁荣度计算比例
            HearthPerLimit = hearthPerLimit; // 初始化户数计算比例
        }
    }

    public class MercenaryLimit
    {
        public List<string> TroopIds { get; } // 佣兵兵种ID列表
        private readonly Settings _settings;

        public MercenaryLimit(List<string> troopIds, Settings settings)
        {
            TroopIds = troopIds;
            _settings = settings;
        }

        public int GetLimitForTier(int tier, int leadershipSkill)
        {
            // 基础值 + 家族等级加成 + 统御技能加成
            int baseLimit = _settings.MercenaryBaseLimit;
            int tierBonus = tier * _settings.MercenaryPerTier;

            // 统御技能加成（每2点增加1，四舍五入）
            int leadershipBonus = (int)Math.Round(leadershipSkill * _settings.LeadershipBonusFactor);

            return baseLimit + tierBonus + leadershipBonus;
        }

        public class BanditLimit
        {
            public List<string> TroopIds { get; } // 强盗兵种ID列表
            private readonly Settings _settings;

            public BanditLimit(List<string> troopIds, Settings settings)
            {
                TroopIds = troopIds;
                _settings = settings;
            }

            public int GetLimit(int leadershipSkill)
            {
                // 基础值 + 统御技能加成（每2点增加1）
                int baseLimit = _settings.BanditBaseLimit;
                int leadershipBonus = (int)Math.Round(leadershipSkill * _settings.BanditLeadershipFactor);
                return baseLimit + leadershipBonus;
            }
        }
    }
}
