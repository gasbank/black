using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class Character3D : MonoBehaviour, IWorldPosition3D
{
    [SerializeField]
    NavMeshAgent agent;

    [SerializeField]
    float walkRadius = 3.0f;

    [SerializeField]
    Camera cam;

    [SerializeField]
    Character character;

    public Vector2 ScreenPoint => cam.WorldToScreenPoint(transform.position);

    public Vector2 VelocityOnScreenPoint =>
        (Vector2) cam.WorldToScreenPoint(transform.position + agent.velocity) - ScreenPoint;

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

    public Vector3 WorldPosition3D => transform.position;

    public void SetSiblingIndexFor2D(int index)
    {
        character.transform.SetSiblingIndex(index);
    }
}