using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionBudget_V2.Data;
using GestionBudget_V2.Models;
using GestionBudget_V2.Services;
using System.Text;
using System.Globalization;

namespace GestionBudget_V2.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ExportService _exportService;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
            _exportService = new ExportService(context);
        }

        // TABLEAU DE BORD
        public IActionResult Dashboard()
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            // Statistiques de base
            var totalBudgets = _context.Budgets.Count();
            var totalDepenses = _context.Depenses.Sum(d => (decimal?)d.Montant) ?? 0;
            var totalComptables = _context.Comptables.Count();
            var budgetTotal = _context.Budgets.Sum(b => (decimal?)b.Montant_alloue) ?? 0;
            var depensesTotal = _context.Depenses.Sum(d => (decimal?)d.Montant) ?? 0;
            var budgetRestant = budgetTotal - depensesTotal;

            var dernieresDepenses = _context.Depenses
                .OrderByDescending(d => d.DateCreation)
                .Take(5)
                .ToList();

            ViewBag.Stats = new
            {
                TotalBudgets = totalBudgets,
                TotalDepenses = totalDepenses,
                TotalComptables = totalComptables,
                BudgetRestant = budgetRestant,
                BudgetTotal = budgetTotal,
                DepensesTotal = depensesTotal
            };

            ViewBag.DernieresDepenses = dernieresDepenses;
            PrepareChartData();

            return View();
        }

        private void PrepareChartData()
        {
            try
            {
                // 1. Dépenses par catégorie
                var depensesParCategorie = _context.Depenses
                    .GroupBy(d => d.Categorie)
                    .Select(g => new
                    {
                        categorie = g.Key ?? "Non catégorisé",
                        total = Math.Round(g.Sum(d => (decimal?)d.Montant) ?? 0, 2)
                    })
                    .OrderByDescending(x => x.total)
                    .ToList();

                if (!depensesParCategorie.Any())
                {
                    depensesParCategorie = new[]
                    {
                        new { categorie = "Fournitures", total = 3000.00m },
                        new { categorie = "Équipement", total = 5500.00m },
                        new { categorie = "Services", total = 12000.00m },
                        new { categorie = "Formation", total = 800.00m },
                        new { categorie = "Déplacement", total = 600.00m }
                    }.ToList();
                }

                ViewBag.DepensesParCategorie = depensesParCategorie;

                // 2. Dépenses des 6 derniers mois
                var sixMois = DateTime.Now.AddMonths(-6);
                var depensesMensuelles = _context.Depenses
                    .Where(d => d.Date_depense >= sixMois)
                    .ToList();

                string[] moisNoms = { "janv.", "févr.", "mars", "avr.", "mai", "juin", "juil.", "août", "sept.", "oct.", "nov.", "déc." };

                var tousLesMois = new List<object>();
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
                ViewBag.BudgetTotal = Math.Round(_context.Budgets.Sum(b => (decimal?)b.Montant_alloue) ?? 10550000, 2);
                ViewBag.DepensesTotal = Math.Round(_context.Depenses.Sum(d => (decimal?)d.Montant) ?? 4500, 2);
            }
            catch (Exception ex)
            {
                ViewBag.DepensesParCategorie = new[]
                {
                    new { categorie = "Fournitures", total = 3000.00m },
                    new { categorie = "Équipement", total = 5500.00m },
                    new { categorie = "Services", total = 12000.00m },
                    new { categorie = "Formation", total = 800.00m },
                    new { categorie = "Déplacement", total = 600.00m }
                };

                ViewBag.DepensesMensuelles = new[]
                {
                    new { mois = "juin 2025", total = 0.00m },
                    new { mois = "juil. 2025", total = 0.00m },
                    new { mois = "août 2025", total = 0.00m },
                    new { mois = "sept. 2025", total = 0.00m },
                    new { mois = "oct. 2025", total = 0.00m },
                    new { mois = "nov. 2025", total = 4500.00m }
                };

                ViewBag.BudgetTotal = 10550000.00m;
                ViewBag.DepensesTotal = 4500.00m;
            }
        }

        // GESTION DES COMPTABLES
        public IActionResult GestionComptables()
        {
            try
            {
                var comptables = _context.Comptables.ToList(); //Recupre les comptables depuis la base de donnes
                return View(comptables);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erreur lors du chargement des comptables : " + ex.Message;
                return View(new List<Comptable>());
            }
        }

        public IActionResult AjouterComptable()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AjouterComptable(Comptable comptable)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    comptable.MotDePasse = PasswordHasher.HashPassword(comptable.MotDePasse);
                    comptable.DateCreation = DateTime.Now;

                    _context.Comptables.Add(comptable);
                    _context.SaveChanges();

                    TempData["Success"] = "Comptable ajouté avec succès !";
                    return RedirectToAction("GestionComptables");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Erreur lors de l'ajout du comptable : " + ex.Message;
                }
            }
            return View(comptable);
        }

        public IActionResult ModifierComptable(int id)
        {
            try
            {
                var comptable = _context.Comptables.Find(id);
                if (comptable == null)
                {
                    TempData["Error"] = "Comptable non trouvé";
                    return RedirectToAction("GestionComptables");
                }
                return View(comptable);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erreur : " + ex.Message;
                return RedirectToAction("GestionComptables");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ModifierComptable(Comptable comptable)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingComptable = _context.Comptables.Find(comptable.Id);
                    if (existingComptable == null)
                    {
                        TempData["Error"] = "Comptable non trouvé";
                        return RedirectToAction("GestionComptables");
                    }

                    existingComptable.Nom = comptable.Nom;
                    existingComptable.Prenom = comptable.Prenom;
                    existingComptable.Email = comptable.Email;

                    if (!string.IsNullOrEmpty(comptable.MotDePasse) && comptable.MotDePasse != existingComptable.MotDePasse)
                    {
                        existingComptable.MotDePasse = PasswordHasher.HashPassword(comptable.MotDePasse);
                    }

                    _context.Comptables.Update(existingComptable);
                    _context.SaveChanges();

                    TempData["Success"] = "Comptable modifié avec succès !";
                    return RedirectToAction("GestionComptables");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Erreur lors de la modification : " + ex.Message;
                }
            }
            return View(comptable);
        }
        // Supprimer un comptable - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SupprimerComptable(int id)
        {
            try
            {
                var comptable = _context.Comptables.Find(id);
                if (comptable != null)
                {
                    _context.Comptables.Remove(comptable);
                    _context.SaveChanges();
                    TempData["Success"] = $"Comptable {comptable.Prenom} {comptable.Nom} supprimé avec succès !";
                }
                else
                {
                    TempData["Error"] = "Comptable non trouvé";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erreur lors de la suppression : " + ex.Message;
            }
            return RedirectToAction("GestionComptables");
        }

        // GESTION DES BUDGETS
        public IActionResult GestionBudgets()
        {
            var budgets = _context.Budgets.ToList();
            return View(budgets);
        }

        public IActionResult DefinirBudget()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DefinirBudget(Budget budget)
        {
            if (ModelState.IsValid)
            {
                _context.Budgets.Add(budget);
                _context.SaveChanges();
                TempData["Success"] = "Budget défini avec succès !";
                return RedirectToAction("GestionBudgets");
            }
            return View(budget);
        }

        public IActionResult ModifierBudget(int id)
        {
            var budget = _context.Budgets.Find(id);
            if (budget == null)
            {
                TempData["Error"] = "Budget non trouvé";
                return RedirectToAction("GestionBudgets");
            }
            return View(budget);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ModifierBudget(Budget budget)
        {
            if (ModelState.IsValid)
            {
                _context.Budgets.Update(budget);
                _context.SaveChanges();
                TempData["Success"] = "Budget modifié avec succès !";
                return RedirectToAction("GestionBudgets");
            }
            return View(budget);
        }

        // Supprimer un budget - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SupprimerBudget(int id)
        {
            try
            {
                var budget = _context.Budgets.Find(id);
                if (budget != null)
                {
                    _context.Budgets.Remove(budget);
                    _context.SaveChanges();
                    TempData["Success"] = $"Budget de {budget.Montant_alloue.ToString("C", new CultureInfo("fr-MA"))} supprimé avec succès !";
                }
                else
                {
                    TempData["Error"] = "Budget non trouvé";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erreur lors de la suppression : " + ex.Message;
            }
            return RedirectToAction("GestionBudgets");
        }
        // RAPPORTS ET CONSULTATION
        public IActionResult Rapports()
        {
            var depenses = _context.Depenses.ToList();

            var totalDepenses = depenses.Sum(d => d.Montant);
            var nombreDepenses = depenses.Count;
            var moyenneDepenses = nombreDepenses > 0 ? totalDepenses / nombreDepenses : 0;

            var depensesParCategorie = depenses
                .GroupBy(d => d.Categorie)
                .Select(g => new { Categorie = g.Key, Total = g.Sum(d => d.Montant) })
                .ToList();

            ViewBag.TotalDepenses = totalDepenses;
            ViewBag.NombreDepenses = nombreDepenses;
            ViewBag.MoyenneDepenses = moyenneDepenses;
            ViewBag.DepensesParCategorie = depensesParCategorie;

            return View(depenses);
        }

        public IActionResult ConsulterDepenses(string categorie, DateTime? dateDebut, DateTime? dateFin)
        {
            var depenses = _context.Depenses.AsQueryable();

            if (!string.IsNullOrEmpty(categorie))
            {
                depenses = depenses.Where(d => d.Categorie == categorie);
            }

            if (dateDebut.HasValue)
            {
                depenses = depenses.Where(d => d.Date_depense >= dateDebut.Value);
            }

            if (dateFin.HasValue)
            {
                depenses = depenses.Where(d => d.Date_depense <= dateFin.Value);
            }

            var result = depenses.ToList();

            ViewBag.Categorie = categorie;
            ViewBag.DateDebut = dateDebut?.ToString("yyyy-MM-dd");
            ViewBag.DateFin = dateFin?.ToString("yyyy-MM-dd");

            return View(result);
        }

        // EXPORTS
        public IActionResult ExportCsv()
        {
            var bytes = _exportService.GenerateCsvReport();
            return File(bytes, "text/csv", $"rapport-depenses-{DateTime.Now:yyyyMMdd-HHmmss}.csv");
        }

        public IActionResult ExportPdf()
        {
            var bytes = _exportService.GeneratePdfReport();
            return File(bytes, "application/pdf", $"rapport-depenses-{DateTime.Now:yyyyMMdd-HHmmss}.pdf");
        }

        public IActionResult ExportCsvDepenses()
        {
            var depenses = _context.Depenses.ToList();
            var csv = new StringBuilder();

            csv.AppendLine("Rapport des Dépenses - Administration");
            csv.AppendLine($"Généré le : {DateTime.Now:dd/MM/yyyy HH:mm}");
            csv.AppendLine();
            csv.AppendLine("ID;Date;Catégorie;Description;Montant;Date Création");

            foreach (var depense in depenses.OrderByDescending(d => d.Date_depense))
            {
                csv.AppendLine($"{depense.Id_depense};{depense.Date_depense:dd/MM/yyyy};{depense.Categorie};{depense.Description};{depense.Montant.ToString("C", CultureInfo.GetCultureInfo("fr-FR"))};{depense.DateCreation:dd/MM/yyyy HH:mm}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"depenses-admin-{DateTime.Now:yyyyMMdd-HHmmss}.csv");
        }

        public IActionResult ExportPdfDepenses()
        {
            var bytes = _exportService.GeneratePdfReport();
            return File(bytes, "application/pdf", $"depenses-admin-{DateTime.Now:yyyyMMdd-HHmmss}.pdf");
        }

        // TEST
        public IActionResult DashboardTest()
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            ViewBag.DepensesParCategorie = new[]
            {
                new { categorie = "Test1", total = 1000.0 },
                new { categorie = "Test2", total = 2000.0 }
            };

            ViewBag.DepensesMensuelles = new[]
            {
                new { mois = "Jan 2024", total = 1500.0 },
                new { mois = "Fév 2024", total = 1800.0 }
            };

            ViewBag.BudgetTotal = 10000.0;
            ViewBag.DepensesTotal = 3500.0;

            return View();
        }
    }
}