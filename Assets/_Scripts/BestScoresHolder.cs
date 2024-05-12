
using System;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

[Serializable]
public class BestScoresHolder
{
    public Dictionary<PartEffectController.GroundType, List<ScoreSingle>> groundToBestCars =
        new Dictionary<PartEffectController.GroundType, List<ScoreSingle>>();

    public Dictionary<PartEffectController.GroundType, GameObject> groundToCarPrefabs =
        new Dictionary<PartEffectController.GroundType, GameObject>();

    public float GetBestSpeed(PartEffectController.GroundType groundType)
    {
        if (groundToBestCars.ContainsKey(groundType))
        {
            return groundToBestCars[groundType][0].speed;
        }

        return -1f;
    }
    
    public GameObject GetBestCarObject(PartEffectController.GroundType groundType)
    {
        if (groundToCarPrefabs.ContainsKey(groundType))
        {
            return groundToCarPrefabs[groundType];
        }

        return null;
    }
    
    public SavedCarProps GetBestCar(PartEffectController.GroundType groundType)
    {
        if (groundToBestCars.ContainsKey(groundType))
        {
            if (groundToBestCars[groundType].Count == 0)
            {
                return null;
            }
            return new SavedCarProps(groundToBestCars[groundType][0].carProps);
        }

        return null;
    }

    public void AddToDictionary(PartEffectController.GroundType groundType, SavedCarProps carProps)
    {
        var speed = PartEffectController.I.GetSpeed(carProps, groundType);
        if (!groundToBestCars.ContainsKey(groundType) || groundToBestCars[groundType] == null)
        {
            groundToBestCars[groundType] = new List<ScoreSingle>();
            groundToBestCars[groundType].Add(new ScoreSingle { carProps = carProps, speed = speed });
        }
        else
        {
            var found = 0;
            for (found = 0; found < groundToBestCars[groundType].Count; found++)
            {
                if (speed >= groundToBestCars[groundType][found].speed)
                {
                    break;
                }
            }

            if (found >= groundToBestCars[groundType].Count)
            {
                groundToBestCars[groundType].Add(new ScoreSingle { carProps = carProps, speed = speed });
            }
            else
            {
                groundToBestCars[groundType].Insert(found, new ScoreSingle { carProps = carProps, speed = speed });
            }
        }
    }
}

[Serializable]
public struct ScoreSingle
{
    public float speed;
    public SavedCarProps carProps;
}
