using UnityEngine;

public class FishAnimate : MonoBehaviour
{
    public enum SwingAxis
    {
        LocalX,
        LocalY,
        LocalZ,
    }

    [Header("Bones")]
    public Transform tailRoot;
    public Transform tailTip; // optional

    [Header("Swing Base")]
    public SwingAxis axis = SwingAxis.LocalY;
    public float tailSwingAngle = 18f;
    public float tailFrequency = 6f;

    [Header("Randomization")]
    [Range(0f, 1f)]
    public float amplitudeRandom = 0.25f;

    [Range(0f, 1f)]
    public float frequencyRandom = 0.25f;

    [Header("Speed Influence")]
    public FishSwim swim;
    public float speedMultiplier = 1.2f;

    float phase;
    float ampMul;
    float freqMul;
    float noiseOffset;

    Quaternion tailRootBase;
    Quaternion tailTipBase;

    void Awake()
    {
        if (tailRoot)
            tailRootBase = tailRoot.localRotation;
        if (tailTip)
            tailTipBase = tailTip.localRotation;

        phase = Random.Range(0f, Mathf.PI * 2f);
        ampMul = Random.Range(1f - amplitudeRandom, 1f + amplitudeRandom);
        freqMul = Random.Range(1f - frequencyRandom, 1f + frequencyRandom);
        noiseOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (!tailRoot || !swim)
            return;

        float sp = Mathf.Lerp(0.6f, speedMultiplier, swim.NormalizedSpeed);

        // micro noise (VERY subtle)
        float noise = (Mathf.PerlinNoise(Time.time * 0.4f + noiseOffset, 0f) - 0.5f) * 2f;

        phase += Time.deltaTime * tailFrequency * freqMul * sp;

        float swing = Mathf.Sin(phase + noise * 0.3f) * tailSwingAngle * ampMul;

        tailRoot.localRotation = tailRootBase * AxisRot(axis, swing);

        if (tailTip)
        {
            tailTip.localRotation = tailTipBase * AxisRot(axis, swing * 1.4f);
        }
    }

    static Quaternion AxisRot(SwingAxis a, float deg)
    {
        return a switch
        {
            SwingAxis.LocalX => Quaternion.AngleAxis(deg, Vector3.right),
            SwingAxis.LocalY => Quaternion.AngleAxis(deg, Vector3.up),
            _ => Quaternion.AngleAxis(deg, Vector3.forward),
        };
    }
}
