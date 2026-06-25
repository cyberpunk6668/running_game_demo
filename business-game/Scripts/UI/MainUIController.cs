using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BusinessGame;

public partial class MainUIController : Node2D
{
    private DataManager _dataManager = null!;
    private EventManager _eventManager = null!;
    private GameManager _gameManager = null!;

    private Label _dateLabel = null!;
    private Label _eraLabel = null!;
    private Button _playPauseButton = null!;
    private Button _stepButton = null!;
    private Button _speedButton = null!;
    private Button _saveButton = null!;
    private Button _loadButton = null!;
    private TabContainer _projectTabs = null!;
    private GridContainer _productionGrid = null!;
    private GridContainer _humanResourcesGrid = null!;
    private GridContainer _marketingGrid = null!;
    private RichTextLabel _statsText = null!;
    private RichTextLabel _financeText = null!;
    private RichTextLabel _marketText = null!;
    private RichTextLabel _logText = null!;
    private PopupPanel _eventDialog = null!;
    private Label _eventTitleLabel = null!;
    private RichTextLabel _eventDescriptionLabel = null!;
    private VBoxContainer _eventOptionContainer = null!;
    private Timer _monthTimer = null!;

    private readonly List<string> _logs = [];

    public override void _Ready()
    {
        _dataManager = DataManager.Instance ?? GetNode<DataManager>("/root/DataManager");
        _eventManager = EventManager.Instance ?? GetNode<EventManager>("/root/EventManager");
        _gameManager = GameManager.Instance ?? GetNode<GameManager>("/root/GameManager");

        CacheNodes();
        BindSignals();
        _gameManager.BindTimer(_monthTimer);
        ConfigureTabs();

        AddLog("原型已启动：使用顶部按钮推进月份，投资右侧项目卡即可改变市场引擎。");
        RefreshAll();

        if (_eventManager.ActiveEvent != null)
        {
            ShowActiveEvent(_eventManager.ActiveEvent);
        }
    }

    public override void _ExitTree()
    {
        _dataManager.StateChanged -= OnStateChanged;
        _dataManager.ProjectPortfolioChanged -= OnProjectPortfolioChanged;
        _dataManager.MonthResolved -= OnMonthResolved;
        _eventManager.ActiveEventChanged -= OnActiveEventChanged;
        _gameManager.PlayStateChanged -= OnPlayStateChanged;
    }

    private void CacheNodes()
    {
        _dateLabel = GetNode<Label>("%DateLabel");
        _eraLabel = GetNode<Label>("%EraLabel");
        _playPauseButton = GetNode<Button>("%PlayPauseButton");
        _stepButton = GetNode<Button>("%StepButton");
        _speedButton = GetNode<Button>("%SpeedButton");
        _saveButton = GetNode<Button>("%SaveButton");
        _loadButton = GetNode<Button>("%LoadButton");
        _projectTabs = GetNode<TabContainer>("%ProjectTabs");
        _productionGrid = GetNode<GridContainer>("%ProductionGrid");
        _humanResourcesGrid = GetNode<GridContainer>("%HumanResourcesGrid");
        _marketingGrid = GetNode<GridContainer>("%MarketingGrid");
        _statsText = GetNode<RichTextLabel>("%StatsText");
        _financeText = GetNode<RichTextLabel>("%FinanceText");
        _marketText = GetNode<RichTextLabel>("%MarketText");
        _logText = GetNode<RichTextLabel>("%LogText");
        _eventDialog = GetNode<PopupPanel>("%EventDialog");
        _eventTitleLabel = GetNode<Label>("%EventTitleLabel");
        _eventDescriptionLabel = GetNode<RichTextLabel>("%EventDescriptionLabel");
        _eventOptionContainer = GetNode<VBoxContainer>("%EventOptionContainer");
        _monthTimer = GetNode<Timer>("%MonthTimer");
    }

