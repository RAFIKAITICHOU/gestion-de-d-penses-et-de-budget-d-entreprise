using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionBudget_V2.Data;
using GestionBudget_V2.Models;
using System.Globalization;

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

            // Statistiques de base
            var totalDepenses = _context.Depenses.Count();
            var montantTotalDepenses = _context.Depenses.Sum(d => d.Montant);
            var budgetActuel =
                _context.Budgets.OrderByDescending(b => b.Date_alloue).FirstOrDefault()?.Montant_alloue ?? 0;

            var stats = new
            {
                TotalDepenses = totalDepenses,
                MontantTotalDepenses = montantTotalDepenses,
                BudgetActuel = budgetActuel
            };

            ViewBag.Stats = stats;

            // Données pour les graphiques
            // 1. Dépenses par catégorie
            var depensesParCategorie = _context.Depenses
                .GroupBy(d => d.Categorie)
                .Select(g => new
                {
                    categorie = g.Key ?? "Non catégorisé",
                    total = Math.Round(g.Sum(d => d.Montant), 2)
                })
                .OrderByDescending(x => x.total)
                .Take(6) // Limiter à 6 catégories pour le graphique
                .ToList();

            ViewBag.DepensesParCategorie = depensesParCategorie;

            // 2. Dépenses des 6 derniers mois
            var sixMois = DateTime.Now.AddMonths(-6);
            var depensesMensuelles = _context.Depenses
                .Where(d => d.Date_depense >= sixMois)
                .ToList();

            // Noms des mois en français
            string[] moisNoms =
                { "janv.", "févr.", "mars", "avr.", "mai", "juin", "juil.", "août", "sept.", "oct.", "nov.", "déc." };

            var tousLesMois = new List<dynamic>();
            for (int i = 5; i >= 0; i--)
            {
                var date = DateTime.Now.AddMonths(-i);
                var moisKey = $"{moisNoms[date.Month - 1]} {date.Year}";

                var totalMois = depensesMensuelles
                    .Where(d => d.Date_depense.Month == date.Month && d.Date_depense.Year == date.Year)
                    .Sum(d => d.Montant);

                tousLesMois.Add(new
                {
                    mois = moisKey,
                    total = Math.Round(totalMois, 2)
                });
            }

            ViewBag.DepensesMensuelles = tousLesMois;

            // 3. Dépenses du mois en cours
            var moisEnCours = DateTime.Now.Month;
            var anneeEnCours = DateTime.Now.Year;
            var depensesMois = _context.Depenses
                .Where(d => d.Date_depense.Month == moisEnCours && d.Date_depense.Year == anneeEnCours)
                .Sum(d => d.Montant);

            ViewBag.DepensesMois = depensesMois;

            // 4. 5 dernières dépenses
            var dernieresDepenses = _context.Depenses
                .OrderByDescending(d => d.Date_depense)
                .Take(5)
                .Select(d => new
                {
                    d.Id_depense,
                    d.Date_depense,
                    d.Categorie,
                    d.Description,
                    d.Montant
                })
                .ToList();

            ViewBag.DernieresDepenses = dernieresDepenses;

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
// Modifier une dépense - GET
        public IActionResult ModifierDepense(int id)
        {
            var comptableId = HttpContext.Session.GetInt32("ComptableId");
            if (comptableId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var depense = _context.Depenses.Find(id);
            if (depense == null)
            {
                TempData["Error"] = "Dépense non trouvée";
                return RedirectToAction("GestionDepenses");
            }

            return View(depense);
        }

// Modifier une dépense - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ModifierDepense(Depense depense)
        {
            var comptableId = HttpContext.Session.GetInt32("ComptableId");
            if (comptableId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingDepense = _context.Depenses.Find(depense.Id_depense);
                    if (existingDepense == null)
                    {
                        TempData["Error"] = "Dépense non trouvée";
                        return RedirectToAction("GestionDepenses");
                    }

                    // Mettre à jour les propriétés
                    existingDepense.Date_depense = depense.Date_depense;
                    existingDepense.Montant = depense.Montant;
                    existingDepense.Categorie = depense.Categorie;
                    existingDepense.Description = depense.Description;
                    existingDepense.DateCreation = DateTime.Now;

                    _context.Depenses.Update(existingDepense);
                    _context.SaveChanges();

                    TempData["Success"] = $"Dépense #{depense.Id_depense} modifiée avec succès !";
                    return RedirectToAction("GestionDepenses");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Une erreur est survenue lors de la modification : " + ex.Message);
                    TempData["Error"] = "Erreur lors de la modification de la dépense";
                }
            }

            return View(depense);
        }
    }
}