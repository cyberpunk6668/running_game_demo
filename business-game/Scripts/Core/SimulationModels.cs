using System;
using System.Collections.Generic;
using Godot;

namespace BusinessGame;

public enum EraType
{
    SpringBreeze,
    Wave,
    Leap,
    Global,
    Digital
}

public enum ProjectType
{
    Production,
    HumanResources,
    Marketing
}

public enum TriggerType
{
    Start,
    DateReached,
    MoneyBelow,
    HappinessBelow,
    ReputationAbove
}

public static class EnumText
{
    public static string ToDisplayName(this EraType era) => era switch
    {
        EraType.SpringBreeze => "春风时代",
        EraType.Wave => "浪潮时代",
        EraType.Leap => "腾跃时代",
        EraType.Global => "全球化时代",
        EraType.Digital => "数字时代",
        _ => "未知时代"
    };

    public static string ToDisplayName(this ProjectType type) => type switch
    {
        ProjectType.Production => "生产车间",
        ProjectType.HumanResources => "人事团队",
        ProjectType.Marketing => "市场营销",
        _ => "未知板块"
    };
}

public sealed class EraDefinition
{
    public EraDefinition()
    {
    }

    public EraDefinition(EraType era, string title, int startYear, int endYear, float baseDemand, float baseUnitProfit, float baseCompetitiveness)
    {
        Era = era;
        Title = title;
        StartYear = startYear;
        EndYear = endYear;
        BaseDemand = baseDemand;
        BaseUnitProfit = baseUnitProfit;
        BaseCompetitiveness = baseCompetitiveness;
    }

    public EraType Era { get; set; } = EraType.SpringBreeze;
    public string Title { get; set; } = "春风时代";
    public int StartYear { get; set; } = 1978;
    public int EndYear { get; set; } = 1984;
    public float BaseDemand { get; set; } = 24.0f;
    public float BaseUnitProfit { get; set; } = 18.0f;
    public float BaseCompetitiveness { get; set; } = 0.9f;
}

public sealed class UnlockCondition
{
    public int MinYear { get; set; } = 1978;
    public int MinMonth { get; set; } = 1;
    public int MinMoney { get; set; } = 0;
    public float MinReputation { get; set; } = 0.0f;
    public EraType? RequiredEra { get; set; }
    public string RequiredProjectId { get; set; } = string.Empty;
    public int RequiredProjectLevel { get; set; } = 0;
}

public sealed class ProjectEffectSet
{
    public float Capacity { get; set; }
    public float Demand { get; set; }
    public float Competitiveness { get; set; }
    public float UnitProfit { get; set; }
    public float MaintenanceDelta { get; set; }
    public float Reputation { get; set; }
    public float Network { get; set; }
    public float Knowledge { get; set; }
    public float Happiness { get; set; }
    public float DemandMultiplier { get; set; }
    public float CapacityMultiplier { get; set; }
    public float CompetitivenessMultiplier { get; set; }
    public float InventoryCostReduction { get; set; }

    public void AddScaled(ProjectEffectSet source, int multiplier)
    {
        Capacity += source.Capacity * multiplier;
        Demand += source.Demand * multiplier;
        Competitiveness += source.Competitiveness * multiplier;
        UnitProfit += source.UnitProfit * multiplier;
        MaintenanceDelta += source.MaintenanceDelta * multiplier;
        Reputation += source.Reputation * multiplier;
        Network += source.Network * multiplier;
        Knowledge += source.Knowledge * multiplier;
        Happiness += source.Happiness * multiplier;
        DemandMultiplier += source.DemandMultiplier * multiplier;
        CapacityMultiplier += source.CapacityMultiplier * multiplier;
        CompetitivenessMultiplier += source.CompetitivenessMultiplier * multiplier;
        InventoryCostReduction += source.InventoryCostReduction * multiplier;
    }
}

public sealed class ProjectDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProjectType Type { get; set; } = ProjectType.Production;
    public int MaxLevel { get; set; } = 1;
    public int BaseUpfrontCost { get; set; } = 500;
    public float CostGrowthFactor { get; set; } = 1.5f;
    public float MaintenanceCost { get; set; } = 20.0f;
    public UnlockCondition UnlockCondition { get; set; } = new();
    public ProjectEffectSet Effects { get; set; } = new();

    public int GetUpgradeCost(int currentLevel)
    {
        return Mathf.RoundToInt(BaseUpfrontCost * Mathf.Pow(CostGrowthFactor, currentLevel));
    }

    public float GetMonthlyMaintenance(int currentLevel)
    {
        return MaintenanceCost * currentLevel;
    }
}

public sealed class EventEffectSet
{
    public int Money { get; set; }
    public float Reputation { get; set; }
    public float Network { get; set; }
    public float Knowledge { get; set; }
    public float Happiness { get; set; }
    public float DemandBonus { get; set; }
    public float CompetitivenessBonus { get; set; }
    public float UnitProfitBonus { get; set; }
}

public sealed class EventOption
{
    public string Text { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public EventEffectSet Effects { get; set; } = new();
}

public sealed class GameEventDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TriggerType TriggerType { get; set; } = TriggerType.Start;
    public string TriggerValue { get; set; } = string.Empty;
    public List<EventOption> Options { get; set; } = new();
    public bool PauseGame { get; set; } = true;
}

public sealed class MarketSnapshot
{
    public float BaseDemand { get; set; }
    public float EffectiveDemand { get; set; }
    public float SupplyCapacity { get; set; }
    public float Competitiveness { get; set; }
    public float UnitProfit { get; set; }
    public float ActualSales { get; set; }
    public float MonthlyIncome { get; set; }
    public float MonthlyExpense { get; set; }
    public float ProjectMaintenance { get; set; }
    public float FixedExpense { get; set; }
    public float InventoryCost { get; set; }
    public float NetProfit { get; set; }
    public float HappinessModifier { get; set; }
    public float PostTickHappinessDelta { get; set; }
    public float PostTickReputationDelta { get; set; }
    public float PostTickKnowledgeDelta { get; set; }
    public float PostTickNetworkDelta { get; set; }
}

public sealed class SimulationState
{
    public int Money { get; set; } = 3000;
    public float Reputation { get; set; } = 10.0f;
    public float Network { get; set; } = 10.0f;
    public float Knowledge { get; set; } = 10.0f;
    public float Happiness { get; set; } = 58.0f;
    public float MonthlyIncome { get; set; }
    public float MonthlyExpense { get; set; }
    public float NetProfit { get; set; }
    public float MarketDemand { get; set; }
    public float SupplyCapacity { get; set; }
    public float MarketCompetitiveness { get; set; }
    public float UnitProfit { get; set; }
    public float InventoryCost { get; set; }
    public int CurrentYear { get; set; } = 1978;
    public int CurrentMonth { get; set; } = 12;
    public EraType CurrentEra { get; set; } = EraType.SpringBreeze;
    public float DemandBonus { get; set; }
    public float CompetitivenessBonus { get; set; }
    public float UnitProfitBonus { get; set; }
    public Dictionary<string, int> ProjectLevels { get; set; } = new();
    public HashSet<string> TriggeredEvents { get; set; } = new();

    public string DateKey => $"{CurrentYear:D4}-{CurrentMonth:D2}";

    public static SimulationState CreateDefault()
    {
        return new SimulationState();
    }

    public void Normalize()
    {
        ProjectLevels ??= new Dictionary<string, int>();
        TriggeredEvents ??= new HashSet<string>();
        CurrentMonth = Math.Clamp(CurrentMonth, 1, 12);
        CurrentYear = Math.Max(CurrentYear, 1978);
    }
}