    private void BindSignals()
    {
        _dataManager.StateChanged += OnStateChanged;
        _dataManager.ProjectPortfolioChanged += OnProjectPortfolioChanged;
        _dataManager.MonthResolved += OnMonthResolved;
        _eventManager.ActiveEventChanged += OnActiveEventChanged;
        _gameManager.PlayStateChanged += OnPlayStateChanged;

        _playPauseButton.Pressed += OnPlayPausePressed;
        _stepButton.Pressed += OnStepPressed;
        _speedButton.Pressed += OnSpeedPressed;
        _saveButton.Pressed += OnSavePressed;
        _loadButton.Pressed += OnLoadPressed;
    }

    private void ConfigureTabs()
    {
        _projectTabs.SetTabTitle(0, "生产车间");
        _projectTabs.SetTabTitle(1, "人事团队");
        _projectTabs.SetTabTitle(2, "市场营销");
    }

    private void RefreshAll()
    {
        RefreshTopBar();
        RefreshLeftPanel();
        RebuildProjectCards();
        RefreshPlayControls();
        RefreshEventDialogState();
    }

    private void RefreshTopBar()
    {
        _dateLabel.Text = $"{_dataManager.State.CurrentYear}年{_dataManager.State.CurrentMonth}月";
        _eraLabel.Text = _dataManager.GetCurrentEraTitle();
    }

    private void RefreshLeftPanel()
    {
        var state = _dataManager.State;
        var snapshot = _dataManager.CurrentSnapshot;

        _statsText.Text =
            $"资金：{state.Money} 元\n" +
            $"声望：{_dataManager.GetDisplayReputation():0.0}\n" +
            $"人脉：{_dataManager.GetDisplayNetwork():0.0}\n" +
            $"知识：{_dataManager.GetDisplayKnowledge():0.0}\n" +
            $"幸福：{_dataManager.GetDisplayHappiness():0.0}\n\n" +
            $"已投项目：{_dataManager.GetOwnedProjectCount()} 个\n" +
            $"生产 / 人事 / 市场：{_dataManager.GetOwnedProjectCount(ProjectType.Production)} / {_dataManager.GetOwnedProjectCount(ProjectType.HumanResources)} / {_dataManager.GetOwnedProjectCount(ProjectType.Marketing)}";

        _financeText.Text =
            $"预计月收入：{state.MonthlyIncome:0} 元\n" +
            $"预计月支出：{state.MonthlyExpense:0} 元\n" +
            $"预计净利润：{state.NetProfit:+0;-0;0} 元\n" +
            $"库存压力成本：{state.InventoryCost:0} 元\n\n" +
            $"当前客单利润：{state.UnitProfit:0.0}\n" +
            $"经营节奏：{_gameManager.SpeedLabel}";

        _marketText.Text =
            $"市场需求：{snapshot.EffectiveDemand:0.0}\n" +
            $"供给能力：{snapshot.SupplyCapacity:0.0}\n" +
            $"市场竞争力：{snapshot.Competitiveness:0.00}\n" +
            $"幸福修正：{snapshot.HappinessModifier:0.00}x\n" +
            $"预计销量：{snapshot.ActualSales:0.0}\n\n" +
            $"固定成本：{snapshot.FixedExpense:0}\n" +
            $"项目维护：{snapshot.ProjectMaintenance:0}";
    }

    private void RefreshPlayControls()
    {
        _playPauseButton.Text = _gameManager.IsRunning ? "暂停" : "开始";
        _speedButton.Text = $"速度 {_gameManager.SpeedLabel}";
        var eventLocked = _eventManager.HasActiveEvent;
        _playPauseButton.Disabled = eventLocked;
        _stepButton.Disabled = eventLocked;
    }

    private void RebuildProjectCards()
    {
        RebuildGrid(_productionGrid, ProjectType.Production);
        RebuildGrid(_humanResourcesGrid, ProjectType.HumanResources);
        RebuildGrid(_marketingGrid, ProjectType.Marketing);
    }

    private void RebuildGrid(GridContainer grid, ProjectType type)
    {
        foreach (var child in grid.GetChildren())
        {
            child.QueueFree();
        }

        foreach (var project in _dataManager.GetProjectsByType(type))
        {
            grid.AddChild(CreateProjectCard(project));
        }
    }

