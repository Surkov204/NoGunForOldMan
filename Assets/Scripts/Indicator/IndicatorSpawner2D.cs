using System.Collections.Generic;
using UnityEngine;

public class IndicatorSpawner2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private IndicatorManager2D manager;
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform container;     // parent UI để chứa tất cả indicator
    [SerializeField] private RectTransform indicatorPrefab; // prefab pivot (parent)

    [Header("UI Tuning")]
    [SerializeField] private Transform player;
    [SerializeField] private float uiRadius = 80f;        // px
    [SerializeField] private float angleOffset = 0f;      // 90 nếu sprite mặc định chỉ lên

    private readonly Dictionary<Transform, IndicatorWidget2D> map = new();

    private void OnEnable()
    {
        manager.OnEnemyEnter += HandleEnter;
        manager.OnEnemyExit += HandleExit;
    }
    private void OnDisable()
    {
        manager.OnEnemyEnter -= HandleEnter;
        manager.OnEnemyExit -= HandleExit;
    }

    private void HandleEnter(Transform enemy)
    {
        if (!enemy || map.ContainsKey(enemy)) return;

        var rt = Instantiate(indicatorPrefab, container);
        var w = rt.GetComponent<IndicatorWidget2D>();
        if (!w) w = rt.gameObject.AddComponent<IndicatorWidget2D>();

        w.Init(canvas, container, player, enemy, uiRadius, angleOffset);
        map[enemy] = w;
    }

    private void HandleExit(Transform enemy)
    {
        if (map.TryGetValue(enemy, out var w) && w)
            Destroy(w.gameObject);
        map.Remove(enemy);
    }
}
