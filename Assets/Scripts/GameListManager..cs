using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameListManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform gamesListContent;
    public GameObject gameEntryPrefab;

    [Header("Demo Games")]
    public List<string> demoGames = new List<string>()
    {
        "Сервер Петра - 192.168.1.101",
        "Игра Алексея - 192.168.1.102",
        "Локальная комната - 192.168.1.103",
        "Тестовый сервер - localhost"
    };

    private List<GameObject> gameEntries = new List<GameObject>();

    void Start()
    {
        RefreshGameList();
    }

    public void RefreshGameList()
    {
        // Очищаем старый список
        foreach (var entry in gameEntries)
        {
            Destroy(entry);
        }
        gameEntries.Clear();

        // Создаем новые записи
        foreach (string gameInfo in demoGames)
        {
            CreateGameEntry(gameInfo);
        }

        // Здесь можно добавить реальный поиск в локальной сети
        Debug.Log("Список игр обновлен");
    }

    void CreateGameEntry(string gameInfo)
    {
        GameObject newEntry = Instantiate(gameEntryPrefab, gamesListContent);
        GameListEntry entryComponent = newEntry.GetComponent<GameListEntry>();

        if (entryComponent != null)
        {
            entryComponent.Setup(gameInfo, OnGameSelected);
        }

        gameEntries.Add(newEntry);
    }

    void OnGameSelected(string gameInfo)
    {
        Debug.Log($"Выбрана игра: {gameInfo}");

        // Автозаполнение полей подключения
        MainMenuManager menuManager = FindObjectOfType<MainMenuManager>();
        if (menuManager != null)
        {
            // Парсим IP из строки (простая логика)
            if (gameInfo.Contains("localhost"))
            {
                menuManager.SetIpAddress("localhost");
            }
            else
            {
                // Ищем IP адрес в строке
                string[] parts = gameInfo.Split('-');
                if (parts.Length > 1)
                {
                    menuManager.SetIpAddress(parts[1].Trim());
                }
            }
        }
    }
}