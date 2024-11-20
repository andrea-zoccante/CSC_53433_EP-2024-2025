using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClusterPlacementBrush : InstanceBrush {

    public int numClusters = 3;
    public int objectsPerCluster = 5;
    public float clusterRadius = 2.0f;

    public override void draw(float x, float z) {
        for (int i = 0; i < numClusters; i++) {
            float clusterCenterX = x + Random.Range(-radius, radius);
            float clusterCenterZ = z + Random.Range(-radius, radius);

            for (int j = 0; j < objectsPerCluster; j++) {
                float offsetX = Random.Range(-clusterRadius, clusterRadius);
                float offsetZ = Random.Range(-clusterRadius, clusterRadius);

                spawnObject(clusterCenterX + offsetX, clusterCenterZ + offsetZ);
            }
        }
    }
}
