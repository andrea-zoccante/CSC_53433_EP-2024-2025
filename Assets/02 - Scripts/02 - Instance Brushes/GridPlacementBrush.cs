using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPlacementBrush : InstanceBrush {

    public float gridSpacing = 1.0f;
    public float rotationAngle = 0.0f;
    public int gridSize = 5;

    public override void draw(float x, float z) {
        float angleRad = Mathf.Deg2Rad * rotationAngle;

        for (int i = 0; i < gridSize; i++) {
            for (int j = 0; j < gridSize; j++) {
                float offsetX = (i - gridSize / 2) * gridSpacing;
                float offsetZ = (j - gridSize / 2) * gridSpacing;

                float rotatedX = Mathf.Cos(angleRad) * offsetX - Mathf.Sin(angleRad) * offsetZ;
                float rotatedZ = Mathf.Sin(angleRad) * offsetX + Mathf.Cos(angleRad) * offsetZ;

                spawnObject(x + rotatedX, z + rotatedZ);
            }
        }
    }
}
