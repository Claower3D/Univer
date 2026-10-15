using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;

public class SimpleMenuManager : MonoBehaviour
{
    public string tutorialScene = "SampleScene";
    public string gameScene = "SampleScene";

    // Переменные для отслеживания открытия окон
    private bool settingsOpen = false;
    private bool gameOptionsOpen = false;
    private bool lobbyBrowserOpen = false;
    private bool inLobby = false;
    private bool inGame = false;
    private bool showPlayerList = false;

    // Переменные для лобби
    private string ipAddress = "127.0.0.1";
    private string playerName = "Игрок";
    private string lobbyName = "Мое лобби";
    private Vector2 scrollPosition = Vector2.zero;

    // Статус подключения
    private string connectionStatus = "Не подключено";
    private bool isConnecting = false;

    // Списки лобби и игроков
    private List<string> availableLobbies = new List<string>();
    private List<string> connectedPlayers = new List<string>();

    // Настройки
    private float volume = 1.0f;
    private bool fullscreen = true;

    // NetworkManager
    private NetworkManager networkManager;

    // Флаг для определения, находимся ли мы в игровой сцене
    private bool isInGameScene = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Проверяем текущую сцену
        CheckCurrentScene();

        // Загружаем настройки
        LoadSettings();

        // Получаем NetworkManager
        networkManager = NetworkManager.Singleton;

        // Настраиваем NetworkManager если он существует
        if (networkManager != null)
        {
            SetupNetworkManager();
        }
        else
        {
            connectionStatus = "Создайте NetworkManager в сцене";
            Debug.LogError("NetworkManager не найден! Создайте GameObject с компонентом NetworkManager");
        }

        // Инициализируем демо-лобби
        InitializeDemoLobbies();

        // Если мы уже в игровой сцене, переходим в режим игры
        if (isInGameScene)
        {
            EnterGameMode();
        }

        // Проверка доступных сцен
        Debug.Log("=== ПРОВЕРКА ДОСТУПНЫХ СЦЕН ===");
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log($"Сцена {i}: {sceneName}");

