using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[DisallowMultipleComponent]
public class SingletonNew<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T i;
    public static T I {
        get {
            if (i == null) {
                i = (T) FindObjectOfType(typeof(T));
            }
            else
            {
                
            }
            return i;
        }
    }
}