using UnityEngine;

public class WaterRipple : MonoBehaviour
{
    public float speedX = 0.02f;
    public float speedY = 0.01f;

    private Renderer rend;
    private Vector2 offset;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        offset.x += speedX * Time.deltaTime;
        offset.y += speedY * Time.deltaTime;
        rend.material.SetTextureOffset("_BaseMap", offset);
    }
}
