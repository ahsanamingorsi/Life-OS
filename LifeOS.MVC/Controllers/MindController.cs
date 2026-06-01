using LifeOS.MVC.Data;
using LifeOS.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace LifeOS.MVC.Controllers
{
    /// <summary>
    /// Mind module — shows habits filtered to categories "Mind" and "Learning".
    /// All CRUD goes through HabitsController.
    /// </summary>
    public class MindController : Controller
    {
        private readonly AppDbContext _db;
        private readonly HealthController _healthCtrl; // reuses the builder

        public MindController(AppDbContext db, ProductivityService productivity)
        {
            _db = db;
            _healthCtrl = new HealthController(db, productivity);
        }

        public async Task<IActionResult> Index()
        {
            // Mind page shows both "Mind" and "Learning" category habits
            var mindVm = await _healthCtrl.BuildViewModelAsync(
                "Mind",
                "Mind & Learning",
                "Sharpen your intellect. Build mental habits daily.");

            // Merge in "Learning" category habits as well
            var learningVm = await _healthCtrl.BuildViewModelAsync(
                "Learning", "", "");

            foreach (var h in learningVm.Habits)
            {
                if (!mindVm.Habits.Any(x => x.Habit.Id == h.Habit.Id))
                    mindVm.Habits.Add(h);
            }

            foreach (var kv in learningVm.Last7DaysCompletion)
            {
                if (mindVm.Last7DaysCompletion.ContainsKey(kv.Key))
                    mindVm.Last7DaysCompletion[kv.Key] += kv.Value;
                else
                    mindVm.Last7DaysCompletion[kv.Key] = kv.Value;
            }

            return View(mindVm);
        }
    }
}