using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TractorMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f; // базовая скорость движения

    [Header("Gearbox Settings")]
    public float[] gearRatios = { 0.3f, 0.6f, 1f, 1.5f, 2f }; // передаточные числа передач
    public int currentGear = 2; // текущая передача (1-5, где 3 — нейтральная)

    [Header("Wheel Settings")]
    public float wheelRotationSpeed = 400f; // скорость вращения колёс
    public float steerAngle = 25f; // угол поворота передних колёс

    [Header("Steering Settings")]
    public string steeringBoneName = "WGT-Bone"; // имя кости/объекта для поворотов
    public float maxSteeringAngle = 45f; // максимальный угол поворота в градусах
    public float maxTurnRadiusPercent = 15f; // максимальный радиус поворота в процентах
    public float maxTotalRotation = 90f; // максимальный общий угол поворота в градусах

    [Header("Bucket Settings")]
    public string bucketName = "Plane.010"; // имя объекта ковша
    public float bucketTiltSpeed = 30f; // скорость наклона ковша
    public float maxBucketAngle = 45f; // максимальный угол наклона ковша

    private Rigidbody rb;
    private float currentSpeed;
    private Transform wheel013; // заднее колесо
    private Transform wheel012; // заднее колесо
    private Transform wheel010; // ковш
    private Transform wheel011; // переднее колесо
    private Transform steeringBone; // объект для поворотов
    private float currentSteeringAngle; // текущий угол поворота
    private float totalRotation; // общий накопленный угол поворота
    private float currentBucketAngle = 0f; // текущий угол ковша
    private bool isBucketMoving = false; // движется ли ковш

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // НАСТРОЙКА RIGIDBODY - ИСПРАВЛЕННЫЕ СВОЙСТВА
        rb.mass = 1000f; // увеличиваем массу трактора
        rb.linearDamping = 0.1f; // линейное сопротивление
        rb.angularDamping = 0.5f; // угловое сопротивление

        // Правильные constraints - убрали заморозку Y, чтобы трактор мог ехать
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                        RigidbodyConstraints.FreezeRotationZ;

        // Устанавливаем начальную скорость
        currentSpeed = moveSpeed * gearRatios[currentGear - 1];

        // Находим колёса и рулевую кость
        FindSpecificWheels();
        FindSteeringBone();

        // Добавляем коллайдер если его нет
        AddColliderIfMissing();

        totalRotation = 0f;
    }

    void Update()
    {
        HandleGearShift();
        RotateWheels();
        SteerFrontWheels();
        HandleBucketControl();
    }

    void FixedUpdate()
    {
        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        // ГАРАНТИРОВАННОЕ ДВИЖЕНИЕ - ПРОСТО И ЭФФЕКТИВНО
        if (Mathf.Abs(moveInput) > 0.1f && currentGear != 3)
        {
            // ПРЯМОЕ ДВИЖЕНИЕ - ГАРАНТИРОВАННО РАБОТАЕТ
            Vector3 moveDirection = transform.forward * moveInput * currentSpeed;
            rb.linearVelocity = new Vector3(moveDirection.x, rb.linearVelocity.y, moveDirection.z);
        }

        // Ограничиваем максимальную скорость
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > Mathf.Abs(currentSpeed))
        {
            horizontalVelocity = horizontalVelocity.normalized * Mathf.Abs(currentSpeed);
            rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
        }

        // Ограничиваем радиус поворота через максимальный угол
        float targetSteering = turnInput * maxSteeringAngle;

        // Плавное изменение угла поворота
        currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, targetSteering, 5f * Time.fixedDeltaTime);

        // Ограничиваем максимальный угол
        currentSteeringAngle = Mathf.Clamp(currentSteeringAngle, -maxSteeringAngle, maxSteeringAngle);

        // Поворот трактора - только если найден WGT-Bone
        if (Mathf.Abs(moveInput) > 0.01f && steeringBone != null)
        {
            // Вычисляем поворот на основе текущего угла
            float rotationSpeed = CalculateRotationSpeed(currentSteeringAngle, moveInput);

            // Проверяем ограничение общего поворота
            if (CanRotate(rotationSpeed * Time.fixedDeltaTime))
            {
                steeringBone.Rotate(0f, rotationSpeed * Time.fixedDeltaTime, 0f, Space.World);
                totalRotation += rotationSpeed * Time.fixedDeltaTime;
            }
        }
        else if (Mathf.Abs(moveInput) > 0.01f && steeringBone == null)
        {
            // Резервный вариант если WGT-Bone не найден - поворачиваем весь трактор
            float rotationSpeed = CalculateRotationSpeed(currentSteeringAngle, moveInput);

            // Проверяем ограничение общего поворота
            if (CanRotate(rotationSpeed * Time.fixedDeltaTime))
            {
                // ИСПРАВЛЕНИЕ: Более сильный поворот
                rb.AddTorque(0f, rotationSpeed * 2f * Time.fixedDeltaTime, 0f, ForceMode.VelocityChange);
                totalRotation += rotationSpeed * Time.fixedDeltaTime;
            }
        }

        // Сбрасываем общий поворот при смене направления
        if (Mathf.Abs(moveInput) < 0.01f)
        {
            totalRotation = 0f;
        }
    }

    void HandleBucketControl()
    {
        if (wheel010 == null) return;

        float targetAngle = currentBucketAngle;
        isBucketMoving = false;

        // Поднять ковш (наклон вперед) - клавиша V
        if (Input.GetKey(KeyCode.V))
        {
            targetAngle = maxBucketAngle;
            isBucketMoving = true;
        }
        // Опустить ковш (наклон назад) - клавиша B
        else if (Input.GetKey(KeyCode.B))
        {
            targetAngle = -maxBucketAngle;
            isBucketMoving = true;
        }
        // Вернуть в нейтральное положение - если не нажаты клавиши
        else
        {
            targetAngle = 0f;
        }

        // Плавное изменение угла ковша
        currentBucketAngle = Mathf.MoveTowards(currentBucketAngle, targetAngle, bucketTiltSpeed * Time.deltaTime);

        // Применяем поворот к ковшу вокруг локальной оси X
        wheel010.localRotation = Quaternion.Euler(currentBucketAngle, 0f, 0f);
    }

    bool CanRotate(float rotationAmount)
    {
        // Проверяем не превышен ли максимальный общий угол поворота
        float newTotalRotation = totalRotation + rotationAmount;
        return Mathf.Abs(newTotalRotation) <= maxTotalRotation;
    }

    float CalculateRotationSpeed(float steeringAngle, float moveInput)
    {
        // Рассчитываем скорость поворота на основе угла руля и скорости движения
        // Нормализуем угол поворота от -1 до 1
        float normalizedSteering = steeringAngle / maxSteeringAngle;

        // Базовая скорость поворота
        float baseRotationSpeed = 50f;

        // Учитываем направление движения
        float directionMultiplier = Mathf.Sign(moveInput);

        // Ограничиваем максимальную скорость поворота для достижения 15% радиуса
        float maxRotationSpeed = baseRotationSpeed * (maxTurnRadiusPercent / 100f);

        // Итоговая скорость поворота с ограничением
        float rotationSpeed = normalizedSteering * baseRotationSpeed * directionMultiplier;

        return Mathf.Clamp(rotationSpeed, -maxRotationSpeed, maxRotationSpeed);
    }

    void FindSteeringBone()
    {
        // Ищем объект WGT-Bone в дочерних объектах
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in allChildren)
        {
            if (child.name == steeringBoneName)
            {
                steeringBone = child;
                Debug.Log($"Найдена рулевая кость: {steeringBoneName}");
                break;
            }
        }

        if (steeringBone == null)
        {
            Debug.LogWarning($"Рулевая кость {steeringBoneName} не найдена! Повороты будут работать через Rigidbody");
        }
    }

    void AddColliderIfMissing()
    {
        // Проверяем есть ли коллайдер
        Collider existingCollider = GetComponent<Collider>();
        if (existingCollider == null)
        {
            // Добавляем BoxCollider по умолчанию
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(2f, 1f, 3f);
            collider.center = new Vector3(0f, 0.5f, 0f);
            Debug.Log("Добавлен BoxCollider к трактору");
        }
    }

    void FindSpecificWheels()
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in allChildren)
        {
            if (child.name == "Plane.013") wheel013 = child; // заднее
            if (child.name == "Plane.012") wheel012 = child; // заднее
            if (child.name == "Plane.010") wheel010 = child; // ковш
            if (child.name == "Plane.011") wheel011 = child; // переднее
        }

        if (wheel013 == null) Debug.LogError("Не найдено колесо Plane.013!");
        if (wheel012 == null) Debug.LogError("Не найдено колесо Plane.012!");
        if (wheel010 == null) Debug.LogWarning("Ковш Plane.010 не найден!");
        if (wheel011 == null) Debug.LogWarning("Переднее колесо Plane.011 не найдено!");
    }

    void HandleGearShift()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeGear(1);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeGear(2);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeGear(3);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) ChangeGear(4);
        else if (Input.GetKeyDown(KeyCode.Alpha5)) ChangeGear(5);
        else if (Input.GetKeyDown(KeyCode.R)) ChangeGear(0);
    }

    void ChangeGear(int newGear)
    {
        if (newGear >= 0 && newGear <= gearRatios.Length)
        {
            currentGear = newGear;

            if (currentGear == 0) // Задняя передача
            {
                currentSpeed = moveSpeed * gearRatios[0] * -1f;
                Debug.Log("Задняя передача включена");
            }
            else if (currentGear == 3) // Нейтраль
            {
                currentSpeed = 0f;
                Debug.Log("Нейтральная передача");
            }
            else
            {
                currentSpeed = moveSpeed * gearRatios[newGear - 1];
                Debug.Log($"Включена {newGear} передача");
            }
        }
    }

    void RotateWheels()
    {
        float moveInput = Input.GetAxis("Vertical");
        if (Mathf.Abs(moveInput) < 0.01f) return;

        float rotationAngle = moveInput * wheelRotationSpeed * Time.deltaTime * (currentGear == 0 ? -1f : 1f);

        // Задние колёса
        if (wheel013 != null) wheel013.Rotate(rotationAngle, 0, 0, Space.Self);
        if (wheel012 != null) wheel012.Rotate(rotationAngle, 0, 0, Space.Self);

        // Передние колёса (ковш не вращается)
        if (wheel011 != null)
            wheel011.Rotate(rotationAngle, 0, 0, Space.Self);
    }

    void SteerFrontWheels()
    {
        // Поворачиваем передние колёса в соответствии с текущим углом поворота
        // Ковш не поворачивается
        if (wheel011 != null)
            wheel011.localRotation = Quaternion.Euler(wheel011.localRotation.eulerAngles.x, currentSteeringAngle, 0f);
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        string gearText = currentGear == 0 ? "R" :
                         currentGear == 3 ? "N" :
                         currentGear.ToString();

        string steeringStatus = steeringBone != null ? "✓" : "✗";
        string bucketStatus = currentBucketAngle > 0 ? "ПОДНЯТ" : currentBucketAngle < 0 ? "ОПУЩЕН" : "НЕЙТРАЛЬНО";

        GUI.Label(new Rect(10, 10, 350, 30), $"Передача: {gearText}", style);
        GUI.Label(new Rect(10, 40, 350, 30), $"Скорость: {rb.linearVelocity.magnitude:F1}", style);
        GUI.Label(new Rect(10, 70, 350, 30), $"Рулевая кость ({steeringBoneName}): {steeringStatus}", style);
        GUI.Label(new Rect(10, 100, 350, 30), $"Угол поворота: {currentSteeringAngle:F1}°", style);
        GUI.Label(new Rect(10, 130, 350, 30), $"Радиус поворота: {CalculateTurnRadius():F1}%", style);
        GUI.Label(new Rect(10, 160, 350, 30), $"Максимум: {maxTurnRadiusPercent}%", style);
        GUI.Label(new Rect(10, 190, 350, 30), $"Общий поворот: {totalRotation:F1}° / {maxTotalRotation}°", style);
        GUI.Label(new Rect(10, 220, 350, 30), $"Ковш: {bucketStatus}", style);
        GUI.Label(new Rect(10, 250, 350, 30), $"Угол ковша: {currentBucketAngle:F1}°", style);
        GUI.Label(new Rect(10, 280, 400, 30), $"Управление ковшом: V - поднять, B - опустить", style);
    }

    float CalculateTurnRadius()
    {
        // Вычисляем текущий радиус поворота в процентах
        float normalizedAngle = Mathf.Abs(currentSteeringAngle) / maxSteeringAngle;
        float turnRadiusPercent = (1f - normalizedAngle) * 100f;
        return Mathf.Clamp(turnRadiusPercent, maxTurnRadiusPercent, 100f);
    }
}