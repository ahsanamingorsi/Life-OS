namespace LifeOS.App.Services
{
    /// <summary>
    /// Focus timer shared by the whole app. It counts down to a real end time,
    /// so it stays correct while you move between pages.
    /// </summary>
    public sealed class FocusTimer : IDisposable
    {
        private readonly System.Threading.Timer _tick;

        public FocusTimer()
        {
            _tick = new System.Threading.Timer(_ => OnTick(), null, Timeout.Infinite, Timeout.Infinite);
        }

        public int TotalSeconds { get; private set; } = 25 * 60;
        public int RemainingSeconds { get; private set; } = 25 * 60;
        public bool IsRunning { get; private set; }
        public bool IsOpen { get; private set; }
        public bool JustFinished { get; private set; }
        public string Label { get; set; } = "";
        private DateTime? _endAt;

        public event Action? Changed;

        public string Display => $"{RemainingSeconds / 60:00}:{RemainingSeconds % 60:00}";
        public double Fraction => TotalSeconds == 0 ? 0 : (double)RemainingSeconds / TotalSeconds;

        public void Toggle() { IsOpen = !IsOpen; Changed?.Invoke(); }
        public void Close() { IsOpen = false; Changed?.Invoke(); }

        public void SetMinutes(int minutes)
        {
            minutes = Math.Clamp(minutes, 1, 300);
            Pause();
            TotalSeconds = RemainingSeconds = minutes * 60;
            JustFinished = false;
            Changed?.Invoke();
        }

        public void Start()
        {
            if (RemainingSeconds <= 0) RemainingSeconds = TotalSeconds;
            _endAt = DateTime.UtcNow.AddSeconds(RemainingSeconds);
            IsRunning = true;
            JustFinished = false;
            _tick.Change(250, 250);
            Changed?.Invoke();
        }

        public void Pause()
        {
            _tick.Change(Timeout.Infinite, Timeout.Infinite);
            IsRunning = false;
            _endAt = null;
            Changed?.Invoke();
        }

        public void Reset()
        {
            Pause();
            RemainingSeconds = TotalSeconds;
            JustFinished = false;
            Changed?.Invoke();
        }

        private void OnTick()
        {
            if (_endAt == null) return;
            var left = (int)Math.Ceiling((_endAt.Value - DateTime.UtcNow).TotalSeconds);
            RemainingSeconds = Math.Max(0, left);
            if (RemainingSeconds == 0)
            {
                _tick.Change(Timeout.Infinite, Timeout.Infinite);
                IsRunning = false;
                _endAt = null;
                JustFinished = true;
                IsOpen = true;
            }
            Changed?.Invoke();
        }

        public void Dispose() => _tick.Dispose();
    }
}
