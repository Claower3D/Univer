using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 30), "ВЕРНУТЬСЯ В МЕНЮ"))
        {
            // Останавливаем сеть и возвращаемся в меню
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene(0); // Главное меню должно быть на сцене 0
        }

        // Показываем информацию о подключении
        string connectionInfo = "";
        if (NetworkManager.Singleton.IsHost) connectionInfo = "Вы: ХОСТ";
        else if (NetworkManager.Singleton.IsClient) connectionInfo = "Вы: КЛИЕНТ";
        else connectionInfo = "Одиночная игра";

        GUI.Label(new Rect(10, 50, 300, 25), connectionInfo);
        GUI.Label(new Rect(10, 80, 300, 25), "Игроков: " + NetworkManager.Singleton.ConnectedClients.Count);

        // Только хост может начать игру
        if (IsServer && GUI.Button(new Rect(10, 120, 150, 30), "НАЧАТЬ ИГРУ"))
        {
            StartGameForAll();
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("Сервер запущен в игровой сцене");
        }
        else if (IsClient)
        {
            Debug.Log("Клиент подключен к игровой сцене");
        }
    }

    void StartGameForAll()
    {
        // Здесь логика начала игры для всех игроков
        Debug.Log("Игра начата для всех игроков!");
    }
}