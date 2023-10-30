using System;
using UnityEngine;

public class PartEffectController : SingletonNew<PartEffectController>
{
    public float minSpeed = 0.1f;
    public float maxSpeed = 1f;
    
    public float baseSpeedKaporta0 = 0.3f;
    public float baseSpeedKaporta1= 0.4f;
    public float baseSpeedKaporta2= 0.5f;

    public float speedEffectLastik0 = -0.05f;
    public float speedEffectLastik1 = 0.0f;
    public float speedEffectLastik2 = 0.05f;

    public float speedEffectMotor0 = 0.8f;
    public float speedEffectMotor1 = 1f;
    public float speedEffectMotor2 = 1.2f;

    public float speedEffectKoltuk0 = -0.15f;
    public float speedEffectKoltuk1 = 0.0f;
    public float speedEffectKoltuk2 = 0.15f;
    
    
    public float speedEffectRuzgarlik0 = -0.1f;
    public float speedEffectRuzgarlik1 = 0.1f;
    public float speedEffectRuzgarlik2 = 0.2f;


    private Vector2 minMaxSpeed;

    public Vector2 GetMinMaxSpeed()
    {
        return minMaxSpeed;
    }
    
    private void Start()
    {
        var min = Mathf.Clamp(
            (baseSpeedKaporta0 * speedEffectMotor0) + speedEffectLastik0 + speedEffectKoltuk0 + speedEffectRuzgarlik0,
            minSpeed, maxSpeed);
        var max = Mathf.Clamp(
            (baseSpeedKaporta2 * speedEffectMotor2) + speedEffectLastik2 + speedEffectKoltuk2 + speedEffectRuzgarlik2,
            minSpeed, maxSpeed);
        minMaxSpeed = new Vector2(min, max);
    }


    public float GetSpeed(SavedCarProps carProps)
    {
        var baseSpeed = minSpeed;
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

        return Mathf.Clamp(baseSpeed, minSpeed, maxSpeed);
    }
}
