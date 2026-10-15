using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public string tutorialScene = "SampleScene";
    public string gameScene = "GameScene";

    void Start()
    {
        // Разблокируем курсор в меню
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnGUI()
    {
        DrawMainMenu();
    }

    void DrawMainMenu()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Темный фон
        GUI.color = new Color(0, 0, 0, 0.8f);
        GUI.DrawTexture(new Rect(0, 0, screenWidth, screenHeight), CreateTexture(Color.black));
        GUI.color = Color.white;

        // Заголовок
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.fontSize = 48;
        titleStyle.normal.textColor = Color.white;

        GUI.Label(new Rect(0, 100, screenWidth, 80), "ГЛАВНОЕ МЕНЮ", titleStyle);

        // Кнопки
        float buttonWidth = 300;
        float buttonHeight = 60;
        float buttonX = (screenWidth - buttonWidth) / 2;
        float startY = 250;
        float spacing = 70;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 18;
        buttonStyle.fontStyle = FontStyle.Bold;

        // 1. Обучение
        if (GUI.Button(new Rect(buttonX, startY, buttonWidth, buttonHeight), "ОБУЧЕНИЕ", buttonStyle))
        {
            StartTutorial();
        }

        // 2. Играть вместе
        if (GUI.Button(new Rect(buttonX, startY + spacing, buttonWidth, buttonHeight), "ИГРАТЬ ВМЕСТЕ", buttonStyle))
        {
            StartMultiplayer();
        }

        // 3. Настройки
        if (GUI.Button(new Rect(buttonX, startY + spacing * 2, buttonWidth, buttonHeight), "НАСТРОЙКИ", buttonStyle))
        {
            ShowSettings();
        }

        // 4. Выход
        if (GUI.Button(new Rect(buttonX, startY + spacing * 3, buttonWidth, buttonHeight), "ВЫХОД", buttonStyle))
        {
            QuitGame();
        }
    }

    // 1. Обучение - переход на SampleScene
    void StartTutorial()
    {
        Debug.Log("Запуск обучения...");
        if (!string.IsNullOrEmpty(tutorialScene))
        {
            SceneManager.LoadScene(tutorialScene);
        }
        else
        {
            SceneManager.LoadScene("SampleScene");
        }
    }

    // 2. Играть вместе - мультиплеер
    void StartMultiplayer()
    {
        Debug.Log("Запуск мультиплеера...");

        // Здесь можно добавить логику сетевого подключения
        // NetworkManager.Singleton.StartHost(); // для создания игры
        // NetworkManager.Singleton.StartClient(); // для подключения

        if (!string.IsNullOrEmpty(gameScene))
        {
            SceneManager.LoadScene(gameScene);
        }
        else
        {
            SceneManager.LoadScene("MultiplayerScene");
        }
    }

    // 3. Настройки
    void ShowSettings()
    {
        Debug.Log("Открытие настроек...");

        // Простое меню настроек
        float width = 400;
        float height = 300;
        float x = (Screen.width - width) / 2;
        float y = (Screen.height - height) / 2;

        // Фон
        GUI.Box(new Rect(x, y, width, height), "НАСТРОЙКИ");

        // Громкость
        GUI.Label(new Rect(x + 20, y + 50, 100, 30), "Громкость:");
        float volume = GUI.HorizontalSlider(new Rect(x + 120, y + 55, 200, 30), 0.8f, 0f, 1f);

        // Качество
        if (GUI.Button(new Rect(x + 20, y + 100, 150, 40), "Качество: Высокое"))
        {
            QualitySettings.SetQualityLevel(2);
        }

        // Полный экран
        bool fullscreen = GUI.Toggle(new Rect(x + 20, y + 150, 200, 30), true, "Полный экран");

        // Применить
        if (GUI.Button(new Rect(x + 20, y + 200, 100, 40), "Применить"))
        {
            AudioListener.volume = volume;
            Screen.fullScreen = fullscreen;
        }

        // Назад
        if (GUI.Button(new Rect(x + 280, y + 200, 100, 40), "Назад"))
        {
            // Возврат в главное меню
        }
    }

    // 4. Выход
    void QuitGame()
    {
        Debug.Log("Выход из игры");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    Texture2D CreateTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}