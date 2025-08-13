using UnityEngine;

public class IndicatorWidget2D : MonoBehaviour
{
    private Canvas canvas;
    private RectTransform container;
    private Transform player;
    private Transform enemy;
    private RectTransform pivot;
    private RectTransform arrow;

    private float uiRadius;
    private float angleOffset;
    private Camera uiCam;

    [SerializeField] private Vector2 uiOffset = new Vector2(0f, 0f);
    [SerializeField] private float yOffsetLeft = -70f;
    [SerializeField] private float yOffsetRight = 97f;
    [SerializeField] private float sideDeadZone = 0.05f; 
    [SerializeField] private float yLerp = 20f;          
    private float yOffsetCurrent;

    public void Init(Canvas canvas, RectTransform container, Transform player, Transform enemy,
                     float uiRadius, float angleOffset)
    {
        this.canvas = canvas;
        this.container = container;
        this.player = player;
        this.enemy = enemy;
        this.uiRadius = uiRadius;
        this.angleOffset = angleOffset;

        pivot = (RectTransform)transform;
        uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        if (pivot.childCount > 0 && pivot.GetChild(0) is RectTransform r)
        {
            arrow = r;
            arrow.anchoredPosition = new Vector2(uiRadius, 0f); 
        }
    }

    private void LateUpdate()
    {
        if (!player || !enemy || !canvas || !container) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            container,
            RectTransformUtility.WorldToScreenPoint(uiCam, player.position),
            uiCam,
            out var playerUIPos);

        float dx = enemy.position.x - player.position.x;
        float targetY;
        if (dx > sideDeadZone) targetY = yOffsetRight; 
        else if (dx < -sideDeadZone) targetY = yOffsetLeft;  
        else targetY = yOffsetCurrent; 

        yOffsetCurrent = Mathf.Lerp(yOffsetCurrent, targetY, Time.deltaTime * yLerp);

        pivot.anchoredPosition = new Vector2(playerUIPos.x + uiOffset.x, playerUIPos.y + yOffsetCurrent);

        Vector2 dir = (Vector2)(enemy.position - player.position);
        if (dir.sqrMagnitude < 1e-6f) return;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        pivot.localRotation = Quaternion.Euler(0, 0, angle + angleOffset);
    }
}
