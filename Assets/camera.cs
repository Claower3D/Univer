using UnityEngine;
using System.Collections.Generic;

public class TripleCameraSystem : MonoBehaviour
{
    [Header("Camera Targets")]
    public Transform excavator; // Цель - экскаватор

    [Header("Cameras")]
    public Camera Main; // Основная камера
    public Camera Left; // Левая камера  
    public Camera Right; // Правая камера

    [Header("Camera Positions")]
    public Vector3 mainOffset = new Vector3(0f, 10f, -10f);
    public Vector3 leftOffset = new Vector3(-8f, 6f, -6f);
    public Vector3 rightOffset = new Vector3(8f, 6f, -6f);

    [Header("Smooth Follow")]
    public float smoothSpeed = 0.125f;
    public bool smoothFollow = true;

    [Header("View Modes")]
    public KeyCode singleViewKey = KeyCode.F1;
    public KeyCode tripleViewKey = KeyCode.F2;
    public KeyCode pipViewKey = KeyCode.F3;

    [Header("Minimap Settings")]
    public float minimapSize = 150f;
    public float minimapMargin = 20f;
    public float mapScale = 0.1f;

    [Header("Quest System")]
    public List<string> currentQuests = new List<string>()
    {
        "Выкопать траншею длиной 50м",
        "Загрузить 10 тонн грунта",
        "Выровнять площадку",
        "Переместить материалы в зону B"
    };

    private enum ViewMode { Single, Triple, PictureInPicture }
    private ViewMode currentViewMode = ViewMode.Triple;

    void Start()
    {
        // Принудительно находим камеры если они не назначены
        FindAllCameras();

        if (excavator == null)
        {
            GameObject excavatorObj = GameObject.FindGameObjectWithTag("Player");
            if (excavatorObj != null)
                excavator = excavatorObj.transform;
        }

        // Устанавливаем тройной режим по умолчанию
        SetTripleView();

        Debug.Log("Система камер запущена. Main: " + (Main != null) + " Left: " + (Left != null) + " Right: " + (Right != null));
    }

    void FindAllCameras()
    {
        // Находим все камеры в сцене
        Camera[] allCameras = FindObjectsOfType<Camera>();

        foreach (Camera cam in allCameras)
        {
            if (Main == null && cam.name.Contains("Main"))
                Main = cam;
            else if (Left == null && cam.name.Contains("Left"))
                Left = cam;
            else if (Right == null && cam.name.Contains("Right"))
                Right = cam;
        }

        // Если какие-то камеры не найдены, создаем их
        if (Main == null)
        {
            Main = CreateCamera("Main Camera", Color.red);
        }
        if (Left == null)
        {
            Left = CreateCamera("Left Camera", Color.blue);
        }
        if (Right == null)
        {
            Right = CreateCamera("Right Camera", Color.green);
        }
    }

    Camera CreateCamera(string name, Color backgroundColor)
    {
        GameObject cameraGO = new GameObject(name);
        Camera camera = cameraGO.AddComponent<Camera>();
        camera.backgroundColor = backgroundColor;
        camera.clearFlags = CameraClearFlags.SolidColor;
        return camera;
    }

    void Update()
    {
        HandleInput();
        UpdateCameraPositions();
    }

