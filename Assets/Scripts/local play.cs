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
        GUI.color = new Color(0, 0, 0, 0.8f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), MakeTex(1, 1, Color.black));
        GUI.color = Color.white;

        DrawLocalGameMenu();
    }

    void DrawLocalGameMenu()
    {
        float width = 500;
        float height = 600;
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
        titleStyle.fontSize = 28;
        titleStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(x, y + 20, width, 40), "ЛОКАЛЬНАЯ ИГРА", titleStyle);

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

        float contentY = y + 80;

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

        // Кнопка закрытия
        if (GUI.Button(new Rect(x + width - 40, y + 10, 30, 30), "X", buttonStyle))
        {
            showLocalMenu = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void DrawMainLocalMenu(float x, float startY, float width, GUIStyle buttonStyle, GUIStyle labelStyle)
    {
        float buttonWidth = 300;
        float buttonHeight = 50;
        float buttonX = x + (width - buttonWidth) / 2;

        // Кнопка создания игры
        if (GUI.Button(new Rect(buttonX, startY, buttonWidth, buttonHeight), "СОЗДАТЬ ИГРУ", buttonStyle))
        {
            isCreatingGame = true;
        }

        // Кнопка подключения к игре
        if (GUI.Button(new Rect(buttonX, startY + 70, buttonWidth, buttonHeight), "ПОДКЛЮЧИТЬСЯ К ИГРЕ", buttonStyle))
        {
            isJoiningGame = true;
        }

        // Список доступных игр
        GUI.Label(new Rect(x + 20, startY + 160, width - 40, 30), "ДОСТУПНЫЕ ИГРЫ:", labelStyle);

        float listHeight = 200;
        scrollPosition = GUI.BeginScrollView(new Rect(x + 20, startY + 190, width - 40, listHeight),
                                           scrollPosition, new Rect(0, 0, width - 60, availableGames.Count * 40));

        for (int i = 0; i < availableGames.Count; i++)
        {
            GUIStyle gameButtonStyle = new GUIStyle(GUI.skin.button);
            gameButtonStyle.alignment = TextAnchor.MiddleLeft;
            gameButtonStyle.fixedHeight = 35;

            if (GUI.Button(new Rect(0, i * 40, width - 80, 35), availableGames[i], gameButtonStyle))
            {
                // Автозаполнение данных при выборе игры из списка
                ipAddress = "192.168.1." + (i + 1);
                playerName = "Игрок" + (i + 1);
                isJoiningGame = true;
            }
        }

        GUI.EndScrollView();

        // Быстрый старт
        if (GUI.Button(new Rect(buttonX, startY + 410, buttonWidth, 40), "БЫСТРЫЙ СТАРТ", buttonStyle))
        {
            StartQuickGame();
        }
    }

    void DrawCreateGameMenu(float x, float startY, float width, GUIStyle labelStyle, GUIStyle textFieldStyle, GUIStyle buttonStyle)
    {
        GUI.Label(new Rect(x + 20, startY, width - 40, 30), "СОЗДАНИЕ ИГРЫ", labelStyle);

        // Поле названия игры
        GUI.Label(new Rect(x + 20, startY + 50, 120, 25), "Название игры:", labelStyle);
        gameName = GUI.TextField(new Rect(x + 150, startY + 50, width - 190, 30), gameName, textFieldStyle);

        // Поле имени игрока
        GUI.Label(new Rect(x + 20, startY + 100, 120, 25), "Ваше имя:", labelStyle);
        playerName = GUI.TextField(new Rect(x + 150, startY + 100, width - 190, 30), playerName, textFieldStyle);

        // Порт
        GUI.Label(new Rect(x + 20, startY + 150, 120, 25), "Порт:", labelStyle);
        port = GUI.TextField(new Rect(x + 150, startY + 150, width - 190, 30), port, textFieldStyle);

        // Настройки игры
        GUI.Label(new Rect(x + 20, startY + 200, width - 40, 25), "НАСТРОЙКИ ИГРЫ:", labelStyle);

        // Максимальное количество игроков
        GUI.Label(new Rect(x + 20, startY + 240, 200, 25), "Макс. игроков: 4", labelStyle);

        // Пароль (опционально)
        bool usePassword = GUI.Toggle(new Rect(x + 20, startY + 280, 200, 25), false, "Использовать пароль", labelStyle);

        // Кнопки
        float buttonWidth = 200;
        float buttonX = x + (width - buttonWidth) / 2;

        if (GUI.Button(new Rect(buttonX, startY + 330, buttonWidth, 40), "ЗАПУСТИТЬ СЕРВЕР", buttonStyle))
        {
            CreateGameServer();
        }

        if (GUI.Button(new Rect(buttonX, startY + 390, buttonWidth, 40), "НАЗАД", buttonStyle))
        {
            isCreatingGame = false;
        }
    }

    void DrawJoinGameMenu(float x, float startY, float width, GUIStyle labelStyle, GUIStyle textFieldStyle, GUIStyle buttonStyle)
    {
        GUI.Label(new Rect(x + 20, startY, width - 40, 30), "ПОДКЛЮЧЕНИЕ К ИГРЕ", labelStyle);

        // IP адрес
        GUI.Label(new Rect(x + 20, startY + 50, 120, 25), "IP адрес:", labelStyle);
        ipAddress = GUI.TextField(new Rect(x + 150, startY + 50, width - 190, 30), ipAddress, textFieldStyle);

        // Порт
        GUI.Label(new Rect(x + 20, startY + 100, 120, 25), "Порт:", labelStyle);
        port = GUI.TextField(new Rect(x + 150, startY + 100, width - 190, 30), port, textFieldStyle);

        // Имя игрока
        GUI.Label(new Rect(x + 20, startY + 150, 120, 25), "Ваше имя:", labelStyle);
        playerName = GUI.TextField(new Rect(x + 150, startY + 150, width - 190, 30), playerName, textFieldStyle);

        // Статус подключения
        GUI.Label(new Rect(x + 20, startY + 200, width - 40, 25), "Статус: Готов к подключению", labelStyle);

        // Кнопки
        float buttonWidth = 200;
        float buttonX = x + (width - buttonWidth) / 2;

        if (GUI.Button(new Rect(buttonX, startY + 250, buttonWidth, 40), "ПОДКЛЮЧИТЬСЯ", buttonStyle))
        {
            JoinGame();
        }

        if (GUI.Button(new Rect(buttonX, startY + 310, buttonWidth, 40), "ОБНОВИТЬ СПИСОК", buttonStyle))
        {
            RefreshGameList();
        }

        if (GUI.Button(new Rect(buttonX, startY + 370, buttonWidth, 40), "НАЗАД", buttonStyle))
        {
            isJoiningGame = false;
        }
    }

    void CreateGameServer()
    {
        Debug.Log($"Запуск сервера: {gameName}");
        Debug.Log($"Порт: {port}");
        Debug.Log($"Игрок: {playerName}");

        // Здесь будет логика создания сервера
        // NetworkManager.StartHost() или аналогичная функция

        ShowMessage($"Сервер '{gameName}' запущен!\nОжидание подключений...");
    }

    void JoinGame()
    {
        Debug.Log($"Подключение к {ipAddress}:{port}");
        Debug.Log($"Игрок: {playerName}");

        // Здесь будет логика подключения к серверу
        // NetworkManager.StartClient() или аналогичная функция

        ShowMessage($"Подключение к {ipAddress}:{port}...");
    }

    void StartQuickGame()
    {
        Debug.Log("Быстрый старт игры");
        // Автоматическое создание или подключение к игре
        gameName = "Быстрая игра " + Random.Range(1000, 9999);
        CreateGameServer();
    }

    void RefreshGameList()
    {
        Debug.Log("Обновление списка игр...");
        // Логика поиска доступных игр в локальной сети
        availableGames.Add("Новая игра " + Random.Range(100, 999));
        ShowMessage("Список игр обновлен!");
    }

    void ShowMessage(string message)
    {
        // В реальной реализации можно сделать красивый popup
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

    // Методы для внешнего управления
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
        // Автогенерация демо-игр
        for (int i = 0; i < 3; i++)
        {
            availableGames.Add("Локальная игра " + (i + 1));
        }
    }
}