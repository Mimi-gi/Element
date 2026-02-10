using UnityEngine;
using System.Collections.Generic;
using Element.Interfaces;

namespace Element
{
    /// <summary>
    /// フォーカスモード中の乗り移り候補検出用コライダー
    /// CircleCollider2D(isTrigger) + Rigidbody2D(Kinematic) をアタッチすること
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class FocusCircle : MonoBehaviour
    {
        private readonly HashSet<IPossable> _candidates = new();

        /// <summary>
        /// 現在の候補一覧
        /// </summary>
        public IReadOnlyCollection<IPossable> Candidates => _candidates;

        private void OnTriggerEnter2D(Collider2D other)
        {
            var possable = other.GetComponent<IPossable>();
            if (possable != null)
            {
                Debug.Log($"FocusCircle: OnTriggerEnter2D{other.name}");
                _candidates.Add(possable);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var possable = other.GetComponent<IPossable>();
            if (possable != null)
            {
                _candidates.Remove(possable);
            }
        }

        /// <summary>
        /// 指定位置に移動して有効化
        /// </summary>
        public void Activate(Vector3 position)
        {
            _candidates.Clear();
            transform.position = position;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 無効化して候補をクリア
        /// </summary>
        public void Deactivate()
        {
            gameObject.SetActive(false);
            _candidates.Clear();
        }

        /// <summary>
        /// 候補の中から指定のIPossableに最も近いものを返す
        /// </summary>
        public IPossable GetNearest(IPossable current)
        {
            if (current?.Core == null) return null;
            Debug.Log("Core is OK");
            IPossable nearest = null;
            float minDistance = float.MaxValue;

            foreach (var candidate in _candidates)
            {
                if (candidate == current) continue;

                float distance = Vector3.Distance(current.Core.position, candidate.Core.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = candidate;
                }
            }

            return nearest;
        }
    }
}
