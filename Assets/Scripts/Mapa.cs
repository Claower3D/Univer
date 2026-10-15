using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Mapa : MonoBehaviour
{
    [Header("=== ОСНОВНЫЕ НАСТРОЙКИ ЛАНДШАФТА ===")]
    [SerializeField] private float terrainFriction = 0.8f;
    [SerializeField] private float terrainBounciness = 0.05f;
    [SerializeField] private float groundHardness = 1.0f;

    [Header("Тип поверхности")]
    public GroundSurfaceType surfaceType = GroundSurfaceType.Dirt;

    [Header("=== НАСТРОЙКИ ФИЗИКИ ===")]
    [SerializeField] private bool enableAdvancedCollision = true;
    [SerializeField] private bool enableSurfaceEffects = true;
    [SerializeField] private float collisionForceMultiplier = 1.0f;

    [Header("=== ВИЗУАЛЬНЫЕ ЭФФЕКТЫ ===")]
    public ParticleSystem dustParticleSystem;
    public GameObject tireMarkPrefab;
    public Material[] surfaceMaterials;

    [Header("=== ОПТИМИЗАЦИЯ ===")]
    [SerializeField] private bool optimizeForLargeTerrain = true;
    [SerializeField] private float updateFrequency = 0.1f;

    // Приватные переменные
    private MeshCollider terrainCollider;
    private PhysicsMaterial terrainPhysicMaterial;
    private Mesh terrainMesh;
    private Dictionary<Collider, float> lastCollisionTime = new Dictionary<Collider, float>();
    private float lastUpdateTime;
    private List<GameObject> activeTireMarks = new List<GameObject>();
    private List<ParticleSystem> activeParticles = new List<ParticleSystem>();

    public enum GroundSurfaceType
    {
        Dirt = 0,
        Grass = 1,
        Sand = 2,
        Mud = 3,
        Asphalt = 4,
        Snow = 5
    }

    [System.Serializable]
    public class SurfaceProperties
    {
        public float friction;
        public float bounciness;
        public float hardness;
        public Color dustColor;
        public float markIntensity;
        public float markDuration;
    }

    private Dictionary<GroundSurfaceType, SurfaceProperties> surfaceProperties = new Dictionary<GroundSurfaceType, SurfaceProperties>();

    void Awake()
    {
        InitializeSurfaceProperties();
        SetupTerrainComponents();
        ConfigurePhysicsMaterial(); // Теперь этот метод существует
        OptimizeTerrainCollision();
    }

    void InitializeSurfaceProperties()
    {
        surfaceProperties.Clear();

        surfaceProperties[GroundSurfaceType.Dirt] = new SurfaceProperties
        {
            friction = 0.7f,
            bounciness = 0.05f,
            hardness = 1.0f,
            dustColor = new Color(0.4f, 0.3f, 0.2f, 0.6f),
            markIntensity = 0.8f,
            markDuration = 15f
        };

        surfaceProperties[GroundSurfaceType.Grass] = new SurfaceProperties
        {
            friction = 0.8f,
            bounciness = 0.02f,
            hardness = 0.8f,
            dustColor = new Color(0.3f, 0.4f, 0.2f, 0.4f),
            markIntensity = 0.3f,
            markDuration = 8f
        };

        surfaceProperties[GroundSurfaceType.Sand] = new SurfaceProperties
        {
            friction = 0.5f,
            bounciness = 0.01f,
            hardness = 0.3f,
            dustColor = new Color(0.8f, 0.7f, 0.5f, 0.5f),
            markIntensity = 0.6f,
            markDuration = 5f
        };

        surfaceProperties[GroundSurfaceType.Mud] = new SurfaceProperties
        {
            friction = 0.3f,
            bounciness = 0.01f,
            hardness = 0.2f,
            dustColor = new Color(0.3f, 0.2f, 0.1f, 0.7f),
            markIntensity = 0.9f,
            markDuration = 25f
        };

        surfaceProperties[GroundSurfaceType.Asphalt] = new SurfaceProperties
        {
            friction = 0.9f,
            bounciness = 0.1f,
            hardness = 1.5f,
            dustColor = new Color(0.2f, 0.2f, 0.2f, 0.3f),
            markIntensity = 0.2f,
            markDuration = 20f
        };

        surfaceProperties[GroundSurfaceType.Snow] = new SurfaceProperties
        {
            friction = 0.4f,
            bounciness = 0.03f,
            hardness = 0.4f,
            dustColor = new Color(0.9f, 0.9f, 0.95f, 0.5f),
            markIntensity = 0.7f,
            markDuration = 12f
        };
    }

    void SetupTerrainComponents()
    {
        // Получаем компоненты
        terrainCollider = GetComponent<MeshCollider>();
        if (terrainCollider == null)
        {
            terrainCollider = gameObject.AddComponent<MeshCollider>();
        }

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            terrainMesh = meshFilter.mesh;
        }

        // Создаем физический материал
        terrainPhysicMaterial = new PhysicsMaterial("TerrainPhysicsMaterial");

        // Применяем настройки для выбранного типа поверхности
        ApplySurfaceProperties();

        terrainCollider.material = terrainPhysicMaterial;
        terrainCollider.convex = false;

        // Убедимся что у нас есть меш для коллайдера
        if (terrainCollider.sharedMesh == null && terrainMesh != null)
        {
            terrainCollider.sharedMesh = terrainMesh;
        }
    }

    void ConfigurePhysicsMaterial() // ДОБАВЛЕННЫЙ МЕТОД
    {
        UpdatePhysicMaterial();
    }

    void ApplySurfaceProperties()
    {
        if (surfaceProperties.ContainsKey(surfaceType))
        {
            SurfaceProperties props = surfaceProperties[surfaceType];
            terrainFriction = props.friction;
            terrainBounciness = props.bounciness;
            groundHardness = props.hardness;

            // Обновляем визуальный материал если есть
            UpdateVisualMaterial();
        }

        UpdatePhysicMaterial();
    }

    void UpdatePhysicMaterial()
    {
        if (terrainPhysicMaterial != null)
        {
            terrainPhysicMaterial.dynamicFriction = terrainFriction;
            terrainPhysicMaterial.staticFriction = terrainFriction * 1.1f;
            terrainPhysicMaterial.bounciness = terrainBounciness;
            terrainPhysicMaterial.frictionCombine = PhysicsMaterialCombine.Maximum;
            terrainPhysicMaterial.bounceCombine = PhysicsMaterialCombine.Average;
        }
    }

    void UpdateVisualMaterial()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && surfaceMaterials != null && surfaceMaterials.Length > (int)surfaceType)
        {
            renderer.material = surfaceMaterials[(int)surfaceType];
        }
    }

    void OptimizeTerrainCollision()
    {
        if (optimizeForLargeTerrain && terrainCollider != null)
        {
            terrainCollider.cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation |
                                           MeshColliderCookingOptions.EnableMeshCleaning |
                                           MeshColliderCookingOptions.WeldColocatedVertices |
                                           MeshColliderCookingOptions.UseFastMidphase;
        }

        // Устанавливаем слой для оптимизации
        gameObject.layer = LayerMask.NameToLayer("Ground");
    }

    void Start()
    {
        // Дополнительная инициализация при старте
        if (terrainCollider == null)
        {
            SetupTerrainComponents();
        }
    }

    void Update()
    {
        // Периодическое обновление для оптимизации
        if (Time.time - lastUpdateTime > updateFrequency)
        {
            CleanupOldEffects();
            lastUpdateTime = Time.time;
        }
    }

    void FixedUpdate()
    {
        // Фиксированное обновление для стабильности физики
        if (enableAdvancedCollision)
        {
            StabilizeTerrainPhysics();
        }
    }

    void StabilizeTerrainPhysics()
    {
        // Дополнительная стабилизация физики
        // Можно добавить дополнительные проверки здесь
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!enableAdvancedCollision) return;

        ProcessCollision(collision, true);
    }

    void OnCollisionStay(Collision collision)
    {
        if (!enableAdvancedCollision) return;

        ProcessCollision(collision, false);
    }

    void ProcessCollision(Collision collision, bool isEnter)
    {
        // Проверяем что это трактор или транспорт
        if (!IsVehicle(collision.gameObject)) return;

        // Ограничиваем частоту обработки для оптимизации
        if (!CanProcessCollision(collision.collider)) return;

        foreach (ContactPoint contact in collision.contacts)
        {
            // Основная физика - предотвращаем проваливание
            ApplyCollisionStabilization(collision, contact);

            // Визуальные эффекты
            if (enableSurfaceEffects)
            {
                if (isEnter && collision.relativeVelocity.magnitude > 1f)
                {
                    CreateImpactEffect(contact.point, contact.normal, collision.relativeVelocity.magnitude);
                }

                CreateMovementEffects(collision, contact);
            }
        }
    }

    bool IsVehicle(GameObject obj)
    {
        // Проверяем является ли объект транспортным средством
        return obj.CompareTag("Tractor") ||
               obj.CompareTag("Vehicle") ||
               obj.GetComponent<Rigidbody>() != null;
    }

    bool CanProcessCollision(Collider collider)
    {
        float currentTime = Time.time;
        if (lastCollisionTime.ContainsKey(collider))
        {
            if (currentTime - lastCollisionTime[collider] < 0.1f) // 10 раз в секунду максимум
                return false;

            lastCollisionTime[collider] = currentTime;
        }
        else
        {
            lastCollisionTime.Add(collider, currentTime);
        }

        return true;
    }

    void ApplyCollisionStabilization(Collision collision, ContactPoint contact)
    {
        Rigidbody otherRb = collision.rigidbody;
        if (otherRb != null)
        {
            // Сила против проваливания - толкаем объект вверх от поверхности
            Vector3 antiSinkForce = contact.normal * groundHardness * collisionForceMultiplier * 10f;
            otherRb.AddForce(antiSinkForce, ForceMode.Force);

            // Дополнительная стабилизация - ограничиваем скорость погружения
            Vector3 localVelocity = transform.InverseTransformDirection(otherRb.linearVelocity);
            if (localVelocity.y < -1f) // Если объект слишком быстро погружается
            {
                Vector3 counterForce = Vector3.up * groundHardness * 50f;
                otherRb.AddForce(counterForce, ForceMode.Acceleration);
            }

            // Применяем трение поверхности
            ApplySurfaceFriction(otherRb, contact.point);
        }
    }

    void ApplySurfaceFriction(Rigidbody rb, Vector3 contactPoint)
    {
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > 0.1f)
        {
            // Сила трения пропорциональна трению поверхности и массе объекта
            float frictionForce = terrainFriction * groundHardness * rb.mass * 0.5f;
            Vector3 frictionDirection = -horizontalVelocity.normalized;
            Vector3 frictionVector = frictionDirection * frictionForce * Time.fixedDeltaTime;

            rb.AddForce(frictionVector, ForceMode.Force);
        }
    }

    void CreateImpactEffect(Vector3 position, Vector3 normal, float impactForce)
    {
        if (dustParticleSystem != null)
        {
            ParticleSystem impactParticles = Instantiate(dustParticleSystem, position, Quaternion.LookRotation(normal));
            ConfigureParticles(impactParticles, impactForce);
            impactParticles.Play();
            activeParticles.Add(impactParticles);

            // Автоочистка
            StartCoroutine(DestroyAfterTime(impactParticles.gameObject, impactParticles.main.duration + 1f));
        }
    }

    void CreateMovementEffects(Collision collision, ContactPoint contact)
    {
        Rigidbody rb = collision.rigidbody;
        if (rb == null) return;

        float speed = rb.linearVelocity.magnitude;

        // Следы от шин при движении
        if (speed > 0.5f && tireMarkPrefab != null && Random.Range(0f, 1f) > 0.8f)
        {
            CreateTireMark(contact.point, contact.normal, speed);
        }

        // Пыль при движении
        if (speed > 1f && dustParticleSystem != null && Random.Range(0f, 1f) > 0.9f)
        {
            CreateDustCloud(contact.point, speed);
        }
    }

    void CreateTireMark(Vector3 position, Vector3 normal, float speed)
    {
        GameObject tireMark = Instantiate(tireMarkPrefab, position + normal * 0.02f, Quaternion.identity);
        tireMark.transform.forward = -normal; // Поворачиваем след вдоль поверхности

        // Настраиваем внешний вид в зависимости от поверхности
        ConfigureTireMark(tireMark, speed);

        activeTireMarks.Add(tireMark);

        // Автоудаление
        float duration = surfaceProperties[surfaceType].markDuration;
        StartCoroutine(DestroyAfterTime(tireMark, duration));
    }

    void ConfigureTireMark(GameObject tireMark, float speed)
    {
        Renderer renderer = tireMark.GetComponent<Renderer>();
        if (renderer != null)
        {
            Color markColor = renderer.material.color;
            markColor.a = surfaceProperties[surfaceType].markIntensity * Mathf.Clamp01(speed / 5f);
            renderer.material.color = markColor;

            // Масштаб в зависимости от скорости
            float scale = 0.8f + Mathf.Clamp01(speed / 10f) * 0.4f;
            tireMark.transform.localScale = Vector3.one * scale;
        }
    }

    void CreateDustCloud(Vector3 position, float speed)
    {
        if (dustParticleSystem != null)
        {
            ParticleSystem dust = Instantiate(dustParticleSystem, position, Quaternion.identity);
            ConfigureParticles(dust, speed);
            dust.Play();
            activeParticles.Add(dust);

            StartCoroutine(DestroyAfterTime(dust.gameObject, dust.main.duration + 1f));
        }
    }

    void ConfigureParticles(ParticleSystem particles, float intensity)
    {
        var main = particles.main;
        var emission = particles.emission;
        var shape = particles.shape;

        // Настраиваем в зависимости от типа поверхности и интенсивности
        SurfaceProperties props = surfaceProperties[surfaceType];

        main.startColor = props.dustColor;
        main.startSpeed = intensity * 0.5f;
        emission.rateOverTime = intensity * 10f;
        shape.radius = Mathf.Clamp(intensity * 0.2f, 0.1f, 1f);
    }

    void CleanupOldEffects()
    {
        // Очищаем старые следы
        for (int i = activeTireMarks.Count - 1; i >= 0; i--)
        {
            if (activeTireMarks[i] == null)
            {
                activeTireMarks.RemoveAt(i);
            }
        }

        // Очищаем старые частицы
        for (int i = activeParticles.Count - 1; i >= 0; i--)
        {
            if (activeParticles[i] == null || !activeParticles[i].IsAlive())
            {
                if (activeParticles[i] != null)
                    Destroy(activeParticles[i].gameObject);
                activeParticles.RemoveAt(i);
            }
        }

        // Очищаем старые записи о столкновениях
        List<Collider> toRemove = new List<Collider>();
        foreach (var entry in lastCollisionTime)
        {
            if (entry.Key == null || Time.time - entry.Value > 5f)
            {
                toRemove.Add(entry.Key);
            }
        }

        foreach (var collider in toRemove)
        {
            lastCollisionTime.Remove(collider);
        }
    }

    System.Collections.IEnumerator DestroyAfterTime(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        if (obj != null)
        {
            Destroy(obj);
        }
    }

    // Публичные методы для изменения свойств во время выполнения
    public void ChangeSurfaceType(GroundSurfaceType newType)
    {
        surfaceType = newType;
        ApplySurfaceProperties();
    }

    public SurfaceProperties GetCurrentSurfaceProperties()
    {
        return surfaceProperties.ContainsKey(surfaceType) ? surfaceProperties[surfaceType] : null;
    }

    public float GetSurfaceFriction()
    {
        return terrainFriction;
    }

    void OnValidate()
    {
        // Обновляем в редакторе при изменении
        if (terrainPhysicMaterial != null)
        {
            UpdatePhysicMaterial();
        }
    }

    void OnDestroy()
    {
        // Очистка ресурсов
        if (terrainPhysicMaterial != null)
        {
            DestroyImmediate(terrainPhysicMaterial);
        }

        // Очищаем все эффекты
        foreach (var effect in activeTireMarks)
        {
            if (effect != null) DestroyImmediate(effect);
        }
        foreach (var particles in activeParticles)
        {
            if (particles != null) DestroyImmediate(particles.gameObject);
        }
    }
}