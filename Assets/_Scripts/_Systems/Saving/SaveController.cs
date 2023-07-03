using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    #region Values to Save
    [HideInInspector]
    public static Vector3 playerPosition { get; private set; }
    [HideInInspector]
    public static Vector3[] flagPositions { get; private set; }
    [HideInInspector]
    public static int tomatoesHeldByPlayer { get; private set; }
    #endregion

    #region Save Control Variables
    bool isSaving = false;
    bool isLoading = false;
    #endregion

    public static SaveController instance;

    private void Start()
    {
        if (instance != null)
            Destroy(this);

        instance = this;
    }

    /// <summary>
    /// Sets playerPosition variable.
    /// </summary>
    /// <param name="_pos"></param>
    public void SetPlayerPosition(Vector3 _pos) => playerPosition = _pos;

    /// <summary>
    /// Sets the flagPositions variable.
    /// </summary>
    /// <param name="_flagPositions"></param>
    public void SetFlagPositions(Vector3[] _flagPositions) => flagPositions = _flagPositions;

    /// <summary>
    /// Sets the tomatoesHeldByPlayer variable.
    /// </summary>
    /// <param name="_count"></param>
    public void SetTomatoesHeldByPlayer(int _count) => tomatoesHeldByPlayer = _count;

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
        FileStream file = File.Create(Application.persistentDataPath + "/SaveData.dat");
        SaveData data = new SaveData(playerPosition, flagPositions, tomatoesHeldByPlayer);
        bf.Serialize(file, data);
        file.Close();

        isSaving = false;
        Debug.Log($"Save complete: {Application.persistentDataPath}");
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

        if (File.Exists(Application.persistentDataPath + "/SaveData.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/SaveData.dat", FileMode.Open);
            SaveData data = (SaveData)bf.Deserialize(file);
            file.Close();

            #region Variable Assignments
            playerPosition = data.playerPosition.ConvertToV3();
            if (data.flagPositions != null)
            {
                flagPositions = new Vector3[data.flagPositions.Length];
                for (int i = 0; i < data.flagPositions.Length; i++)
                {
                    flagPositions[i] = data.flagPositions[i].ConvertToV3();
                }
            }
            tomatoesHeldByPlayer = data.tomatoesHeldByPlayer;
            #endregion

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

        if (File.Exists(Application.persistentDataPath + "/SaveData.dat"))
        {
            File.Delete(Application.persistentDataPath + "/SaveData.dat");
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
    public SerializableVector3[] flagPositions { get; private set; }
    public int tomatoesHeldByPlayer { get; private set; }

    public SaveData(Vector3 _playerPosition, Vector3[] _flagPositions = null, int _tomatoesHeldByPlayer = 0)
    {
        //Player Position
        playerPosition = new SerializableVector3(
            _playerPosition.x,
            _playerPosition.y,
            _playerPosition.z);

        //Respawn Flag Positions
        if (_flagPositions != null)
        {
            for (int i = 0; i < _flagPositions.Length; i++)
            {
                flagPositions = new SerializableVector3[_flagPositions.Length];
                flagPositions[i] = new SerializableVector3(
                    _flagPositions[i].x,
                    _flagPositions[i].y,
                    _flagPositions[i].z);
            }
        }

        //Tomato Count
        tomatoesHeldByPlayer = _tomatoesHeldByPlayer;

        Debug.Log($"Save Data created.");
    }
}

/// <summary>
/// A class used to convert Unity's Vector3 into values that can run through the BinaryFormatter and back.
/// </summary>
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