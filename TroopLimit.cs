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
}
