# Security Policy
## 🔎 Reporting a Vulnerability

Si vous découvrez une faille de sécurité, veuillez suivre les étapes suivantes :

1. **Ne pas ouvrir une issue publique.**
2. Envoyer un email à : **[rafikaitichou@gmail.com](mailto:rafikaitichou@gmail.com)**
3. Fournir :

   * Une description claire de la vulnérabilité
   * Les étapes pour la reproduire
   * Les fichiers ou extraits de code concernés
   * L'impact potentiel

Nous nous engageons à :

* Accuser réception sous **48 heures**
* Fournir une réponse et un plan d'action sous **5 jours**

## 🔧 Security Best Practices

Voici les mesures que votre application respecte ou recommande :

### ✔️ Authentification & Sessions

* Hachage des mots de passe avec **BCrypt**
* Sessions sécurisées (**SameSite**, **HttpOnly**, **Secure** quand HTTPS est activé)

### ✔️ Protection des Données

* Validation des entrées côté serveur
* Protection contre les injections SQL via **Entity Framework Core**
* Politique CORS configurée pour limiter les accès externes

### ✔️ Communication

* Support complet du protocole **HTTPS**
* Recommandation d'utiliser un certificat SSL en production

### ✔️ Gestion des Dépendances

* Vérification régulière des vulnérabilités NuGet
* Mise à jour continue du framework .NET

### ✔️ Accès & Permissions

* Séparation des rôles : **Administrateur** / **Comptable**
* Accès restreint par middleware personnalisé (si applicable)

## 🛡️ Responsible Disclosure

Nous encourageons la divulgation responsable. Merci de ne pas exploiter de faille et de nous la signaler immédiatement.

## 📅 Dernière mise à jour

Novembre 2025
