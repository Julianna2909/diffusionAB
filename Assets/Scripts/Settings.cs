using UnityEngine;

public enum DisplayMode
{
    AB = 0,
    Greyscale = 1,
    Delta1 = 2,
    Delta2 = 3,
    Colored = 4
}

[CreateAssetMenu(menuName = "Create Settings", fileName = "Settings", order = 0)]
public class Settings : ScriptableObject
{
    public Texture InitMap;
    public DisplayMode DisplayMode;
    
    [Header("Behaviour")]
    [Range(0, 0.1f)]
    public float feedRate;
    [Range(0, 0.1f)]
    public float removeRate;
    [Range(0, 1f)]
    public float diffuseRateA;
    [Range(0, 1f)]
    public float diffuseRateB;
    [Range(2, 8)]
    public int diffuseRadius;

    public Color colorA;
    public Color colorB;

    public void SetShaderParameters(ComputeShader compute)
    {
        compute.SetFloat(nameof(feedRate), feedRate);
        compute.SetFloat(nameof(removeRate), removeRate);
        compute.SetFloat(nameof(diffuseRateA), diffuseRateA);
        compute.SetFloat(nameof(diffuseRateB), diffuseRateB);
        compute.SetInt(nameof(diffuseRadius), diffuseRadius);
        compute.SetVector(nameof(colorA), colorA);
        compute.SetVector(nameof(colorB), colorB);
    }
}