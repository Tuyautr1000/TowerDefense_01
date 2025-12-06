# agents.md

## 🎮 Projet : Tower Defense – Godot 4 (C#)

### 📌 Contexte
Ce projet est un **Tower Defense** développé avec **Godot 4.2+ en C#**.  
L’objectif est de permettre au joueur :
- De sélectionner des tours via une interface utilisateur (UI).
- De les placer sur une carte 2D.
- De respecter les règles de placement (zones interdites, proximité, ressources).

---

## ⚙️ Règles de Placement

Une tour **peut être placée** uniquement si :
1. Le joueur ne clique **pas sur l’UI** (`GuiGetHoveredControl()`).
2. La position n’est **pas dans une zone "NoBuildZone"** (`Area2D + CollisionShape2D`).
3. La tour n’est **pas trop proche d’une autre tour** (`DistanceSquaredTo < 2500`).
4. Le joueur a **suffisamment d’or** (`Goldmanager`).

Sinon :
- La tour reste **en mode placement**, suit la souris et s’affiche en **rouge transparent**.
- Aucun retrait d’or.
- La tour **n’est pas ajoutée** au groupe `"towers"`.

---

## 🐞 Bug corrigé
**Problème :** plusieurs clics sur le bouton d’achat créaient plusieurs tours "fantômes", donnant l’impression qu’une tour se plaçait derrière l’UI (`VBoxContainer`).  
**Solution :** suppression de l’ancienne tour en cours de placement si une nouvelle est créée.

---

## 🎨 Feedback visuel
- **Valide** → tour en blanc semi-transparent.
- **Invalide** → tour en rouge semi-transparent.
- **Placée** → tour opaque.

---

## 📂 Organisation des scènes
Game (Node)
├── Towers (Node2D) ← Parent des tours placées
├── Path2D/NoBuildZone ← Zones où placer une tour est interdit
│ └── CollisionShape2D
├── UiManager (Control/Panel)
│ ├── Panel/VBoxContainer/TowerButton
│ └── GoldManager

## 🔑 Points importants pour les agents
- Projet en **Godot 4 + C#**.
- Les tours (`Tower`) sont des `Node2D` ou `Area2D`.
- **Toujours** ajouter les tours en enfant de `Towers`, **jamais** dans l’UI.
- Une seule tour peut être en cours de placement (`isPlacing`).
- Vérifications de placement centralisées dans `IsValidPlacement()`.

---

## 🚀 Améliorations futures
- Ajout d’une touche **Échap** pour annuler un placement.
- Curseur personnalisé pendant le placement.
- Cercle de portée affiché autour de la tour avant placement.
- Effets visuels/sonores lors d’un refus de placement.
- Gestion avancée de plusieurs types de tours.

---

👉 Ce fichier doit être lu par tout agent ou contributeur avant de modifier le projet.  
Il garantit que la **logique de placement** reste cohérente et que les tours ne se mélangent pas avec l’UI.
