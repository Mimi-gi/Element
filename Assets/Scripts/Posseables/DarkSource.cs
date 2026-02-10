using UnityEngine;

public class DarkSource : MonoBehaviour
{
    [Header("Area Settings")]
    [Tooltip("このDarkSourceのエリアID")]
    public string AreaId = "default";

    /// <summary>
    /// リスポーン位置（このオブジェクトの位置）
    /// </summary>
    public Vector3 SpawnPosition => transform.position;

    private void OnDrawGizmos()
    {
        // エディタで視覚化
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 1f);
    }
}