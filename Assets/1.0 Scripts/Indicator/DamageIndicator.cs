using UnityEngine;

public class DamageIndicator : MonoBehaviour
{
    [SerializeField] private Transform player;                 
    [SerializeField] private Transform damageSource;           
    [SerializeField] private RectTransform indicatorPivot;
    [SerializeField] private float angleOffset = 0f;          

    private void LateUpdate()
    {
        if (!player || !damageSource || !indicatorPivot) return;
        Vector2 dir = (Vector2)(damageSource.position - player.position);
        if (dir.sqrMagnitude < 0.0001f) return;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        indicatorPivot.localRotation = Quaternion.Euler(0f, 0f, angle + angleOffset);
    }
}
