using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LocalGameMenu : MonoBehaviour
{
    private bool showLocalMenu = false;
    private string gameName = "Моя игра";
    private string playerName = "Игрок";
    private string ipAddress = "localhost";
    private string port = "7777";
    public string targetGameScene = "SampleScene"; // Сцена игры
    private Vector2 scrollPosition;
    private bool isCreatingGame = false;
    private bool isJoiningGame = false;

    // Список доступных игр (для демонстрации)
    private List<string> availableGames = new List<string>()
    {
        "Игра Петра",
        "Сервер Алексея",
        "Локалка Марии",
        "Тестовый сервер"
    };

    void Update()
    {
        // Открытие/закрытие меню по клавише L
        if (Input.GetKeyDown(KeyCode.L))
        {
            showLocalMenu = !showLocalMenu;
            Cursor.lockState = showLocalMenu ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = showLocalMenu;
        }
    }

    void OnGUI()
    {
        if (!showLocalMenu) return;

        // Темный фон
        GUI.color = new Color(0, 0, 0, 0.85f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), MakeTex(1, 1, Color.black));
        GUI.color = Color.white;

        DrawLocalGameMenu();
    }

    void DrawLocalGameMenu()
    {
        float width = 560;
        float height = 680;
        float x = (Screen.width - width) / 2;
        float y = (Screen.height - height) / 2;

        // Фон меню
        GUIStyle menuStyle = new GUIStyle(GUI.skin.box);
        menuStyle.normal.background = MakeTex(2, 2, new Color(0.12f, 0.12f, 0.14f, 0.98f));
        GUI.Box(new Rect(x, y, width, height), "", menuStyle);

        // Заголовок
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.fontSize = 26;
        titleStyle.normal.textColor = new Color(1f, 0.8f, 0.2f);
        GUI.Label(new Rect(x, y + 15, width, 35), "ЛОКАЛЬНАЯ ИГРА И ЛОББИ", titleStyle);

        // Стили
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontSize = 14;

        GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField);
        textFieldStyle.fontSize = 14;
        textFieldStyle.fixedHeight = 30;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 14;
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.fixedHeight = 35;

        float contentY = y + 60;

        if (!isCreatingGame && !isJoiningGame)
        {
            DrawMainLocalMenu(x, contentY, width, buttonStyle, labelStyle);
        }
        else if (isCreatingGame)
        {
            DrawCreateGameMenu(x, contentY, width, labelStyle, textFieldStyle, buttonStyle);
        }
        else if (isJoiningGame)
        {
            DrawJoinGameMenu(x, contentY, width, labelStyle, textFieldStyle, buttonStyle);
        }

        // Кнопка закрытия (крестик)
        if (GUI.Button(new Rect(x + width - 40, y + 10, 30, 30), "X", buttonStyle))
        {
            showLocalMenu = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void DrawVehicleSelector(float x, float startY, float width, GUIStyle buttonStyle, GUIStyle labelStyle)
    {
        // Рамка секции выбора техники
        float boxHeight = 150;
        GUIStyle sectionBoxStyle = new GUIStyle(GUI.skin.box);
        sectionBoxStyle.normal.background = MakeTex(2, 2, new Color(0.18f, 0.18f, 0.22f, 0.9f));
        GUI.Box(new Rect(x + 20, startY, width - 40, boxHeight), "", sectionBoxStyle);

        // Заголовок секции
        GUIStyle headerStyle = new GUIStyle(labelStyle);
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.normal.textColor = new Color(0.4f, 0.85f, 1f);
        GUI.Label(new Rect(x + 20, startY + 8, width - 40, 22), "ВЫБОР СПЕЦТЕХНИКИ ДЛЯ ИГРЫ:", headerStyle);

        // Получаем информацию о текущей технике
        VehicleInfo currentInfo = VehicleSelectionManager.GetInfo(VehicleSelectionManager.SelectedVehicle);

        // Кнопка Влево
        if (GUI.Button(new Rect(x + 35, startY + 36, 45, 36), "◀", buttonStyle))
        {
            VehicleSelectionManager.SelectPrevious();
        }

        // Название техники
        GUIStyle vehicleTitleStyle = new GUIStyle(labelStyle);
        vehicleTitleStyle.alignment = TextAnchor.MiddleCenter;
        vehicleTitleStyle.fontSize = 18;
        vehicleTitleStyle.fontStyle = FontStyle.Bold;
        vehicleTitleStyle.normal.textColor = Color.yellow;
        GUI.Label(new Rect(x + 85, startY + 36, width - 170, 36), currentInfo.displayName, vehicleTitleStyle);

        // Кнопка Вправо
        if (GUI.Button(new Rect(x + width - 80, startY + 36, 45, 36), "▶", buttonStyle))
        {
            VehicleSelectionManager.SelectNext();
        }

        // Описание техники
        GUIStyle descStyle = new GUIStyle(labelStyle);
        descStyle.fontSize = 12;
        descStyle.wordWrap = true;
        descStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
        GUI.Label(new Rect(x + 35, startY + 76, width - 70, 38), currentInfo.description, descStyle);

        // Подсказка по управлению
        GUIStyle hintStyle = new GUIStyle(labelStyle);
        hintStyle.fontSize = 11;
        hintStyle.fontStyle = FontStyle.Italic;
        hintStyle.normal.textColor = new Color(0.5f, 1f, 0.5f);
        GUI.Label(new Rect(x + 35, startY + 115, width - 70, 25), $"Управление: {currentInfo.controlsHint.Replace("\n", " | ")}", hintStyle);
    }

    void DrawMainLocalMenu(float x, float startY, float width, GUIStyle buttonStyle, GUIStyle labelStyle)
    {
        // 1. Блок выбора спецтехники
        DrawVehicleSelector(x, startY, width, buttonStyle, labelStyle);

        float nextY = startY + 165;
        float buttonWidth = 320;
        float buttonHeight = 45;
        float buttonX = x + (width - buttonWidth) / 2;

        // 2. Кнопки лобби
        if (GUI.Button(new Rect(buttonX, nextY, buttonWidth, buttonHeight), "СОЗДАТЬ ИГРУ (СЕРВЕР)", buttonStyle))
        {
            isCreatingGame = true;
        }

        if (GUI.Button(new Rect(buttonX, nextY + 55, buttonWidth, buttonHeight), "ПОДКЛЮЧИТЬСЯ К ИГРЕ", buttonStyle))
        {
            isJoiningGame = true;
        }

        // 3. Список доступных серверов
        GUI.Label(new Rect(x + 25, nextY + 115, width - 50, 25), "ДОСТУПНЫЕ СЕРВЕРЫ В СЕТИ:", labelStyle);

        float listHeight = 150;
        scrollPosition = GUI.BeginScrollView(new Rect(x + 20, nextY + 140, width - 40, listHeight),
                                           scrollPosition, new Rect(0, 0, width - 60, availableGames.Count * 40));

        for (int i = 0; i < availableGames.Count; i++)
        {
            GUIStyle gameButtonStyle = new GUIStyle(GUI.skin.button);
            gameButtonStyle.alignment = TextAnchor.MiddleLeft;
            gameButtonStyle.fixedHeight = 35;

            if (GUI.Button(new Rect(0, i * 40, width - 80, 35), availableGames[i], gameButtonStyle))
            {
                ipAddress = "192.168.1." + (i + 1);
                playerName = "Игрок" + (i + 1);
                isJoiningGame = true;
            }
        }
        GUI.EndScrollView();

        // 4. Кнопка прямого старта
        if (GUI.Button(new Rect(buttonX, nextY + 300, buttonWidth, 42), "БЫСТРЫЙ СТАРТ С ВЫБРАННОЙ ТЕХНИКОЙ", buttonStyle))
        {
            StartQuickGame();
        }
    }

    void DrawCreateGameMenu(float x, float startY, float width, GUIStyle labelStyle, GUIStyle textFieldStyle, GUIStyle buttonStyle)
    {
        GUI.Label(new Rect(x + 20, startY, width - 40, 25), "ПАРАМЕТРЫ СЕРВЕРА:", labelStyle);

        // Название игры
        GUI.Label(new Rect(x + 25, startY + 30, 120, 25), "Название игры:", labelStyle);
        gameName = GUI.TextField(new Rect(x + 150, startY + 30, width - 180, 28), gameName, textFieldStyle);

        // Имя игрока
        GUI.Label(new Rect(x + 25, startY + 65, 120, 25), "Ваше имя:", labelStyle);
        playerName = GUI.TextField(new Rect(x + 150, startY + 65, width - 180, 28), playerName, textFieldStyle);

        // Порт
        GUI.Label(new Rect(x + 25, startY + 100, 120, 25), "Порт:", labelStyle);
        port = GUI.TextField(new Rect(x + 150, startY + 100, width - 180, 28), port, textFieldStyle);

        // Выбор техники в меню создания
        DrawVehicleSelector(x, startY + 140, width, buttonStyle, labelStyle);

        // Кнопки
        float buttonWidth = 240;
        float buttonX = x + (width - buttonWidth) / 2;

        if (GUI.Button(new Rect(buttonX, startY + 310, buttonWidth, 42), "ЗАПУСТИТЬ СЕРВЕР И ВОЙТИ", buttonStyle))
        {
            CreateGameServer();
        }

        if (GUI.Button(new Rect(buttonX, startY + 365, buttonWidth, 38), "НАЗАД В ЛОББИ", buttonStyle))
        {
            isCreatingGame = false;
        }
    }

    void DrawJoinGameMenu(float x, float startY, float width, GUIStyle labelStyle, GUIStyle textFieldStyle, GUIStyle buttonStyle)
    {
        GUI.Label(new Rect(x + 20, startY, width - 40, 25), "ПОДКЛЮЧЕНИЕ К СЕРВЕРУ:", labelStyle);

        // IP адрес
        GUI.Label(new Rect(x + 25, startY + 30, 120, 25), "IP адрес:", labelStyle);
        ipAddress = GUI.TextField(new Rect(x + 150, startY + 30, width - 180, 28), ipAddress, textFieldStyle);

        // Порт
        GUI.Label(new Rect(x + 25, startY + 65, 120, 25), "Порт:", labelStyle);
        port = GUI.TextField(new Rect(x + 150, startY + 65, width - 180, 28), port, textFieldStyle);

        // Имя игрока
        GUI.Label(new Rect(x + 25, startY + 100, 120, 25), "Ваше имя:", labelStyle);
        playerName = GUI.TextField(new Rect(x + 150, startY + 100, width - 180, 28), playerName, textFieldStyle);

        // Выбор техники в меню подключения
        DrawVehicleSelector(x, startY + 140, width, buttonStyle, labelStyle);

        // Кнопки
        float buttonWidth = 240;
        float buttonX = x + (width - buttonWidth) / 2;

        if (GUI.Button(new Rect(buttonX, startY + 310, buttonWidth, 42), "ПОДКЛЮЧИТЬСЯ С ТЕХНИКОЙ", buttonStyle))
        {
            JoinGame();
        }

        if (GUI.Button(new Rect(buttonX, startY + 365, buttonWidth, 38), "НАЗАД В ЛОББИ", buttonStyle))
        {
            isJoiningGame = false;
        }
    }

    void CreateGameServer()
    {
        Debug.Log($"Запуск сервера: {gameName}, Игрок: {playerName}, Техника: {VehicleSelectionManager.SelectedVehicle}");
        LoadGameScene();
    }

    void JoinGame()
    {
        Debug.Log($"Подключение к {ipAddress}:{port}, Игрок: {playerName}, Техника: {VehicleSelectionManager.SelectedVehicle}");
        LoadGameScene();
    }

    void StartQuickGame()
    {
        Debug.Log($"Быстрый старт с техникой: {VehicleSelectionManager.SelectedVehicle}");
        LoadGameScene();
    }

    void LoadGameScene()
    {
        showLocalMenu = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (!string.IsNullOrEmpty(targetGameScene) && Application.CanStreamedLevelBeLoaded(targetGameScene))
        {
            SceneManager.LoadScene(targetGameScene);
        }
        else
        {
            Debug.LogWarning($"[LocalGameMenu] Сцена '{targetGameScene}' не найдена в Build Settings! Загружаем сцену по индексу 1...");
            if (SceneManager.sceneCountInBuildSettings > 1)
            {
                SceneManager.LoadScene(1);
            }
        }
    }

    void RefreshGameList()
    {
        availableGames.Add("Сервер #" + Random.Range(10, 99));
        ShowMessage("Список серверов обновлен!");
    }

    void ShowMessage(string message)
    {
        Debug.Log(message);
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

    public void ShowLocalMenu()
    {
        showLocalMenu = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HideLocalMenu()
    {
        showLocalMenu = false;
        isCreatingGame = false;
        isJoiningGame = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            availableGames.Add("Локальный сервер " + (i + 1));
        }
    }
}