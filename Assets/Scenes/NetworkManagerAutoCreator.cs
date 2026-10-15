using UnityEngine;
using Unity.Netcode;

public class NetworkManagerAutoCreator : MonoBehaviour
{
    void Start()
    {
        // Проверяем, существует ли NetworkManager
        if (FindAnyObjectByType<NetworkManager>() == null)
        {
            CreateNetworkManager();
        }
    }

    void CreateNetworkManager()
    {
        // Создаем объект NetworkManager
        GameObject networkManagerObject = new GameObject("NetworkManager");
        DontDestroyOnLoad(networkManagerObject);

        // Добавляем компонент NetworkManager
        var networkManager = networkManagerObject.AddComponent<NetworkManager>();

        // Создаем простой PlayerPrefab
        CreatePlayerPrefab(networkManager);

        Debug.Log("✅ NetworkManager автоматически создан!");
    }

    void CreatePlayerPrefab(NetworkManager networkManager)
    {
        // Создаем простой PlayerPrefab
        GameObject playerPrefab = new GameObject("PlayerPrefab");
        playerPrefab.SetActive(false);

        // Добавляем обязательные компоненты
        playerPrefab.AddComponent<NetworkObject>();

        // Создаем визуальное представление
        var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.transform.SetParent(playerPrefab.transform);
        capsule.transform.localPosition = Vector3.zero;

        // Убираем коллайдер если не нужен
        DestroyImmediate(capsule.GetComponent<Collider>());

        // Сохраняем как префаб (в реальном проекте нужно сохранить в Assets)
        // Для демо используем временный объект
        networkManager.NetworkConfig.PlayerPrefab = playerPrefab;
    }
}