using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;

public class PartEffectController : SingletonNew<PartEffectController>
{
    // public float minSpeed = 0.01f;
    // public float maxSpeed = 2f;
    
    private float baseSpeedKaporta0 = 0.3f;
    private float baseSpeedKaporta1= 0.4f;
    private float baseSpeedKaporta2= 0.6f;

    private float speedEffectLastik0 = -0.05f;
    private float speedEffectLastik1 = 0.0f;
    private float speedEffectLastik2 = 0.05f;

    private float speedEffectMotor0 = 1.2f;
    private float speedEffectMotor1 = 0.8f;
    private float speedEffectMotor2 = 1f;

    private float speedEffectKoltuk0 = 0.15f;
    private float speedEffectKoltuk1 = 0.0f;
    private float speedEffectKoltuk2 = -0.15f;
    
    
    private float speedEffectRuzgarlik0 = 0.2f;
    private float speedEffectRuzgarlik1 = 0.1f;
    private float speedEffectRuzgarlik2 = -0.1f;


    private float speedEffectToprak = 0.6f;
    private float speedEffectMicir = 0.8f;
    private float speedEffectAsfalt = 1f;
    private float speedEffectBuz = 1.2f;


    public List<float> massKaporta = new List<float> { 800, 600, 400 };
    public List<float> massLastik = new List<float> { 800, 600, 400 };
    public List<float> massMotor = new List<float> { 800, 600, 400 };
    public List<float> massKoltuk = new List<float> { 800, 600, 400 };
    public List<float> massRuzgarlik = new List<float> { 800, 600, 400 };

    private Vector2 minMaxSpeed;

    public Vector2 shownSpeedRange = new Vector2(60f, 200f);

    public float maxSpeedDuration = 0.93f;
    
    
    
    public float GetDuration(float currentSpeed)
    {
        return Mathf.Lerp(maxSpeedDuration * (shownSpeedRange.y / shownSpeedRange.x), maxSpeedDuration,
            GetPercentage(currentSpeed));
    }
    
    public float GetPercentage(float currentSpeed)
    {
        return (currentSpeed - minMaxSpeed.x) / (minMaxSpeed.y - minMaxSpeed.x);
    }

    public float GetProjectedSpeed(float currentSpeed)
    {
        return Mathf.Lerp(shownSpeedRange.x, shownSpeedRange.y, GetPercentage(currentSpeed));
    }

    public float GetMass(SavedCarProps newProps)
    {
        var total = 0f;
        total += massKaporta[newProps.kaportaId];
        total += massLastik[newProps.lastikId];
        total += massMotor[newProps.motorId];
        total += massKoltuk[newProps.koltukId];
        total += massRuzgarlik[newProps.ruzgarlikId];
        return total;
    }
    
    public Vector2 GetMinMaxSpeed()
    {
        return minMaxSpeed;
    }
    
    private void Start()
    {
        // var min = Mathf.Clamp(
        //     ((baseSpeedKaporta0 * speedEffectMotor1) + speedEffectLastik0 + speedEffectKoltuk2 + speedEffectRuzgarlik0) * speedEffectToprak,
        //     minSpeed, maxSpeed);
        // var max = Mathf.Clamp(
        //     ((baseSpeedKaporta2 * speedEffectMotor0) + speedEffectLastik2 + speedEffectKoltuk0 + speedEffectRuzgarlik2) * speedEffectBuz,
        //     minSpeed, maxSpeed);
        var min =
            ((baseSpeedKaporta0 * speedEffectMotor1) + speedEffectLastik0 + speedEffectKoltuk2 + speedEffectRuzgarlik2) * speedEffectToprak;
        var max =
            ((baseSpeedKaporta2 * speedEffectMotor0) + speedEffectLastik2 + speedEffectKoltuk0 + speedEffectRuzgarlik0) * speedEffectBuz;
        minMaxSpeed = new Vector2(min, max);
    }


    public enum GroundType
    {
        NONE,
        Toprak,
        Micir,
        Asfalt,
        Buz,
    }
    
