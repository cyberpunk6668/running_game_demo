using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace BusinessGame;

public partial class EventManager : Node
{
    [Signal]
    public delegate void ActiveEventChangedEventHandler();

    public static EventManager? Instance { get; private set; }

    private const string EventsPath = "res://Data/events.json";

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly List<GameEventDefinition> _events = new();

    public GameEventDefinition? ActiveEvent { get; private set; }
    public bool HasActiveEvent => ActiveEvent != null;

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
        LoadEvents();

        if (DataManager.Instance != null)
        {
            DataManager.Instance.MonthResolved += OnMonthResolved;
        }

        CallDeferred(nameof(TryTriggerPendingEvent));
    }

    public void ResetActiveEvent()
    {
        ActiveEvent = null;
        EmitSignal(SignalName.ActiveEventChanged);
    }

    public void TryTriggerPendingEvent()
    {
        if (ActiveEvent != null || DataManager.Instance == null)
        {
            return;
        }

        foreach (var definition in _events)
        {
            if (DataManager.Instance.State.TriggeredEvents.Contains(definition.Id))
            {
                continue;
            }

            if (!MatchesTrigger(definition, DataManager.Instance.State))
            {
                continue;
            }

            ActiveEvent = definition;
            if (definition.PauseGame)
            {
                GameManager.Instance?.PauseForEvent();
            }

            EmitSignal(SignalName.ActiveEventChanged);
            return;
        }
    }

    public bool ResolveActiveEvent(int optionIndex)
    {
        if (ActiveEvent == null || DataManager.Instance == null)
        {
            return false;
        }

        if (optionIndex < 0 || optionIndex >= ActiveEvent.Options.Count)
        {
            return false;
        }

        var selectedOption = ActiveEvent.Options[optionIndex];
        DataManager.Instance.ApplyEventEffects(selectedOption.Effects);
        DataManager.Instance.State.TriggeredEvents.Add(ActiveEvent.Id);
        var shouldResume = ActiveEvent.PauseGame;
        ActiveEvent = null;
        EmitSignal(SignalName.ActiveEventChanged);

        if (shouldResume)
        {
            GameManager.Instance?.ResumeAfterEvent();
        }

        CallDeferred(nameof(TryTriggerPendingEvent));
        return true;
    }

    private void LoadEvents()
    {
        _events.Clear();
        if (!FileAccess.FileExists(EventsPath))
        {
            GD.PushError($"找不到事件数据：{EventsPath}");
            return;
        }

        var json = FileAccess.GetFileAsString(EventsPath);
        var parsedEvents = JsonSerializer.Deserialize<List<GameEventDefinition>>(json, _jsonOptions) ?? new List<GameEventDefinition>();
        _events.AddRange(parsedEvents);
    }

    private void OnMonthResolved()
    {
        TryTriggerPendingEvent();
    }

    private static bool MatchesTrigger(GameEventDefinition definition, SimulationState state)
    {
        return definition.TriggerType switch
        {
            TriggerType.Start => true,
            TriggerType.DateReached => MatchesDate(definition.TriggerValue, state),
            TriggerType.MoneyBelow => int.TryParse(definition.TriggerValue, out var threshold) && state.Money <= threshold,
            TriggerType.HappinessBelow => float.TryParse(definition.TriggerValue, out var happiness) && state.Happiness <= happiness,
            TriggerType.ReputationAbove => float.TryParse(definition.TriggerValue, out var reputation) && state.Reputation >= reputation,
            _ => false
        };
    }

    private static bool MatchesDate(string triggerValue, SimulationState state)
    {
        var parts = triggerValue.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 || !int.TryParse(parts[0], out var year) || !int.TryParse(parts[1], out var month))
        {
            return false;
        }

        return state.CurrentYear > year || (state.CurrentYear == year && state.CurrentMonth >= month);
    }
}
