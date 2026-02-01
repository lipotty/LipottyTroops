using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace LipottyTroops
{
    public class ModIntroductionBehavior : CampaignBehaviorBase
    {
        private bool _introductionShown = false;
        private bool _isNewGame = false;
        private bool _hourlyCheckStarted = false; // 新增：标记是否已开始每小时检查

        // 新增：保存之前的时间状态
        private CampaignTimeControlMode _previousTimeControlMode;
        private float _previousSpeedMultiplier;
        private bool _timeStateSaved = false;

        public override void RegisterEvents()
        {
            // 注册新游戏创建事件
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnNewGameCreated));
            // 注册游戏加载事件
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnGameLoaded));
            // 注册每小时事件，用于延迟显示
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, new Action(this.OnHourlyTick));
        }

        public override void SyncData(IDataStore dataStore)
        {
            // 同步介绍是否已显示的状态
            dataStore.SyncData("ModIntroductionShown", ref _introductionShown);
            dataStore.SyncData("IsNewGame", ref _isNewGame);
            dataStore.SyncData("HourlyCheckStarted", ref _hourlyCheckStarted);
        }

        private void OnNewGameCreated(CampaignGameStarter starter)
        {
            // 新游戏创建时标记需要显示介绍
            _introductionShown = false;
            _isNewGame = true;
            _hourlyCheckStarted = false; // 重置每小时检查标记
            _timeStateSaved = false; // 重置状态保存标记
        }

        private void OnGameLoaded(CampaignGameStarter starter)
        {
            // 游戏加载时标记不是新游戏
            _isNewGame = false;
        }

        private void OnHourlyTick()
        {
            // 每小时检查是否需要显示介绍
            if (!_introductionShown && _isNewGame)
            {
                // 确保只检查一次
                if (!_hourlyCheckStarted)
                {
                    _hourlyCheckStarted = true;
                    ShowModIntroduction();
                }
            }
        }

        private void ShowModIntroduction()
        {
            if (_introductionShown) return;

            // 保存当前时间状态
            SaveCurrentTimeState();

            // 暂停游戏时间
            PauseGameTime();

            // 创建介绍文本
            TextObject titleText = new TextObject("{=LRM_MOD_026}Welcome to Lipotty's Realistic Militari!");
            TextObject descriptionText = new TextObject("{=LRM_MOD_027}" +
                "This mod introduces the following new mechanics to the game:\n" +
                "• At the start, you cannot recruit soldiers directly from any settlement.\n" +
                "• As you progress, you will gradually unlock recruitment permissions in settlements.\n" +
                "• Taverns in towns host various mercenaries, serving as the main way to recruit troops in the early stages.\n" +
                "• regular troops, nobles, elites, mercenaries, and bandit units each have independent restriction rules.\n\n" +
                "To help you begin your journey, we have prepared 4,000 Denars in starting funds for you!\n" +
                "Click 'Confirm' to claim them. Good luck, have fun!");

            TextObject confirmText = new TextObject("{=LRM_MOD_028}Confirm");
            TextObject cancelText = new TextObject("{=LRM_MOD_029}No, thanks");

            // 创建询问对话框
            InformationManager.ShowInquiry(new InquiryData(
                titleText.ToString(),
                descriptionText.ToString(),
                true,
                true,
                confirmText.ToString(),
                cancelText.ToString(),
                new Action(OnIntroductionConfirmed),
                new Action(OnIntroductionCancelled),
                ""
            ));
        }

        // 新增方法：保存当前时间状态
        private void SaveCurrentTimeState()
        {
            Campaign campaign = Campaign.Current;
            if (campaign != null)
            {
                _previousTimeControlMode = campaign.TimeControlMode;
                _previousSpeedMultiplier = campaign.SpeedUpMultiplier;
                _timeStateSaved = true;

                // 调试信息
                // InformationManager.DisplayMessage(new InformationMessage($"保存时间状态: 模式={_previousTimeControlMode}, 倍数={_previousSpeedMultiplier}"));
            }
        }

        // 新增方法：暂停游戏时间
        private void PauseGameTime()
        {
            Campaign campaign = Campaign.Current;
            if (campaign != null)
            {
                // 设置时间为暂停模式
                campaign.TimeControlMode = CampaignTimeControlMode.Stop;
                campaign.SpeedUpMultiplier = 1f; // 确保速度为1
            }
        }

        // 新增方法：恢复之前的时间状态
        private void RestoreTimeState()
        {
            if (_timeStateSaved)
            {
                Campaign campaign = Campaign.Current;
                if (campaign != null)
                {
                    campaign.TimeControlMode = _previousTimeControlMode;
                    campaign.SpeedUpMultiplier = _previousSpeedMultiplier;

                    // 调试信息
                    // InformationManager.DisplayMessage(new InformationMessage($"恢复时间状态: 模式={_previousTimeControlMode}, 倍数={_previousSpeedMultiplier}"));
                }
                _timeStateSaved = false; // 重置标记
            }
        }

        private void OnIntroductionConfirmed()
        {
            // 恢复之前的时间状态
            RestoreTimeState();

            // 赠送4000第纳尔
            Hero.MainHero.ChangeHeroGold(4000);

            // 显示确认消息
            TextObject message = new TextObject("{=LRM_MOD_030}You have received 4,000 Denars in starting funds. Good luck, have fun!");
            InformationManager.DisplayMessage(new InformationMessage(message.ToString(), new Color(0f, 1f, 0f)));

            _introductionShown = true;
            _isNewGame = false;
        }

        private void OnIntroductionCancelled()
        {
            // 恢复之前的时间状态
            RestoreTimeState();

            // 取消时显示信息
            TextObject message = new TextObject("{=LRM_MOD_031}Good luck, have fun!");
            InformationManager.DisplayMessage(new InformationMessage(message.ToString(), new Color(1f, 1f, 0f)));

            _introductionShown = true;
            _isNewGame = false;
        }
    }
}
