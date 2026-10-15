using UnityEngine;

public class CompleteLandscapeSetup : MonoBehaviour
{
    [Header("НАСТРОЙКИ ЛАНДШАФТА")]
    [SerializeField] private float planeSize = 100f;
    [SerializeField] private Material groundMaterial;
    [SerializeField] private bool generateCollider = true;

    [Header("ФИЗИКА")]
    [SerializeField] private float dynamicFriction = 0.8f;
    [SerializeField] private float staticFriction = 1.0f;
    [SerializeField] private float bounciness = 0.0f;

    [Header("ПРОВЕРКА ПОВЕРХНОСТИ")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private float raycastOffset = 0.5f;

    private GameObject landscape;
    private bool isGrounded;

    void Start()
    {
        CreateGroundLayer();
        CreateLandscapePlane();
        SetupPhysicsMaterial();
    }

    void Update()
    {
        CheckGroundBelow();
        PreventFallingThrough();
    }

    /// <summary>
    /// Создает слой Ground если он не существует
    /// </summary>
    void CreateGroundLayer()
    {
        // Проверяем существование слоя Ground
        int groundLayer = LayerMask.NameToLayer("Ground");

        if (groundLayer == -1)
        {
            Debug.LogWarning("Слой 'Ground' не создан! Создайте слой 'Ground' в Project Settings → Tags and Layers");
            Debug.Log("Используется слой по умолчанию (Default)");
        }
        else
        {
            Debug.Log("Слой 'Ground' найден: " + groundLayer);
        }
    }

    /// <summary>
    /// Создает Plane для ландшафта
    /// </summary>
    void CreateLandscapePlane()
    {
        landscape = GameObject.CreatePrimitive(PrimitiveType.Plane);
        landscape.name = "LandscapeGround";
        landscape.transform.localScale = new Vector3(planeSize / 10f, 1, planeSize / 10f);
        landscape.transform.position = Vector3.zero;

        // Назначаем материал
        if (groundMaterial != null)
        {
            landscape.GetComponent<Renderer>().material = groundMaterial;
        }

        // Настраиваем коллайдер
        if (generateCollider)
        {
            MeshCollider meshCollider = landscape.GetComponent<MeshCollider>();
            if (meshCollider == null)
            {
                meshCollider = landscape.AddComponent<MeshCollider>();
            }
            meshCollider.convex = false;
        }
        else
        {
            Collider existingCollider = landscape.GetComponent<Collider>();
            if (existingCollider != null)
                Destroy(existingCollider);
        }

        // Назначаем слой Ground если он существует
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer != -1)
        {
            landscape.layer = groundLayer;
        }

        Debug.Log("Ландшафт создан: " + landscape.name);
    }

    /// <summary>
    /// Настраивает физический материал для поверхности
    /// </summary>
    void SetupPhysicsMaterial()
    {
        if (!generateCollider) return;

        PhysicsMaterial physMat = new PhysicsMaterial();
        physMat.name = "GroundPhysics";
        physMat.dynamicFriction = dynamicFriction;
        physMat.staticFriction = staticFriction;
        physMat.bounciness = bounciness;
        physMat.frictionCombine = PhysicsMaterialCombine.Maximum;
        physMat.bounceCombine = PhysicsMaterialCombine.Minimum;

        MeshCollider collider = landscape.GetComponent<MeshCollider>();
        if (collider != null)
        {
            collider.material = physMat;
        }

        Debug.Log("Физический материал применен");
    }

    /// <summary>
    /// Проверяет наличие поверхности под объектом
    /// </summary>
    void CheckGroundBelow()
    {
        int groundLayerMask = GetGroundLayerMask();
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * raycastOffset;

        isGrounded = Physics.Raycast(rayStart, Vector3.down, out hit,
                                   groundCheckDistance + raycastOffset, groundLayerMask);

        // Визуализация в редакторе
        Debug.DrawRay(rayStart, Vector3.down * (groundCheckDistance + raycastOffset),
                     isGrounded ? Color.green : Color.red);
    }

    /// <summary>
    /// Получает маску слоя Ground
    /// </summary>
    int GetGroundLayerMask()
    {
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer != -1)
        {
            return 1 << groundLayer;
        }
        return ~0; // Все слои если Ground не найден
    }

    /// <summary>
    /// Предотвращает проваливание через поверхность
    /// </summary>
    void PreventFallingThrough()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        // Если объект падает и нет земли под ним
        if (!isGrounded && rb.linearVelocity.y < -5f)
        {
            SnapToGround();
        }
    }

    /// <summary>
    /// Принудительно перемещает объект на поверхность
    /// </summary>
    void SnapToGround()
    {
        int groundLayerMask = GetGroundLayerMask();
        RaycastHit hit;

        // Бросаем луч сверху вниз для поиска поверхности
        if (Physics.Raycast(transform.position + Vector3.up * 50f, Vector3.down,
                           out hit, Mathf.Infinity, groundLayerMask))
        {
            transform.position = hit.point + Vector3.up * 0.1f;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                rb.angularVelocity = Vector3.zero;
            }

            Debug.Log("Объект возвращен на поверхность!");
        }
    }

    /// <summary>
    /// Публичный метод для принудительной проверки земли
    /// </summary>
    public bool IsGrounded()
    {
        return isGrounded;
    }

    /// <summary>
    /// Публичный метод для получения информации о поверхности
    /// </summary>
    public RaycastHit GetGroundInfo()
    {
        int groundLayerMask = GetGroundLayerMask();
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * raycastOffset;

        Physics.Raycast(rayStart, Vector3.down, out hit,
                       groundCheckDistance + raycastOffset, groundLayerMask);
        return hit;
    }

    /// <summary>
    /// Визуализация в редакторе
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // Отображение зоны проверки земли
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 checkStart = transform.position + Vector3.up * raycastOffset;
        Vector3 checkEnd = checkStart + Vector3.down * (groundCheckDistance + raycastOffset);
        Gizmos.DrawLine(checkStart, checkEnd);

        // Отображение безопасной зоны
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(1, 0.1f, 1));
    }
}