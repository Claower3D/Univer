using UnityEngine;
using System.Collections;

public class Excavator : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 90f;

    [Header("Ground Settings")]
    public LayerMask groundLayerMask = 1; // Слой земли
    public float groundCheckDistance = 10f; // Дистанция проверки до земли
    public float hoverHeight = 2f; // Высота над землей
    public float groundFollowSpeed = 5f; // Скорость следования рельефу

    [Header("Excavator Parts")]
    public Transform cabin;
    public Transform boom;      // стрела
    public Transform arm;       // рукоять
    public Transform bucket;    // ковш

    [Header("Animation Settings")]
    public float boomMinAngle = 20f;
    public float boomMaxAngle = 70f;
    public float armMinAngle = -80f;
    public float armMaxAngle = -20f;
    public float bucketMinAngle = -80f;
    public float bucketMaxAngle = -20f;

    // Текущие углы
    private float currentBoomAngle = 45f;
    private float currentArmAngle = -30f;
    private float currentBucketAngle = -45f;

    // Состояния
    private bool isMoving = false;
    private bool isWorking = false;
    private bool hasSoil = false;

    // Анимация
    private float animationTime = 0f;
    private float animationSpeed = 0.5f;

    // Компоненты
    private Rigidbody rb;
    private float currentHeight; // Текущая высота над землей

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.mass = 1000f; // Большая масса для устойчивости
        }

        InitializeExcavatorParts();
        UpdateGroundHeight(); // Инициализация высоты
    }

    void InitializeExcavatorParts()
    {
        // Инициализация начальных позиций механизмов
        UpdateExcavatorParts();
    }

    void Update()
    {
        HandleInput();
        UpdateGroundHeight(); // Обновление высоты над землей
        UpdateMovement();
        UpdateAnimation();
        UpdateExcavatorParts();
    }

    void UpdateGroundHeight()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 5f; // Начинаем луч немного выше

        if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance, groundLayerMask))
        {
            // Плавно корректируем высоту до нужного уровня над землей
            float targetHeight = hit.point.y + hoverHeight;
            currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * groundFollowSpeed);

            // Применяем новую позицию
            Vector3 newPosition = new Vector3(transform.position.x, currentHeight, transform.position.z);
            transform.position = newPosition;
        }
        else
        {
            // Если земля не найдена, медленно опускаемся
            currentHeight = Mathf.Lerp(currentHeight, transform.position.y, Time.deltaTime * groundFollowSpeed * 0.1f);
        }
    }

    void HandleInput()
    {
        // Движение вперед/назад
        float moveInput = Input.GetAxis("Vertical");
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            Move(moveInput);
        }
        else
        {
            StopMoving();
        }

        // Поворот
        float rotateInput = Input.GetAxis("Horizontal");
        if (Mathf.Abs(rotateInput) > 0.1f)
        {
            Rotate(rotateInput);
        }

        // Управление экскаватором
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleWorking();
        }

        // Ручное управление механизмами
        if (Input.GetKey(KeyCode.Q))
        {
            currentBoomAngle = Mathf.Clamp(currentBoomAngle + rotationSpeed * Time.deltaTime, boomMinAngle, boomMaxAngle);
        }
        if (Input.GetKey(KeyCode.E))
        {
            currentBoomAngle = Mathf.Clamp(currentBoomAngle - rotationSpeed * Time.deltaTime, boomMinAngle, boomMaxAngle);
        }
        if (Input.GetKey(KeyCode.A))
        {
            currentArmAngle = Mathf.Clamp(currentArmAngle + rotationSpeed * Time.deltaTime, armMinAngle, armMaxAngle);
        }
        if (Input.GetKey(KeyCode.D))
        {
            currentArmAngle = Mathf.Clamp(currentArmAngle - rotationSpeed * Time.deltaTime, armMinAngle, armMaxAngle);
        }
        if (Input.GetKey(KeyCode.Z))
        {
            currentBucketAngle = Mathf.Clamp(currentBucketAngle + rotationSpeed * Time.deltaTime, bucketMinAngle, bucketMaxAngle);
        }
        if (Input.GetKey(KeyCode.X))
        {
            currentBucketAngle = Mathf.Clamp(currentBucketAngle - rotationSpeed * Time.deltaTime, bucketMinAngle, bucketMaxAngle);
        }
    }

    void UpdateMovement()
    {
        // Дополнительная логика движения может быть добавлена здесь
    }

    void UpdateAnimation()
    {
        if (!isWorking) return;

        animationTime += Time.deltaTime * animationSpeed;

        // Циклическая анимация копания
        float phase = (Mathf.Sin(animationTime * Mathf.PI * 2f) + 1f) * 0.5f;

        // Анимация стрелы
        currentBoomAngle = Mathf.Lerp(boomMinAngle, boomMaxAngle, phase);

        // Анимация рукояти (противоположная фазе стрелы)
        currentArmAngle = Mathf.Lerp(armMaxAngle, armMinAngle, phase);

        // Анимация ковша
        currentBucketAngle = Mathf.Lerp(bucketMaxAngle, bucketMinAngle, phase);

        // Симуляция грунта в ковше
        if (phase > 0.3f && phase < 0.7f && !hasSoil)
        {
            hasSoil = true;
            OnSoilCollected();
        }
        else if ((phase <= 0.3f || phase >= 0.7f) && hasSoil)
        {
            hasSoil = false;
            OnSoilReleased();
        }
    }

    void UpdateExcavatorParts()
    {
        // Обновление вращения механизмов
        if (boom != null)
        {
            boom.localRotation = Quaternion.Euler(currentBoomAngle, 0f, 0f);
        }

        if (arm != null)
        {
            arm.localRotation = Quaternion.Euler(currentArmAngle, 0f, 0f);
        }

        if (bucket != null)
        {
            bucket.localRotation = Quaternion.Euler(currentBucketAngle, 0f, 0f);
        }
    }

    void Move(float direction)
    {
        isMoving = true;
        Vector3 movement = transform.forward * direction * moveSpeed * Time.deltaTime;

        // Двигаем только по X и Z, высота контролируется отдельно
        Vector3 newPosition = transform.position + movement;
        newPosition.y = currentHeight; // Сохраняем текущую высоту
        transform.position = newPosition;
    }

    void Rotate(float direction)
    {
        float rotation = direction * rotationSpeed * Time.deltaTime;
        transform.Rotate(0f, rotation, 0f);
    }

    void StopMoving()
    {
        isMoving = false;
    }

    public void StartWorking()
    {
        isWorking = true;
        animationTime = 0f;
    }

    public void StopWorking()
    {
        isWorking = false;
    }

    public void ToggleWorking()
    {
        isWorking = !isWorking;
        if (isWorking)
        {
            animationTime = 0f;
        }
    }

    void OnSoilCollected()
    {
        // Визуальные или звуковые эффекты при сборе грунта
        Debug.Log("Soil collected!");

        // Можно добавить частицы или изменение материала
        if (bucket != null)
        {
            Renderer bucketRenderer = bucket.GetComponent<Renderer>();
            if (bucketRenderer != null)
            {
                // Временное изменение цвета для визуализации грунта
                bucketRenderer.material.color = new Color(0.5f, 0.3f, 0.1f);
            }
        }
    }

    void OnSoilReleased()
    {
        // Визуальные или звуковые эффекты при выгрузке грунта
        Debug.Log("Soil released!");

        // Возврат исходного цвета
        if (bucket != null)
        {
            Renderer bucketRenderer = bucket.GetComponent<Renderer>();
            if (bucketRenderer != null)
            {
                bucketRenderer.material.color = Color.white;
            }
        }
    }

    // Методы для внешнего управления
    public void SetBoomAngle(float angle)
    {
        currentBoomAngle = Mathf.Clamp(angle, boomMinAngle, boomMaxAngle);
    }

    public void SetArmAngle(float angle)
    {
        currentArmAngle = Mathf.Clamp(angle, armMinAngle, armMaxAngle);
    }

    public void SetBucketAngle(float angle)
    {
        currentBucketAngle = Mathf.Clamp(angle, bucketMinAngle, bucketMaxAngle);
    }

    public bool IsWorking()
    {
        return isWorking;
    }

    public bool HasSoil()
    {
        return hasSoil;
    }

    // Для отладки в Inspector
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Визуализация зоны действия экскаватора
        Gizmos.color = hasSoil ? Color.red : Color.green;
        if (bucket != null)
        {
            Gizmos.DrawWireSphere(bucket.position, 0.5f);
        }

        // Визуализация луча проверки земли
        Gizmos.color = Color.blue;
        Vector3 rayStart = transform.position + Vector3.up * 5f;
        Gizmos.DrawLine(rayStart, rayStart + Vector3.down * groundCheckDistance);
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, currentHeight, transform.position.z), 0.3f);
    }
}

// Дополнительный класс для управления камерой, следящей за экскаватором
public class ExcavatorCamera : MonoBehaviour
{
    public Transform target; // Экскаватор
    public Vector3 offset = new Vector3(0f, 10f, -10f);
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        transform.LookAt(target);
    }
}