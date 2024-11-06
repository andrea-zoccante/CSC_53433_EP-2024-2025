using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseBrush : TerrainBrush {

    public float height = 5;

    private float smoothstep(float w) {
        if (w <= 0.0) return 0.0;
        if (w >= 1.0) return 1.0;
        
        return w * w * (3.0 - 2.0 * w);
    }

    private float interpolate(float a0, float a1, float w) {
        return a0 + (a1 - a0) * smoothstep(w);
    }

    private float dotGridGradient(int ix, int iy, float x, float y) {
 
        // Precomputed (or otherwise) gradient vectors at each grid node
        extern float Gradient[IYMAX][IXMAX][2];
    
        // Compute the distance vector
        float dx = x - (float) ix;
        float dy = y - (float) iy;
    
        // Compute the dot-product
        return (dx*Gradient[iy][ix][0] + dy*Gradient[iy][ix][1]);
    }

    public override void draw(int x, int z) {
        for (int zi = -radius; zi <= radius; zi++) {
            for (int xi = -radius; xi <= radius; xi++) {
                int x0 = floor(x);
                int x1 = x + xi;
                int z0 = floor(z);
                int z1 = z0 + 1;
            
                // Determine interpolation weights
                // Could also use higher order polynomial/s-curve here
                float sx = x - (float)x0;
                float sz = z - (float)z0;
            
                // Interpolate between grid point gradients
                float n0, n1, ix0, ix1, value;
                n0 = dotGridGradient(x0, z0, x, z);
                n1 = dotGridGradient(x1, z0, x, z);
                ix0 = interpolate(n0, n1, sx);
                n0 = dotGridGradient(x0, z1, x, z);
                n1 = dotGridGradient(x1, z1, x, z);
                ix1 = interpolate(n0, n1, sx);
                value = interpolate(ix0, ix1, sz);

                terrain.set(x + xi, z + zi, value);
            }
        }
    }
}
