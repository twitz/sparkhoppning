using System;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField]
    private ZoneData[] zones;

    [SerializeField]
    private float startOffset;

    [SerializeField]
    private float endOffset;

    [Serializable]
    public struct ZoneData
    {
        [SerializeField]
        public GameObject[] potentialObstacles;

        [SerializeField]
        public int minObstacles;

        [SerializeField]
        public int maxObstacles;
    }
    
    [SerializeField]
    private Collider bounds;

    [SerializeField]
    private Transform slope;
    
    private Bounds _bounds;

    private void Awake()
    {
        _bounds = bounds.bounds;
    }

    private void Start()
    {
        GenerateObstacles();
    }

    private void Update()
    {
       // GenerateObstacles();
    }

    private void GenerateObstacles()
    {
        ClearObstacles();

        var zoneCount = zones.Length;
        var size = slope.localScale;
        var zoneLength = size.z / zoneCount;
        var zoneWidth = size.x / 2;

        var startPos = new Vector3(0, _bounds.max.y, _bounds.min.z);
        var endPos = new Vector3(0, _bounds.min.y, _bounds.max.z);
        for (var i = 0; i < zoneCount; i++)
        {
            var fractionOffset = zoneLength / (size.z * 2);
            var fraction = zoneLength * i / size.z + fractionOffset;
            var zoneOrigin = Vector3.Lerp(startPos, endPos, fraction);

            var zoneData = zones[i];
            var obstacleCount = Random.Range(zoneData.minObstacles, zoneData.maxObstacles);
            for (var j = 0; j < obstacleCount; j++)
            {
                var randomWithinZone = zoneLength * i / size.z +
                                           Random.Range(fractionOffset, fractionOffset * 2);
                var spawnOrigin = Vector3.Lerp(startPos, endPos, randomWithinZone);
                var spawnAt = new Vector3(Random.Range(-zoneWidth, zoneWidth), spawnOrigin.y, spawnOrigin.z);
                var obstacle = Random.Range(0, zoneData.potentialObstacles.Length);
                var clone = Instantiate(zoneData.potentialObstacles[obstacle], spawnAt, slope.rotation);
                clone.transform.parent = transform;
            }
        }
    }

    private void ClearObstacles()
    {
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        var zoneCount = zones.Length;
        var size = slope.localScale;
        var zoneLength = size.z / zoneCount;
        var zoneWidth = size.x / 2;
        var pointSize = new Vector3(5, 10, 5);

        var startPos = new Vector3(0, _bounds.max.y, _bounds.min.z);
        var endPos = new Vector3(0, _bounds.min.y, _bounds.max.z);
        for (var i = 0; i < zoneCount; i++)
        {
            var fraction = zoneLength * i / size.z + zoneLength / (size.z * 2);
            var position = Vector3.Lerp(startPos, endPos, fraction);
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawCube(position, pointSize);
        }
    }
}
