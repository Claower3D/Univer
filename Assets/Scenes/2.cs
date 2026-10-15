using UnityEngine;

[ExecuteAlways]
public class TractorAutoSetup : MonoBehaviour
{
    [Header("Физика трактора")]
    public float tractorMass = 1200f;
    public float drag = 0.1f;
    public float angularDrag = 0.3f;
    public float groundOffset = 0.5f; // Насколько выше земли спавнится трактор

    [Header("Стабилизация")]
    public bool freezeXZRotation = true;
    public bool autoAlignToTerrain = true;

    [Header("Terrain Setup")]
    public bool setupTerrainPhysics = true;
    public bool showDebug = true;

    private Rigidbody rb;

    void Start()
    {
        SetupTerrain();
        SetupTractor();
        AlignToGround();
    }

    void SetupTractor()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.mass = tractorMass;
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.sleepThreshold = 0.05f;

        if (freezeXZRotation)
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (showDebug)
            Debug.Log("✅ Rigidbody настроен");
    }

    void SetupTerrain()
    {
        if (!setupTerrainPhysics) return;

        Terrain terrain = FindObjectOfType<Terrain>();
        if (terrain == null)
        {
            if (showDebug)
                Debug.LogWarning("⚠️ Terrain не найден!");
            return;
        }

        TerrainCollider tc = terrain.GetComponent<TerrainCollider>();
        if (tc == null)
        {
            tc = terrain.gameObject.AddComponent<TerrainCollider>();
            tc.terrainData = terrain.terrainData;
        }

        PhysicsMaterial mat = new PhysicsMaterial("TerrainMat");
        mat.dynamicFriction = 1.2f;
        mat.staticFriction = 1.4f;
        mat.bounciness = 0f;
        mat.frictionCombine = PhysicsMaterialCombine.Multiply;
        tc.material = mat;

        if (showDebug)
            Debug.Log("✅ Terrain настроен");
    }

    void AlignToGround()
    {
        if (!autoAlignToTerrain) return;

        Terrain terrain = FindObjectOfType<Terrain>();
        if (terrain == null) return;

        Vector3 pos = transform.position;
        float terrainHeight = terrain.SampleHeight(pos);
        pos.y = terrainHeight + groundOffset;
        transform.position = pos;

        if (showDebug)
            Debug.Log($"✅ Трактор выставлен на высоту земли ({terrainHeight:F2})");
    }

    void FixedUpdate()
    {
        // Плавное прижатие к земле (чтобы не подбрасывало)
        Ray ray = new Ray(transform.position + Vector3.up * 1f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            if (Vector3.Distance(transform.position, hit.point) > groundOffset)
            {
                Vector3 targetPos = hit.point + Vector3.up * groundOffset;
                rb.MovePosition(Vector3.Lerp(transform.position, targetPos, Time.fixedDeltaTime * 10f));
            }

            if (autoAlignToTerrain)
            {
                Quaternion targetRot = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * 5f);
            }
        }
    }
}
