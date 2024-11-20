using UnityEngine;

public class PerlinNoiseBrush : TerrainBrush {
    public float height = 10f; // Maximum height offset
    public float scale = 0.1f; // Controls the frequency of the noise

    public override void draw(int x, int z) {
        for (int zi = -radius; zi <= radius; zi++) {
            for (int xi = -radius; xi <= radius; xi++) {
                int pos_x = x + xi;
                int pos_z = z + zi;

                float distance = Mathf.Sqrt(xi * xi + zi * zi);
                float maxDistance = radius;
                float falloff = Mathf.Pow(Mathf.Clamp01(1 - (distance / maxDistance)), 2);

                float sample = Mathf.PerlinNoise((pos_x + 1000) * scale, (pos_z + 1000) * scale);
                float newHeight = sample * height * falloff;

                float currentHeight = terrain.getInterp(pos_x, pos_z);
                terrain.set(pos_x, pos_z, currentHeight + newHeight);
            }
        }
    }
}
