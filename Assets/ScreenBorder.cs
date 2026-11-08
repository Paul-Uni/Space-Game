using UnityEngine;

// Auf die Main Camera ziehen.
// Baut einen unsichtbaren Rand exakt an den Bildschirmkanten (Layer "Wall"),
// passt sich an jedes Seitenverhältnis/Fenstergröße an und schaltet die alten Canvas-Walls ab.
[RequireComponent(typeof(Camera))]
public class ScreenBorder : MonoBehaviour
{
    [Tooltip("Dicke der Wände (liegen komplett außerhalb des Bildes). Dick = kein Durchtunneln beim Dash.")]
    public float thickness = 2f;
    [Tooltip("Alte Spieler-Walls (Tag 'Wall' auf Layer 'Wall') beim Start deaktivieren.")]
    public bool disableOldWalls = true;

    private Camera cam;
    private BoxCollider2D left, right, top, bottom;
    private float lastAspect = -1f, lastSize = -1f;

    void Awake()
    {
        cam = GetComponent<Camera>();
        int wallLayer = LayerMask.NameToLayer("Wall");

        if (disableOldWalls)
        {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Wall"))
                if (go.layer == wallLayer) go.SetActive(false); // "Enemy Wall" bleibt aktiv
        }

        GameObject root = new GameObject("Screen Border");
        root.layer = wallLayer;
        left = CreateWall(root, "Left", wallLayer);
        right = CreateWall(root, "Right", wallLayer);
        top = CreateWall(root, "Top", wallLayer);
        bottom = CreateWall(root, "Bottom", wallLayer);

        UpdateBorder();
    }

    void LateUpdate()
    {
        // Neu berechnen, wenn sich Fenster/Game-View-Größe oder Kamera ändert
        // (Kameraposition bewusst nicht prüfen -> Screen Shake soll den Rand nicht verschieben)
        if (!Mathf.Approximately(cam.aspect, lastAspect) ||
            !Mathf.Approximately(cam.orthographicSize, lastSize))
        {
            UpdateBorder();
        }
    }

    private BoxCollider2D CreateWall(GameObject root, string name, int layer)
    {
        GameObject go = new GameObject(name);
        go.layer = layer;
        go.transform.SetParent(root.transform, false);
        return go.AddComponent<BoxCollider2D>();
    }

    private void UpdateBorder()
    {
        lastAspect = cam.aspect;
        lastSize = cam.orthographicSize;

        float h = cam.orthographicSize;   // halbe Höhe in Welt-Einheiten
        float w = h * cam.aspect;         // halbe Breite
        float t = thickness;
        Vector2 c = cam.transform.position;

        Place(left,   new Vector2(c.x - w - t / 2f, c.y), new Vector2(t, 2f * h + 2f * t));
        Place(right,  new Vector2(c.x + w + t / 2f, c.y), new Vector2(t, 2f * h + 2f * t));
        Place(top,    new Vector2(c.x, c.y + h + t / 2f), new Vector2(2f * w + 2f * t, t));
        Place(bottom, new Vector2(c.x, c.y - h - t / 2f), new Vector2(2f * w + 2f * t, t));
    }

    private void Place(BoxCollider2D box, Vector2 pos, Vector2 size)
    {
        box.transform.position = pos;
        box.size = size;
    }

    void OnDrawGizmos()
    {
        Camera c = GetComponent<Camera>();
        if (c == null || !c.orthographic) return;
        float h = c.orthographicSize, w = h * c.aspect;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector2)c.transform.position, new Vector3(2f * w, 2f * h, 0f));
    }
}