    /// <summary>
    /// Iterates over all combinations of (kaporta, lastik, motor, koltuk, ruzgarlik) (each 0–2) and
    /// for each combination and each road type (Toprak, Micir, Asfalt, Buz) computes the mass,
    /// speed, projected speed, and duration. The results are written to (or overwrite) a CSV file.
    /// </summary>
    ///
    /// <summary>
    /// Iterates over all combinations of (kaporta, lastik, motor, koltuk, ruzgarlik) (each 0–2) and
    /// for each combination and each road type (Toprak, Micir, Asfalt, Buz) computes the mass,
    /// speed, projected speed, and duration. The results are written to (or overwrite) a CSV file.
    /// This CSV is formatted so that Excel can easily open it.
    /// </summary>
    [Button]
    public void WriteCarEffectCSV()
    {
        var min =
            ((baseSpeedKaporta0 * speedEffectMotor1) + speedEffectLastik0 + speedEffectKoltuk2 + speedEffectRuzgarlik2) * speedEffectToprak;
        var max =
            ((baseSpeedKaporta2 * speedEffectMotor0) + speedEffectLastik2 + speedEffectKoltuk0 + speedEffectRuzgarlik0) * speedEffectBuz;
        minMaxSpeed = new Vector2(min, max);
        
        // Define the output file path (for example, inside the Assets folder)
        string filePath = Path.Combine(Application.dataPath, "CarEffectResults.csv");
        StringBuilder csvContent = new StringBuilder();

        // Write CSV header
        csvContent.AppendLine("kaporta,lastik,motor,koltuk,ruzgarlik,groundType,mass,speed,projectedSpeed,duration");

        // Iterate over all part combinations (0,1,2 for each)
        for (int kaporta = 0; kaporta < 3; kaporta++)
        {
            for (int lastik = 0; lastik < 3; lastik++)
            {
                for (int motor = 0; motor < 3; motor++)
                {
                    for (int koltuk = 0; koltuk < 3; koltuk++)
                    {
                        for (int ruzgarlik = 0; ruzgarlik < 3; ruzgarlik++)
                        {
                            // Create a new SavedCarProps instance with the current indices
                            SavedCarProps props = new SavedCarProps();
                            props.kaportaId = kaporta;
                            props.lastikId = lastik;
                            props.motorId = motor;
                            props.koltukId = koltuk;
                            props.ruzgarlikId = ruzgarlik;

                            // Calculate the mass for this combination
                            float mass = GetMass(props);

                            // Process each road type
                            GroundType[] roadTypes = { GroundType.Toprak, GroundType.Micir, GroundType.Asfalt, GroundType.Buz };
                            foreach (GroundType road in roadTypes)
                            {
                                // Calculate speed with the current ground type
                                float speed = GetSpeed(props, road);
                                float projectedSpeed = GetProjectedSpeed(speed);
                                float duration = GetDuration(speed);

                                // Append a new row to the CSV content
                                csvContent.AppendLine(
                                    $"{kaporta},{lastik},{motor},{koltuk},{ruzgarlik},{road},{mass},{speed},{projectedSpeed},{duration}"
                                );
                            }
                        }
                    }
                }
            }
        }

        // Write or overwrite the CSV file
        File.WriteAllText(filePath, csvContent.ToString());
        Debug.Log("CSV file written to " + filePath);
    }    
    public float GetSpeed(SavedCarProps carProps, GroundType newGroundType = GroundType.NONE)
    {
        // var baseSpeed = minSpeed;
        var baseSpeed = minMaxSpeed.x;
        if (carProps.kaportaId == 0)
        {
            baseSpeed = baseSpeedKaporta0;
        }
        else if (carProps.kaportaId == 1)
        {
            baseSpeed = baseSpeedKaporta1;
        }
        else
        {
            baseSpeed = baseSpeedKaporta2;
        }

        if (carProps.motorId == 0)
        {
            baseSpeed *= speedEffectMotor0;
        }
        else if (carProps.motorId == 1)
        {
            baseSpeed *= speedEffectMotor1;
        }
        else
        {
            baseSpeed *= speedEffectMotor2;
        }
        
        if (carProps.lastikId == 0)
        {
            baseSpeed += speedEffectLastik0;
        }
        else if (carProps.lastikId == 1)
        {
            baseSpeed += speedEffectLastik1;
        }
        else
        {
            baseSpeed += speedEffectLastik2;
        }

        

        if (carProps.koltukId == 0)
        {
            baseSpeed += speedEffectKoltuk0;
        }
        else if (carProps.koltukId == 1)
        {
            baseSpeed += speedEffectKoltuk1;            
        }
        else
        {
            baseSpeed += speedEffectKoltuk2;
        }

        if (carProps.ruzgarlikId == 0)
        {
            baseSpeed += speedEffectRuzgarlik0;
        }
        else if (carProps.ruzgarlikId == 1)
        {
            baseSpeed += speedEffectRuzgarlik1;
        }
        else
        {
            baseSpeed += speedEffectRuzgarlik2;
        }

        switch (newGroundType)
        {
            case GroundType.NONE:
                break;
            case GroundType.Toprak:
                baseSpeed *= speedEffectToprak;
                break;
            case GroundType.Micir:
                baseSpeed *= speedEffectMicir;
                break;
            case GroundType.Asfalt:
                baseSpeed *= speedEffectAsfalt;
                break;
            case GroundType.Buz:
                baseSpeed *= speedEffectBuz;
                break;
            
        }
        
        // return Mathf.Clamp(baseSpeed, minSpeed, maxSpeed);
        return baseSpeed;
    }
}
