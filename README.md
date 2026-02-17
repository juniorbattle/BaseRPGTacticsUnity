# BaseRPGTacticsUnity – Prototype de jeu de tactiques au tour par tour (Unity)

Bienvenue sur **BaseRPGTacticsUnity**, un projet de jeu vidéo 3D de type tactical RPG (comme *Fire Emblem* ou *Final Fantasy Tactics*) développé sous Unity (ancienne version). Ce dépôt contient les scripts C# et une scène de test qui posent les bases d’un système de combat tactique au tour par tour.

## 🎯 Description

Ce projet est une base technique pour un jeu de tactiques. Il implémente les mécaniques fondamentales :
- Déplacement case par case sur une grille.
- Gestion des tours de jeu (joueur / IA).
- Système d’actions (attaques, compétences).
- Barres de vie, projectiles, caméra tactique.
- Structure modulaire pour ajouter facilement des unités, des compétences et des comportements.

L’objectif est de fourir un point de départ solide pour tout développeur souhaitant créer son propre tactical RPG sans repartir de zéro.

## ✨ Fonctionnalités principales

- **Système de tours** : Gestion des unités alliées et ennemies via un `TurnManager`.
- **Déplacement tactique** : Script `TacticsMove` pour le mouvement case par case avec calcul de chemin.
- **Grille de jeu** : Représentation par des `Tile` (cases) avec états (libre, occupée, etc.).
- **Combat** : Gestion des attaques, projectiles (`ProjectileController`), dégâts et barres de vie (`Healthbar`).
- **Caméra tactique** : Script `TacticsCamera` pour suivre l’action et se positionner sur la grille.
- **États et buffs** : `TacticStatus` pour gérer les altérations d’état.
- **Actions/Compétences** : Dossier `Act` contenant des ScriptableObjects pour définir des actions (fichier `New Act.asset`).
- **Menu de base** : `MenuScript` pour l’interface utilisateur.
- **Boost** : `BoostControl` pour des bonus temporaires.
- **Scène de test** : `Test.unity` permettant de lancer et d’expérimenter le prototype immédiatement.

## 📁 Structure du dépôt

BaseRPGTacticsUnity/
├── Act/ # ScriptableObjects pour les actions/compétences
│ └── New Act.asset
├── NPC/ # Scripts et données pour les unités non-joueurs
├── Player/ # Scripts et données pour le joueur
├── Resources/ # Ressources diverses (préfabs, assets)
├── BoostControl.cs # Contrôle des bonus/boost
├── Healthbar.cs # Barre de vie UI
├── MenuScript.cs # Script du menu principal
├── ProjectileController.cs # Gestion des projectiles (attaques à distance)
├── TacticStatus.cs # Gestion des états (poison, paralysie, etc.)
├── TacticsAct.cs # Classe de base pour les actions tactiques
├── TacticsCamera.cs # Caméra adaptée au gameplay tactique
├── TacticsMove.cs # Mouvement case par case
├── Tile.cs # Représentation d'une case de la grille
├── TurnManager.cs # Gestion des tours
├── Test.unity # Scène de test jouable
└── (fichiers .meta associés)


## 🎮 Gameplay (dans l’état actuel)

- Le joueur contrôle une ou plusieurs unités.
- À son tour, il peut sélectionner une unité, la déplacer sur les cases accessibles, puis choisir une action (attaque, etc.).
- L’IA ennemie (dans `NPC`) prend ses décisions automatiquement.
- Les projectiles sont instanciés lors des attaques à distance.
- La barre de vie se met à jour en fonction des dégâts.
- Le jeu passe au tour suivant automatiquement.

> **Remarque** : Le projet étant une base, certaines fonctionnalités peuvent être incomplètes ou nécessiter des ajustements.

## 🤝 Pourquoi ce projet ?

Ce dépôt est idéal pour :

- Apprendre la structure d’un tactical RPG sous Unity.
- Réutiliser des mécaniques éprouvées pour démarrer un nouveau projet.
- Étudier l’organisation de scripts pour un jeu au tour par tour.

---

**Amusez-vous à construire votre propre tactical RPG !** 🎮  
Si vous avez des questions ou suggestions, n’hésitez pas à [ouvrir une issue](https://github.com/juniorbattle/BaseRPGTacticsUnity/issues) sur GitHub.
