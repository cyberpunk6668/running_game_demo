using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace BusinessGame;

public partial class DataManager : Node
{
    [Signal]
    public delegate void StateChangedEventHandler();

    [Signal]
    public delegate void ProjectPortfolioChangedEventHandler();

    [Signal]
    public delegate void MonthResolvedEventHandler();

    public static DataManager? Instance { get; private set; }

    private const string ProjectsPath = "res://Data/projects.json";
    private const string SavePath = "user://business_game_save.json";

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly List<EraDefinition> _eras =
    [
        new EraDefinition(EraType.SpringBreeze, "春风时代", 1978, 1984, 26.0f, 19.0f, 0.82f),
        new EraDefinition(EraType.Wave, "浪潮时代", 1985, 1992, 44.0f, 25.0f, 1.05f),
        new EraDefinition(EraType.Leap, "腾跃时代", 1993, 2001, 64.0f, 31.0f, 1.24f),
        new EraDefinition(EraType.Global, "全球化时代", 2002, 2008, 88.0f, 37.0f, 1.42f),
        new EraDefinition(EraType.Digital, "数字时代", 2009, 2100, 118.0f, 45.0f, 1.68f)
    ];

    private readonly List<ProjectDefinition> _projects = new();
    private readonly Dictionary<string, ProjectDefinition> _projectLookup = new();

    public SimulationState State { get; private set; } = SimulationState.CreateDefault();
    public MarketSnapshot CurrentSnapshot { get; private set; } = new();
    public MarketSnapshot LastResolvedSnapshot { get; private set; } = new();
    public IReadOnlyList<ProjectDefinition> Projects => _projects;
    public IReadOnlyList<EraDefinition> Eras => _eras;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public override void _Ready()
    {
        LoadProjectDefinitions();
        InitializeNewGame();
    }

    public void InitializeNewGame()
    {
        State = SimulationState.CreateDefault();
        UpdateCurrentEra();
        RefreshForecast(false);
        EmitSignal(SignalName.StateChanged);
        EmitSignal(SignalName.ProjectPortfolioChanged);
    }

    public IEnumerable<ProjectDefinition> GetProjectsByType(ProjectType type)
    {
        return _projects.Where(project => project.Type == type);
    }

    public int GetProjectLevel(string projectId)
    {
        return State.ProjectLevels.TryGetValue(projectId, out var level) ? level : 0;
    }

    public int GetOwnedProjectCount(ProjectType? type = null)
    {
        return _projects.Count(project => GetProjectLevel(project.Id) > 0 && (!type.HasValue || project.Type == type.Value));
    }

    public float GetDisplayReputation()
    {
        var effects = GetAggregateEffects();
        return GetDisplayReputation(effects);
    }

    public float GetDisplayNetwork()
    {
        var effects = GetAggregateEffects();
        return GetDisplayNetwork(effects);
    }

    public float GetDisplayKnowledge()
    {
        var effects = GetAggregateEffects();
        return GetDisplayKnowledge(effects);
    }

    public float GetDisplayHappiness()
    {
        var effects = GetAggregateEffects();
        return GetDisplayHappiness(effects);
    }

    public EraDefinition GetCurrentEraDefinition()
    {
        return _eras.First(era => State.CurrentYear >= era.StartYear && State.CurrentYear <= era.EndYear);
    }

    public string GetCurrentEraTitle()
    {
        return GetCurrentEraDefinition().Title;
    }

    public bool IsUnlocked(ProjectDefinition project)
    {
        var unlock = project.UnlockCondition;
        if (State.CurrentYear < unlock.MinYear)
        {
            return false;
        }

        if (State.CurrentYear == unlock.MinYear && State.CurrentMonth < unlock.MinMonth)
        {
            return false;
        }

        if (State.Money < unlock.MinMoney || GetDisplayReputation() < unlock.MinReputation)
        {
            return false;
        }

        if (unlock.RequiredEra.HasValue && State.CurrentEra != unlock.RequiredEra.Value)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(unlock.RequiredProjectId) && GetProjectLevel(unlock.RequiredProjectId) < unlock.RequiredProjectLevel)
        {
            return false;
        }

