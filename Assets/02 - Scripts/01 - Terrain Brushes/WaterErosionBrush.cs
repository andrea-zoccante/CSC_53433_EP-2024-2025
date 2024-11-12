using UnityEngine;

public class WaterErosionBrush : TerrainBrush {
    public int erosionSteps = 1;
    public float initialWaterAmount = 1.0f;
    public float sedimentCapacity = 0.1f;
    public float erosionRate = 0.05f;
    public float depositionRate = 0.02f;
    public float minSlope = 0.01f;

    public override void draw(int x, int z) {
        for (int zi = -radius; zi <= radius; zi++) {
            for (int xi = -radius; xi <= radius; xi++) {
                int pos_x = x + xi;
                int pos_z = z + zi;

                // Perform N erosion steps for each point in the radius
                for (int i = 0; i < erosionSteps; i++) {
                    PerformErosionStep(pos_x, pos_z);
                }
            }
        }
    }

    private void PerformErosionStep(int x, int z) {
        float waterAmount = initialWaterAmount;
        float sediment = 0f;

        // Particle position and height
        int pos_x = x;
        int pos_z = z;

        while (waterAmount > 0) {
            // Get current height and slope of the surrounding area
            float currentHeight = terrain.get(pos_x, pos_z);
            Vector2Int nextPos = FindLowestNeighbor(pos_x, pos_z, currentHeight, out float nextHeight);

            float slope = currentHeight - nextHeight;
            if (slope < minSlope) {
                // Not enough slope to carry sediment further
                break;
            }

            // Erode the current position based on slope and erosion rate
            float erodedAmount = Mathf.Min(erosionRate * slope, currentHeight);
            terrain.set(pos_x, pos_z, currentHeight - erodedAmount);

            // Carry sediment with the particle
            sediment += erodedAmount;

            // Deposit sediment if sediment capacity is exceeded
            if (sediment > sedimentCapacity) {
                float depositAmount = (sediment - sedimentCapacity) * depositionRate;
                terrain.set(pos_x, pos_z, terrain.get(pos_x, pos_z) + depositAmount);
                sediment -= depositAmount;
            }

            // Move the particle to the next lower point
            pos_x = nextPos.x;
            pos_z = nextPos.y;
            waterAmount *= 0.9f; // Evaporate the water slowly
        }
    }

    private Vector2Int FindLowestNeighbor(int x, int z, float currentHeight, out float lowestHeight) {
        Vector2Int lowestNeighbor = new Vector2Int(x, z);
        lowestHeight = currentHeight;

        // Check all 8 neighbors for the lowest height
        for (int zi = -1; zi <= 1; zi++) {
            for (int xi = -1; xi <= 1; xi++) {
                if (xi == 0 && zi == 0) continue;

                int nx = x + xi;
                int nz = z + zi;

                float neighborHeight = terrain.get(nx, nz);
                if (neighborHeight < lowestHeight) {
                    lowestHeight = neighborHeight;
                    lowestNeighbor = new Vector2Int(nx, nz);
                }
            }
        }

        return lowestNeighbor;
    }
}
