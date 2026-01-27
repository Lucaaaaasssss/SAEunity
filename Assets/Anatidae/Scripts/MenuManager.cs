/*
 Gère le "bouton blanc" et le minuteur d'inactivité.
*/

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [SerializeField] TMP_Text quitText;
    const float AfkTime = 30f;
    float afkTimer = 0f;
    const float HeldQuitTime = 1.5f;
    float heldQuitTimer = 0f;
    const string MenuMessage = "Retour au menu";

    #if UNITY_IOS || UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void BackToMenuNative();
    #endif

    public static void BackToMenu()
    {
        #if UNITY_IOS || UNITY_WEBGL
        BackToMenuNative();
        #elif UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    void Update()
    {
        // DÉSACTIVÉ: les appels à BackToMenu() qui crashent sur Windows
        // if (heldQuitTimer >= HeldQuitTime || afkTimer >= AfkTime) {
        //     BackToMenu();
        // }

        // DÉSACTIVÉ: Échap pour quitter (crash sur Windows)
        // if (Input.GetKeyDown(KeyCode.Escape)) {
        //     BackToMenu();
        // }

        if (Input.GetButton("Coin"))
            heldQuitTimer += Time.deltaTime;
        else
            heldQuitTimer = 0f;

        if (Mathf.Abs(Input.GetAxisRaw("P1_Horizontal")) > 0.5f || Mathf.Abs(Input.GetAxisRaw("P1_Vertical")) > 0.5f || Input.GetButton("P1_Start") || Input.GetButton("P1_B1") || Input.GetButton("P1_B2") || Input.GetButton("P1_B3") || Input.GetButton("P1_B4") || Input.GetButton("P1_B5") || Input.GetButton("P1_B6") ||
            Mathf.Abs(Input.GetAxisRaw("P2_Horizontal")) > 0.5f || Mathf.Abs(Input.GetAxisRaw("P2_Vertical")) > 0.5f || Input.GetButton("P2_Start") || Input.GetButton("P2_B1") || Input.GetButton("P2_B2") || Input.GetButton("P2_B3") || Input.GetButton("P2_B4") || Input.GetButton("P2_B5") || Input.GetButton("P2_B6"))
            afkTimer = 0f;
        else
            afkTimer += Time.deltaTime;

        if (heldQuitTimer != 0 || afkTimer - AfkTime + 6f > 0f) {
            if (quitText != null) {
                quitText.gameObject.SetActive(true);
                quitText.text = MenuMessage + new string('.', (int)Mathf.Min(Mathf.Max(heldQuitTimer * 3f, afkTimer - AfkTime + 10f * 0.4f), 3));
            }
        } else {
            if (quitText != null) {
                quitText.gameObject.SetActive(false);
            }
        }
    }
}
