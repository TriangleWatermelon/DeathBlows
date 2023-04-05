using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    Vector3 playerPositionToSave;

    //In-Progress
    public void StartSaving()
    {
        SaveGame();
    }

    //In-Progress
    void SaveGame()
    {
        Debug.Log("Starting save process");

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + ".." + "SaveData.dat");
        SaveData data = new SaveData();
        data.playerPosition = playerPositionToSave;
        bf.Serialize(file, data);
        file.Close();

        Debug.Log("Save complete.");
    }
}

[Serializable]
public class SaveData
{
    public Vector3 playerPosition;
}