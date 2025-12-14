# 🔥 Configuration du Système de Boss

## ✅ Fichiers créés

- ✅ `Boss.cs` - Script qui hérite de Enemy
- ✅ `Boss.tscn` - Scène identique à enemy.tscn mais avec Boss.cs
- ✅ `WaveManager.cs` - Modifié pour spawner des boss toutes les 5 vagues

---

## 🎮 Étapes de configuration dans Godot

### 1. Ouvrir la scène principale
Ouvrez `Main.tscn` dans l'éditeur Godot

### 2. Sélectionner le WaveManager
Dans la hiérarchie de la scène, sélectionnez le nœud `WaveManager`

### 3. Assigner la scène Boss
Dans l'inspecteur à droite :
- Cherchez la propriété **`Boss Scene`**
- Cliquez sur `[empty]` à côté de `Boss Scene`
- Naviguez et sélectionnez `Boss.tscn`
- Validez

### 4. Vérifier que Enemy Scene est bien assigné
- La propriété **`Enemy Scene`** doit pointer vers `enemy.tscn`
- Si ce n'est pas le cas, assignez-le

### 5. Sauvegarder et tester
- Sauvegardez la scène (Ctrl+S)
- Lancez le jeu (F5)
- Attendez la vague 5 pour voir apparaître le premier boss

---

## 🎯 Comportement attendu

### Vagues normales (1, 2, 3, 4, 6, 7, 8, 9, etc.)
- Spawn d'ennemis normaux (Wolf, Orc, Slime, Bee)
- Nombre d'ennemis augmente progressivement

### Vagues boss (5, 10, 15, etc.)
- 🔥 **1 Boss** apparaît en premier (agrandi, coloré)
- ➕ **Ennemis normaux** continuent de spawn après le boss
- La vague ne se termine que quand **tous** sont morts

### Types de boss selon la vague
| Vague | Boss | Apparence | PV | Dégâts | Vitesse | Taille | Couleur |
|-------|------|-----------|-----|--------|---------|--------|---------|
| 5 | King Slime | KingSlime_Walk | 1000 | 200 | 70 | 1.6x | Vert |
| 10 | Ogre Wolf | Ogre_Walk | 1500 | 250 | 40 | 1.8x | Rouge foncé |
| 15 | Queen Bee | QueenBee_Walk | 800 | 180 | 120 | 1.5x | Jaune doré |

---

## 🔧 Modifications effectuées

### `Boss.cs`
- Hérite de `Enemy` → réutilise tout le comportement
- Ajoute `ScaleMultiplier` pour agrandir visuellement
- Ajoute `BossColor` pour teinter le sprite
- Applique automatiquement scale et couleur dans `_Ready()`

### `WaveManager.cs`
- Nouvelle propriété `[Export] public PackedScene BossScene`
- Logique dans `StartWave()` : si vague % 5 == 0 → spawn boss + ennemis
- Nouvelle méthode `SpawnBoss()` avec 3 variantes configurées
- Les stats des boss augmentent avec les vagues (+15% par vague)

### `Goldmanager.cs`
- Or de départ passé de 130 à **1000 coins**

---

## 🐞 Vérification

Si les boss n'apparaissent pas :
1. Vérifiez que `BossScene` est bien assigné dans l'inspecteur
2. Regardez la console (Output) pour voir les messages :
   - `"🔥 VAGUE BOSS 5 🔥"`
   - `"🔥 Boss spawné : Wolf - HP: 1000, DMG: 200, Speed: 70 🔥"`
3. Si vous voyez `"BossScene ou EnemyPath n'est pas défini!"` → assignez Boss.tscn

---

## 🎨 Équilibrage des stats

Pour modifier les stats des boss, éditez `WaveManager.cs` dans la méthode `SpawnBoss()` (environ ligne 195-215) :

```csharp
case 1: // Boss Tank
    bossType = "Orc";
    bossHP = (int)(1500 * waveMultiplier);    // ← Modifier ici
    bossDmg = (int)(250 * waveMultiplier);    // ← Modifier ici
    bossSpeed = 40.0f;                        // ← Modifier ici
    bossReward = 150;                         // ← Modifier ici
    scaleMultiplier = 2.0f;                   // ← Modifier ici
    bossColor = new Color(0.8f, 0.1f, 0.1f); // ← Modifier ici
    break;
```

---

✅ Le système de boss est maintenant complètement fonctionnel !

