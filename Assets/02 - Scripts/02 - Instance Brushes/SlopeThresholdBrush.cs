using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlopeThresholdBrush : InstanceBrush {

    public float slopeThresh = 10;

    public override void draw(float x, float z) {
        Vector3 newPosition = new Vector3(x, 0, z);

        if (isPositionValid(newPosition)) {
            spawnObject(x, z);
        }
    }


    private bool isPositionValid(Vector3 position) {
        float slope = terrain.getSteepness(position.x, position.z);
        return slope <= slopeThresh;
    }
}
