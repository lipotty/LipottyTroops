using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MCM.Abstractions.Base.Global;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace LipottyTroops
{
    public class SubModule : MBSubModuleBase
    {
        private bool _shouldLoadPatches = true;
        public static Settings ModSettings { get; private set; }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            // 初始化 ModSettings
            ModSettings = GlobalSettings<Settings>.Instance;

            // 加载 Harmony 补丁
            if (_shouldLoadPatches)
            {
                new Harmony("LipottyTroops").PatchAll(Assembly.GetExecutingAssembly());
                _shouldLoadPatches = false;
            }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);

            // 仅在 Campaign 模式下注册行为
            if (game.GameType is Campaign)
            {
                CampaignGameStarter campaignStarter = (CampaignGameStarter)gameStarter;

                // 注册 SoldierLimitBehavior
                campaignStarter.AddBehavior(new SoldierLimitBehavior());

            }
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

            // 确保 ModSettings 已初始化
            if (ModSettings == null)
            {
                ModSettings = GlobalSettings<Settings>.Instance;
            }
            // 显示加载消息
            InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=LRM_MOD_010}LRM has loaded.", null).ToString()));
        }
    }
}
