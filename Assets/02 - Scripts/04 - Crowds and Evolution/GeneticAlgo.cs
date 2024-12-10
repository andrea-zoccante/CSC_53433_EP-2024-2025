using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GeneticAlgo : MonoBehaviour
{
    [Header("Genetic Algorithm parameters")]
    public int popSize = 100;
    public GameObject animalPrefab;

    private float meanLifeSuccess = 0f;
    private float stdLifeSuccess = 0.001f;

    [Header("Dynamic elements")]
    public float vegetationGrowthRate = 1.0f;
    public float currentGrowth;
    public float reprodThresholdDist = 20.0f;

    private List<GameObject> animals;
    protected Terrain terrain;
    protected CustomTerrain customTerrain;
    protected float width;
    protected float height;

    private string csvFilePath;

    private int countMatingAccept = 0;
    private int countMatingDecline = 0;

    void Start()
    {
        // Retrieve terrain.
        terrain = Terrain.activeTerrain;
        customTerrain = GetComponent<CustomTerrain>();
        width = terrain.terrainData.size.x;
        height = terrain.terrainData.size.z;

        // Initialize terrain growth.
        currentGrowth = 0.0f;

        // Initialize animals array.
        animals = new List<GameObject>();
        for (int i = 0; i < popSize; i++)
        {
            GameObject animal = makeAnimal();
            animals.Add(animal);
        }

        // Set up the CSV file path.
        csvFilePath = Path.Combine(Application.dataPath, "AnimalResourceData.csv");

        // Write the header to the CSV file.
        File.WriteAllText(csvFilePath, "Timestamp,NAnimals,NResources,NFemales,NMales,MeanLifeSuccess,MeanAge,%MatingAccept\n");
        SaveToCSV();
    }

    void Update()
    {
        // Keeps animal to a minimum.
        while (animals.Count < popSize / 4)
        {
            animals.Add(makeAnimal());
            SaveToCSV();
        }
        // customTerrain.debug.text = "Nº animals: " + animals.Count.ToString();

        // Update grass elements/food resources.
        UpdateResources();
        UpdateReproduction();
    }

    /// <summary>
    /// Method to place grass or other resource in the terrain.
    /// </summary>
    public void UpdateResources() {
        Vector2 detail_sz = customTerrain.detailSize();
        int[,] details = customTerrain.getDetails();
        currentGrowth += vegetationGrowthRate;

        while (currentGrowth > 1.0f) {
            int x = (int)(UnityEngine.Random.value * detail_sz.x);
            int y = (int)(UnityEngine.Random.value * detail_sz.y);

            // Convert detail coordinates to world coordinates
            float worldX = x / detail_sz.x * terrain.terrainData.size.x;
            float worldZ = y / detail_sz.y * terrain.terrainData.size.z;

            // Get the height and steepness at this position
            float height = customTerrain.getInterp(worldX, worldZ);
            float steepness = customTerrain.getSteepness(worldX, worldZ);

            // Only place a resource if the height is between 25 and 35 and steepness is less than 30 degrees
            if (height >= 25.0f && height <= 35.0f && steepness < 30.0f) {
                details[y, x] = 1; // Place resource
                currentGrowth -= 1.0f;
            }
        }

        customTerrain.saveDetails();
        SaveToCSV();
    }

    public void UpdateReproduction()
    {
        foreach (var maleObj in animals)
        {
            Animal maleAnimal = maleObj.GetComponent<Animal>();

            // Check if this animal is male
            if (maleAnimal.gender != 1)
                continue;

            Vector3 malePosition = maleObj.transform.position;
            GameObject closestFemale = null;
            float closestDistance = float.MaxValue;

            // Find the closest female
            foreach (var femaleObj in animals)
            {
                Animal femaleAnimal = femaleObj.GetComponent<Animal>();

                // Check if this animal is female
                if (femaleAnimal.gender != 0)
                    continue;

                float distance = Vector3.Distance(malePosition, femaleObj.transform.position);

                if (distance < closestDistance && distance < reprodThresholdDist)
                {
                    closestFemale = femaleObj;
                    closestDistance = distance;
                }
            }

            // If a close female is found, check reproduction criteria
            if (closestFemale != null)
            {
                Animal femaleAnimal = closestFemale.GetComponent<Animal>();

                if (maleAnimal.ShouldReproduceWith(femaleAnimal.lifeSuccess) &&
                    femaleAnimal.ShouldReproduceWith(maleAnimal.lifeSuccess))
                {
                    bool inheritFromMale = UnityEngine.Random.value < 0.5f;

                    if (inheritFromMale)
                    {
                        addOffspring(maleAnimal);
                    }
                    else
                    {
                        addOffspring(femaleAnimal);
                    }

                    // Adjust female energy based on male's normalized life success
                    float maleZScore = (maleAnimal.lifeSuccess - meanLifeSuccess) / stdLifeSuccess;

                    float adjustmentFactor = 1 + Mathf.Abs(maleZScore);
                    float energyLoss = femaleAnimal.reprodEnergy;

                    if (maleZScore < 0)
                    {
                        // Male life success is below average, female loses more energy
                        energyLoss *= adjustmentFactor;
                    }
                    else
                    {
                        // Male life success is above average, female loses less energy
                        energyLoss /= adjustmentFactor;
                    }

                    if (energyLoss > 4.0) {
                        energyLoss = 4.0f;
                    }

                    customTerrain.debug.text = "Female Energy Loss: " + energyLoss.ToString();
                    femaleAnimal.ReduceEnergy(energyLoss);
                    maleAnimal.ReduceEnergy(maleAnimal.reprodEnergy);

                    countMatingAccept += 1;
                }
                else
                {
                    countMatingDecline += 1;
                }
            }
        }
    }



    /// <summary>
    /// Method to instantiate an animal prefab. It must contain the animal.cs class attached.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public GameObject makeAnimal(Vector3 position)
    {
        GameObject animal = Instantiate(animalPrefab, transform);
        int gender = UnityEngine.Random.value < 0.5f ? 0 : 1;

        animal.GetComponent<Animal>().Setup(customTerrain, this, gender);
        animal.transform.position = position;
        animal.transform.Rotate(0.0f, UnityEngine.Random.value * 360.0f, 0.0f);
        return animal;
    }

    /// <summary>
    /// If makeAnimal() is called without position, we randomize it on the terrain.
    /// </summary>
    /// <returns></returns>
    public GameObject makeAnimal()
    {
        Vector3 scale = terrain.terrainData.heightmapScale;
        float x = UnityEngine.Random.value * width;
        float z = UnityEngine.Random.value * height;
        float y = customTerrain.getInterp(x / scale.x, z / scale.z);
        return makeAnimal(new Vector3(x, y, z));
    }

    /// <summary>
    /// Method to add an animal inherited from another. It spawns where the parent was.
    /// </summary>
    /// <param name="parent"></param>
    public void addOffspring(Animal parent)
    {
        GameObject animal = makeAnimal(parent.transform.position);
        animal.GetComponent<Animal>().InheritBrain(parent.GetBrain(), parent.GetBrainReprod(), true);
        animals.Add(animal);
        SaveToCSV();
    }

    /// <summary>
    /// Remove instance of an animal.
    /// </summary>
    /// <param name="animal"></param>
    public void removeAnimal(Animal animal)
    {
        animals.Remove(animal.transform.gameObject);
        Destroy(animal.transform.gameObject);
        SaveToCSV();
    }

    /// <summary>
    /// Saves the current number of animals and resources to a CSV file, including mean lifeSuccess and age.
    /// </summary>
    private void SaveToCSV()
    {
        int numberOfAnimals = animals.Count;
        int numberOfResources = GetResourceCount();

        // Calculate mean and standard deviation of lifeSuccess and mean age
        float totalLifeSuccess = 0f;
        float totalAge = 0f;
        int malePopSize = 0;
        int femalePopSize = 0;

        List<float> lifeSuccessValues = new List<float>(); // Store lifeSuccess values for std calculation

        foreach (var animalObj in animals)
        {
            Animal animal = animalObj.GetComponent<Animal>();
            totalLifeSuccess += animal.lifeSuccess;  // Assuming lifeSuccess is a property of the Animal class
            totalAge += animal.timeAlive;  // Assuming age is a property of the Animal class

            lifeSuccessValues.Add(animal.lifeSuccess); // Collect lifeSuccess for std calculation

            if (animal.gender == 1)
            {
                malePopSize += 1;
            }
            else
            {
                femalePopSize += 1;
            }
        }

        meanLifeSuccess = totalLifeSuccess / numberOfAnimals;
        float meanAge = totalAge / numberOfAnimals;

        // Calculate standard deviation of lifeSuccess
        float variance = 0f;
        foreach (float lifeSuccess in lifeSuccessValues)
        {
            variance += Mathf.Pow(lifeSuccess - meanLifeSuccess, 2);
        }
        stdLifeSuccess = Mathf.Sqrt(variance / numberOfAnimals);
        float ratioMatingAccept = 0;

        if ((countMatingAccept + countMatingDecline) != 0) {
            ratioMatingAccept = countMatingAccept / (countMatingAccept + countMatingDecline);
        }

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string dataLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}\n", 
            timestamp, 
            numberOfAnimals, 
            numberOfResources, 
            femalePopSize,
            malePopSize,
            meanLifeSuccess.ToString("F2"),  // Format to 2 decimal places
            meanAge.ToString("F2"),          // Format to 2 decimal places
            ratioMatingAccept.ToString("F2")
        );

        File.AppendAllText(csvFilePath, dataLine);
    }

    /// <summary>
    /// Gets the current count of resources in the terrain.
    /// </summary>
    /// <returns></returns>
    private int GetResourceCount()
    {
        int[,] details = customTerrain.getDetails();
        int count = 0;
        foreach (int value in details)
        {
            count += value;
        }
        return count;
    }
}
