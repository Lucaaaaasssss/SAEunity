/*
 Logique pour l'affichage d'une entrée highscore pour le menu HighscoreUI.
*/

using UnityEngine;
using TMPro;

namespace Anatidae {
    public class HighscoreEntryGo : MonoBehaviour
    {
        [SerializeField] TMP_Text nameText;
        [SerializeField] TMP_Text scoreText;

        public void SetData(HighscoreManager.HighscoreEntry entry)
        {
            nameText.text = entry.name;
            // Convertir le score (en centièmes de seconde) en format temps MM:SS.CC
            scoreText.text = FormatTime(entry.score);
        }

        string FormatTime(int centiseconds)
        {
            int minutes = centiseconds / 6000;
            int seconds = (centiseconds / 100) % 60;
            int centis = centiseconds % 100;
            return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, centis);
        }

        public void SetScale(float scale)
        {
            nameText.fontSize = (int)(nameText.fontSize * scale);
            scoreText.fontSize = (int)(scoreText.fontSize * scale);
        }
    }
}