using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static bool gameWon = false;
    private static bool gameOver = false;
    private static bool inEscapeMenu = false;
    private static float soundVolume = 50f;
    private static float mouseSensitivity = 4;
    private static bool topViewMode = true;
    private static float gameCountDown = 0;

    public static bool GameWon { get => gameWon; set => gameWon = value; }
    public static bool InEscapeMenu { get => inEscapeMenu; set => inEscapeMenu = value; }
    public static float SoundVolume { get => soundVolume; set => soundVolume = value; }
    public static float MouseSensitivity { get => mouseSensitivity; set => mouseSensitivity = value; }
    public static bool TopViewMode { get => topViewMode; set => topViewMode = value; }
    public static float GameCountDown { get => gameCountDown; set => gameCountDown = value; }
    public static bool GameOver { get => gameOver; set => gameOver = value; }
}
