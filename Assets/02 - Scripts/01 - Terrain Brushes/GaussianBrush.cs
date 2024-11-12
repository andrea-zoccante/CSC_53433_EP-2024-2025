using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GaussianBrush : TerrainBrush {

    public float scale = 100;
    public float amplitude = 100;

    public override void draw(int x, int z) {
        decimal dec = new decimal(scale);
        double sigma   = (double)dec;
        for (int zi = -radius; zi <= radius; zi++) {
            for (int xi = -radius; xi <= radius; xi++) {
                double normal = 1 / (2 * Math.PI * Math.Pow(sigma, 2.0));
                double exponential = Math.Exp(-0.5 * (Math.Pow((xi/sigma), 2.0) + Math.Pow((zi/sigma), 2.0)));
                terrain.set(x + xi, z + zi, (float)(normal*exponential*amplitude));
            }
        }
    }
}
