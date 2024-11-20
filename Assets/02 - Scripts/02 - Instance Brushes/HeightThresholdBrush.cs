using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightThresholdBrush : InstanceBrush {

    public float heightThresh = 10;

    public override void draw(float x, float z) {
        Vector3 newPosition = new Vector3(x, 0, z);

        if (isPositionValid(newPosition)) {
            spawnObject(x, z);
        }
    }

    private bool isPositionValid(Vector3 position) {
        float terrainHeight = terrain.getInterp(position.x, position.z);
        return terrainHeight <= heightThresh;
    }
}
