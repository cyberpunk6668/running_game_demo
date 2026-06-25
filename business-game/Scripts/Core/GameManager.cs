using Godot;

namespace BusinessGame;

public partial class GameManager : Node
{
    [Signal]
    public delegate void PlayStateChangedEventHandler();

    public static GameManager? Instance { get; private set; }

    private readonly float[] _waitTimes = [1.0f, 0.6f, 0.3f];

    private Timer? _monthTimer;
    private int _speedIndex;
    private bool _resumeAfterEvent;

    public bool IsRunning { get; private set; }
    public string SpeedLabel => _speedIndex switch
    {
        0 => "1x",
        1 => "2x",
        _ => "4x"
    };

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

    public void BindTimer(Timer timer)
    {
        if (_monthTimer != null)
        {
            _monthTimer.Timeout -= OnMonthTimerTimeout;
        }

        _monthTimer = timer;
        _monthTimer.OneShot = false;
        _monthTimer.Autostart = false;
        _monthTimer.WaitTime = _waitTimes[_speedIndex];
        _monthTimer.Timeout += OnMonthTimerTimeout;
        ApplyTimerState();
    }

    public void TogglePlay()
    {
        if (EventManager.Instance?.HasActiveEvent == true)
        {
            return;
        }

        if (IsRunning)
        {
            Pause();
        }
        else
        {
            Start();
        }
    }

    public void Start()
    {
        IsRunning = true;
        ApplyTimerState();
        EmitSignal(SignalName.PlayStateChanged);
    }

    public void Pause()
    {
        IsRunning = false;
        ApplyTimerState();
        EmitSignal(SignalName.PlayStateChanged);
    }

    public void StepMonth()
    {
        if (EventManager.Instance?.HasActiveEvent == true)
        {
            return;
        }

        DataManager.Instance?.ProcessMonth();
    }

    public void CycleSpeed()
    {
        _speedIndex = (_speedIndex + 1) % _waitTimes.Length;
        ApplyTimerState();
        EmitSignal(SignalName.PlayStateChanged);
    }

    public void PauseForEvent()
    {
        _resumeAfterEvent = IsRunning;
        Pause();
    }

    public void ResumeAfterEvent()
    {
        if (_resumeAfterEvent)
        {
            Start();
        }

        _resumeAfterEvent = false;
    }

    public bool SaveGame()
    {
        return DataManager.Instance?.SaveGame() ?? false;
    }

    public bool LoadGame()
    {
        Pause();
        EventManager.Instance?.ResetActiveEvent();
        return DataManager.Instance?.LoadGame() ?? false;
    }

    private void ApplyTimerState()
    {
        if (_monthTimer == null)
        {
            return;
        }

        _monthTimer.WaitTime = _waitTimes[_speedIndex];
        if (IsRunning)
        {
            _monthTimer.Start();
        }
        else
        {
            _monthTimer.Stop();
        }
    }

    private void OnMonthTimerTimeout()
    {
        if (EventManager.Instance?.HasActiveEvent == true)
        {
            return;
        }

        DataManager.Instance?.ProcessMonth();
    }
}
