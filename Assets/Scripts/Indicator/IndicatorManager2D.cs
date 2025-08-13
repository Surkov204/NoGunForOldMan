using System.Collections.Generic;
using UnityEngine;
using System;

public class IndicatorManager2D : MonoBehaviour
{
    [Header("Detect")]
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float trackRadius = 30f;
    [SerializeField] private float scanInterval = 0.2f;

    public event Action<Transform> OnEnemyEnter;
    public event Action<Transform> OnEnemyExit;

    private readonly HashSet<Transform> inside = new();
    private readonly Collider2D[] buf = new Collider2D[128];
    private float t;

    private void Update()
    {
        if (!player) return;
        t += Time.deltaTime;
        if (t < scanInterval) return;
        t = 0f;

        int n = Physics2D.OverlapCircleNonAlloc(player.position, trackRadius, buf, enemyLayer);

        var now = new HashSet<Transform>();
        for (int i = 0; i < n; i++)
        {
            var tr = buf[i].transform;
            if (!tr) continue;
            now.Add(tr);
            if (!inside.Contains(tr))
                OnEnemyEnter?.Invoke(tr);
        }

        foreach (var tr in inside)
            if (tr == null || !now.Contains(tr))
                OnEnemyExit?.Invoke(tr);

        inside.Clear();
        foreach (var tr in now) inside.Add(tr);
    }

    private void OnDrawGizmosSelected()
    {
        if (!player) return;
        Gizmos.color = new Color(1f, .6f, .2f, .6f);
        Gizmos.DrawWireSphere(player.position, trackRadius);
    }
}
