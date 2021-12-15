using UnityEngine;

public class Minime : MonoBehaviour
{
    Camera mainCamera;

    [SerializeField]
    Vector2 roamExtent = new Vector2(5.0f, 5.0f);

    [SerializeField]
    float roamRetargetIntervalMax = 5.0f;

    [SerializeField]
    float roamRetargetIntervalMin = 1.0f;

    [SerializeField]
    float roamRetargetRemainTime = 3.0f;

    [SerializeField]
    float roamSmoothTime = 0.1f;

    [SerializeField]
    Vector3 roamTargetPos;

    [SerializeField]
    Vector3 roamVelocity;

    [SerializeField]
    SpriteRenderer SpriteRenderer;

    void Awake()
    {
        RefillRoamRetargetRemainTime();
        mainCamera = Camera.main;
    }

    void Update()
    {
        transform.localPosition =
            Vector3.SmoothDamp(transform.localPosition, roamTargetPos, ref roamVelocity, roamSmoothTime);

        var roamVelocityInCamera = mainCamera.worldToCameraMatrix * roamVelocity;

        SpriteRenderer.flipX = Mathf.Sign(roamVelocityInCamera.x) < 0;
        roamRetargetRemainTime -= Time.deltaTime;
        if (roamRetargetRemainTime <= 0)
        {
            var randomDiff = Random.insideUnitCircle;
            roamTargetPos = transform.localPosition + new Vector3(randomDiff.x, 0, randomDiff.y);
            roamTargetPos = new Vector3(
                Mathf.Clamp(roamTargetPos.x, -roamExtent.x / 2, roamExtent.x / 2),
                0,
                Mathf.Clamp(roamTargetPos.z, -roamExtent.y / 2, roamExtent.y / 2));
            RefillRoamRetargetRemainTime();
        }
    }

    void RefillRoamRetargetRemainTime()
    {
        roamRetargetRemainTime = Random.Range(roamRetargetIntervalMin, roamRetargetIntervalMax);
    }
}