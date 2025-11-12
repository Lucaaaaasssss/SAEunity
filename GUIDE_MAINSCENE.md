# 🎮 Guide d'intégration - Scène MainScene

## ✅ Assets transférés et disponibles

Tous les éléments de votre jeu ont été copiés dans le projet JeuToolkit et sont prêts à être utilisés dans la scène **MainScene**.

### 📂 Scripts (dans Assets/Scripts/)
- **PlayerMovement.cs** - Contrôle du joueur (configuré pour la borne arcade)
- **ArcadeCameraSetup.cs** - Configuration caméra pour l'arcade (1920x1080)
- **CameraFollowPlayer.cs** - Caméra qui suit le joueur
- **ArcadeGround.cs** - Création du terrain/fond

### 🎨 Sprites (dans Assets/GameSprites/)
- Cobayedos1.png, Cobayedos2.png
- Cobayeface1.png, Cobayeface2.png
- cote1.png, cote2.png
- policier de dos 0.png, policier de dos 1.png
- sprite_0.png, sprite_1.png

### 🎬 Animations (dans Assets/GameAnimations/)
- Walk.anim
- WalkUp.anim
- WalkRight.anim
- Idle.anim
- PlayerAnimator.controller

---

## 🔧 Comment utiliser dans la scène MainScene

### 1. Ouvrir Unity et charger la scène
```
1. Ouvrez Unity
2. Ouvrez la scène: Assets/Scenes/MainScene.unity
```

### 2. Créer votre joueur
```
1. Dans la hiérarchie, clic droit > 2D Object > Sprite
2. Renommez-le "Player"
3. Ajoutez le tag "Player" (Inspector > Tag > Player)
4. Assignez un de vos sprites (ex: Cobayeface1.png)
5. Ajoutez les composants suivants :
   - PlayerMovement (script)
   - Animator (et assignez PlayerAnimator.controller)
   - Collider2D si nécessaire (BoxCollider2D ou CapsuleCollider2D)
```

### 3. Configurer le PlayerMovement
Dans l'Inspector du Player :
- ✅ **Use Arcade Controls** : coché (par défaut)
- **Horizontal Axis** : P1_Horizontal
- **Vertical Axis** : P1_Vertical
- **Move Speed** : 5
- **Horizontal Speed** : 3
- **Constrain To Bounds** : coché
- **Auto Find Ground** : coché

### 4. Créer le terrain (Ground)
```
1. Dans la hiérarchie, clic droit > Create Empty
2. Renommez-le "Ground"
3. Ajoutez le composant SpriteRenderer
4. Ajoutez le composant ArcadeGround (script)
5. Configurez dans l'Inspector:
   - Target Width : 1920
   - Target Height : 1080
   - Height Multiplier : 5 (pour un terrain plus long)
   - Ground Color : Vert par défaut (modifiable)
```

### 5. Configurer la caméra
La caméra "Main Camera" est déjà présente et configurée en mode 2D orthographique. Ajoutez-lui :
```
1. Le composant ArcadeCameraSetup (script)
   - Screen Width : 1920
   - Screen Height : 1080
   - Pixels Per Unit : 100

2. Le composant CameraFollowPlayer (script)
   - Auto Find Player : coché (ou assignez manuellement le Player)
   - Follow X : décoché (caméra fixe horizontalement)
   - Follow Y : coché (suit le joueur verticalement)
   - Smooth Follow : coché
   - Smooth Speed : 5
```

### 6. Ajouter le système de highscores Anatidae
```
1. Dans le dossier Assets/Anatidae/, glissez le prefab "AnatidaeInterface" dans la scène
2. Sélectionnez AnatidaeInterface dans la hiérarchie
3. Dans l'Inspector, trouvez HighscoreManager
4. Changez GAME_NAME avec le nom de votre jeu (ex: "MyGame")
   ⚠️ IMPORTANT : Pas d'espaces, pas d'accents dans le nom !
```

### 7. Intégration des highscores dans votre code
Créez un script pour gérer la fin de partie (GameManager.cs par exemple) :

```csharp
using UnityEngine;
using Anatidae;

public class GameManager : MonoBehaviour
{
    private int currentScore = 0;

    // Appeler cette méthode quand la partie se termine
    public void GameOver()
    {
        // Vérifier si c'est un highscore
        if (HighscoreManager.IsHighscore(currentScore))
        {
            // Afficher le menu de saisie du nom
            HighscoreManager.ShowHighscoreInput(currentScore);
        }
        else
        {
            // Pas de highscore, retour au menu ou restart
            Debug.Log("Pas de highscore cette fois !");
        }
    }

    // Méthode pour ajouter des points
    public void AddScore(int points)
    {
        currentScore += points;
        Debug.Log("Score: " + currentScore);
    }
}
```

