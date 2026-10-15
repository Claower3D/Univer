using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject localGamePanel;
    public GameObject settingsPanel;

    [Header("Network Settings")]
    public string gameSceneName = "GameScene";
    private string gameName = "Моя игра";
    private string playerName = "Игрок";
    private string ipAddress = "localhost";
    private string port = "7777";

    void Start()
    {
        // Показываем главное меню при старте
        ShowMainMenu();

        // Загружаем сохраненные настройки
        playerName = PlayerPrefs.GetString("PlayerName", "Игрок");
    }

    // === ОСНОВНЫЕ КНОПКИ ГЛАВНОГО МЕНЮ ===

    public void OnLocalPlayClicked()
    {
        ShowLocalGameMenu();
        Debug.Log("Local Play button clicked");
    }

    public void OnOnlinePlayClicked()
    {
        Debug.Log("Online Play button clicked");
        // Здесь может быть логика для онлайн игры
    }

    public void OnSettingsClicked()
    {
        ShowSettingsMenu();
        Debug.Log("Settings button clicked");
    }

    public void OnQuitClicked()
    {
        Debug.Log("Quit button clicked");
        Application.Quit();
    }

    // === ЛОКАЛЬНАЯ ИГРА ===

    public void OnCreateGameClicked()
    {
        Debug.Log($"Creating game: {gameName}, Player: {playerName}, Port: {port}");

        // Здесь логика создания сервера
        // NetworkManager.Singleton.StartHost();

        // Переход в игровую сцену
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnJoinGameClicked()
    {
        Debug.Log($"Joining game: {ipAddress}:{port}, Player: {playerName}");

        // Здесь логика подключения к серверу
        // NetworkManager.Singleton.StartClient();

        // Переход в игровую сцену
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnQuickPlayClicked()
    {
        gameName = "Быстрая игра " + Random.Range(1000, 9999);
        Debug.Log($"Quick play: {gameName}");
        OnCreateGameClicked();
    }

    // === НАСТРОЙКИ ===

    public void OnSettingsApplied()
    {
        // Сохраняем настройки
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();

        ShowMainMenu();
        Debug.Log("Settings applied");
    }

    // === УПРАВЛЕНИЕ ПАНЕЛЯМИ ===

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        localGamePanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public void ShowLocalGameMenu()
    {
        mainMenuPanel.SetActive(false);
        localGamePanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void ShowSettingsMenu()
    {
        mainMenuPanel.SetActive(false);
        localGamePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // === МЕТОДЫ ДЛЯ UI ===

    public void SetGameName(string name)
    {
        gameName = name;
    }

    public void SetPlayerName(string name)
    {
        playerName = name;
    }

    public void SetIpAddress(string ip)
    {
        ipAddress = ip;
    }

    public void SetPort(string newPort)
    {
        port = newPort;
    }

    // === BACK BUTTONS ===

    public void OnBackFromLocalGame()
    {
        ShowMainMenu();
    }

    public void OnBackFromSettings()
    {
        ShowMainMenu();
    }
}