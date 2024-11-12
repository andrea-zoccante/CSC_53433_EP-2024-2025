using UnityEngine;

public class PerlinNoiseBrush : TerrainBrush {
    public float height = 10f;
    public float scale = 0.1f;  // Controls the frequency of the noise

    public override void draw(int x, int z) {
        for (int zi = -radius; zi <= radius; zi++) {
            for (int xi = -radius; xi <= radius; xi++) {
                int pos_x = x + xi;
                int pos_z = z + zi;

                // Use world-space coordinates scaled down for Perlin noise
                float sample = Mathf.PerlinNoise((pos_x + 1000) * scale, (pos_z + 1000) * scale);
                
                // Apply the Perlin noise sample to the height
                terrain.set(pos_x, pos_z, sample * height);
            }
        }
    }
}
