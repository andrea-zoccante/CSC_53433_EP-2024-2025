using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Animal : MonoBehaviour
{

    [Header("Animal parameters")]
    public float swapRate = 0.01f;
    public float mutateRate = 0.01f;
    public float swapStrength = 10.0f;
    public float mutateStrength = 0.5f;
    public float maxAngle = 50.0f;
    public float lifeSuccess = 0.0f;
    public int timeAlive = 0;
    public int pubertyAge = 10;

    [Header("Energy parameters")]
    public float maxEnergy = 10.0f;
    public float lossEnergy = 0.1f;
    public float gainEnergy = 10.0f;
    public float reprodEnergy = 2.0f;
    private float energy;

    [Header("Sensor - Vision")]
    public float maxVision = 20.0f;
    public float stepAngle = 10.0f;
    public int nEyes = 5;

    [Header("Sensor - Smell")]
    public float maxSmellRange = 5.0f;
    public float smellAngle = 360.0f;
    public int nNose = 20;

    [Header("Reproduction")]
    public float failCost = 2.0f;
    public float successCost = 1.0f;
    public float successThresh = 5.0f;
    public int gender = 0;

    private int[] networkStruct;
    private SimpleNeuralNet brain = null;

    private int[] networkReprodStruct;
    private SimpleNeuralNet brainReprod = null;

    // Terrain.
    private CustomTerrain terrain = null;
    private int[,] details = null;
    private Vector2 detailSize;
    private Vector2 terrainSize;

    // Animal.
    private Transform tfm;
    private float[] vision;
    private float[] visionSteepness;
    private float[] smell;

    // Genetic alg.
    private GeneticAlgo genetic_algo = null;

    // Renderer.
    private Material mat = null;

    void Start()
    {
        // Network: 1 input per receptor, 1 output per actuator.
        vision = new float[nEyes];
        visionSteepness = new float[nEyes];
        smell = new float[nNose];
        networkStruct = new int[] { nEyes + nEyes + nNose, 10, 5, 2 };
        networkReprodStruct = new int[] { 2, 5, 1 };
        energy = maxEnergy;
        tfm = transform;

        // Renderer used to update animal color.
        // It needs to be updated for more complex models.
        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
        if (renderer != null)
            mat = renderer.material;
    }

    void Update() {
        timeAlive += 1;

        // Ensure necessary components are initialized
        if (brain == null) brain = new SimpleNeuralNet(networkStruct);
        if (brainReprod == null) brainReprod = new SimpleNeuralNet(networkReprodStruct);
        if (terrain == null) return;
        if (details == null) {
            UpdateSetup();
            return;
        }

        // Retrieve animal location in the heightmap
        int dx = (int)((tfm.position.x / terrainSize.x) * detailSize.x);
        int dy = (int)((tfm.position.z / terrainSize.y) * detailSize.y);

        // Energy loss and eating logic
        energy -= lossEnergy;
        if ((dx >= 0) && dx < details.GetLength(1) && (dy >= 0) && dy < details.GetLength(0) && details[dy, dx] > 0) {
            details[dy, dx] = 0;
            energy += gainEnergy;
            if (energy > maxEnergy) energy = maxEnergy;
            genetic_algo.addOffspring(this);
        }

        // If energy is below zero, the animal dies
        if (energy < 0) {
            energy = 0.0f;
            genetic_algo.removeAnimal(this);
            return;
        }

        // Check terrain steepness at the current position
        float steepness = terrain.getSteepness(tfm.position.x, tfm.position.z);
        float steepnessThreshold = maxAngle; // Define a threshold for tolerable steepness

        // Reduce health proportionally to how much steepness exceeds the threshold
        if (steepness > steepnessThreshold) {
            float excessSteepness = steepness - steepnessThreshold;
            float healthLoss = excessSteepness * 0.1f; // Scale factor for health loss
            energy -= healthLoss;

            // If energy drops below zero due to steepness, the animal dies
            if (energy <= 0) {
                energy = 0.0f;
                genetic_algo.removeAnimal(this);
                return;
            }
        }

        // Update color based on energy
        if (mat != null) mat.color = Color.white * (energy / maxEnergy);

        // Update vision and smell receptors
        UpdateVision();
        UpdateSmell();

        // Neural network output for movement
        float[] output = brain.getOutput(vision.Concat(visionSteepness).Concat(smell).ToArray());
        float angle = (output[0] * 2.0f - 1.0f) * maxAngle;

        // Rotate and move forward
        tfm.Rotate(0.0f, angle, 0.0f);
        tfm.Translate(tfm.forward * Time.deltaTime);

        lifeSuccess = GetHealth() * timeAlive;
    }

    /// <summary>
    /// Calculate distance to the nearest food resource, if there is any.
    /// </summary>
    private void UpdateVision() {
        float startingAngle = -((float)nEyes / 2.0f) * stepAngle;
        Vector2 ratio = detailSize / terrainSize;

        // Initialize vision and visionSteepness arrays
        for (int i = 0; i < nEyes; i++) {
            vision[i] = 1.0f; // Default to maximum vision range
            visionSteepness[i] = 0.0f; // Default to no steepness
        }

        for (int i = 0; i < nEyes; i++) {
            Quaternion rotAnimal = tfm.rotation * Quaternion.Euler(0.0f, startingAngle + (stepAngle * i), 0.0f);
            Vector3 forwardAnimal = rotAnimal * Vector3.forward;
            float sx = tfm.position.x * ratio.x;
            float sy = tfm.position.z * ratio.y;

            float totalSteepness = 0.0f;
            int steepnessCount = 0;

            // Iterate over vision length
            for (float distance = 1.0f; distance < maxVision; distance += 0.5f) {
                // Position where we are looking at
                float px = sx + (distance * forwardAnimal.x * ratio.x);
                float py = sy + (distance * forwardAnimal.z * ratio.y);

                // Wrap around terrain edges
                if (px < 0) px += detailSize.x;
                else if (px >= detailSize.x) px -= detailSize.x;
                if (py < 0) py += detailSize.y;
                else if (py >= detailSize.y) py -= detailSize.y;

                // Check if within terrain bounds
                if ((int)px >= 0 && (int)px < details.GetLength(1) && (int)py >= 0 && (int)py < details.GetLength(0)) {
                    // Record distance to nearest food
                    if (details[(int)py, (int)px] > 0) {
                        vision[i] = distance / maxVision;
                        break;
                    }

                    // Calculate interpolated steepness at this point
                    float worldX = px / ratio.x;
                    float worldZ = py / ratio.y;
                    float currentSteepness = terrain.getSteepness(worldX, worldZ);

                    totalSteepness += currentSteepness + 0.01f;
                    steepnessCount++;
                }
            }

            // Calculate average steepness for this eye
            if (steepnessCount > 0) {
                visionSteepness[i] = totalSteepness / steepnessCount / maxAngle; // Normalize by maxAngle
            }

            // Draw the vision ray for this eye
            Debug.DrawRay(tfm.position, forwardAnimal * maxVision, Color.green);
        }
    }

    private void UpdateSmell()
    {
        float startingAngle = -((float)nNose / 2.0f) * smellAngle;
        Vector2 ratio = detailSize / terrainSize;

        for (int i = 0; i < nNose; i++)
        {
            Quaternion rotAnimal = tfm.rotation * Quaternion.Euler(0.0f, startingAngle + (smellAngle * i), 0.0f);
            Vector3 forwardAnimal = rotAnimal * Vector3.forward;
            float sx = tfm.position.x * ratio.x;
            float sy = tfm.position.z * ratio.y;
            smell[i] = 1.0f;

            Vector3 rayDirection = forwardAnimal * maxSmellRange;

            // Interate over vision length.
            for (float distance = 1.0f; distance < maxSmellRange; distance += 0.5f)
            {
                // Position where we are looking at.
                float px = (sx + (distance * forwardAnimal.x * ratio.x));
                float py = (sy + (distance * forwardAnimal.z * ratio.y));

                if (px < 0)
                    px += detailSize.x;
                else if (px >= detailSize.x)
                    px -= detailSize.x;
                if (py < 0)
                    py += detailSize.y;
                else if (py >= detailSize.y)
                    py -= detailSize.y;

                if ((int)px >= 0 && (int)px < details.GetLength(1) && (int)py >= 0 && (int)py < details.GetLength(0) && details[(int)py, (int)px] > 0)
                {
                    smell[i] = distance / maxSmellRange;
                    break;
                }
            }

            // Draw the vision ray for this eye
            Debug.DrawRay(tfm.position, rayDirection, Color.red);
        }

    }

    public void Setup(CustomTerrain ct, GeneticAlgo ga, int inGender)
    {
        terrain = ct;
        genetic_algo = ga;
        gender = inGender;
        UpdateSetup();
    }

    private void UpdateSetup()
    {
        detailSize = terrain.detailSize();
        Vector3 gsz = terrain.terrainSize();
        terrainSize = new Vector2(gsz.x, gsz.z);
        details = terrain.getDetails();
    }

    public void InheritBrain(SimpleNeuralNet other, SimpleNeuralNet reprod, bool mutate)
    {
        brain = new SimpleNeuralNet(other);
        brainReprod = new SimpleNeuralNet(reprod);
        if (mutate)
            brain.mutate(swapRate, mutateRate, swapStrength, mutateStrength);
            reprod.mutate(swapRate, mutateRate, swapStrength, mutateStrength);
    }
    public SimpleNeuralNet GetBrain()
    {
        return brain;
    }

    public SimpleNeuralNet GetBrainReprod()
    {
        return brainReprod;
    }
    public float GetHealth()
    {
        return energy / maxEnergy;
    }

    public bool ShouldReproduceWith(float life_success) {
        if (timeAlive < pubertyAge) { return false; }

        // Concatenate the animal's energy with the life_success argument.
        float[] input = new float[] { energy / maxEnergy, life_success };

        float[] output = brainReprod.getOutput(input);
        float sigmoidOutput = 1.0f / (1.0f + Mathf.Exp(-output[0]));

        return sigmoidOutput > 0.5f;
    }

    public void ReduceEnergy(float energyLoss) {
        energy -= energyLoss;
    }
}
