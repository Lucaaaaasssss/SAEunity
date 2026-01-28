/*
 Affichage du leaderboard en 3 colonnes.
*/

using System.Collections;
using System.Text;
using UnityEngine;
using TMPro;

namespace Anatidae
{
    public class LeaderboardDisplay : MonoBehaviour
    {
        [Header("Titre mode")]
        [SerializeField] TMP_Text titleText;

        [Header("Colonne Classement (positions)")]
        [SerializeField] TMP_Text rankColumn;

        [Header("Colonne Pseudo")]
        [SerializeField] TMP_Text nameColumn;

        [Header("Colonne Chrono")]
        [SerializeField] TMP_Text timeColumn;

        [Header("Options")]
        [SerializeField] int maxEntries = 10;

        void OnEnable()
        {
            StartCoroutine(LoadAndDisplay());
        }

        IEnumerator LoadAndDisplay()
        {
            // Titre selon le mode
            if (titleText != null)
            {
                bool isDuo = GameModeManager.Instance != null && GameModeManager.Instance.IsDuo();
                titleText.text = isDuo ? "CLASSEMENT DUO" : "CLASSEMENT SOLO";
            }

            yield return HighscoreManager.FetchHighscores();

            if (HighscoreManager.Highscores == null) yield break;

            int count = Mathf.Min(maxEntries, HighscoreManager.Highscores.Count);

            StringBuilder ranks = new StringBuilder();
            StringBuilder names = new StringBuilder();
            StringBuilder times = new StringBuilder();

            for (int i = 0; i < count; i++)
            {
                var entry = HighscoreManager.Highscores[i];
                ranks.AppendLine((i + 1).ToString());
                names.AppendLine(entry.name);
                times.AppendLine(FormatTime(entry.score));
            }

            if (rankColumn != null) rankColumn.text = ranks.ToString();
            if (nameColumn != null) nameColumn.text = names.ToString();
            if (timeColumn != null) timeColumn.text = times.ToString();
        }

        string FormatTime(int centiseconds)
        {
            int minutes = centiseconds / 6000;
            int seconds = (centiseconds / 100) % 60;
            int centis = centiseconds % 100;
            return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, centis);
        }

        public void Refresh()
        {
            StartCoroutine(LoadAndDisplay());
        }
    }
}