---

## 🎮 Contrôles de la borne d'arcade

### Contrôles du joueur 1 (configurés par défaut)
- **Joystick** : P1_Horizontal, P1_Vertical
  - Clavier (test) : Flèches directionnelles
- **Boutons** : P1_B1 à P1_B6
  - Clavier (test) : A, Z, E, R, T, Y
- **Start** : P1_Start
  - Clavier (test) : Entrée

### Contrôles du joueur 2 (disponibles)
- **Joystick** : P2_Horizontal, P2_Vertical
  - Clavier (test) : Q, S, D, F
- **Boutons** : P2_B1 à P2_B6
  - Clavier (test) : U, I, O, P, M, L
- **Start** : P2_Start
  - Clavier (test) : Espace

Pour tester les contrôles, utilisez la scène **InputTester.unity**

---

## 📝 Notes importantes

### Scène déjà configurée en 2D ✓
✅ La scène MainScene a été automatiquement configurée :
- Caméra en mode orthographique (z=-10)
- Skybox désactivé
- Ambient Mode : Flat
- Lightmaps désactivés

### Tags recommandés
Pour que les scripts fonctionnent automatiquement :
- Taggez votre joueur avec le tag **"Player"**
- La caméra doit avoir le tag **"MainCamera"** (déjà fait)

### Ordre de rendu (Sorting Layers)
Pour gérer l'affichage en 2D, créez des Sorting Layers :
1. Edit > Project Settings > Tags and Layers
2. Ajoutez dans "Sorting Layers" :
   - Background (ordre -1)
   - Ground (ordre 0)
   - Player (ordre 1)
   - Enemies (ordre 2)
   - UI (ordre 10)

### Structure de la scène recommandée
```
MainScene
├── Main Camera (avec ArcadeCameraSetup + CameraFollowPlayer)
├── Ground (avec ArcadeGround)
├── Player (avec PlayerMovement + Animator)
├── AnatidaeInterface (prefab pour les highscores)
└── GameManager (script vide pour gérer le jeu)
```

---

## 🚀 Prochaines étapes

1. ✅ Ouvrez la scène MainScene dans Unity
2. ✅ Créez votre joueur avec les sprites et animations
3. ✅ Créez le terrain avec ArcadeGround
4. ✅ Configurez la caméra avec les scripts fournis
5. ✅ Ajoutez des ennemis/obstacles
6. ✅ Implémentez la logique de jeu (collision, score, Game Over)
7. ✅ Testez avec les contrôles arcade (scène InputTester)
8. ✅ Intégrez le système de highscores Anatidae
9. ✅ Build en WebGL pour la borne

---

## 🔍 Dépannage

### Le joueur ne bouge pas
- ✅ Vérifiez que "Use Arcade Controls" est coché dans PlayerMovement
- ✅ Vérifiez les axes dans Edit > Project Settings > Input Manager
- ✅ Testez avec les flèches du clavier

### La caméra ne suit pas
- ✅ Vérifiez que CameraFollowPlayer a bien la référence au Player
- ✅ Ou activez "Auto Find Player"
- ✅ Vérifiez que le Player a bien le tag "Player"

### Les animations ne fonctionnent pas
- ✅ Vérifiez que le Animator a bien le controller assigné (PlayerAnimator.controller)
- ✅ Ouvrez l'Animator window pour vérifier les transitions
- ✅ Vérifiez les paramètres : isMovingDown, isMovingUp, isMovingRight

### Le Ground ne s'affiche pas
- ✅ Vérifiez que le script ArcadeGround est bien attaché
- ✅ Vérifiez qu'il y a un SpriteRenderer sur le GameObject
- ✅ Vérifiez que la caméra est bien trouvée (Camera.main)

### Les highscores ne fonctionnent pas
- ✅ Vérifiez que AnatidaeInterface est bien dans la scène
- ✅ Vérifiez que GAME_NAME est défini (sans espaces)
- ✅ Testez dans le navigateur (WebGL) car certaines fonctionnalités ne marchent qu'en WebGL

---

## 📦 Build WebGL pour la borne

1. File > Build Settings
2. Sélectionnez WebGL
3. Switch Platform
4. Ajoutez votre scène MainScene
5. Player Settings :
   - Resolution : 1920 x 1080
   - Fullscreen Mode : Windowed
6. Build

Le dossier de build doit avoir le même nom que GAME_NAME défini dans HighscoreManager !

---

Bon développement ! 🎉
