using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public KeyCode pauseKey = KeyCode.Escape;
    public string mainMenuSceneName = "Menu";
    private bool isPaused = false;
    private bool showSettings = false;

    // Настройки
    private float musicVolume = 0.8f;
    private float soundVolume = 0.8f;
    private float mouseSensitivity = 1.0f;
    private bool fullscreen = true;
    private int qualityLevel = 2;
    private string[] qualityNames = { "Низкая", "Средняя", "Высокая" };

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
                ContinueGame();
            else
                PauseGame();
        }
    }

    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ContinueGame()
    {
        isPaused = false;
        showSettings = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void OnGUI()
    {
        if (!isPaused) return;

        // Темный фон
        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), MakeTex(1, 1, Color.black));
        GUI.color = Color.white;

        if (showSettings)
            DrawSettingsMenu();
        else
            DrawPauseMenu();
    }

    void DrawPauseMenu()
    {
        float width = 300;
        float height = 250;
        float x = (Screen.width - width) / 2;
        float y = (Screen.height - height) / 2;

        // Фон меню
        GUIStyle menuStyle = new GUIStyle(GUI.skin.box);
        menuStyle.normal.background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.95f));
        GUI.Box(new Rect(x, y, width, height), "", menuStyle);

        // Заголовок
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.fontSize = 24;
        titleStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(x, y + 20, width, 30), "ПАУЗА", titleStyle);

        // Кнопки
        float buttonWidth = 200;
        float buttonHeight = 40;
        float buttonX = x + (width - buttonWidth) / 2;
        float buttonY = y + 70;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 14;
        buttonStyle.fontStyle = FontStyle.Bold;

        if (GUI.Button(new Rect(buttonX, buttonY, buttonWidth, buttonHeight), "ПРОДОЛЖИТЬ", buttonStyle))
        {
            ContinueGame();
        }

        if (GUI.Button(new Rect(buttonX, buttonY + 50, buttonWidth, buttonHeight), "НАСТРОЙКИ", buttonStyle))
        {
            showSettings = true;
        }

        if (GUI.Button(new Rect(buttonX, buttonY + 100, buttonWidth, buttonHeight), "ГЛАВНОЕ МЕНЮ", buttonStyle))
        {
            GoToMainMenu();
        }
    }

    void DrawSettingsMenu()
    {
        float width = 400;
        float height = 400;
        float x = (Screen.width - width) / 2;
        float y = (Screen.height - height) / 2;

        // Фон меню
        GUIStyle menuStyle = new GUIStyle(GUI.skin.box);
        menuStyle.normal.background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.95f));
        GUI.Box(new Rect(x, y, width, height), "", menuStyle);

        // Заголовок
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.fontSize = 24;
        titleStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(x, y + 20, width, 30), "НАСТРОЙКИ", titleStyle);

        // Стили для текста
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontSize = 14;

        float startY = y + 70;
        float sliderWidth = 200f;

        // Громкость музыки
        GUI.Label(new Rect(x + 20, startY, 150, 25), "Громкость музыки:", labelStyle);
        musicVolume = GUI.HorizontalSlider(new Rect(x + 170, startY + 5, sliderWidth, 20), musicVolume, 0.0f, 1.0f);
        GUI.Label(new Rect(x + 380, startY, 50, 25), Mathf.Round(musicVolume * 100) + "%", labelStyle);

        // Громкость звуков
        GUI.Label(new Rect(x + 20, startY + 40, 150, 25), "Громкость звуков:", labelStyle);
        soundVolume = GUI.HorizontalSlider(new Rect(x + 170, startY + 45, sliderWidth, 20), soundVolume, 0.0f, 1.0f);
        GUI.Label(new Rect(x + 380, startY + 40, 50, 25), Mathf.Round(soundVolume * 100) + "%", labelStyle);

        // Чувствительность мыши
        GUI.Label(new Rect(x + 20, startY + 80, 150, 25), "Чувствительность мыши:", labelStyle);
        mouseSensitivity = GUI.HorizontalSlider(new Rect(x + 170, startY + 85, sliderWidth, 20), mouseSensitivity, 0.1f, 3.0f);
        GUI.Label(new Rect(x + 380, startY + 80, 50, 25), mouseSensitivity.ToString("F1"), labelStyle);

        // Качество графики
        GUI.Label(new Rect(x + 20, startY + 120, 150, 25), "Качество графики:", labelStyle);
        qualityLevel = GUI.Toolbar(new Rect(x + 170, startY + 120, sliderWidth, 25), qualityLevel, qualityNames);

        // Полноэкранный режим
        fullscreen = GUI.Toggle(new Rect(x + 20, startY + 160, 200, 25), fullscreen, "Полноэкранный режим", labelStyle);

        // Кнопки
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 14;
        buttonStyle.fontStyle = FontStyle.Bold;

        float buttonWidth = 180;
        float buttonHeight = 35;
        float buttonX = x + (width - buttonWidth) / 2;

        // Кнопка Применить
        if (GUI.Button(new Rect(buttonX, startY + 220, buttonWidth, buttonHeight), "ПРИМЕНИТЬ", buttonStyle))
        {
            ApplySettings();
        }

        // Кнопка Назад
        if (GUI.Button(new Rect(buttonX, startY + 270, buttonWidth, buttonHeight), "НАЗАД", buttonStyle))
        {
            showSettings = false;
        }

        // Кнопка Сбросить
        if (GUI.Button(new Rect(buttonX, startY + 320, buttonWidth, buttonHeight), "СБРОСИТЬ НАСТРОЙКИ", buttonStyle))
        {
            ResetSettings();
        }
    }

    void ApplySettings()
    {
        // Применяем настройки
        AudioListener.volume = soundVolume;
        Screen.fullScreen = fullscreen;
        QualitySettings.SetQualityLevel(qualityLevel);

        // Сохраняем настройки в PlayerPrefs
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SoundVolume", soundVolume);
        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
        PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
        PlayerPrefs.SetInt("QualityLevel", qualityLevel);
        PlayerPrefs.Save();

        Debug.Log("Настройки применены и сохранены!");
    }

    void ResetSettings()
    {
        // Сбрасываем настройки к значениям по умолчанию
        musicVolume = 0.8f;
        soundVolume = 0.8f;
        mouseSensitivity = 1.0f;
        fullscreen = true;
        qualityLevel = 2;

        // Применяем сброшенные настройки
        ApplySettings();
        Debug.Log("Настройки сброшены!");
    }

    void LoadSettings()
    {
        // Загружаем сохраненные настройки
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        soundVolume = PlayerPrefs.GetFloat("SoundVolume", 0.8f);
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1.0f);
        fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        qualityLevel = PlayerPrefs.GetInt("QualityLevel", 2);

        // Применяем загруженные настройки
        ApplySettings();
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        LoadSettings(); // Загружаем настройки при старте
    }
}