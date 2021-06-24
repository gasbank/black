using SixLabors.ImageSharp.PixelFormats;
using UnityEngine;

public class SixLaborsTester : MonoBehaviour {
    void Start() {
        var x = new SixLabors.ImageSharp.Image<Rgba32>(100, 200);
        Debug.Log(x);
    }
}