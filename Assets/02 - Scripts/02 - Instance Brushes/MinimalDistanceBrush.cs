using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimalDistanceBrush : InstanceBrush {

    public float minDistance = 2.0f;
    private List<Vector3> placedPositions = new List<Vector3>();

    public override void draw(float x, float z) {
        Vector3 newPosition = new Vector3(x, 0, z);

        if (isPositionValid(newPosition)) {
            spawnObject(x, z);
            placedPositions.Add(newPosition);
        }
    }

    private bool isPositionValid(Vector3 position) {
        foreach (Vector3 placedPosition in placedPositions) {
            if (Vector3.Distance(position, placedPosition) < minDistance) {
                return false;
            }
        }
        return true;
    }
}