        return true;
    }

    public string GetUnlockHint(ProjectDefinition project)
    {
        if (IsUnlocked(project))
        {
            return "已解锁";
        }

        var unlock = project.UnlockCondition;
        var hints = new List<string>();

        if (State.CurrentYear < unlock.MinYear || (State.CurrentYear == unlock.MinYear && State.CurrentMonth < unlock.MinMonth))
        {
            hints.Add($"时间 {unlock.MinYear}年{unlock.MinMonth}月");
        }

        if (State.Money < unlock.MinMoney)
        {
            hints.Add($"资金 ≥ {unlock.MinMoney}元");
        }

        if (GetDisplayReputation() < unlock.MinReputation)
        {
            hints.Add($"声望 ≥ {unlock.MinReputation:0}");
        }

        if (unlock.RequiredEra.HasValue && State.CurrentEra != unlock.RequiredEra.Value)
        {
            hints.Add($"时代：{unlock.RequiredEra.Value.ToDisplayName()}");
        }

        if (!string.IsNullOrWhiteSpace(unlock.RequiredProjectId) && _projectLookup.TryGetValue(unlock.RequiredProjectId, out var requiredProject))
        {
            hints.Add($"{requiredProject.Name} Lv.{unlock.RequiredProjectLevel}");
        }

        return hints.Count == 0 ? "条件未满足" : string.Join(" / ", hints);
    }

    public bool TryInvestProject(string projectId, out string message)
    {
        if (!_projectLookup.TryGetValue(projectId, out var project))
        {
            message = "项目不存在。";
            return false;
        }

        var currentLevel = GetProjectLevel(projectId);
        if (currentLevel >= project.MaxLevel)
        {
            message = "项目已满级。";
            return false;
        }

        if (!IsUnlocked(project))
        {
            message = $"尚未解锁：{GetUnlockHint(project)}";
            return false;
        }

        var cost = project.GetUpgradeCost(currentLevel);
        if (State.Money < cost)
        {
            message = $"资金不足，还差 {cost - State.Money} 元。";
            return false;
        }

        State.Money -= cost;
        State.ProjectLevels[projectId] = currentLevel + 1;
        RefreshForecast(false);
        EmitSignal(SignalName.StateChanged);
        EmitSignal(SignalName.ProjectPortfolioChanged);
        message = $"{project.Name} 升至 Lv.{currentLevel + 1}，花费 {cost} 元。";
        return true;
    }

    public void ApplyEventEffects(EventEffectSet effects)
    {
        State.Money += effects.Money;
        State.Reputation = ClampGeneral(State.Reputation + effects.Reputation);
        State.Network = ClampGeneral(State.Network + effects.Network);
        State.Knowledge = ClampGeneral(State.Knowledge + effects.Knowledge);
        State.Happiness = ClampHappiness(State.Happiness + effects.Happiness);
        State.DemandBonus += effects.DemandBonus;
        State.CompetitivenessBonus += effects.CompetitivenessBonus;
        State.UnitProfitBonus += effects.UnitProfitBonus;
        RefreshForecast(false);
        EmitSignal(SignalName.StateChanged);
        EmitSignal(SignalName.ProjectPortfolioChanged);
    }

    public void ProcessMonth()
    {
        LastResolvedSnapshot = BuildSnapshot();
        State.Money += Mathf.RoundToInt(LastResolvedSnapshot.NetProfit);
        State.Happiness = ClampHappiness(State.Happiness + LastResolvedSnapshot.PostTickHappinessDelta);
        State.Reputation = ClampGeneral(State.Reputation + LastResolvedSnapshot.PostTickReputationDelta);
        State.Knowledge = ClampGeneral(State.Knowledge + LastResolvedSnapshot.PostTickKnowledgeDelta);
        State.Network = ClampGeneral(State.Network + LastResolvedSnapshot.PostTickNetworkDelta);

        AdvanceCalendar();
        UpdateCurrentEra();
        RefreshForecast(false);

        EmitSignal(SignalName.StateChanged);
        EmitSignal(SignalName.ProjectPortfolioChanged);
        EmitSignal(SignalName.MonthResolved);
    }

    public bool SaveGame()
    {
        var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
        if (file == null)
        {
            GD.PushError("无法创建存档文件。");
            return false;
        }

        file.StoreString(JsonSerializer.Serialize(State, _jsonOptions));
        return true;
    }

    public bool LoadGame()
    {
        if (!FileAccess.FileExists(SavePath))
        {
            return false;
        }

        var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PushError("无法读取存档文件。");
            return false;
        }

        var loadedState = JsonSerializer.Deserialize<SimulationState>(file.GetAsText(), _jsonOptions);
        if (loadedState == null)
        {
            GD.PushError("存档内容无效。");
            return false;
        }

        loadedState.Normalize();
        State = loadedState;
        UpdateCurrentEra();
        RefreshForecast(false);
        EmitSignal(SignalName.StateChanged);
        EmitSignal(SignalName.ProjectPortfolioChanged);
        return true;
    }

    private void LoadProjectDefinitions()
    {
        _projects.Clear();
        _projectLookup.Clear();

        if (!FileAccess.FileExists(ProjectsPath))
        {
            GD.PushError($"找不到项目数据：{ProjectsPath}");
            return;
        }

        var json = FileAccess.GetFileAsString(ProjectsPath);
        var parsedProjects = JsonSerializer.Deserialize<List<ProjectDefinition>>(json, _jsonOptions) ?? new List<ProjectDefinition>();

        foreach (var project in parsedProjects)
        {
            _projects.Add(project);
            _projectLookup[project.Id] = project;
        }
    }

    private void AdvanceCalendar()
    {
        State.CurrentMonth += 1;
        if (State.CurrentMonth > 12)
        {
            State.CurrentMonth = 1;
            State.CurrentYear += 1;
        }
    }

    private void UpdateCurrentEra()
    {
        State.CurrentEra = GetCurrentEraDefinition().Era;
    }

    private void RefreshForecast(bool emitSignals)
    {
        CurrentSnapshot = BuildSnapshot();
        State.MonthlyIncome = CurrentSnapshot.MonthlyIncome;
        State.MonthlyExpense = CurrentSnapshot.MonthlyExpense;
        State.NetProfit = CurrentSnapshot.NetProfit;
        State.MarketDemand = CurrentSnapshot.EffectiveDemand;
        State.SupplyCapacity = CurrentSnapshot.SupplyCapacity;
        State.MarketCompetitiveness = CurrentSnapshot.Competitiveness;
        State.UnitProfit = CurrentSnapshot.UnitProfit;
        State.InventoryCost = CurrentSnapshot.InventoryCost;

        if (emitSignals)
        {
            EmitSignal(SignalName.StateChanged);
            EmitSignal(SignalName.ProjectPortfolioChanged);
        }
    }

    private MarketSnapshot BuildSnapshot()
    {
        var era = GetCurrentEraDefinition();
        var effects = GetAggregateEffects();

        var displayReputation = GetDisplayReputation(effects);
        var displayNetwork = GetDisplayNetwork(effects);
        var displayKnowledge = GetDisplayKnowledge(effects);
        var displayHappiness = GetDisplayHappiness(effects);

        var happinessModifier = Mathf.Clamp(0.55f + displayHappiness / 100.0f, 0.45f, 1.40f);
        var networkDemandFactor = 1.0f + displayNetwork * 0.0025f;

        var baseDemand = era.BaseDemand + State.DemandBonus;
        var effectiveDemand = (baseDemand + effects.Demand + displayReputation * 0.85f) * (1.0f + effects.DemandMultiplier) * networkDemandFactor;
        var supplyCapacity = (12.0f + effects.Capacity) * happinessModifier * (1.0f + effects.CapacityMultiplier);
        var competitiveness = Mathf.Clamp(
            era.BaseCompetitiveness +
            State.CompetitivenessBonus +
            effects.Competitiveness +
            displayReputation * 0.015f +
            displayKnowledge * 0.010f +
            displayNetwork * 0.007f +
            effects.CompetitivenessMultiplier,
            0.45f,
            4.50f);

        var unitProfit = era.BaseUnitProfit + State.UnitProfitBonus + effects.UnitProfit + displayKnowledge * 0.12f + displayReputation * 0.04f;
        var actualSales = MathF.Min(supplyCapacity, effectiveDemand * competitiveness);
        var monthlyIncome = actualSales * unitProfit;

        var projectMaintenance = GetProjectMaintenanceCost() + effects.MaintenanceDelta;
        var fixedExpense = 120.0f + MathF.Max(0.0f, 55.0f - displayHappiness) * 1.8f;
        var inventoryCost = MathF.Max(0.0f, supplyCapacity - actualSales) * MathF.Max(1.15f - effects.InventoryCostReduction, 0.25f);
        var monthlyExpense = projectMaintenance + fixedExpense + inventoryCost;
        var netProfit = monthlyIncome - monthlyExpense;

        var postTickHappinessDelta = netProfit >= 0.0f ? 0.45f : -1.35f;
        if (monthlyExpense > monthlyIncome * 0.92f)
        {
            postTickHappinessDelta -= 0.35f;
        }

        if (displayHappiness < 42.0f)
        {
            postTickHappinessDelta -= 0.45f;
        }

        var postTickReputationDelta = netProfit >= 0.0f ? MathF.Min(0.85f, netProfit / 2200.0f) : -0.35f;
        if (displayHappiness > 78.0f)
        {
            postTickReputationDelta += 0.15f;
        }

        var postTickKnowledgeDelta = MathF.Max(0.0f, effects.Knowledge) * 0.035f;
        var postTickNetworkDelta = MathF.Max(0.0f, effects.Network) * 0.030f;

        return new MarketSnapshot
        {
            BaseDemand = baseDemand,
            EffectiveDemand = effectiveDemand,
            SupplyCapacity = supplyCapacity,
            Competitiveness = competitiveness,
            UnitProfit = unitProfit,
            ActualSales = actualSales,
            MonthlyIncome = monthlyIncome,
            ProjectMaintenance = projectMaintenance,
            FixedExpense = fixedExpense,
            InventoryCost = inventoryCost,
            MonthlyExpense = monthlyExpense,
            NetProfit = netProfit,
            HappinessModifier = happinessModifier,
            PostTickHappinessDelta = postTickHappinessDelta,
            PostTickReputationDelta = postTickReputationDelta,
            PostTickKnowledgeDelta = postTickKnowledgeDelta,
            PostTickNetworkDelta = postTickNetworkDelta
        };
    }

    private ProjectEffectSet GetAggregateEffects()
    {
        var aggregate = new ProjectEffectSet();
        foreach (var project in _projects)
        {
            var level = GetProjectLevel(project.Id);
            if (level <= 0)
            {
                continue;
            }

            aggregate.AddScaled(project.Effects, level);
        }

        return aggregate;
    }

    private float GetProjectMaintenanceCost()
    {
        return _projects.Sum(project => project.GetMonthlyMaintenance(GetProjectLevel(project.Id)));
    }

    private float GetDisplayReputation(ProjectEffectSet effects)
    {
        return ClampGeneral(State.Reputation + effects.Reputation);
    }

    private float GetDisplayNetwork(ProjectEffectSet effects)
    {
        return ClampGeneral(State.Network + effects.Network);
    }

    private float GetDisplayKnowledge(ProjectEffectSet effects)
    {
        return ClampGeneral(State.Knowledge + effects.Knowledge);
    }

    private float GetDisplayHappiness(ProjectEffectSet effects)
    {
        return ClampHappiness(State.Happiness + effects.Happiness);
    }

    private static float ClampGeneral(float value)
    {
        return Mathf.Clamp(value, 0.0f, 200.0f);
    }

    private static float ClampHappiness(float value)
    {
        return Mathf.Clamp(value, 0.0f, 100.0f);
    }
}
