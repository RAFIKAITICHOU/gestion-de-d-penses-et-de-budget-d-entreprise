using Microsoft.EntityFrameworkCore;
using GestionBudget_V2.Data;
using GestionBudget_V2.Services;
using GestionBudget_V2.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

// Configuration de la base de données MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var app = builder.Build();

// Créer le compte administrateur si il n'existe pas
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Vérifier si la table Administrateurs existe
        context.Database.EnsureCreated();

        // Vérifier si l'administrateur existe déjà
        var adminExists = context.Administrateurs.Any(a => a.Email == "admin@entreprise.com");

        if (!adminExists)
        {
            var admin = new Administrateur // ← Changé de Comptable à Administrateur
            {
                Nom = "Admin",
                Prenom = "System",
                Email = "admin@entreprise.com",
                MotDePasse = PasswordHasher.HashPassword("admin123"),
                DateCreation = DateTime.Now
            };

            context.Administrateurs.Add(admin);
            context.SaveChanges();

            Console.WriteLine("✅ Compte ADMINISTRATEUR créé avec succès !");
            Console.WriteLine($"📧 Email: admin@entreprise.com");
            Console.WriteLine($"🔑 Mot de passe: admin123");
            Console.WriteLine("👤 Type: Administrateur");
        }
        else
        {
            Console.WriteLine("⚠️  Le compte administrateur existe déjà.");
        }
        
        // Optionnel: Vérifier aussi s'il y a des comptables
        var comptableCount = context.Comptables.Count();
        Console.WriteLine($"📊 Nombre de comptables: {comptableCount}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erreur lors de la création du compte administrateur: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

// Configuration des routes
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=Dashboard}/{id?}",
    defaults: new { controller = "Admin" });

app.MapControllerRoute(
    name: "comptable",
    pattern: "Comptable/{action=Dashboard}/{id?}",
    defaults: new { controller = "Comptable" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();