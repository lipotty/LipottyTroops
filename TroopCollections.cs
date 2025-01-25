using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LipottyTroops
{
    public static class TroopCollections
    {
        // 战兵集合
        public static List<string> CommonTroops = new List<string>
        {
            "LI_vlandian_pavise_crossbowman", // 瓦兰迪亚巨盾弩兵
            "vlandian_hardened_crossbowman",   // 瓦兰迪亚硬弩手
            "vlandian_sharpshooter",   // 瓦兰迪亚神射手
            "vlandian_spearman", // 瓦兰迪亚矛兵
            "vlandian_billman",   // 瓦兰迪亚钩镰兵
            "vlandian_voulgier",   // 瓦兰迪亚斧枪兵
            "LI_vlandian_pikeman", // 瓦兰迪亚长枪兵
            "vlandian_pikeman",   // 瓦兰迪亚资深长枪兵
            "vlandian_infantry",   // 瓦兰迪亚步兵
            "vlandian_swordsman", // 瓦兰迪亚剑士
            "vlandian_sergeant",   // 瓦兰迪亚军士
            "vlandian_light_cavalry",   // 瓦兰迪亚轻骑兵
            "vlandian_cavalry",   // 瓦兰迪亚骑兵
            "vlandian_vanguard",   // 瓦兰迪亚先锋骑兵
            "LI_aserai_skirmisher",   // 阿塞莱轻装射手
            "aserai_archer",   // 阿塞莱射手
            "aserai_master_archer",   // 阿塞莱弓箭大师
            "aserai_footman",   // 阿塞莱步卒
            "aserai_infantry",   // 阿塞莱步兵
            "aserai_veteran_infantry",   // 阿塞莱资深步兵
            "aserai_mameluke_axeman",   // 阿塞莱马穆鲁克斧兵
            "aserai_mameluke_guard",   // 阿塞莱马穆鲁克卫士
            "mamluke_palace_guard",   // 阿塞莱马穆鲁克宫廷卫士
            "aserai_mameluke_regular",   // 阿塞莱马穆鲁克骑手
            "aserai_mameluke_cavalry",   // 阿塞莱马穆鲁克弓骑兵
            "aserai_mameluke_heavy_cavalry",   // 阿塞莱马穆鲁克重装弓骑兵
            "aserai_tawashi_cavalry",   // 阿塞莱马穆鲁克骑兵
            "aserai_tawashi_heavy_cavalry",   // 阿塞莱马穆鲁克重骑兵
            "sturgian_hunter",   // 斯特吉亚猎人
            "sturgian_archer",   // 斯特吉亚射手
            "sturgian_veteran_bowman",   // 斯特吉亚资深射手
            "sturgian_brigand",   // 斯特吉亚匪兵
            "sturgian_hardened_brigand",   // 斯特吉亚老练匪兵
            "sturgian_horse_raider",   // 斯特吉亚骑马掠夺者
            "LI_sturgian_infantry",   // 斯特吉亚士兵
            "sturgian_berzerker",   // 斯特吉亚破阵者
            "sturgian_ulfhednar",   // 斯特吉亚破阵勇士
            "sturgian_spearman",   // 斯特吉亚矛兵
            "sturgian_shock_troop",   // 斯特吉亚重装矛兵
            "sturgian_veteran_warrior",   // 斯特吉亚重装斧兵
            "LI_khuzait_ghazi",   // 库塞特勇士
            "khuzait_horse_archer",   // 库塞特骑射手
            "khuzait_heavy_horse_archer",   // 库塞特重装骑射手
            "khuzait_lancer",   // 库塞特枪骑兵
            "khuzait_heavy_lancer",   // 库塞特重装枪骑兵
            "khuzait_hunter",   // 库塞特猎人
            "khuzait_archer",   // 库塞特射手
            "khuzait_marksman",   // 库塞特神弓手
            "khuzait_spearman",   // 库塞特矛兵
            "khuzait_spear_infantry",   // 库塞特持矛步兵
            "khuzait_darkhan",   // 库塞特达尔罕
            "LI_battanian_archer",   // 巴旦尼亚射手
            "LI_battanian_veteran_archer",   // 巴旦尼亚资深射手
            "LI_battanian_saethwyr",   // 巴旦尼亚萨伊维尔
            "battanian_skirmisher",   // 巴旦尼亚散兵
            "battanian_veteran_skirmisher",   // 巴旦尼亚资深散兵
            "battanian_wildling",   // 巴旦尼亚野人
            "battanian_trained_warrior",   // 巴旦尼亚氏族勇士
            "battanian_picked_warrior",   // 巴旦尼亚受选勇士
            "battanian_oathsworn",   // 巴旦尼亚誓约者
            "battanian_falxman",   // 巴旦尼亚镰兵
            "battanian_veteran_falxman",   // 巴旦尼亚资深镰兵
            "battanian_raider",   // 巴旦尼亚掠袭者
            "battanian_scout",   // 巴旦尼亚斥候
            "battanian_horseman",   // 巴旦尼亚骑手
            "battanian_mounted_skirmisher",   // 巴旦尼亚游骑兵
            "LI_imperial_legionary",   // 帝国军团步兵
            "imperial_veteran_infantryman",   // 帝国资深军团步兵
            "imperial_legionary",   // 帝国禁卫军团步兵
            "imperial_menavliaton",   // 帝国双刃枪兵
            "imperial_elite_menavliaton",   // 帝国精英双刃枪兵
            "imperial_trained_archer",   // 帝国射手
            "imperial_veteran_archer",   // 帝国资深射手
            "imperial_palatine_guard",   // 帝国禁卫射手
            "imperial_crossbowman",   // 帝国弩手
            "imperial_sergeant_crossbowman",   // 帝国弩手军士
            "LI_imperial_trapezitos",   // 帝国轻骑兵
            "LI_imperial_stratiotai",   // 帝国骑兵
            "LI_imperial_legionary_cavalry",   // 帝国重装骑兵
            "bucellarii"   // 帝国私属骑兵
        };

        // 贵族兵集合
        public static List<string> NobleTroops = new List<string>
        {
            "vlandian_knight",   // 瓦兰迪亚骑士
            "vlandian_champion",  // 瓦兰迪亚冠军骑士
            "aserai_faris",   // 阿塞莱泰瓦希
            "aserai_veteran_faris",  // 阿塞莱资深泰瓦希
            "varyag_veteran",   // 斯特吉亚贵族亲卫
            "druzhinnik",  // 斯特吉亚精英贵族亲卫
            "khuzait_torguud",   // 库塞特土尔扈特
            "khuzait_kheshig",  // 库塞特怯薛
            "battanian_hero",   // 巴旦尼亚英雄
            "battanian_fian",  // 巴旦尼亚费奥纳勇士
            "imperial_heavy_horseman",  // 帝国普罗尼亚骑兵
            "imperial_cataphract"   // 帝国普罗尼亚骑士
        };

        // 顶级兵集合
        public static List<string> EliteTroops = new List<string>
        {
            "vlandian_banner_knight", // 瓦兰迪亚方旗骑士
            "aserai_vanguard_faris", // 阿塞莱法里斯
            "druzhinnik_champion", // 斯特吉亚王公亲卫骑兵
            "khuzait_khans_guard", // 库塞特可汗卫士
            "battanian_fian_champion", // 巴旦尼亚费奥纳冠军
            "imperial_elite_cataphract", // 帝国具装骑兵
            "LI_vaegir_guard", // 维吉亚卫士
            "LI_vardariotai"     // 瓦达瑞泰军团骑兵
        };
    }
}
