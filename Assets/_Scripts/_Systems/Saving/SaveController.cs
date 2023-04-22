using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    #region Values to Save
    Vector3 playerPosition;
    #endregion

    #region Save Control
    bool isSaving = false;
    bool isLoading = false;
    #endregion

    /// <summary>
    /// This function will handle the process of ensuring the save function is
    /// safe to run before actually executing it.
    /// </summary>
    public void StartSaving()
    {
        if (isSaving || isLoading)
            return;

        SaveGame();
    }

    //In-Progress
    void SaveGame()
    {
        Debug.Log("Starting save process");
        isSaving = true;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath +
            "/.." + "SaveData.dat");
        SaveData data = new SaveData(playerPosition);
        bf.Serialize(file, data);
        file.Close();

        isSaving = false;
        Debug.Log("Save complete.");
    }

    //In-Progress
    public void StartLoading()
    {
        if (isLoading || isSaving)
            return;

        LoadGame();
    }

    //In-Progress
    void LoadGame()
    {
        Debug.Log("Starting to load game");
        isLoading = true;

        if (File.Exists(Application.persistentDataPath +
            "/.." + "/SaveData.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath +
                "/.." + "/SaveData.dat", FileMode.Open);
            SaveData data = (SaveData)bf.Deserialize(file);
            file.Close();
            playerPosition = data.playerPosition.ConvertToV3();
            isLoading = false;
            Debug.Log("Game data loaded!");
        }
        else
        {
            isLoading = false;
            Debug.LogError("There is no save data!");
        }
    }

    //In-Progress
    void ResetData()
    {
        if (isSaving || isLoading)
            return;

        if (File.Exists(Application.persistentDataPath
                      + "/MySaveData.dat"))
        {
            File.Delete(Application.persistentDataPath +
                "/.." + "/SaveData.dat");
            Debug.Log("Data reset complete!");
        }
        else
            Debug.LogError("No save data to delete.");
    }
}

//In-Progress
[Serializable]
public class SaveData
{
    public SerializableVector3 playerPosition { get; private set; }

    public SaveData(Vector3 _playerPosition)
    {
        playerPosition = new SerializableVector3(
            _playerPosition.x,
            _playerPosition.y,
            _playerPosition.z);
    }
}

[Serializable]
public class SerializableVector3
{
    public float x { get; private set; }
    public float y { get; private set; }
    public float z { get; private set; }

    public SerializableVector3(float _x, float _y, float _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }

    public Vector3 ConvertToV3()
    {
        Vector3 newVector3 = new Vector3(x, y, z);
        return newVector3;
    }
}