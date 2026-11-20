namespace LifeOS.App.Services
{
    /// <summary>Lets any page open a popup form, announce saved data and show a toast.</summary>
    public sealed class AppUi
    {
        public event Action<string, int, int?>? FormRequested;
        public event Action? DataChanged;
        public event Action<string>? ToastRequested;

        /// <summary>
        /// kind: task, goal, project, project-task (id = project), project-task-edit (id = task),
        /// study-session, skill-log, study-note. id 0 = new. context: for "task", the goal to link.
        /// </summary>
        public void Open(string kind, int id = 0, int? context = null) => FormRequested?.Invoke(kind, id, context);
        public void NotifyDataChanged() => DataChanged?.Invoke();
        public void Toast(string message) => ToastRequested?.Invoke(message);
    }
}
