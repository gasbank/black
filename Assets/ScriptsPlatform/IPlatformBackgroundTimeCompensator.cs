using UnityEngine;

public interface IPlatformBackgroundTimeCompensator {
    void BeginBackgroundState(MonoBehaviour behaviour);
    void EndBackgroundState(MonoBehaviour behaviour);
}