            if (sceneName == gameScene)
            {
                Debug.Log($"✓ Целевая сцена найдена: {gameScene}");
            }
        }

        // Проверяем, что gameScene действительно "SampleScene"
        if (gameScene != "SampleScene")
        {
            Debug.LogError($"ОШИБКА: gameScene должно быть 'SampleScene', а сейчас: '{gameScene}'");
            gameScene = "SampleScene";
        }
    }

    void CheckCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        isInGameScene = currentScene == gameScene || currentScene == "SampleScene";

        if (isInGameScene)
        {
            Debug.Log($"Обнаружена игровая сцена: {currentScene}, активируем игровой режим");
        }
        else
        {
            Debug.Log($"Текущая сцена: {currentScene}, режим меню");
        }
    }

    void EnterGameMode()
    {
        inGame = true;
        inLobby = false;
        settingsOpen = false;
        gameOptionsOpen = false;
        lobbyBrowserOpen = false;

        Debug.Log("Активирован игровой режим, меню лобби скрыто");
    }

    void Update()
    {
        // Обработка нажатия Tab для показа/скрытия списка игроков в игре
        if (inGame && Input.GetKeyDown(KeyCode.Tab))
        {
            showPlayerList = !showPlayerList;
        }

        // Автоматически определяем, находимся ли мы в игровой сцене
        if (!isInGameScene && (SceneManager.GetActiveScene().name == gameScene || SceneManager.GetActiveScene().name == "SampleScene"))
        {
            EnterGameMode();
            isInGameScene = true;
        }
    }

    void SetupNetworkManager()
    {
        // Проверяем что networkManager не null
        if (networkManager == null)
        {
            Debug.LogError("NetworkManager is null in SetupNetworkManager!");
            return;
        }

        // Подписываемся на события сети
        networkManager.OnClientConnectedCallback += OnClientConnected;
        networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        networkManager.OnServerStarted += OnServerStarted;

        // Подписываемся на события загрузки сцен
        if (networkManager.SceneManager != null)
        {
            networkManager.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
            networkManager.SceneManager.OnSynchronizeComplete += OnSceneSyncComplete;
        }

        connectionStatus = "Готов к подключению";
        Debug.Log("NetworkManager настроен");
    }

    void InitializeDemoLobbies()
    {
        // Добавляем демо-лобби для тестирования
        availableLobbies.Add("Демо-лобби 1");
        availableLobbies.Add("Демо-лобби 2");
        availableLobbies.Add("Демо-лобби 3");
    }

    void OnGUI()
    {
        // Если мы в игровой сцене, показываем только игровой UI
        if (inGame)
        {
            DrawInGameUI();
            return;
        }

        // Иначе показываем меню
        if (inLobby)
        {
            DrawLobbyScreen();
        }
        else if (!settingsOpen && !gameOptionsOpen && !lobbyBrowserOpen)
        {
            DrawMainMenu();
        }
        else if (settingsOpen)
        {
            DrawSettingsWindow();
        }
        else if (gameOptionsOpen)
        {
            DrawGameOptionsWindow();
        }
        else if (lobbyBrowserOpen)
        {
            DrawLobbyBrowserWindow();
        }
    }

    void DrawInGameUI()
    {
        // В игре отображаем только список игроков по нажатию Tab
        if (showPlayerList)
        {
            DrawPlayerListInGame();
        }

        // Минимальная информация о подключении (только для отладки)
        GUIStyle infoStyle = new GUIStyle(GUI.skin.label);
        infoStyle.fontSize = 12;
        infoStyle.normal.textColor = Color.green;

        string playersInfo = networkManager != null && networkManager.IsListening ?
            $"Игроков: {networkManager.ConnectedClients.Count}" :
            "Одиночная игра";

        GUI.Label(new Rect(10, 10, 200, 25), playersInfo, infoStyle);

        // Кнопка возврата в меню (маленькая и в углу)
        if (GUI.Button(new Rect(Screen.width - 100, 10, 90, 25), "МЕНЮ"))
        {
            ReturnToMenu();
        }
    }

    void DrawPlayerListInGame()
    {
        float width = 300;
        float height = 400;
        float x = (Screen.width - width) / 2;
        float y = 50;

        // Полупрозрачный фон
        GUI.color = new Color(0, 0, 0, 0.8f);
        GUI.DrawTexture(new Rect(x, y, width, height), CreateSolidTexture(Color.black));
        GUI.color = Color.white;

        // Заголовок
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.fontSize = 20;
        titleStyle.normal.textColor = Color.yellow;

        GUI.Label(new Rect(x, y + 10, width, 30), "СПИСОК ИГРОКОВ", titleStyle);

        // Инструкция
        GUIStyle hintStyle = new GUIStyle(GUI.skin.label);
        hintStyle.alignment = TextAnchor.MiddleCenter;
        hintStyle.fontSize = 12;
        hintStyle.normal.textColor = Color.green;
        GUI.Label(new Rect(x, y + 40, width, 20), "Нажмите TAB чтобы скрыть", hintStyle);

        // Список игроков
        float listStartY = y + 70;
        for (int i = 0; i < connectedPlayers.Count; i++)
        {
            GUIStyle playerStyle = new GUIStyle(GUI.skin.label);
            playerStyle.fontSize = 16;
            playerStyle.alignment = TextAnchor.MiddleLeft;
            playerStyle.normal.textColor = i == 0 ? Color.yellow : Color.white;

            // Фон для элемента списка
            GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.6f);
            GUI.DrawTexture(new Rect(x + 10, listStartY + i * 35, width - 20, 30), CreateSolidTexture(Color.gray));
            GUI.color = Color.white;

            GUI.Label(new Rect(x + 20, listStartY + i * 35, width - 40, 30),
                     $"{i + 1}. {connectedPlayers[i]} {(i == 0 ? " (Хост)" : "")}", playerStyle);
        }

        // Если игроков нет
        if (connectedPlayers.Count == 0)
        {
            GUIStyle emptyStyle = new GUIStyle(GUI.skin.label);
            emptyStyle.alignment = TextAnchor.MiddleCenter;
            emptyStyle.fontSize = 14;
            emptyStyle.normal.textColor = Color.gray;
            GUI.Label(new Rect(x, listStartY + 50, width, 30), "Нет подключенных игроков", emptyStyle);
        }
    }

    void DrawMainMenu()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Фон
        GUI.color = new Color(0, 0, 0, 0.8f);
        GUI.DrawTexture(new Rect(0, 0, screenWidth, screenHeight), CreateSolidTexture(Color.black));
        GUI.color = Color.white;

        // Главный заголовок игры
        GUIStyle gameTitleStyle = new GUIStyle(GUI.skin.label);
        gameTitleStyle.alignment = TextAnchor.MiddleCenter;
        gameTitleStyle.fontStyle = FontStyle.Bold;
        gameTitleStyle.fontSize = 48;
        gameTitleStyle.normal.textColor = Color.yellow;

        GUI.Label(new Rect(0, 50, screenWidth, 80), "MINING QUARRY SIMULATOR", gameTitleStyle);

        // Подзаголовок меню
        GUIStyle menuTitleStyle = new GUIStyle(GUI.skin.label);
        menuTitleStyle.alignment = TextAnchor.MiddleCenter;
        menuTitleStyle.fontStyle = FontStyle.Bold;
        menuTitleStyle.fontSize = 32;
        menuTitleStyle.normal.textColor = Color.white;

        GUI.Label(new Rect(0, 120, screenWidth, 60), "ГЛАВНОЕ МЕНЮ", menuTitleStyle);

        // Кнопки
        float buttonWidth = 300;
        float buttonHeight = 60;
        float buttonX = (screenWidth - buttonWidth) / 2;
        float startY = 220;
        float spacing = 70;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 18;
        buttonStyle.fontStyle = FontStyle.Bold;

        // Кнопка Обучение
        if (GUI.Button(new Rect(buttonX, startY, buttonWidth, buttonHeight), "ОБУЧЕНИЕ", buttonStyle))
        {
            LoadScene(tutorialScene);
        }

        // Кнопка Играть
        if (GUI.Button(new Rect(buttonX, startY + spacing, buttonWidth, buttonHeight), "ИГРАТЬ", buttonStyle))
        {
            OpenGameOptions();
        }

        // Кнопка Настройки
        if (GUI.Button(new Rect(buttonX, startY + spacing * 2, buttonWidth, buttonHeight), "НАСТРОЙКИ", buttonStyle))
        {
            OpenSettings();
        }

        // Кнопка Выход
        if (GUI.Button(new Rect(buttonX, startY + spacing * 3, buttonWidth, buttonHeight), "ВЫХОД", buttonStyle))
        {
            QuitGame();
        }

        // Статус сети
        GUIStyle statusStyle = new GUIStyle(GUI.skin.label);
        statusStyle.alignment = TextAnchor.LowerLeft;
        statusStyle.fontSize = 12;
        statusStyle.normal.textColor = GetStatusColor();

        GUI.Label(new Rect(10, screenHeight - 30, 400, 25), "Статус: " + connectionStatus, statusStyle);

        // Инструкция если NetworkManager не найден
        if (networkManager == null)
        {
            GUIStyle warningStyle = new GUIStyle(GUI.skin.label);
            warningStyle.alignment = TextAnchor.MiddleCenter;
            warningStyle.fontSize = 14;
            warningStyle.normal.textColor = Color.red;
            GUI.Label(new Rect(0, screenHeight - 100, screenWidth, 30), "⚠️ NetworkManager не найден! Создайте его вручную.", warningStyle);
        }

        // Версия игры
        GUIStyle versionStyle = new GUIStyle(GUI.skin.label);
        versionStyle.alignment = TextAnchor.LowerRight;
        versionStyle.fontSize = 12;
        versionStyle.normal.textColor = Color.gray;

        GUI.Label(new Rect(screenWidth - 150, screenHeight - 30, 140, 25), "Версия 1.0", versionStyle);
    }

    void DrawGameOptionsWindow()
    {
        float width = 500;
        float height = 400;
        float x = (Screen.width - width) / 2;
        float y = (Screen.height - height) / 2;

        // Фон окна
        GUI.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        GUI.DrawTexture(new Rect(x, y, width, height), CreateSolidTexture(Color.gray));
        GUI.color = Color.white;

        // Заголовок окна
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.fontSize = 24;
        titleStyle.normal.textColor = Color.yellow;

        GUI.Label(new Rect(x, y + 15, width, 40), "ВЫБОР РЕЖИМА ИГРЫ", titleStyle);

        // Область контента
        float contentWidth = width - 40;
        float contentHeight = 250;
        float contentX = x + 20;
        float contentY = y + 70;

        // Фон области контента
        GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        GUI.DrawTexture(new Rect(contentX, contentY, contentWidth, contentHeight), CreateSolidTexture(Color.black));
        GUI.color = Color.white;

        float currentY = contentY + 20;

        // Имя игрока
        GUI.Label(new Rect(contentX + 20, currentY, 100, 25), "Ваше имя:");
        playerName = GUI.TextField(new Rect(contentX + 120, currentY, contentWidth - 140, 25), playerName);
        currentY += 40;

        // Инструкции
        GUIStyle infoStyle = new GUIStyle(GUI.skin.label);
        infoStyle.alignment = TextAnchor.MiddleCenter;
        infoStyle.fontSize = 12;
        infoStyle.normal.textColor = Color.green;

        if (networkManager == null)
        {
            GUI.Label(new Rect(contentX, currentY, contentWidth, 40),
                     "NetworkManager не найден!\nСоздайте GameObject с NetworkManager", infoStyle);
            currentY += 50;
        }
        else
        {
            GUI.Label(new Rect(contentX, currentY, contentWidth, 40),
                     "Реальная сетевая система\nМожно подключаться друг к другу!", infoStyle);
            currentY += 50;
        }

        // Кнопка создания лобби (ХОСТ)
        GUIStyle hostButtonStyle = new GUIStyle(GUI.skin.button);
        hostButtonStyle.fontSize = 16;
        hostButtonStyle.fontStyle = FontStyle.Bold;

        GUI.enabled = networkManager != null;
        if (GUI.Button(new Rect(contentX + 20, currentY, contentWidth - 40, 50), "СОЗДАТЬ ЛОББИ (ХОСТ)", hostButtonStyle))
        {
            CreateLobbyAsHost();
        }
        currentY += 70;

        // Кнопка поиска лобби
        GUIStyle browseButtonStyle = new GUIStyle(GUI.skin.button);
        browseButtonStyle.fontSize = 16;
        browseButtonStyle.fontStyle = FontStyle.Bold;

        if (GUI.Button(new Rect(contentX + 20, currentY, contentWidth - 40, 50), "ПОДКЛЮЧИТЬСЯ К ЛОББИ", browseButtonStyle))
        {
            BrowseLobbies();
        }
        GUI.enabled = true;

        // Статус подключения
        GUIStyle statusStyle = new GUIStyle(GUI.skin.label);
        statusStyle.alignment = TextAnchor.MiddleCenter;
        statusStyle.normal.textColor = GetStatusColor();
        GUI.Label(new Rect(contentX, y + height - 60, contentWidth, 25), connectionStatus, statusStyle);

        // Кнопка Назад
        GUIStyle backButtonStyle = new GUIStyle(GUI.skin.button);
        backButtonStyle.fontSize = 14;
        backButtonStyle.fontStyle = FontStyle.Bold;

        if (GUI.Button(new Rect(x + (width - 100) / 2, y + height - 45, 100, 35), "НАЗАД", backButtonStyle))
        {
            CloseGameOptions();
        }
    }

    void DrawLobbyBrowserWindow()
    {
        float width = 600;
        float height = 500;
        float x = (Screen.width - width) / 2;
        float y = (Screen.height - height) / 2;

        // Фон окна
        GUI.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        GUI.DrawTexture(new Rect(x, y, width, height), CreateSolidTexture(Color.gray));
        GUI.color = Color.white;

        // Заголовок окна
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.fontSize = 24;
        titleStyle.normal.textColor = Color.yellow;

        GUI.Label(new Rect(x, y + 15, width, 40), "ПОДКЛЮЧЕНИЕ К ЛОББИ", titleStyle);

        // Имя игрока
        GUI.Label(new Rect(x + 20, y + 60, 100, 25), "Ваше имя:");
        playerName = GUI.TextField(new Rect(x + 120, y + 60, 200, 25), playerName);

        // Прямое подключение по IP
        GUIStyle ipStyle = new GUIStyle(GUI.skin.label);
        ipStyle.normal.textColor = Color.white;

        GUI.Label(new Rect(x + 20, y + 90, 100, 25), "IP сервера:", ipStyle);
        ipAddress = GUI.TextField(new Rect(x + 120, y + 90, 150, 25), ipAddress);

        GUI.enabled = networkManager != null;
        if (GUI.Button(new Rect(x + 280, y + 90, 120, 25), "ПОДКЛЮЧИТЬСЯ ПО IP"))
        {
            ConnectToServerByIP();
        }

        // Кнопка обновления
        if (GUI.Button(new Rect(x + 410, y + 90, 120, 25), "ОБНОВИТЬ СПИСОК"))
        {
            RefreshLobbies();
        }
        GUI.enabled = true;

        // Информация о подключении
        GUIStyle helpStyle = new GUIStyle(GUI.skin.label);
        helpStyle.alignment = TextAnchor.MiddleCenter;
        helpStyle.fontSize = 11;
        helpStyle.normal.textColor = Color.cyan;
        GUI.Label(new Rect(x + 20, y + 120, width - 40, 30),
                 "Для подключения к другу: введи его IP и нажми 'ПОДКЛЮЧИТЬСЯ ПО IP'", helpStyle);

        // Список лобби
        float listWidth = width - 40;
        float listHeight = 250;
        float listX = x + 20;
        float listY = y + 150;

        GUI.Box(new Rect(listX, listY, listWidth, listHeight), "ДОСТУПНЫЕ ЛОББИ (ДЕМО)");

        // Заголовок списка
        GUIStyle listHeaderStyle = new GUIStyle(GUI.skin.label);
        listHeaderStyle.fontStyle = FontStyle.Bold;
        listHeaderStyle.fontSize = 16;
        listHeaderStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(listX + 10, listY + 10, listWidth - 20, 25), "ДЕМО-ЛОББИ (для тестирования):", listHeaderStyle);

        // Прокручиваемый список лобби
        scrollPosition = GUI.BeginScrollView(new Rect(listX + 5, listY + 40, listWidth - 10, listHeight - 45),
                                           scrollPosition,
                                           new Rect(0, 0, listWidth - 30, availableLobbies.Count * 40));

        for (int i = 0; i < availableLobbies.Count; i++)
        {
            float itemY = i * 40;

            // Фон элемента
            GUI.color = i % 2 == 0 ? new Color(0.3f, 0.3f, 0.3f, 0.8f) : new Color(0.25f, 0.25f, 0.25f, 0.8f);
            GUI.DrawTexture(new Rect(5, itemY, listWidth - 40, 35), CreateSolidTexture(Color.gray));
            GUI.color = Color.white;

            // Название лобби
            GUI.Label(new Rect(15, itemY + 5, listWidth - 150, 25), availableLobbies[i]);

            // Кнопка присоединения (для демо используем локальный хост)
            GUI.enabled = networkManager != null;
            if (GUI.Button(new Rect(listWidth - 130, itemY + 5, 100, 25), "ТЕСТ ПОДКЛ."))
            {
                TestConnectToLocalHost();
            }
            GUI.enabled = true;
        }

        GUI.EndScrollView();

        // Создание быстрого лобби
        float quickY = listY + listHeight + 10;
        GUIStyle quickSectionStyle = new GUIStyle(GUI.skin.label);
        quickSectionStyle.fontStyle = FontStyle.Bold;
        quickSectionStyle.fontSize = 16;
        quickSectionStyle.normal.textColor = Color.green;
        GUI.Label(new Rect(listX, quickY, listWidth, 25), "БЫСТРОЕ СОЗДАНИЕ ЛОББИ:", quickSectionStyle);

        GUI.Label(new Rect(listX, quickY + 30, 100, 25), "Название:");
        lobbyName = GUI.TextField(new Rect(listX + 80, quickY + 30, 200, 25), lobbyName);

        GUI.enabled = networkManager != null;
        if (GUI.Button(new Rect(listX + 290, quickY + 30, 120, 25), "СОЗДАТЬ ЛОББИ"))
        {
            CreateQuickLobby();
        }
        GUI.enabled = true;

        // Статус
        GUIStyle statusStyle = new GUIStyle(GUI.skin.label);
        statusStyle.alignment = TextAnchor.MiddleCenter;
        statusStyle.normal.textColor = GetStatusColor();
        GUI.Label(new Rect(listX, quickY + 70, listWidth, 25), connectionStatus, statusStyle);

        // Кнопка Назад
        GUIStyle backButtonStyle = new GUIStyle(GUI.skin.button);
        backButtonStyle.fontSize = 14;
        backButtonStyle.fontStyle = FontStyle.Bold;

        if (GUI.Button(new Rect(x + (width - 100) / 2, y + height - 45, 100, 35), "НАЗАД", backButtonStyle))
        {
            CloseLobbyBrowser();
        }
    }

    void DrawLobbyScreen()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Фон
        GUI.color = new Color(0, 0.1f, 0.1f, 0.9f);
        GUI.DrawTexture(new Rect(0, 0, screenWidth, screenHeight), CreateSolidTexture(Color.black));
        GUI.color = Color.white;

        // Заголовок лобби
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.fontSize = 36;
        titleStyle.normal.textColor = Color.cyan;

        GUI.Label(new Rect(0, 50, screenWidth, 60), "ЛОББИ", titleStyle);

        // Информация о лобби
        GUIStyle infoStyle = new GUIStyle(GUI.skin.label);
        infoStyle.alignment = TextAnchor.MiddleCenter;
        infoStyle.fontSize = 18;
        infoStyle.normal.textColor = Color.white;

        bool isHost = networkManager != null && networkManager.IsHost;
        string lobbyInfo = isHost ?
            $"Вы ХОСТ лобби: {lobbyName}" :
            $"Вы подключены к лобби";

        GUI.Label(new Rect(0, 120, screenWidth, 30), lobbyInfo, infoStyle);

        // Информация о подключении
        GUIStyle connStyle = new GUIStyle(GUI.skin.label);
        connStyle.alignment = TextAnchor.MiddleCenter;
        connStyle.fontSize = 14;
        connStyle.normal.textColor = Color.yellow;

        string connInfo = networkManager != null ?
            $"Игроков в сети: {networkManager.ConnectedClients.Count}" :
            "Сеть не активна";
        GUI.Label(new Rect(0, 150, screenWidth, 25), connInfo, connStyle);

        // Список игроков
        float playersWidth = 400;
        float playersHeight = 300;
        float playersX = (screenWidth - playersWidth) / 2;
        float playersY = 190;

        GUI.Box(new Rect(playersX, playersY, playersWidth, playersHeight), "ИГРОКИ В ЛОББИ");

        // Обновляем список игроков
        UpdatePlayerList();

        // Отображаем список игроков
        float listStartY = playersY + 40;
        for (int i = 0; i < connectedPlayers.Count; i++)
        {
            GUIStyle playerStyle = new GUIStyle(GUI.skin.label);
            playerStyle.fontSize = 16;
            playerStyle.normal.textColor = i == 0 ? Color.yellow : Color.white;

            GUI.Label(new Rect(playersX + 20, listStartY + i * 30, playersWidth - 40, 25),
                     $"{i + 1}. {connectedPlayers[i]} {(i == 0 ? " (Хост)" : "")}", playerStyle);
        }

        // Кнопки управления
        float buttonWidth = 200;
        float buttonHeight = 50;
        float buttonX = (screenWidth - buttonWidth) / 2;
        float buttonY = playersY + playersHeight + 30;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 16;
        buttonStyle.fontStyle = FontStyle.Bold;

        // Только хост может начать игру
        GUI.enabled = networkManager != null && networkManager.IsHost;
        if (GUI.Button(new Rect(buttonX, buttonY, buttonWidth, buttonHeight), "НАЧАТЬ ИГРУ", buttonStyle))
        {
            StartGameForAllPlayers();
        }
        GUI.enabled = true;

        // Кнопка выхода из лобби
        if (GUI.Button(new Rect(buttonX, buttonY + 70, buttonWidth, buttonHeight), "ПОКИНУТЬ ЛОББИ", buttonStyle))
        {
            LeaveLobby();
        }

        // Инструкция для хоста
        if (networkManager != null && networkManager.IsHost)
        {
            GUIStyle hostStyle = new GUIStyle(GUI.skin.label);
            hostStyle.alignment = TextAnchor.MiddleCenter;
            hostStyle.fontSize = 12;
            hostStyle.normal.textColor = Color.green;
            GUI.Label(new Rect(0, buttonY + 140, screenWidth, 25),
                     "Сообщите свой IP друзьям для подключения: " + GetLocalIP(), hostStyle);
        }

        // Статус
        GUIStyle statusStyle = new GUIStyle(GUI.skin.label);
        statusStyle.alignment = TextAnchor.MiddleCenter;
        statusStyle.normal.textColor = GetStatusColor();
        GUI.Label(new Rect(0, screenHeight - 50, screenWidth, 25), connectionStatus, statusStyle);
    }

    void UpdatePlayerList()
    {
        connectedPlayers.Clear();

        if (networkManager != null && networkManager.IsServer)
        {
            // Добавляем хоста
            connectedPlayers.Add(playerName + " (Вы)");

            // Добавляем подключенных клиентов
            foreach (var client in networkManager.ConnectedClients)
            {
                if (client.Key != networkManager.LocalClientId)
                {
                    connectedPlayers.Add($"Игрок {client.Key}");
                }
            }
        }
        else if (networkManager != null && networkManager.IsClient)
        {
            // Для клиента показываем только себя
            connectedPlayers.Add(playerName + " (Вы)");
        }
        else
        {
            connectedPlayers.Add(playerName + " (Вы)");
        }
    }

    Color GetStatusColor()
    {
        if (connectionStatus.Contains("Успешно") || connectionStatus.Contains("Подключено") || connectionStatus.Contains("Запущен"))
            return Color.green;
        if (connectionStatus.Contains("Ошибка") || connectionStatus.Contains("Не удалось") || connectionStatus.Contains("Отключено"))
            return Color.red;
        if (connectionStatus.Contains("Подключение") || connectionStatus.Contains("Запуск") || connectionStatus.Contains("Ожидание"))
            return Color.yellow;
        return Color.white;
    }

    // === СЕТЕВЫЕ МЕТОДЫ ===

    void CreateLobbyAsHost()
    {
        if (string.IsNullOrEmpty(playerName))
        {
            connectionStatus = "Введите имя игрока!";
            return;
        }

        if (networkManager == null)
        {
            connectionStatus = "NetworkManager не найден!";
            return;
        }

        StartCoroutine(CreateLobbyCoroutine());
    }

    IEnumerator CreateLobbyCoroutine()
    {
        connectionStatus = "Запуск сервера...";
        isConnecting = true;

        // Останавливаем предыдущие подключения
        if (networkManager != null && networkManager.IsListening)
        {
            networkManager.Shutdown();
            yield return new WaitForSeconds(0.5f);
        }

        try
        {
            // Запускаем хост
            if (networkManager != null && networkManager.StartHost())
            {
                connectionStatus = "Лобби создано! Ожидание игроков...";
                inLobby = true;
                Debug.Log("Лобби создано успешно! IP: " + GetLocalIP());
            }
            else
            {
                connectionStatus = "Ошибка создания лобби";
                Debug.LogError("Не удалось запустить хост!");
            }
        }
        catch (System.Exception e)
        {
            connectionStatus = "Ошибка: " + e.Message;
            Debug.LogError($"Ошибка при создании лобби: {e.Message}");
        }

        isConnecting = false;
    }

    void ConnectToServerByIP()
    {
        if (string.IsNullOrEmpty(playerName))
        {
            connectionStatus = "Введите имя игрока!";
            return;
        }

        if (string.IsNullOrEmpty(ipAddress) || ipAddress == "127.0.0.1")
        {
            connectionStatus = "Введите реальный IP адрес сервера!";
            return;
        }

        if (networkManager == null)
        {
            connectionStatus = "NetworkManager не найден!";
            return;
        }

        StartCoroutine(ConnectToServerCoroutine());
    }

    IEnumerator ConnectToServerCoroutine()
    {
        connectionStatus = $"Подключение к {ipAddress}...";
        isConnecting = true;

        // Останавливаем предыдущие подключения
        if (networkManager != null && networkManager.IsListening)
        {
            networkManager.Shutdown();
            yield return new WaitForSeconds(0.5f);
        }

        try
        {
            // Пытаемся подключиться как клиент
            if (networkManager != null && networkManager.StartClient())
            {
                connectionStatus = "Подключение установлено!";
                Debug.Log($"Подключение к серверу {ipAddress}");
            }
            else
            {
                connectionStatus = "Ошибка подключения";
                Debug.LogError("Не удалось подключиться к серверу!");
            }
        }
        catch (System.Exception e)
        {
            connectionStatus = "Ошибка: " + e.Message;
            Debug.LogError($"Ошибка при подключении: {e.Message}");
        }

        isConnecting = false;
    }

    void TestConnectToLocalHost()
    {
        // Для тестирования подключаемся к локальному хосту
        ipAddress = "127.0.0.1";
        ConnectToServerByIP();
    }

    void CreateQuickLobby()
    {
        if (string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(lobbyName))
        {
            connectionStatus = "Заполните все поля!";
            return;
        }

        CreateLobbyAsHost();
    }

    // === СЕТЕВЫЕ СОБЫТИЯ ===

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Клиент подключен: {clientId}");

        if (networkManager != null && clientId == networkManager.LocalClientId)
        {
            connectionStatus = "Успешно подключено к лобби!";
            isConnecting = false;
            inLobby = true;

            // Закрываем меню лобби
            lobbyBrowserOpen = false;
            gameOptionsOpen = false;
        }

        // Обновляем список игроков при подключении нового клиента
        UpdatePlayerList();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Клиент отключен: {clientId}");

        if (networkManager != null && clientId == networkManager.LocalClientId)
        {
            connectionStatus = "Отключено от лобби";
            isConnecting = false;
            inLobby = false;
        }

        // Обновляем список игроков при отключении клиента
        UpdatePlayerList();
    }

    private void OnServerStarted()
    {
        Debug.Log("Сервер запущен!");
        connectionStatus = "Лобби создано! Ожидание игроков...";
        isConnecting = false;
        inLobby = true;
    }

    private void OnSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        Debug.Log($"Сцена загружена: {sceneName} для клиентов: {clientsCompleted.Count}");

        // Проверяем, является ли загруженная сцена игровой
        if (sceneName == gameScene || sceneName == "SampleScene")
        {
            EnterGameMode();
            Debug.Log($"✅ Все {clientsCompleted.Count} игроков успешно загрузились на карту: {sceneName}");
        }
        else
        {
            // Если это не игровая сцена, остаемся в режиме меню
            inGame = false;
            inLobby = false;
        }

        // Обновляем список игроков для отображения в игре
        UpdatePlayerList();
    }

    private void OnSceneSyncComplete(ulong clientId)
    {
        Debug.Log($"Клиент {clientId} синхронизировал сцену");

        // Когда клиент синхронизирует сцену, он тоже попадает на ту же карту
        if (networkManager != null && networkManager.IsServer)
        {
            Debug.Log($"✅ Все игроки теперь на одной карте. Подключено игроков: {networkManager.ConnectedClients.Count}");
        }
    }

    // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ===

    string GetLocalIP()
    {
        try
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка получения IP: {e.Message}");
        }
        return "127.0.0.1";
    }

    void RefreshLobbies()
    {
        connectionStatus = "Обновление списка лобби...";
        StartCoroutine(UpdateLobbyList());
    }

    IEnumerator UpdateLobbyList()
    {
        yield return new WaitForSeconds(1f);
        connectionStatus = "Список лобби обновлен";
    }

    void StartGameForAllPlayers()
    {
        Debug.Log("Хост начинает игру для всех игроков...");

        if (networkManager == null || !networkManager.IsServer)
        {
            Debug.LogError("Только хост может начать игру!");
            connectionStatus = "Ошибка: только хост может начать игру";
            return;
        }

        // Принудительно устанавливаем SampleScene
        string sceneToLoad = "SampleScene";
        Debug.Log($"Хост начинает загрузку сцены для всех игроков: {sceneToLoad}");

        // Используем NetworkSceneManager для загрузки сцены на всех клиентах
        if (networkManager.SceneManager != null)
        {
            connectionStatus = "Загружаем игру для всех игроков...";

            // Загружаем сцену для всех подключенных клиентов
            var loadResult = networkManager.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);

            if (loadResult == SceneEventProgressStatus.Started)
            {
                Debug.Log($"✅ Сцена {sceneToLoad} начала загружаться для всех игроков");
                connectionStatus = "Переносим всех игроков на карту...";
            }
            else
            {
                Debug.LogError($"❌ Не удалось начать загрузку сцены {sceneToLoad} через NetworkSceneManager. Статус: {loadResult}");
                connectionStatus = "Ошибка загрузки сцены!";

                // Fallback: простая загрузка сцены только для хоста
                LoadSceneDirectly(sceneToLoad);
            }
        }
        else
        {
            Debug.LogError("❌ NetworkSceneManager не доступен! Используем обычную загрузку.");
            LoadSceneDirectly(sceneToLoad);
        }
    }

    void LoadSceneDirectly(string sceneName)
    {
        // Прямая загрузка сцены без сетевых функций
        SceneManager.LoadScene(sceneName);
        // inGame будет установлено в Start() при загрузке новой сцены
    }

    void LeaveLobby()
    {
        if (networkManager != null)
        {
            networkManager.Shutdown();
        }

        inLobby = false;
        connectionStatus = "Отключено от лобби";
        connectedPlayers.Clear();
    }

    void ReturnToMenu()
    {
        if (networkManager != null)
        {
            networkManager.Shutdown();
        }

        inGame = false;
        inLobby = false;
        isInGameScene = false;
        connectionStatus = "Не подключено";
        connectedPlayers.Clear();

        // Перезагружаем меню сцену чтобы сбросить состояние
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OpenGameOptions()
    {
        gameOptionsOpen = true;
        connectionStatus = "Выберите действие";
    }

    void CloseGameOptions()
    {
        gameOptionsOpen = false;
    }

    void BrowseLobbies()
    {
        lobbyBrowserOpen = true;
        gameOptionsOpen = false;
        connectionStatus = "Поиск доступных лобби...";
    }

    void CloseLobbyBrowser()
    {
        lobbyBrowserOpen = false;
        connectionStatus = "Не подключено";
    }

    // Настройки
    void OpenSettings()
    {
        settingsOpen = true;
    }

    void DrawSettingsWindow()
    {
        float width = 450;
        float height = 350;
        float x = (Screen.width - width) / 2;
        float y = (Screen.height - height) / 2;

        // Фон окна настроек
        GUI.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        GUI.DrawTexture(new Rect(x, y, width, height), CreateSolidTexture(Color.gray));
        GUI.color = Color.white;

        // Заголовок окна настроек
        GUIStyle settingsTitleStyle = new GUIStyle(GUI.skin.label);
        settingsTitleStyle.alignment = TextAnchor.MiddleCenter;
        settingsTitleStyle.fontStyle = FontStyle.Bold;
        settingsTitleStyle.fontSize = 20;
        settingsTitleStyle.normal.textColor = Color.yellow;

        GUI.Label(new Rect(x, y + 15, width, 30), "MINING QUARRY SIMULATOR - НАСТРОЙКИ", settingsTitleStyle);

        // Основной заголовок настроек
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(x, y + 45, width, 25), "НАСТРОЙКИ ИГРЫ", labelStyle);

        // Настройка громкости
        GUI.Label(new Rect(x + 20, y + 80, 100, 30), "Громкость:");
        volume = GUI.HorizontalSlider(new Rect(x + 120, y + 85, 200, 30), volume, 0f, 1f);
        GUI.Label(new Rect(x + 330, y + 80, 50, 30), Mathf.RoundToInt(volume * 100) + "%");
        AudioListener.volume = volume;

        // Настройка качества графики
        GUI.Label(new Rect(x + 20, y + 120, 100, 30), "Качество:");
        GUIStyle qualityButtonStyle = new GUIStyle(GUI.skin.button);
        qualityButtonStyle.fontSize = 10;

        if (GUI.Button(new Rect(x + 120, y + 120, 80, 30), "Низкое", qualityButtonStyle))
        {
            QualitySettings.SetQualityLevel(0);
        }
        if (GUI.Button(new Rect(x + 210, y + 120, 80, 30), "Среднее", qualityButtonStyle))
        {
            QualitySettings.SetQualityLevel(2);
        }
        if (GUI.Button(new Rect(x + 300, y + 120, 80, 30), "Высокое", qualityButtonStyle))
        {
            QualitySettings.SetQualityLevel(4);
        }

        // Текущий уровень качества
        string currentQuality = QualitySettings.names[QualitySettings.GetQualityLevel()];
        GUI.Label(new Rect(x + 120, y + 155, 200, 20), "Текущее: " + currentQuality);

        // Полноэкранный режим
        fullscreen = GUI.Toggle(new Rect(x + 20, y + 180, 200, 30), fullscreen, " Полноэкранный режим");
        Screen.fullScreen = fullscreen;

        // Разрешение экрана
        GUI.Label(new Rect(x + 20, y + 220, 100, 30), "Разрешение:");
        if (GUI.Button(new Rect(x + 120, y + 220, 100, 30), "1280x720"))
        {
            Screen.SetResolution(1280, 720, fullscreen);
        }
        if (GUI.Button(new Rect(x + 230, y + 220, 100, 30), "1920x1080"))
        {
            Screen.SetResolution(1920, 1080, fullscreen);
        }

        // Сброс настроек
        if (GUI.Button(new Rect(x + 20, y + 270, 120, 30), "Сбросить настройки"))
        {
            ResetSettings();
        }

        // Сохранение настроек
        if (GUI.Button(new Rect(x + 150, y + 270, 120, 30), "Сохранить"))
        {
            SaveSettings();
        }

        // Кнопка Назад
        GUIStyle backButtonStyle = new GUIStyle(GUI.skin.button);
        backButtonStyle.fontSize = 14;
        backButtonStyle.fontStyle = FontStyle.Bold;

        if (GUI.Button(new Rect(x + 175, y + 310, 100, 35), "НАЗАД", backButtonStyle))
        {
            CloseSettings();
        }
    }

    void CloseSettings()
    {
        settingsOpen = false;
        SaveSettings();
    }

    void LoadSettings()
    {
        volume = PlayerPrefs.GetFloat("Volume", 1.0f);
        fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        playerName = PlayerPrefs.GetString("PlayerName", "Игрок");

        AudioListener.volume = volume;
        Screen.fullScreen = fullscreen;
    }

    void SaveSettings()
    {
        PlayerPrefs.SetFloat("Volume", volume);
        PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();
    }

    void ResetSettings()
    {
        volume = 1.0f;
        fullscreen = true;
        AudioListener.volume = volume;
        Screen.fullScreen = fullscreen;
        QualitySettings.SetQualityLevel(2);
    }

    void LoadScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            if (networkManager != null)
            {
                networkManager.Shutdown();
            }

            // Принудительно устанавливаем SampleScene если передан GameScene
            string sceneToLoad = sceneName == "GameScene" ? "SampleScene" : sceneName;
            Debug.Log($"Загружаем сцену: {sceneToLoad} (вместо {sceneName})");

            // ПРОСТАЯ ЗАГРУЗКА СЦЕНЫ
            SceneManager.LoadScene(sceneToLoad);
            // inGame будет установлено автоматически в Start() при загрузке новой сцены
        }
    }

    void QuitGame()
    {
        if (networkManager != null)
        {
            networkManager.Shutdown();
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    Texture2D CreateSolidTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    void OnDestroy()
    {
        if (networkManager != null)
        {
            networkManager.OnClientConnectedCallback -= OnClientConnected;
            networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            networkManager.OnServerStarted -= OnServerStarted;
            if (networkManager.SceneManager != null)
            {
                networkManager.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
                networkManager.SceneManager.OnSynchronizeComplete -= OnSceneSyncComplete;
            }
        }
    }
}