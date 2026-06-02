using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class WaterRipple : MonoBehaviour
{
    public float speedX = 0.02f;
    public float speedY = 0.01f;

    private Renderer rend;
    private Vector2 offset;

    void Start()
    {
        // Resolve safely; without a Renderer there is nothing to scroll, so disable
        // the component instead of dereferencing null every frame (Week 3 BUG-07).
        if (!TryGetComponent(out rend))
        {
            Debug.LogWarning($"{nameof(WaterRipple)} on '{name}' has no Renderer — disabling.", this);
            enabled = false;
        }
    }

    void Update()
    {
        if (rend == null)
            return;

        offset.x += speedX * Time.deltaTime;
        offset.y += speedY * Time.deltaTime;
        rend.material.SetTextureOffset("_BaseMap", offset);
    }
}
