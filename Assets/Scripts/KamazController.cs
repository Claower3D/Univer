using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class KamazController : MonoBehaviour
{
    [Header("Характеристики двигателя")]
    [Tooltip("Мощность разгона")]
    public float motorTorque = 1200f;
    [Tooltip("Сила торможения")]
    public float brakeTorque = 2500f;
    [Tooltip("Максимальная скорость (км/ч)")]
    public float maxSpeedKmH = 80f;
    [Tooltip("Скорость заднего хода")]
    public float reverseMaxSpeedKmH = 25f;

    [Header("Рулевое управление")]
    [Tooltip("Максимальный угол поворота передних колес")]
    public float maxSteerAngle = 32f;
    [Tooltip("Скорость возврата руля")]
    public float steerSpeed = 60f;

    [Header("Кузов самосвала (Dump Bed)")]
    [Tooltip("Трансформ откидной части кузова")]
    public Transform dumpBed;
    [Tooltip("Максимальный угол подъема кузова")]
    public float maxDumpAngle = 50f;
    [Tooltip("Скорость подъема/опускания кузова")]
    public float dumpSpeed = 25f;
    [Tooltip("Текущий угол кузова")]
    [Range(0f, 60f)]
    public float currentDumpAngle = 0f;

    [Header("Освещение и эффекты")]
    public Light[] headLights;
    public Light[] brakeLights;
    public Light[] reverseLights;
    public AudioSource engineAudio;
    public AudioSource hornAudio;

    [Header("Колеса (опционально, если есть меши)")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform[] rearWheels;

    private Rigidbody rb;
    private float currentSteerAngle = 0f;
    private bool lightsOn = false;
    private bool isDumping = false;
    private bool isLowering = false;

    public float CurrentSpeedKmH => rb != null ? rb.linearVelocity.magnitude * 3.6f : 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 8000f; // 8 тонн для порожнего КамАЗа
        rb.centerOfMass = new Vector3(0f, -0.5f, 0.2f); // Низкий центр тяжести для устойчивости

        // Если кузов не задан, ищем дочерний объект с именем Dump/Кузов
        if (dumpBed == null)
        {
            Transform found = transform.Find("DumpBed") ?? transform.Find("Body") ?? transform.Find("Кузов");
            if (found != null) dumpBed = found;
        }
    }

    void Update()
    {
        HandleInputs();
        UpdateDumpBed();
        UpdateLights();
        UpdateWheelVisuals();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleInputs()
    {
        // Управление подъемом кузова (Q — поднять, E — опустить)
        isDumping = Input.GetKey(KeyCode.Q);
        isLowering = Input.GetKey(KeyCode.E);

        // Фары (клавиша F)
        if (Input.GetKeyDown(KeyCode.F))
        {
            lightsOn = !lightsOn;
        }

        // Сигнал (клавиша H)
        if (Input.GetKeyDown(KeyCode.H) && hornAudio != null)
        {
            hornAudio.Play();
        }
    }

    private void HandleMovement()
    {
        float v = Input.GetAxis("Vertical");   // W/S или Стрелки вверх/вниз
        float h = Input.GetAxis("Horizontal"); // A/D или Стрелки влево/вправо

        // Плавный поворот колес
        float targetSteer = h * maxSteerAngle;
        currentSteerAngle = Mathf.MoveTowards(currentSteerAngle, targetSteer, steerSpeed * Time.fixedDeltaTime);

        // Движение вперед / назад
        float currentSpeed = rb.linearVelocity.magnitude * 3.6f;
        bool isMovingForward = Vector3.Dot(rb.linearVelocity, transform.forward) > 0.1f;

        // Поворот грузовика вокруг оси
        if (Mathf.Abs(v) > 0.05f || currentSpeed > 1f)
        {
            float direction = (v >= 0) ? 1f : -1f;
            float turnAmount = (currentSteerAngle / maxSteerAngle) * 35f * direction * Time.fixedDeltaTime;
            transform.Rotate(0, turnAmount, 0);
        }

        // Тяга
        if (v > 0.05f && currentSpeed < maxSpeedKmH)
        {
            rb.AddForce(transform.forward * motorTorque * v, ForceMode.Acceleration);
        }
        else if (v < -0.05f)
        {
            if (isMovingForward && currentSpeed > 2f)
            {
                // Торможение при нажатии назад во время движения вперед
                rb.AddForce(-transform.forward * brakeTorque, ForceMode.Acceleration);
            }
            else if (currentSpeed < reverseMaxSpeedKmH)
            {
                // Задний ход
                rb.AddForce(transform.forward * motorTorque * 0.6f * v, ForceMode.Acceleration);
            }
        }

        // Ручной тормоз (Space)
        if (Input.GetKey(KeyCode.Space))
        {
            rb.linearDamping = 2.5f;
        }
        else
        {
            rb.linearDamping = 0.15f;
        }
    }

    private void UpdateDumpBed()
    {
        if (dumpBed == null) return;

        if (isDumping && currentDumpAngle < maxDumpAngle)
        {
            currentDumpAngle += dumpSpeed * Time.deltaTime;
        }
        else if (isLowering && currentDumpAngle > 0f)
        {
            currentDumpAngle -= dumpSpeed * Time.deltaTime;
        }

        currentDumpAngle = Mathf.Clamp(currentDumpAngle, 0f, maxDumpAngle);
        // Поворот кузова вокруг оси X назад
        dumpBed.localRotation = Quaternion.Euler(-currentDumpAngle, 0f, 0f);
    }

    private void UpdateLights()
    {
        if (headLights != null)
        {
            foreach (var l in headLights)
            {
                if (l != null) l.enabled = lightsOn;
            }
        }

        bool braking = Input.GetKey(KeyCode.Space) || (Input.GetAxis("Vertical") < 0 && Vector3.Dot(rb.linearVelocity, transform.forward) > 1f);
        if (brakeLights != null)
        {
            foreach (var l in brakeLights)
            {
                if (l != null) l.enabled = braking || lightsOn;
            }
        }
    }

    private void UpdateWheelVisuals()
    {
        // Поворот передних колес вокруг оси Y
        if (frontLeftWheel != null)
            frontLeftWheel.localRotation = Quaternion.Euler(0, currentSteerAngle, 0);
        if (frontRightWheel != null)
            frontRightWheel.localRotation = Quaternion.Euler(0, currentSteerAngle, 0);
    }

    // Метод для создания процедурного визуального прототипа КамАЗа, если 3D-модели еще нет
    public static GameObject CreateKamazPrototype()
    {
        GameObject root = new GameObject("Kamaz_65115");
        var controller = root.AddComponent<KamazController>();
        var rb = root.GetComponent<Rigidbody>();

        // Основной коллайдер шасси
        var boxCol = root.AddComponent<BoxCollider>();
        boxCol.size = new Vector3(2.5f, 1.2f, 6.5f);
        boxCol.center = new Vector3(0f, 0.6f, 0f);

        // Рама / Шасси
        GameObject chassis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chassis.name = "Chassis";
        chassis.transform.SetParent(root.transform, false);
        chassis.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        chassis.transform.localScale = new Vector3(2.2f, 0.6f, 6.5f);
        DestroyImmediate(chassis.GetComponent<Collider>());

        // Кабина (оранжевая, характерная для КамАЗ)
        GameObject cab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cab.name = "Cabin";
        cab.transform.SetParent(root.transform, false);
        cab.transform.localPosition = new Vector3(0f, 1.6f, 1.8f);
        cab.transform.localScale = new Vector3(2.4f, 1.6f, 2.0f);
        DestroyImmediate(cab.GetComponent<Collider>());

        // Откидной кузов (DumpBed)
        GameObject dumpPivot = new GameObject("DumpPivot");
        dumpPivot.transform.SetParent(root.transform, false);
        dumpPivot.transform.localPosition = new Vector3(0f, 1.0f, -2.8f); // Точка крепления сзади

        GameObject dumpBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dumpBox.name = "DumpBox";
        dumpBox.transform.SetParent(dumpPivot.transform, false);
        dumpBox.transform.localPosition = new Vector3(0f, 0.6f, 1.8f);
        dumpBox.transform.localScale = new Vector3(2.4f, 1.2f, 4.0f);
        DestroyImmediate(dumpBox.GetComponent<Collider>());

        controller.dumpBed = dumpPivot.transform;

        return root;
    }
}
