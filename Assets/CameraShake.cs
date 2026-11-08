using UnityEngine;

// Aufruf von überall: CameraShake.Shake(stärke, dauer);
// Hängt sich bei Bedarf selbst an die Main Camera.
public class CameraShake : MonoBehaviour
{
    private static CameraShake instance;

    private Vector3 basePosition;
    private float strength;
    private float duration;
    private float timeLeft;

    public static void Shake(float strength, float duration)
    {
        if (instance == null)
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            instance = cam.GetComponent<CameraShake>();
            if (instance == null) instance = cam.gameObject.AddComponent<CameraShake>();
        }
        instance.Add(strength, duration);
    }

    void Awake()
    {
        basePosition = transform.localPosition;
    }

    private void Add(float s, float d)
    {
        // Ein schwacher Shake überschreibt keinen stärkeren, der gerade läuft
        float current = duration > 0f ? strength * (timeLeft / duration) : 0f;
        if (s < current) return;
        strength = s;
        duration = d;
        timeLeft = d;
    }

    void LateUpdate()
    {
        if (timeLeft > 0f)
        {
            timeLeft -= Time.unscaledDeltaTime;
            float falloff = Mathf.Clamp01(timeLeft / duration);
            Vector2 offset = Random.insideUnitCircle * strength * falloff;
            transform.localPosition = basePosition + (Vector3)offset;
        }
        else
        {
            transform.localPosition = basePosition;
        }
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}
