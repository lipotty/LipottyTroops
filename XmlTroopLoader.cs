using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.ObjectSystem;
using TaleWorlds.Library;

namespace LipottyTroops
{
    public static class XmlTroopLoader
    {
        public static Dictionary<string, List<string>> LoadTroopCategories(string xmlPath)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(xmlPath);

                var categories = new Dictionary<string, List<string>>();
                XmlNodeList categoryNodes = doc.SelectNodes("/TroopConfig/TroopCategory");

                if (categoryNodes == null || categoryNodes.Count == 0)
                {
                    Debug.Print("[ERROR] No valid TroopCategory nodes found");
                    return categories;
                }

                foreach (XmlNode categoryNode in categoryNodes)
                {
                    XmlAttribute idAttr = categoryNode.Attributes?["id"];
                    if (idAttr == null) continue;

                    var troopIds = new List<string>();
                    foreach (XmlNode troopNode in categoryNode.SelectNodes("Troop"))
                    {
                        XmlAttribute troopIdAttr = troopNode.Attributes?["id"];
                        if (troopIdAttr != null && !string.IsNullOrWhiteSpace(troopIdAttr.Value))
                        {
                            troopIds.Add(troopIdAttr.Value);
                        }
                    }

                    categories.Add(idAttr.Value, troopIds);
                }

                return categories;
            }
            catch (Exception ex)
            {
                Debug.Print($"[ERROR] Failed to load troop XML: {ex.Message}");
                return new Dictionary<string, List<string>>();
            }
        }

        public static void ValidateTroopIds(Dictionary<string, List<string>> categories)
        {
            foreach (var category in categories)
            {
                foreach (string troopId in category.Value)
                {
                    if (MBObjectManager.Instance.GetObject<CharacterObject>(troopId) == null)
                    {
                        Debug.Print($"[WARNING] Invalid troop ID: {troopId} in category {category.Key}");
                    }
                }
            }
        }
    }
}
