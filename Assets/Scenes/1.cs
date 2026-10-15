using UnityEngine;

[ExecuteInEditMode]
public class CompleteTerrainAutoSetup : MonoBehaviour
{
    [Header("Настройки")]
    public bool autoSetupOnStart = true;
    public bool setupPhysicsSettings = true;

    void Start()
    {
        if (autoSetupOnStart)
            CompleteSetup();
    }

    [ContextMenu("⚙️ ПОЛНАЯ АВТОНАСТРОЙКА КАРТЫ")]
    public void CompleteSetup()
    {
        Debug.Log("🚀 ЗАПУСК ПОЛНОЙ АВТОНАСТРОЙКИ КАРТЫ...");

        // 1. Настройка физики проекта
        if (setupPhysicsSettings)
            SetupProjectPhysics();

        // 2. Настройка карты
        SetupTerrain();

        // 3. Финальные проверки
        FinalChecks();

        Debug.Log("🎉 КАРТА ПОЛНОСТЬЮ ГОТОВА! Можно кататься!");
    }

    void SetupProjectPhysics()
    {
        Debug.Log("📊 Настраиваю физику проекта...");

        // Настройки гравитации
        Physics.gravity = new Vector3(0, -9.81f, 0);

        // Основные настройки физики
        Physics.defaultContactOffset = 0.01f;
        Physics.sleepThreshold = 0.005f;
        Physics.bounceThreshold = 2f;
        Physics.defaultSolverIterations = 10;
        Physics.defaultSolverVelocityIterations = 6;
        Physics.queriesHitBackfaces = false;
        Physics.queriesHitTriggers = true;

        // Настройка времени
        Time.fixedDeltaTime = 0.01666667f; // 60 FPS
        Time.maximumDeltaTime = 0.3333333f;

        Debug.Log("✅ Физика проекта настроена");
    }

    void SetupTerrain()
    {
        Debug.Log("🗺️ Настраиваю карту...");

        GameObject terrain = gameObject;

        // Создаём или получаем Terrain компонент
        Terrain terrainComp = terrain.GetComponent<Terrain>();
        if (terrainComp == null)
        {
            terrainComp = terrain.AddComponent<Terrain>();
            Debug.Log("✅ Terrain компонент добавлен");
        }

        // Создаём TerrainCollider
        TerrainCollider terrainCollider = terrain.GetComponent<TerrainCollider>();
        if (terrainCollider == null)
        {
            terrainCollider = terrain.AddComponent<TerrainCollider>();

            // Создаём TerrainData если нет
            if (terrainComp.terrainData == null)
            {
                TerrainData terrainData = new TerrainData();
                terrainData.size = new Vector3(100, 100, 100);
                terrainComp.terrainData = terrainData;
            }

            terrainCollider.terrainData = terrainComp.terrainData;
            Debug.Log("✅ TerrainCollider настроен");
        }

        // Создаём Rigidbody для статичности
        Rigidbody rb = terrain.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = terrain.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            Debug.Log("✅ Rigidbody карты настроен (статичный)");
        }

        // Создаём физический материал
        PhysicsMaterial terrainMaterial = new PhysicsMaterial("TerrainPhysics")
        {
            dynamicFriction = 0.6f,
            staticFriction = 0.7f,
            bounciness = 0.0f,
            frictionCombine = PhysicsMaterialCombine.Average,
            bounceCombine = PhysicsMaterialCombine.Average
        };

        // Применяем материал к коллайдеру
        if (terrainCollider != null)
        {
            terrainCollider.material = terrainMaterial;
        }

        // Устанавливаем слой
        terrain.layer = LayerMask.NameToLayer("Default");

        Debug.Log("✅ Карта полностью настроена");
    }

    void FinalChecks()
    {
        Debug.Log("🔍 Выполняю финальные проверки...");

        // Проверяем карту
        if (GetComponent<Terrain>() == null)
            Debug.LogError("❌ Terrain компонент отсутствует");
        else
            Debug.Log("✅ Terrain компонент на месте");

        if (GetComponent<TerrainCollider>() == null)
            Debug.LogError("❌ TerrainCollider отсутствует");
        else
            Debug.Log("✅ TerrainCollider на месте");

        if (GetComponent<Rigidbody>() == null)
            Debug.LogError("❌ Rigidbody отсутствует");
        else
            Debug.Log("✅ Rigidbody на месте");

        // Проверяем физику
        if (Physics.gravity.y > -5)
            Debug.LogWarning("⚠️ Слабая гравитация");

        Debug.Log("✅ Все проверки завершены");
        Debug.Log("💡 Карта готова! Добавьте объекты с Rigidbody и Collider для тестирования.");
    }

    [ContextMenu("🔧 БЫСТРАЯ НАСТРОЙКА ТОЛЬКО КАРТЫ")]
    void QuickTerrainSetup()
    {
        Debug.Log("🔧 Быстрая настройка карты...");
        SetupTerrain();
        Debug.Log("✅ Карта настроена!");
    }
}