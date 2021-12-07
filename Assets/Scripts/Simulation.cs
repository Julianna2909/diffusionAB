using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

public class Simulation : MonoBehaviour
{
    [SerializeField, Range(0, 10)] private int simulationSpeed;

    [SerializeField] private Button restartButton; 
    [SerializeField] private RawImage rawImage;
    [SerializeField] private Settings settings;
    [SerializeField] private ComputeShader compute;
    [SerializeField] private Vector2Int screenSize;
    
    private RenderTexture map;
    private RenderTexture newMap;
    private RenderTexture displayMap;
    
    private bool displayIsNeeded;
    
    private const int InitKernel = 0;
    private const int SimulateKernel = 1;
    private const int DisplayKernel = 2;

    private static Dictionary<Texture, string> textureIds;

    private void Start()
    {
        restartButton.onClick.AddListener(OnResetButton);
        
        CreateTextures();
        rawImage.texture = displayMap;

        Init();
    }

    private void Update()
    {
        if (displayIsNeeded)
        {
            Display();
            displayIsNeeded = false;
        }
    }

    private void FixedUpdate()
    {
        for (var i = 0; i < simulationSpeed; i++)
        {
            Simulate();
        }
        displayIsNeeded = true;
    }

    private void OnResetButton()
    {
        Init();
    }

    private void CreateTextures()
    {
        var filterMode = FilterMode.Point;
        var format = GraphicsFormat.R16G16B16A16_SFloat;
        
        CreateRenderTexture(ref map, screenSize.x, screenSize.y, filterMode, format, "Map");
        CreateRenderTexture(ref newMap, screenSize.x, screenSize.y, filterMode, format, "New Map");
        CreateRenderTexture(ref displayMap, screenSize.x, screenSize.y, filterMode, format, "Display Map");

        textureIds = new Dictionary<Texture, string>
        {
            {map, "Map"},
            {newMap, "NewMap"},
            {displayMap, "DisplayMap"},
            {settings.InitMap, "InitMap"}
        };
    }

    private void Init()
    {
        compute.SetInt("width", screenSize.x);
        compute.SetInt("height", screenSize.y);

        RunKernel(InitKernel, map, settings.InitMap);
    }

    private void Simulate()
    {
        settings.SetShaderParameters(compute);
        RunKernel(SimulateKernel, map, newMap);

        Graphics.Blit(newMap, map);
    }

    private void Display()
    {
        compute.SetInt("displayMode", (int) settings.DisplayMode);

        RunKernel(DisplayKernel, map, displayMap);
    }

    private void RunKernel(int kernelIndex, Texture firstTexture, Texture secondTexture)
    {
        compute.SetTexture(kernelIndex, textureIds[firstTexture], firstTexture);
        compute.SetTexture(kernelIndex, textureIds[secondTexture], secondTexture);
        Dispatch(compute, screenSize.x, screenSize.y, 1, kernelIndex);
    }
    
    public static void CreateRenderTexture(
        ref RenderTexture texture, 
        int width, 
        int height,
        FilterMode filterMode,
        GraphicsFormat format,
        string name = "Unnamed"
        )
    {
        if (texture == null
            || !texture.IsCreated()
            || texture.width != width
            || texture.height != height
            || texture.graphicsFormat != format)
        {
            if (texture != null)
            {
                texture.Release();
            }
            texture = new RenderTexture(width, height, 0);
            texture.graphicsFormat = format;
            texture.enableRandomWrite = true;

            texture.autoGenerateMips = false;
            texture.Create();
        }
        texture.name = name;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = filterMode;
    }

    public static void Dispatch(
        ComputeShader cs,
        int numIterationsX,
        int numIterationsY = 1,
        int numIterationsZ = 1,
        int kernelIndex = 0
        )
    {
        var threadGroupSizes = GetThreadGroupSizes(cs, kernelIndex);
        var numGroupsX = Mathf.CeilToInt(numIterationsX / (float)threadGroupSizes.x);
        var numGroupsY = Mathf.CeilToInt(numIterationsY / (float)threadGroupSizes.y);
        var numGroupsZ = Mathf.CeilToInt(numIterationsZ / (float)threadGroupSizes.z);
        cs.Dispatch(kernelIndex, numGroupsX, numGroupsY, numGroupsZ);
    }

    public static Vector3Int GetThreadGroupSizes(ComputeShader compute, int kernelIndex = 0)
    {
        compute.GetKernelThreadGroupSizes(kernelIndex, out var x, out var y, out var z);
        return new Vector3Int((int)x, (int)y, (int)z);
    }
}