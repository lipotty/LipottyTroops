using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;
using System.Reflection;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace LipottyTroops
{
    public static class TroopCollections
    {
        private static Dictionary<string, List<string>> _cachedCategories;
        private static readonly object _lockObj = new object();

        public static IReadOnlyDictionary<string, List<string>> AllCategories
        {
            get
            {
                if (_cachedCategories == null)
                {
                    // 确保在游戏对象初始化后加载
                    if (Game.Current != null)
                    {
                        lock (_lockObj)
                        {
                            _cachedCategories = LoadCategories();
                        }
                    }
                }
                return _cachedCategories;
            }
        }

        public static List<string> RegularTroops =>
            GetTroopList("regulars");

        public static List<string> NobleTroops =>
            GetTroopList("nobles");

        public static List<string> EliteTroops =>
            GetTroopList("elites");

        public static List<string> MercenaryTroops =>
            GetTroopList("mercenary");

        public static List<string> BanditTroops =>
            GetTroopList("bandits");


        private static List<string> GetTroopList(string categoryId)
        {
            return AllCategories.TryGetValue(categoryId, out var list)
                ? list
                : new List<string>();
        }

        private static Dictionary<string, List<string>> LoadCategories()
        {
            // 通过SubModule获取MOD根路径
            string moduleRoot = SubModule.ModulePath;
            // 组合配置文件路径
            string xmlPath = Path.Combine(moduleRoot, "Config", "LRM_TroopConfig.xml");
            // 检查文件是否存在
            if (!File.Exists(xmlPath))
            {
                Debug.Print("[ERROR] Configuration file not found!");
                return new Dictionary<string, List<string>>();
            }
            var categories = XmlTroopLoader.LoadTroopCategories(xmlPath);
            XmlTroopLoader.ValidateTroopIds(categories);
            return categories ?? new Dictionary<string, List<string>>();
        }
    }
}
