## Tower Defense – Godot 4 (C#)

Un jeu Tower Defense développé avec Godot 4.2+ en C#. Le joueur achète et place des tours sur une carte 2D pour défendre un chemin emprunté par des vagues d’ennemis. Le projet met l’accent sur une logique de placement robuste, une économie simple et des systèmes modulaires (tours, ennemis, projectiles, carte, UI).

## Fonctionnalités

- **Placement de tours** avec validations (UI, zones interdites, proximité, or suffisant)
- **Feedback visuel** pendant le placement (valide/invalide/placée)
- **Amélioration de tours** (jusqu’à 5 niveaux, stats et apparence)
- **Vagues d’ennemis** avec types variés et progression automatique
- **Projectiles** avec suivi dynamique et impact
- **Gestion de l’or** (gain à la mort des ennemis, dépenses pour tours/upgrades)
- **Carte** chargée/générée via JSON et **objets animés** décoratifs
- **Caméra** centrée et ajustée pour mobile

## Lancer le projet

1. Ouvrir ce dossier dans Godot 4.2+.
2. Vérifier la scène principale dans `project.godot`:
   - `run/main_scene = res://Main.tscn`
3. Lancer la scène principale (F5).

## Structure des scènes

```
Main.tscn (Scene principale)
├── Camera2D (centrage/zoom mobile)
├── TileMap (main.cs)
├── AnimatedObjects (Node2D) – objets décoratifs animés
├── Path2D/NoBuildZone (Area2D + CollisionShape2D) – zones interdites
├── Towers (Node2D) – parent de toutes les tours placées
├── UI (Control)
│   ├── Panel/VBoxContainer/TowerButton – bouton d’achat de tour
│   └── GoldManager – gestion et affichage de l’or
└── WaveManager – gestion des vagues d’ennemis
```

## Règles de placement (critères)

Une tour peut être placée uniquement si:

- Le clic n’est **pas sur l’UI** (`GuiGetHoveredControl() == null`)
- La position n’est **pas dans** une `NoBuildZone` (rectangles via `Area2D + CollisionShape2D`)
- La tour n’est **pas trop proche** d’une autre (`DistanceSquaredTo(position) >= 2500` → ≥ 50 px)
- Le joueur a **suffisamment d’or** (`Goldmanager`)

Sinon:

- La tour reste en **mode placement** et suit la souris
- **Aucun retrait d’or**
- **Non ajoutée** au groupe `"towers"`

## Feedback visuel

- **Valide** → tour en blanc semi-transparent
- **Invalide** → tour en rouge semi-transparent
- **Placée** → tour opaque

## Systèmes et scripts clés

### Carte et caméra

- `main.cs` (TileMap): initialise la carte, charge les objets animés, centre et ajuste la caméra
- `GenerateMap.cs` / `ReadMap.cs`: sérialisation JSON de la carte (`CellData`)
- `LoadAnimatedObjects.cs` / `SaveAnimatedObjets.cs`: charge/sauvegarde des `AnimatedSprite2D` décoratifs (`AnimatedElementData`)
- `CenterCamera.cs` / `AdjustCameraForMobile.cs`: centrage et zoom pour écrans mobiles

### UI et économie

- `UiManager.cs`:
  - Gère l’état de placement (`isPlacing`) et l’instance courante de tour
  - Suit la souris et applique la **validation de placement**
  - Place les tours en enfant de `Towers` et les ajoute au groupe `towers`
- `Goldmanager.cs`:
  - Or initial: 150
  - `SpendCoins`, `EarnCoins`, `IsEnoughCoin` et mise à jour UI

### Tours, projectiles et upgrades

- `Tower.cs`:
  - Détection d’ennemis via `Area2D`
  - Timer d’attaque selon `AttackSpeed`
  - Tir de projectiles (`ProjectileScene`) et animations directionnelles
  - Améliorations (clic droit, jusqu’à 5 niveaux):
    - N2: 75 or, 60 dégâts, 1.1 att/s, soldat visible: false
    - N3: 125 or, 70 dégâts, 1.2 att/s, visible: true
    - N4: 175 or, 80 dégâts, 1.3 att/s, visible: true
    - N5: 225 or, 85 dégâts, 1.4 att/s, visible: false

- `Projectile.cs`:
  - Suivi dynamique de la cible et application des dégâts à l’impact (seuil ~10 px)
  - Vitesse par défaut: 300 px/s (export)

### Ennemis et vagues

- `Enemy.cs`:
  - Mouvement via `PathFollow2D`
  - Types pris en charge: Wolf, Orc, Slime, Bee (animations dédiées)
  - Gain d’or à la mort (`Goldmanager`)

- `Scripts/WaveManager.cs`:
  - Lancement de vagues croissantes (multiplicateur ~×1.2)
  - Apparition d’ennemis toutes les 1s, type choisi aléatoirement
  - Connexion au signal `EnemyDied` pour suivre la population

## Détails de l’implémentation du placement (UiManager.cs)

- Le bouton `TowerButton` instancie une tour et active le **mode placement**
- La tour suit la souris, change de `Modulate` selon la validité
- Au clic gauche:
  - Ignore si la souris est au-dessus de l’UI
  - Vérifie zones interdites (intersection avec rectangles `NoBuildZone`)
  - Vérifie la proximité avec les tours existantes (groupe `towers`)
  - Vérifie l’or disponible, dépense et finalise le placement

## Bonnes pratiques intégrées

- Prévention des tours « fantômes »: suppression de la tour en cours si on reclique le bouton d’achat
- Nettoyage mémoire: projectiles et ennemis `QueueFree()` après usage
- Séparation claire des responsabilités entre scripts
- Couches de rendu dédiées: `enemy`, `tower`, `projectile`, `path`

## Améliorations futures (proposées)

- Touche **Échap** pour annuler un placement
- Curseur personnalisé pendant le placement
- Cercle de portée affiché avant le placement
- Effets visuels/sonores lors d’un refus de placement
- Gestion avancée de **plusieurs types de tours**

## Arborescence (extrait)

```
Assets/
  Map/
    AnimatedObjectsData.json
    level1.json
  Towers/
    ... (sprites tours et projectiles)
Scripts/
  WaveManager.cs
Main.tscn
project.godot
UiManager.cs
Tower.cs
Enemy.cs
Projectile.cs
Goldmanager.cs
main.cs
```

## Configuration Godot pertinente

- `run/main_scene = res://Main.tscn`
- `config/features = ["4.3", "C#", "Mobile"]`
- Rendu: `renderer/rendering_method = mobile`
- Couches 2D: `enemy`, `tower`, `projectile`, `path`

## Licence

Projet éducatif. Adapter la licence selon vos besoins.


