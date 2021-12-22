using ConditionalDebug;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UnityEngine;

public class SixLaborsTester : MonoBehaviour
{
    void Start()
    {
        var x = new Image<Rgba32>(100, 200);
        ConDebug.Log(x);
    }
}