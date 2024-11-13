using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleBrush : TerrainBrush {
    public float height = 5f;
    public bool addHeightOnHold = true;
    public float decayRate = 0.1f;
    private float clickDuration = 0f;

    public override void draw(int x, int z) {
        if (addHeightOnHold) {
            clickDuration += Time.deltaTime; // Increase based on time
        }

        for (int zi = -radius; zi <= radius; zi++) {
            for (int xi = -radius; xi <= radius; xi++) {
                if (xi * xi + zi * zi <= radius * radius) {
                    float currentHeight = terrain.get(x + xi, z + zi);
                    if (addHeightOnHold) {
                        // Apply the diminishing height effect with each click duration
                        float adjustedHeight = height / (1 + decayRate * clickDuration);
                        terrain.set(x + xi, z + zi, currentHeight + adjustedHeight);
                    } else {
                        terrain.set(x + xi, z + zi, height);
                    }
                }
            }
        }
    }
}
