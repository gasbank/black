using black_dev_tools;
using UnityEngine;

[DisallowMultipleComponent]
public class StageImporter : MonoBehaviour
{
    [SerializeField]
    bool testImport;
    
#if UNITY_EDITOR
    void OnValidate() {
        AutoBindUtil.BindAll(this);
    }
#endif

    void Start()
    {
        if (testImport)
        {
            Program.Main(new[] {"dit", "/Users/gb/two-circle.png", "30"});
        }
    }
}