    private Control CreateProjectCard(ProjectDefinition project)
    {
        var level = _dataManager.GetProjectLevel(project.Id);
        var isUnlocked = _dataManager.IsUnlocked(project);
        var cost = project.GetUpgradeCost(level);
        var canInvest = level < project.MaxLevel && isUnlocked && _dataManager.State.Money >= cost;

        var card = new PanelContainer
        {
            CustomMinimumSize = new Vector2(260.0f, 260.0f),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 12);
        margin.AddThemeConstantOverride("margin_top", 10);
        margin.AddThemeConstantOverride("margin_right", 12);
        margin.AddThemeConstantOverride("margin_bottom", 10);
        card.AddChild(margin);

        var content = new VBoxContainer
        {
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        margin.AddChild(content);

        var title = new Label
        {
            Text = $"{project.Name}  Lv.{level}/{project.MaxLevel}",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        content.AddChild(title);

        var detail = new Label
        {
            Text =
                $"{project.Description}\n\n" +
                $"升级成本：{cost} 元\n" +
                $"维护成本：{project.GetMonthlyMaintenance(level + 1):0} / 月\n" +
                $"解锁条件：{_dataManager.GetUnlockHint(project)}",
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        content.AddChild(detail);

        var effect = new Label
        {
            Text = BuildProjectEffectSummary(project.Effects),
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        content.AddChild(effect);

        var button = new Button
        {
            Text = level >= project.MaxLevel ? "已满级" : isUnlocked ? "投资 / 升级" : "尚未解锁",
            Disabled = !canInvest,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        button.Pressed += () => OnInvestPressed(project.Id);
        content.AddChild(button);

        return card;
    }

    private void RefreshEventDialogState()
    {
        if (_eventManager.ActiveEvent == null)
        {
            _eventDialog.Hide();
            return;
        }

        ShowActiveEvent(_eventManager.ActiveEvent);
    }

    private void ShowActiveEvent(GameEventDefinition definition)
    {
        _eventTitleLabel.Text = definition.Title;
        _eventDescriptionLabel.Text = definition.Description;

        foreach (var child in _eventOptionContainer.GetChildren())
        {
            child.QueueFree();
        }

        for (var index = 0; index < definition.Options.Count; index += 1)
        {
            var optionIndex = index;
            var option = definition.Options[index];

            var wrapper = new VBoxContainer();
            var optionButton = new Button
            {
                Text = option.Text,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            optionButton.Pressed += () => OnEventOptionPressed(optionIndex);
            wrapper.AddChild(optionButton);

            if (!string.IsNullOrWhiteSpace(option.Description))
            {
                var description = new Label
                {
                    Text = $"{option.Description}\n{BuildEventEffectSummary(option.Effects)}",
                    AutowrapMode = TextServer.AutowrapMode.WordSmart
                };
                wrapper.AddChild(description);
            }

            _eventOptionContainer.AddChild(wrapper);
        }

        _eventDialog.PopupCentered(new Vector2I(620, 420));
    }

    private void OnStateChanged()
    {
        RefreshAll();
    }

    private void OnProjectPortfolioChanged()
    {
        RebuildProjectCards();
    }

    private void OnMonthResolved()
    {
        var report = _dataManager.LastResolvedSnapshot;
        AddLog(
            $"{_dataManager.State.CurrentYear}年{_dataManager.State.CurrentMonth}月前夕结算：" +
            $"收入 {report.MonthlyIncome:0}，支出 {report.MonthlyExpense:0}，净利润 {report.NetProfit:+0;-0;0}。");
    }

    private void OnActiveEventChanged()
    {
        RefreshEventDialogState();
        RefreshPlayControls();
    }

    private void OnPlayStateChanged()
    {
        RefreshPlayControls();
        RefreshLeftPanel();
    }

    private void OnPlayPausePressed()
    {
        if (_eventManager.HasActiveEvent)
        {
            AddLog("请先处理当前时代事件。");
            return;
        }

        _gameManager.TogglePlay();
        AddLog(_gameManager.IsRunning ? "时间开始流动。" : "时间已暂停。");
    }

    private void OnStepPressed()
    {
        if (_eventManager.HasActiveEvent)
        {
            AddLog("当前有事件待处理，无法推进月份。");
            return;
        }

        _gameManager.StepMonth();
    }

    private void OnSpeedPressed()
    {
        _gameManager.CycleSpeed();
        AddLog($"时间速度调整为 {_gameManager.SpeedLabel}。");
        RefreshPlayControls();
        RefreshLeftPanel();
    }

    private void OnSavePressed()
    {
        AddLog(_gameManager.SaveGame() ? "已保存当前经营状态。" : "保存失败。");
    }

    private void OnLoadPressed()
    {
        AddLog(_gameManager.LoadGame() ? "已读取最近一次存档。" : "没有可读取的存档。");
        RefreshAll();
    }

    private void OnInvestPressed(string projectId)
    {
        if (_dataManager.TryInvestProject(projectId, out var message))
        {
            AddLog(message);
        }
        else
        {
            AddLog($"投资失败：{message}");
        }
    }

    private void OnEventOptionPressed(int optionIndex)
    {
        var activeEvent = _eventManager.ActiveEvent;
        if (activeEvent == null)
        {
            return;
        }

        if (_eventManager.ResolveActiveEvent(optionIndex))
        {
            AddLog($"事件已处理：{activeEvent.Title}");
        }
    }

    private void AddLog(string text)
    {
        _logs.Add($"• {text}");
        while (_logs.Count > 10)
        {
            _logs.RemoveAt(0);
        }

        _logText.Text = string.Join("\n", _logs);
    }

    private static string BuildProjectEffectSummary(ProjectEffectSet effects)
    {
        var lines = new List<string>();
        if (effects.Capacity > 0.0f)
        {
            lines.Add($"产能 +{effects.Capacity:0}");
        }

        if (effects.Demand > 0.0f)
        {
            lines.Add($"需求 +{effects.Demand:0}");
        }

        if (effects.Reputation > 0.0f)
        {
            lines.Add($"声望 +{effects.Reputation:0.#}");
        }

        if (effects.Network > 0.0f)
        {
            lines.Add($"人脉 +{effects.Network:0.#}");
        }

        if (effects.Knowledge > 0.0f)
        {
            lines.Add($"知识 +{effects.Knowledge:0.#}");
        }

        if (effects.Happiness != 0.0f)
        {
            lines.Add($"幸福 {effects.Happiness:+0.#;-0.#;0}");
        }

        if (effects.Competitiveness > 0.0f || effects.CompetitivenessMultiplier > 0.0f)
        {
            lines.Add($"竞争力 +{effects.Competitiveness + effects.CompetitivenessMultiplier:0.##}");
        }

        if (effects.UnitProfit > 0.0f)
        {
            lines.Add($"客单利润 +{effects.UnitProfit:0.#}");
        }

        return lines.Count == 0 ? "暂无数值增益" : string.Join(" · ", lines);
    }

    private static string BuildEventEffectSummary(EventEffectSet effects)
    {
        var parts = new List<string>();
        if (effects.Money != 0)
        {
            parts.Add($"资金 {effects.Money:+#;-#;0}");
        }

        if (effects.Reputation != 0.0f)
        {
            parts.Add($"声望 {effects.Reputation:+0.#;-0.#;0}");
        }

        if (effects.Network != 0.0f)
        {
            parts.Add($"人脉 {effects.Network:+0.#;-0.#;0}");
        }

        if (effects.Knowledge != 0.0f)
        {
            parts.Add($"知识 {effects.Knowledge:+0.#;-0.#;0}");
        }

        if (effects.Happiness != 0.0f)
        {
            parts.Add($"幸福 {effects.Happiness:+0.#;-0.#;0}");
        }

        if (effects.DemandBonus != 0.0f)
        {
            parts.Add($"需求基数 {effects.DemandBonus:+0.#;-0.#;0}");
        }

        if (effects.CompetitivenessBonus != 0.0f)
        {
            parts.Add($"竞争力 {effects.CompetitivenessBonus:+0.##;-0.##;0}");
        }

        if (effects.UnitProfitBonus != 0.0f)
        {
            parts.Add($"利润 {effects.UnitProfitBonus:+0.##;-0.##;0}");
        }

        return parts.Count == 0 ? "无额外数值变化" : string.Join(" · ", parts);
    }
}
