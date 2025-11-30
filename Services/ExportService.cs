using System.Text;
using System.Globalization;
using GestionBudget_V2.Data;
using GestionBudget_V2.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GestionBudget_V2.Services
{
    public class ExportService
    {
        private readonly ApplicationDbContext _context;

        public ExportService(ApplicationDbContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateCsvReport()
        {
            var depenses = _context.Depenses.ToList();
            var budgets = _context.Budgets.ToList();

            var csv = new StringBuilder();

            // En-tête
            csv.AppendLine("Rapport des Dépenses - Gestion Budget");
            csv.AppendLine($"Généré le : {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}");
            csv.AppendLine();

            // Statistiques
            csv.AppendLine("STATISTIQUES GÉNÉRALES");
            csv.AppendLine($"Total Dépenses;{depenses.Sum(d => d.Montant).ToString("C", CultureInfo.GetCultureInfo("fr-FR"))}");
            csv.AppendLine($"Nombre de Dépenses;{depenses.Count}");
            csv.AppendLine($"Dépense Moyenne;{(depenses.Count > 0 ? depenses.Average(d => d.Montant).ToString("C", CultureInfo.GetCultureInfo("fr-FR")) : "0,00 MDH")}");
            csv.AppendLine($"Nombre de Budgets;{budgets.Count}");
            csv.AppendLine();

            // Dépenses par catégorie
            csv.AppendLine("DÉPENSES PAR CATÉGORIE");
            csv.AppendLine("Catégorie;Total;Pourcentage");
            var depensesParCategorie = depenses
                .GroupBy(d => d.Categorie)
                .Select(g => new { Categorie = g.Key, Total = g.Sum(d => d.Montant) });

            foreach (var item in depensesParCategorie)
            {
                var pourcentage = depenses.Sum(d => d.Montant) > 0 ?
                    (item.Total / depenses.Sum(d => d.Montant) * 100) : 0;
                csv.AppendLine($"{item.Categorie};{item.Total.ToString("C", CultureInfo.GetCultureInfo("fr-FR"))};{pourcentage.ToString("F1")}%");
            }
            csv.AppendLine();

            // Détail des dépenses
            csv.AppendLine("DÉTAIL DES DÉPENSES");
            csv.AppendLine("ID;Date;Catégorie;Description;Montant;Date Création");
            foreach (var depense in depenses.OrderByDescending(d => d.Date_depense))
            {
                csv.AppendLine($"{depense.Id_depense};{depense.Date_depense.ToString("dd/MM/yyyy")};{depense.Categorie};{depense.Description};{depense.Montant.ToString("C", CultureInfo.GetCultureInfo("fr-FR"))};{depense.DateCreation.ToString("dd/MM/yyyy HH:mm")}");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        public byte[] GeneratePdfReport()
        {
            var depenses = _context.Depenses.ToList();
            var budgets = _context.Budgets.ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .AlignCenter()
                        .Text("📊 Rapport des Dépenses - Gestion Budget")
                        .SemiBold().FontSize(18).FontColor(Colors.Blue.Darken3);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(20);

                            // Informations générales
                            column.Item().Background(Colors.Grey.Lighten3).Padding(10).Column(infoColumn =>
                            {
                                infoColumn.Spacing(5);
                                infoColumn.Item().Text($"Généré le : {DateTime.Now:dd/MM/yyyy à HH:mm}");
                                infoColumn.Item().Text($"Administrateur : {GetAdminName()}");
                            });

                            // Statistiques
                            column.Item().Text("📈 Statistiques Générales").SemiBold().FontSize(14);
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Total Dépenses").SemiBold();
                                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Nombre Dépenses").SemiBold();
                                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Dépense Moyenne").SemiBold();
                                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Nombre Budgets").SemiBold();

                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text(depenses.Sum(d => d.Montant).ToString("C", CultureInfo.GetCultureInfo("fr-FR")));
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text(depenses.Count.ToString());
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text((depenses.Count > 0 ? depenses.Average(d => d.Montant).ToString("C", CultureInfo.GetCultureInfo("fr-FR")) : "0,00 MDH"));
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text(budgets.Count.ToString());
                                });
                            });

                            // Dépenses par catégorie
                            column.Item().Text("📊 Dépenses par Catégorie").SemiBold().FontSize(14);
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Green.Lighten3).Padding(5).Text("Catégorie").SemiBold();
                                    header.Cell().Background(Colors.Green.Lighten3).Padding(5).Text("Total").SemiBold();
                                    header.Cell().Background(Colors.Green.Lighten3).Padding(5).Text("Pourcentage").SemiBold();
                                });

                                var depensesParCategorie = depenses
                                    .GroupBy(d => d.Categorie)
                                    .Select(g => new { Categorie = g.Key, Total = g.Sum(d => d.Montant) });

                                foreach (var item in depensesParCategorie)
                                {
                                    var pourcentage = depenses.Sum(d => d.Montant) > 0 ?
                                        (item.Total / depenses.Sum(d => d.Montant) * 100) : 0;

                                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text(item.Categorie);
                                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text(item.Total.ToString("C", CultureInfo.GetCultureInfo("fr-FR")));
                                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text($"{pourcentage:F1}%");
                                }
                            });

                            // Détail des dépenses
                            column.Item().Text("📋 Détail des Dépenses").SemiBold().FontSize(14);
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Orange.Lighten3).Padding(5).Text("ID").SemiBold();
                                    header.Cell().Background(Colors.Orange.Lighten3).Padding(5).Text("Date").SemiBold();
                                    header.Cell().Background(Colors.Orange.Lighten3).Padding(5).Text("Catégorie").SemiBold();
                                    header.Cell().Background(Colors.Orange.Lighten3).Padding(5).Text("Montant").SemiBold();
                                });

                                foreach (var depense in depenses.OrderByDescending(d => d.Date_depense).Take(50)) // Limite à 50 lignes
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(depense.Id_depense.ToString());
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(depense.Date_depense.ToString("dd/MM/yyyy"));
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(depense.Categorie);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(depense.Montant.ToString("C", CultureInfo.GetCultureInfo("fr-FR")));
                                }
                            });

                            // Pied de page
                            column.Item().AlignCenter().Text("--- Fin du rapport ---").Italic().FontColor(Colors.Grey.Medium);
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }

        private string GetAdminName()
        {
            // Récupérer le nom de l'admin connecté depuis la session
            return "Administrateur"; // À adapter selon le système d'authentification
        }
    }
}