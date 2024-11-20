using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleInstanceBrush : InstanceBrush {

    public override void draw(float x, float z) {
        int numInstances = 5;

        for (int i = 0; i < numInstances; i++) {
            float angle = Random.Range(0, 2 * Mathf.PI); 
            float distance = Random.Range(0, radius);

            float offsetX = Mathf.Cos(angle) * distance;
            float offsetZ = Mathf.Sin(angle) * distance;

            spawnObject(x + offsetX, z + offsetZ);
        }
    }
}