    void HandleInput()
    {
        // Переключение режимов отображения
        if (Input.GetKeyDown(singleViewKey))
        {
            SetSingleView(Main);
        }
        else if (Input.GetKeyDown(tripleViewKey))
        {
            SetTripleView();
        }
        else if (Input.GetKeyDown(pipViewKey))
        {
            SetPictureInPictureView();
        }

        // Быстрое переключение на конкретные камеры
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetSingleView(Main);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetSingleView(Left);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetSingleView(Right);
        }
    }

    void UpdateCameraPositions()
    {
        if (excavator == null) return;

        // Обновляем позиции всех камер
        UpdateCameraTransform(Main, mainOffset);
        UpdateCameraTransform(Left, leftOffset);
        UpdateCameraTransform(Right, rightOffset);
    }

    void UpdateCameraTransform(Camera camera, Vector3 offset)
    {
        if (camera == null || excavator == null) return;

        // Вычисляем желаемую позицию с учетом поворота экскаватора
        Vector3 desiredPosition = excavator.position +
                                 excavator.right * offset.x +
                                 excavator.up * offset.y +
                                 excavator.forward * offset.z;

        // Плавное перемещение или мгновенное
        if (smoothFollow)
        {
            Vector3 smoothedPosition = Vector3.Lerp(camera.transform.position, desiredPosition, smoothSpeed);
            camera.transform.position = smoothedPosition;
        }
        else
        {
            camera.transform.position = desiredPosition;
        }

        // Направляем камеру на экскаватор
        camera.transform.LookAt(excavator.position + Vector3.up * 2f);
    }

    void SetSingleView(Camera activeCamera)
    {
        if (activeCamera == null) return;

        currentViewMode = ViewMode.Single;

        Debug.Log("Активация одиночной камеры: " + activeCamera.name);

        // Включаем ВСЕ камеры, но настраиваем их viewport
        EnableAllCameras();

        // Активная камера - на весь экран
        if (activeCamera != null)
        {
            activeCamera.rect = new Rect(0, 0, 1, 1);
            activeCamera.depth = 1;
        }

        // Остальные камеры - вне экрана
        if (Main != null && Main != activeCamera)
        {
            Main.rect = new Rect(0, 0, 0, 0);
            Main.depth = 0;
        }
        if (Left != null && Left != activeCamera)
        {
            Left.rect = new Rect(0, 0, 0, 0);
            Left.depth = 0;
        }
        if (Right != null && Right != activeCamera)
        {
            Right.rect = new Rect(0, 0, 0, 0);
            Right.depth = 0;
        }
    }

    void SetTripleView()
    {
        currentViewMode = ViewMode.Triple;

        Debug.Log("Активация ТРОЙНОГО режима");

        // Включаем ВСЕ камеры
        EnableAllCameras();

        // Настраиваем области отображения для всех трех камер
        if (Main != null)
        {
            Main.rect = new Rect(0.33f, 0.5f, 0.34f, 0.5f); // Центр сверху
            Main.depth = 0;
        }

        if (Left != null)
        {
            Left.rect = new Rect(0, 0, 0.33f, 1f); // Слева во всю высоту
            Left.depth = 0;
        }

        if (Right != null)
        {
            Right.rect = new Rect(0.67f, 0, 0.33f, 1f); // Справа во всю высоту
            Right.depth = 0;
        }
    }

    void SetPictureInPictureView()
    {
        currentViewMode = ViewMode.PictureInPicture;

        // Включаем ВСЕ камеры
        EnableAllCameras();

        // Основная камера - на весь экран
        if (Main != null)
        {
            Main.rect = new Rect(0, 0, 1, 1);
            Main.depth = -1;
        }

        // Левая камера - маленькая в углу
        if (Left != null)
        {
            Left.rect = new Rect(0.02f, 0.02f, 0.3f, 0.3f);
            Left.depth = 0;
        }

        // Правая камера - маленькая в углу
        if (Right != null)
        {
            Right.rect = new Rect(0.68f, 0.02f, 0.3f, 0.3f);
            Right.depth = 0;
        }
    }

    void EnableAllCameras()
    {
        // Принудительно включаем все камеры
        if (Main != null)
        {
            Main.enabled = true;
            Main.gameObject.SetActive(true);
        }
        if (Left != null)
        {
            Left.enabled = true;
            Left.gameObject.SetActive(true);
        }
        if (Right != null)
        {
            Right.enabled = true;
            Right.gameObject.SetActive(true);
        }
    }

    void OnGUI()
    {
        DrawMinimap();
        DrawQuestList();
        DrawHUDInfo();
    }

    void DrawMinimap()
    {
        float minimapX = minimapMargin;
        float minimapY = minimapMargin;

        // Рисуем фон миникарты
        GUIStyle minimapStyle = new GUIStyle(GUI.skin.box);
        minimapStyle.normal.background = CreateColorTexture(new Color(0.1f, 0.1f, 0.1f, 0.8f));

        GUI.Box(new Rect(minimapX, minimapY, minimapSize, minimapSize), "", minimapStyle);

        // Заголовок миникарты
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.alignment = TextAnchor.UpperCenter;

        GUI.Label(new Rect(minimapX, minimapY - 20, minimapSize, 20), "МИНИКАРТА", labelStyle);

        // Рисуем игрока на миникарте
        if (excavator != null)
        {
            float centerX = minimapX + minimapSize / 2f;
            float centerY = minimapY + minimapSize / 2f;

            // Позиция игрока относительно центра миникарты
            float playerX = centerX + excavator.position.x * mapScale;
            float playerY = centerY + excavator.position.z * mapScale;

            // Желтый маркер игрока
            GUIStyle playerStyle = new GUIStyle();
            playerStyle.normal.background = CreateColorTexture(Color.yellow);

            // Рисуем маркер игрока
            DrawPlayerMarker(playerX, playerY, excavator.eulerAngles.y, 8f);

            // Точка в центре маркера
            GUI.Box(new Rect(playerX - 2, playerY - 2, 4, 4), "", playerStyle);
        }

        // Сетка миникарты
        DrawMinimapGrid(minimapX, minimapY, minimapSize);
    }

    void DrawPlayerMarker(float x, float y, float rotation, float size)
    {
        // Создаем треугольник, направленный по курсу игрока
        Vector2 center = new Vector2(x, y);
        float angle = rotation * Mathf.Deg2Rad;

        // Вершины треугольника
        Vector2 front = new Vector2(
            center.x + Mathf.Sin(angle) * size,
            center.y - Mathf.Cos(angle) * size
        );

        Vector2 left = new Vector2(
            center.x + Mathf.Sin(angle - 2.2f) * size * 0.6f,
            center.y - Mathf.Cos(angle - 2.2f) * size * 0.6f
        );

        Vector2 right = new Vector2(
            center.x + Mathf.Sin(angle + 2.2f) * size * 0.6f,
            center.y - Mathf.Cos(angle + 2.2f) * size * 0.6f
        );

        // Рисуем треугольник линиями
        DrawLine(front, left, Color.yellow, 2f);
        DrawLine(front, right, Color.yellow, 2f);
        DrawLine(left, right, Color.yellow, 2f);
    }

    void DrawLine(Vector2 start, Vector2 end, Color color, float width)
    {
        Texture2D tex = CreateColorTexture(color);
        Vector2 direction = (end - start).normalized;
        float length = Vector2.Distance(start, end);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GUIUtility.RotateAroundPivot(angle, start);
        GUI.DrawTexture(new Rect(start.x, start.y - width / 2, length, width), tex);
        GUIUtility.RotateAroundPivot(-angle, start);
    }

    void DrawMinimapGrid(float x, float y, float size)
    {
        Texture2D gridTex = CreateColorTexture(new Color(1, 1, 1, 0.3f));

        // Вертикальные линии
        for (int i = 1; i < 5; i++)
        {
            float lineX = x + (size / 5) * i;
            GUI.DrawTexture(new Rect(lineX, y, 1, size), gridTex);
        }

        // Горизонтальные линии
        for (int i = 1; i < 5; i++)
        {
            float lineY = y + (size / 5) * i;
            GUI.DrawTexture(new Rect(x, lineY, size, 1), gridTex);
        }
    }

    void DrawQuestList()
    {
        float questX = Screen.width - minimapSize - minimapMargin;
        float questY = minimapMargin;
        float questWidth = minimapSize;
        float questHeight = minimapSize * 1.5f;

        // Фон списка заданий
        GUIStyle questStyle = new GUIStyle(GUI.skin.box);
        questStyle.normal.background = CreateColorTexture(new Color(0.1f, 0.1f, 0.1f, 0.8f));

        GUI.Box(new Rect(questX, questY, questWidth, questHeight), "", questStyle);

        // Заголовок
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.normal.textColor = Color.white;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.UpperCenter;

        GUI.Label(new Rect(questX, questY - 20, questWidth, 20), "ЗАДАНИЯ", titleStyle);

        // Список заданий
        GUIStyle questTextStyle = new GUIStyle(GUI.skin.label);
        questTextStyle.normal.textColor = Color.white;
        questTextStyle.wordWrap = true;
        questTextStyle.padding = new RectOffset(10, 10, 5, 5);

        float currentY = questY + 10;
        foreach (string quest in currentQuests)
        {
            string questText = "• " + quest;
            float textHeight = questTextStyle.CalcHeight(new GUIContent(questText), questWidth - 20);

            GUI.Label(new Rect(questX + 10, currentY, questWidth - 20, textHeight), questText, questTextStyle);
            currentY += textHeight + 5;
        }
    }

    void DrawHUDInfo()
    {
        // Отображение текущего режима камер
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.normal.textColor = Color.white;
        style.fontSize = 14;
        style.fontStyle = FontStyle.Bold;

        string modeText = "";
        switch (currentViewMode)
        {
            case ViewMode.Single: modeText = "Одиночный режим"; break;
            case ViewMode.Triple: modeText = "ТРОЙНОЙ РЕЖИМ"; break;
            case ViewMode.PictureInPicture: modeText = "Режим Картинка-в-Картинке"; break;
        }

        // Центральная информация HUD
        GUI.Label(new Rect(Screen.width / 2 - 150, 10, 300, 30), "Режим: " + modeText, style);
        GUI.Label(new Rect(Screen.width / 2 - 150, 30, 300, 30), "Управление: F1-Main F2-Тройная F3-PiP", style);

        // Координаты игрока
        if (excavator != null)
        {
            string coordText = $"Позиция: X:{excavator.position.x:F1} Y:{excavator.position.y:F1} Z:{excavator.position.z:F1}";
            GUI.Label(new Rect(Screen.width / 2 - 150, 50, 300, 30), coordText, style);
        }
    }

    Texture2D CreateColorTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    // Для отладки в Inspector
    void OnDrawGizmos()
    {
        if (excavator == null) return;

        // Визуализация позиций камер
        Gizmos.color = Color.red;
        if (Main != null)
        {
            Gizmos.DrawLine(excavator.position, Main.transform.position);
            Gizmos.DrawWireSphere(Main.transform.position, 0.5f);
        }

        Gizmos.color = Color.blue;
        if (Left != null)
        {
            Gizmos.DrawLine(excavator.position, Left.transform.position);
            Gizmos.DrawWireSphere(Left.transform.position, 0.5f);
        }

        Gizmos.color = Color.green;
        if (Right != null)
        {
            Gizmos.DrawLine(excavator.position, Right.transform.position);
            Gizmos.DrawWireSphere(Right.transform.position, 0.5f);
        }
    }
}