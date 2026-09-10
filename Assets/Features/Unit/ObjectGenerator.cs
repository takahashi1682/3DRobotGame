using UnityEngine;
using Random = UnityEngine.Random;

namespace Features.Unit
{
    /// <summary>
    /// オブジェクトを一定間隔で生成する機能
    /// </summary>
    public class ObjectGenerator : MonoBehaviour
    {
        [Header("References")]
        public GameObject SpawnPrefab;

        [Header("Settings")]
        public bool IsSpawning = true;
        public float SpawnInterval = 1f;

        [Header("Debug")]
        public bool IsDrawGizmos;
        public Color GizmosColor = Color.yellow;

        private float _spawnTime;
        private Transform[] _spawnPoints;

        private void Awake()
        {
            CollectSpawnPoints();
        }

        private void Update()
        {
            if (!IsSpawning) return;

            // 現在時間 - 前回生成した時間 >= 生成インターバル　なら生成する
            if (Time.time - _spawnTime >= SpawnInterval)
            {
                _spawnTime = Time.time;
                RandomSpawn();
            }
        }

        private void CollectSpawnPoints()
        {
            // 子オブジェクトのTransformを取得
            _spawnPoints = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
                _spawnPoints[i] = transform.GetChild(i);
        }

        private void RandomSpawn()
        {
            if (_spawnPoints.Length == 0) return;

            var randomIndex = Random.Range(0, _spawnPoints.Length);
            var spawnPoint = _spawnPoints[randomIndex];

            Instantiate(SpawnPrefab, spawnPoint.position, spawnPoint.rotation);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // デバック用のエリア表示
            if (!IsDrawGizmos) return;

            CollectSpawnPoints();

            Gizmos.color = GizmosColor;
            foreach (var point in _spawnPoints)
                Gizmos.DrawSphere(point.position, 0.2f);
        }
#endif
    }
}