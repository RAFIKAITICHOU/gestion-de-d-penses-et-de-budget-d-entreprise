using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionBudget_V2.Data;
using GestionBudget_V2.Models;

namespace GestionBudget_V2.Controllers
{
    public class ComptableController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComptableController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Tableau de bord Comptable
        public IActionResult Dashboard()
        {
            var comptableId = HttpContext.Session.GetInt32("ComptableId");
            if (comptableId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var stats = new
            {
                TotalDepenses = _context.Depenses.Count(),
                MontantTotalDepenses = _context.Depenses.Sum(d => d.Montant),
                BudgetActuel = _context.Budgets.OrderByDescending(b => b.Date_alloue).FirstOrDefault()?.Montant_alloue ?? 0
            };

            ViewBag.Stats = stats;
            return View();
        }

        // Gestion des dépenses
        public IActionResult GestionDepenses()
        {
            var depenses = _context.Depenses.ToList();
            return View(depenses);
        }

        // Ajouter une dépense
        public IActionResult AjouterDepense()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AjouterDepense(Depense depense)
        {
            if (ModelState.IsValid)
            {
                _context.Depenses.Add(depense);
                _context.SaveChanges();
                return RedirectToAction("GestionDepenses");
            }
            return View(depense);
        }

        // Supprimer une dépense (Comptable)
        [HttpPost]
        public IActionResult SupprimerDepense(int id)
        {
            var dep = _context.Depenses.Find(id);
            if (dep == null) return NotFound();
            try
            {
                _context.Depenses.Remove(dep);
                _context.SaveChanges();
            }
            catch
            {
                // log or handle error
            }
            return RedirectToAction("GestionDepenses");
        }

        // Consulter les budgets
        public IActionResult ConsulterBudgets()
        {
            var budgets = _context.Budgets.ToList();
            return View(budgets);
        }

        // Consulter les rapports
        public IActionResult Rapports()
        {
            var depenses = _context.Depenses.ToList();
            return View(depenses);
        }
    }
}