using GestionBudget_V2.Models;

namespace GestionBudget_V2.Services
{
    public class EmailTemplateService
    {
        public string GeneratePasswordResetEmail(string typeUtilisateur, string nouveauMotDePasse)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{
            font-family: 'Arial', sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            text-align: center;
            border-radius: 10px 10px 0 0;
        }}
        .content {{
            background: #f8f9fa;
            padding: 30px;
            border-radius: 0 0 10px 10px;
        }}
        .password-box {{
            background: white;
            border: 2px solid #e74c3c;
            border-radius: 8px;
            padding: 20px;
            margin: 20px 0;
            text-align: center;
        }}
        .password {{
            font-size: 24px;
            color: #e74c3c;
            font-weight: bold;
            letter-spacing: 2px;
            font-family: 'Courier New', monospace;
        }}
        .security-note {{
            background: #fff3cd;
            border: 1px solid #ffeaa7;
            border-radius: 5px;
            padding: 15px;
            margin: 20px 0;
        }}
        .footer {{
            text-align: center;
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #ddd;
            color: #7f8c8d;
            font-size: 12px;
        }}
        .btn {{
            display: inline-block;
            background: #3498db;
            color: white;
            padding: 12px 30px;
            text-decoration: none;
            border-radius: 5px;
            margin: 10px 0;
        }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>🔒 Gestion Budget</h1>
        <p>Réinitialisation de votre mot de passe</p>
    </div>
    
    <div class='content'>
        <h2>Bonjour,</h2>
        
        <p>Vous avez demandé la réinitialisation du mot de passe pour votre compte <strong>{typeUtilisateur}</strong> sur la plateforme Gestion Budget.</p>
        
        <p>Voici votre nouveau mot de passe :</p>
        
        <div class='password-box'>
            <p style='margin: 0 0 10px 0; font-weight: bold;'>Nouveau mot de passe :</p>
            <div class='password'>{nouveauMotDePasse}</div>
        </div>
        
        <div class='security-note'>
            <p>💡 <strong>Conseil de sécurité :</strong></p>
            <p>Nous vous recommandons fortement de :</p>
            <ul>
                <li>Changer ce mot de passe après votre première connexion</li>
                <li>Utiliser un mot de passe unique et complexe</li>
                <li>Ne jamais partager vos identifiants</li>
            </ul>
        </div>
        
        <p>Pour vous connecter, rendez-vous sur la plateforme :</p>
        <a href='#' class='btn'>Se connecter à Gestion Budget</a>
        
        <p>Si vous n'êtes pas à l'origine de cette demande, veuillez ignorer cet email et contacter immédiatement l'administrateur.</p>
    </div>
    
    <div class='footer'>
        <p>Cet email a été envoyé automatiquement. Merci de ne pas y répondre.</p>
        <p>© 2024 Gestion Budget Entreprise. Tous droits réservés.</p>
    </div>
</body>
</html>";
        }

        public string GenerateWelcomeEmail(string nom, string prenom, string typeUtilisateur, string motDePasseTemporaire)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{
            font-family: 'Arial', sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .header {{
            background: linear-gradient(135deg, #27ae60 0%, #2ecc71 100%);
            color: white;
            padding: 30px;
            text-align: center;
            border-radius: 10px 10px 0 0;
        }}
        .content {{
            background: #f8f9fa;
            padding: 30px;
            border-radius: 0 0 10px 10px;
        }}
        .credentials-box {{
            background: white;
            border: 2px solid #3498db;
            border-radius: 8px;
            padding: 20px;
            margin: 20px 0;
        }}
        .footer {{
            text-align: center;
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #ddd;
            color: #7f8c8d;
            font-size: 12px;
        }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>👋 Bienvenue sur Gestion Budget</h1>
        <p>Votre compte a été créé avec succès</p>
    </div>
    
    <div class='content'>
        <h2>Bonjour {prenom} {nom},</h2>
        
        <p>Votre compte {typeUtilisateur} sur la plateforme Gestion Budget a été créé avec succès.</p>
        
        <div class='credentials-box'>
            <h3>📋 Vos identifiants de connexion :</h3>
            <p><strong>Type de compte :</strong> {typeUtilisateur}</p>
            <p><strong>Email :</strong> Votre adresse email</p>
            <p><strong>Mot de passe temporaire :</strong> {motDePasseTemporaire}</p>
        </div>
        
        <p>Pour votre première connexion :</p>
        <ol>
            <li>Rendez-vous sur la plateforme Gestion Budget</li>
            <li>Connectez-vous avec vos identifiants ci-dessus</li>
            <li>Changez votre mot de passe dans les paramètres du compte</li>
        </ol>
        
        <p style='color: #e74c3c;'><strong>⚠️ Important :</strong> Pour des raisons de sécurité, changez votre mot de passe dès votre première connexion.</p>
    </div>
    
    <div class='footer'>
        <p>Cet email a été envoyé automatiquement. Merci de ne pas y répondre.</p>
        <p>© 2024 Gestion Budget Entreprise. Tous droits réservés.</p>
    </div>
</body>
</html>";
        }
    }
}