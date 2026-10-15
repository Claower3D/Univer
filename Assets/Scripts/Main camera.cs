using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;          // Объект, вокруг которого вращаемся (твой трактор)
    public float distance = 15f;      // Расстояние от трактора
    public float heightOffset = 4f;   // Насколько камера поднята над трактором
    public float sensitivity = 200f;  // Чувствительность мыши
    public float minY = -30f;         // Минимальный угол наклона
    public float maxY = 70f;          // Максимальный угол наклона

    private float rotationX;
    private float rotationY;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("⚠️ Не назначен объект target (трактор)!");
        }

        // Скрываем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Начальные углы
        rotationX = transform.eulerAngles.y;
        rotationY = transform.eulerAngles.x;
    }

    void LateUpdate()
    {
        if (!target) return;

        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        rotationX += mouseX;
        rotationY -= mouseY;
        rotationY = Mathf.Clamp(rotationY, minY, maxY);

        // Позиция камеры выше и позади трактора
        Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0);
        Vector3 direction = new Vector3(0, heightOffset, -distance);

        transform.position = target.position + rotation * new Vector3(0, 0, -distance) + Vector3.up * heightOffset;
        transform.LookAt(target.position + Vector3.up * heightOffset);
    }
}
