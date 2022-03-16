using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class CharAgent : MonoBehaviour
{
    [SerializeField]
    NavMeshAgent agent;

    [SerializeField]
    float walkRadius = 3.0f;

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    void Start()
    {
        StartCoroutine(RoamCoro());
    }

    IEnumerator RoamCoro()
    {
        while (Application.isPlaying)
        {
            var randomDirection = Random.insideUnitSphere * walkRadius;
            randomDirection += transform.position;
            NavMesh.SamplePosition(randomDirection, out var hit, walkRadius, 1);
            var finalPosition = hit.position;
            agent.SetDestination(finalPosition);
            
            yield return new WaitForSeconds(Random.Range(2.0f, 3.0f));
        }
    }
}