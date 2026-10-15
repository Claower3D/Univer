using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
    [Header("Префабы техники")]
    [Tooltip("Префаб КамАЗа")]
    public GameObject kamazPrefab;
    [Tooltip("Префаб Экскаватора")]
    public GameObject excavatorPrefab;
    [Tooltip("Префаб Трактора")]
    public GameObject tractorPrefab;

    [Header("Точка спавна")]
    public Transform spawnPoint;

    [Header("Камеры игрока")]
    public CameraOrbit orbitCamera;
    public TripleCameraSystem tripleCameraSystem;

    private GameObject currentSpawnedVehicle;

    void Start()
    {
        SpawnSelectedVehicle();
    }

    public GameObject SpawnSelectedVehicle()
    {
        VehicleType selectedType = VehicleSelectionManager.SelectedVehicle;
        Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

        // Если машина уже была заспавнена, удаляем предыдущую
        if (currentSpawnedVehicle != null)
        {
            Destroy(currentSpawnedVehicle);
        }

        GameObject prefabToSpawn = null;
        switch (selectedType)
        {
            case VehicleType.Kamaz:
                prefabToSpawn = kamazPrefab;
                break;
            case VehicleType.Excavator:
                prefabToSpawn = excavatorPrefab;
                break;
            case VehicleType.Tractor:
                prefabToSpawn = tractorPrefab;
                break;
        }

        if (prefabToSpawn != null)
        {
            currentSpawnedVehicle = Instantiate(prefabToSpawn, position, rotation);
        }
        else
        {
            // Фоллбэк: если префаб не назначен в инспекторе
            if (selectedType == VehicleType.Kamaz)
            {
                currentSpawnedVehicle = KamazController.CreateKamazPrototype();
                currentSpawnedVehicle.transform.position = position;
                currentSpawnedVehicle.transform.rotation = rotation;
            }
            else
            {
                Debug.LogWarning($"[VehicleSpawner] Префаб для {selectedType} не назначен в инспекторе! Ищу на сцене существующий объект...");
                // Пытаемся найти уже стоящий на сцене объект
                if (selectedType == VehicleType.Excavator)
                {
                    var found = FindAnyObjectByType<Excavator>();
                    if (found != null) currentSpawnedVehicle = found.gameObject;
                }
                else if (selectedType == VehicleType.Tractor)
                {
                    var found = FindAnyObjectByType<TractorMove>();
                    if (found != null) currentSpawnedVehicle = found.gameObject;
                }
            }
        }

        if (currentSpawnedVehicle != null)
        {
            AttachCamerasToVehicle(currentSpawnedVehicle.transform);
            Debug.Log($"✅ [VehicleSpawner] Успешно заспавнен транспорт: {selectedType} ({currentSpawnedVehicle.name})");
        }

        return currentSpawnedVehicle;
    }

    private void AttachCamerasToVehicle(Transform vehicleTransform)
    {
        // Привязываем орбитальную камеру
        if (orbitCamera == null)
        {
            orbitCamera = FindAnyObjectByType<CameraOrbit>();
        }
        if (orbitCamera != null)
        {
            orbitCamera.target = vehicleTransform;
        }

        // Привязываем систему тройных камер экскаватора, если есть
        if (tripleCameraSystem == null)
        {
            tripleCameraSystem = FindAnyObjectByType<TripleCameraSystem>();
        }
        if (tripleCameraSystem != null)
        {
            tripleCameraSystem.excavator = vehicleTransform;
        }
    }
}
