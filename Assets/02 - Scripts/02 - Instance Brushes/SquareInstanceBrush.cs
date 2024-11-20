using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareInstanceBrush : InstanceBrush {
    public int numInstances = 5; 

    public override void draw(float x, float z) {
        int numInstances = 5;

        for (int i = 0; i < numInstances; i++) {
            float offsetX = Random.Range(-radius, radius);
            float offsetZ = Random.Range(-radius, radius);

            spawnObject(x + offsetX, z + offsetZ);
        }
    }
}
