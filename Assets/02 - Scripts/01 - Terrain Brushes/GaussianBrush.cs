using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GaussianBrush : TerrainBrush {
    public float scale = 3f;
    public float amplitude = 10f;
    public bool addHeightOnHold = true;
    public float decayRate = 0.1f;
    private float clickDuration = 0f;

    public override void draw(int x, int z) {
        decimal dec = new decimal(scale);
        double sigma = (double)dec;
        
        if (addHeightOnHold) {
            clickDuration += Time.deltaTime;
        }

        for (int zi = -radius; zi <= radius; zi++) {
            for (int xi = -radius; xi <= radius; xi++) {
                double normal = 1 / (2 * Math.PI * Math.Pow(sigma, 2.0));
                double exponential = Math.Exp(-0.5 * (Math.Pow((xi / sigma), 2.0) + Math.Pow((zi / sigma), 2.0)));

                float currentHeight = terrain.get(x + xi, z + zi);
                float height = (float)(normal * exponential * amplitude);

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
