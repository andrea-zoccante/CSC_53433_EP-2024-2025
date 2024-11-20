using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InstanceBrush : Brush {

    private int prefab_idx;

    private Dictionary<Vector3, float> recentlyDrawnAreas = new Dictionary<Vector3, float>();
    public float cooldownDuration = 1.0f;
    public bool enableCooldown = true;

    public override void callDraw(float x, float z) {
        if (terrain.object_prefab)
            prefab_idx = terrain.registerPrefab(terrain.object_prefab);
        else {
            prefab_idx = -1;
            terrain.debug.text = "No prefab to instantiate";
            return;
        }
        Vector3 grid = terrain.world2grid(x, z);

        if (!enableCooldown || !isInCooldown(grid)) {
            draw(grid.x, grid.z);
            updateCooldown(grid);
        }
    }

    public override void draw(int x, int z) {
        draw((float)x, (float)z);
    }

    public void spawnObject(float x, float z) {
        if (prefab_idx == -1) {
            return;
        }
        float scale_diff = Mathf.Abs(terrain.max_scale - terrain.min_scale);
        float scale_min = Mathf.Min(terrain.max_scale, terrain.min_scale);
        float scale = (float)CustomTerrain.rnd.NextDouble() * scale_diff + scale_min;
        terrain.spawnObject(terrain.getInterp3(x, z), scale, prefab_idx);
    }

    private bool isInCooldown(Vector3 grid) {
        // Check if the area has been recently drawn on
        if (recentlyDrawnAreas.TryGetValue(grid, out float lastDrawTime)) {
            return Time.time - lastDrawTime < cooldownDuration;
        }
        return false;
    }

    private void updateCooldown(Vector3 grid) {
        // Update the last draw time for the area
        recentlyDrawnAreas[grid] = Time.time;
    }
}
