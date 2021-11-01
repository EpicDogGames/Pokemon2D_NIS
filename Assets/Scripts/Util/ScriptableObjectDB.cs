using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableObjectDB<T> : MonoBehaviour where T: ScriptableObject         // T will represent Type which must be scriptable objects only
{
    static Dictionary<string, T> objects;

    public static void Init()
    {
        objects = new Dictionary<string, T>();

        var objectArray = Resources.LoadAll<T>("");
        foreach (var obj in objectArray)
        {
            // lowercase n is name of the scriptable object file, uppercase N is the value in the inspector
            if (objects.ContainsKey(obj.name))
            {
                Debug.Log($"There are two objects with the name {obj.name}");
                continue;
            }
            objects[obj.name] = obj;
        }
    }

    public static T GetObjectByName(string name)
    {
        if (!objects.ContainsKey(name))
        {
            Debug.LogError($"Object with name {name} not found in the database");
            return null;
        }

        return objects[name];
    }
}


