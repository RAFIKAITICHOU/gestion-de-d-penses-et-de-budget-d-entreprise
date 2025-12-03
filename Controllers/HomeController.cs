using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionBudget_V2.Data;
using GestionBudget_V2.Models;
using GestionBudget_V2.Services;
using Microsoft.Extensions.Configuration;

namespace GestionBudget_V2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        // UN SEUL CONSTRUCTEUR
        public HomeController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _emailService = new EmailService(configuration);
        }

        // Page d'accueil - Redirige vers Login
        public IActionResult Index()
        {
            return RedirectToAction("Login");
        }

        // Page de connexion
        public IActionResult Login()
        {
            // Vérifier si déjà connecté
            if (HttpContext.Session.GetInt32("AdminId") != null)
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            if (HttpContext.Session.GetInt32("ComptableId") != null)
            {
                return RedirectToAction("Dashboard", "Comptable");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string motDePasse)
        {
            // Essayer d'abord comme administrateur
            var admin = _context.Administrateurs.FirstOrDefault(a => a.Email == email);
            if (admin != null && PasswordHasher.VerifyPassword(motDePasse, admin.MotDePasse))
            {
                HttpContext.Session.SetInt32("AdminId", admin.Id);
                HttpContext.Session.SetString("AdminNom", $"{admin.Prenom} {admin.Nom}");
                return RedirectToAction("Dashboard", "Admin");
            }

            // Essayer ensuite comme comptable
            var comptable = _context.Comptables.FirstOrDefault(c => c.Email == email);
            if (comptable != null && PasswordHasher.VerifyPassword(motDePasse, comptable.MotDePasse))
            {
                HttpContext.Session.SetInt32("ComptableId", comptable.Id);
                HttpContext.Session.SetString("ComptableNom", $"{comptable.Prenom} {comptable.Nom}");
                return RedirectToAction("Dashboard", "Comptable");
            }

            ViewBag.Error = "Email ou mot de passe incorrect";
            return View();
        }

        // Mot de passe oublié - Formulaire
        public IActionResult ForgotPassword()
        {
            return View("Login");
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    ViewBag.ForgotError = "Veuillez entrer votre adresse email";
                    return View("Login");
                }

                var nouveauMotDePasse = GenerateRandomPassword();
                var motDePasseHache = PasswordHasher.HashPassword(nouveauMotDePasse);

                bool utilisateurTrouve = false;
                string typeUtilisateur = "";

                // Essayer d'abord comme administrateur
                var admin = _context.Administrateurs.FirstOrDefault(a => a.Email == email);
                if (admin != null)
                {
                    admin.MotDePasse = motDePasseHache;
                    _context.SaveChanges();
                    utilisateurTrouve = true;
                    typeUtilisateur = "Administrateur";
                    await _emailService.SendPasswordResetEmail(email, typeUtilisateur, nouveauMotDePasse);
                }
                else
                {
                    // Essayer comme comptable
                    var comptable = _context.Comptables.FirstOrDefault(c => c.Email == email);
                    if (comptable != null)
                    {
                        comptable.MotDePasse = motDePasseHache;
                        _context.SaveChanges();
                        utilisateurTrouve = true;
                        typeUtilisateur = "Comptable";
                        await _emailService.SendPasswordResetEmail(email, typeUtilisateur, nouveauMotDePasse);
                    }
                }

                if (utilisateurTrouve)
                {
                    ViewBag.ForgotSuccess = "Un nouveau mot de passe a été envoyé à votre adresse email";
                }
                else
                {
                    ViewBag.ForgotError = "Aucun utilisateur trouvé avec cet email";
                }
            }
            catch (Exception)
            {
                ViewBag.ForgotError = "Une erreur est survenue lors de la réinitialisation du mot de passe";
            }

            return View("Login");
        }

        // Générer un mot de passe aléatoire
        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var password = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return password + "1!";
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}