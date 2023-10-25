using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton : MonoBehaviour
{
    private static Singleton instance;

    private void Awake()
    {
        // Check if an instance already exists
        if (instance != null && instance != this)
        {
            // Destroy the duplicate instance if found
            Destroy(gameObject);
            return;
        }

        // Set this as the instance if none exists
        instance = this;

        // Mark this object as "Don't Destroy On Load"
        DontDestroyOnLoad(gameObject);
    }

    // Add your specific functionality here...
}
