using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using NUnit.Framework.Internal;
using Unity.Services.CloudSave;
using UnityEngine;

public class CloudSaveFile : MonoBehaviour
{
    
    public static CloudSaveFile Instance { set; get; }

    private void Awake()
    {
        Instance = this;
    }
    
    public async void SavePlayerFile(string filePath, string fileName)
    {
        try
        {
            byte[] file = await File.ReadAllBytesAsync(filePath);
            await CloudSaveService.Instance.Files.Player.SaveAsync(fileName, file);
        }
        catch (CloudSaveException e)
        {
            Debug.LogError($"Error saving data to Cloud Save: {e}");
        }
        catch (ArgumentException ex)
        {
            Debug.LogError($"JSON parse error: {ex.Message}");
        }
    }
    
    public async Task GetPlayerFileAsStream(string fileName)
    {
        try
        {
            // Load the stream from the cloud
            Stream cloudFileStream = await CloudSaveService.Instance.Files.Player.LoadStreamAsync(fileName);
            
            if (cloudFileStream == null)
            {
                Debug.Log("Cloud file stream is empty or null.");
                return;
            }

            var localFilePath = GetPath(fileName);

            await using (FileStream fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write))
            {
                await cloudFileStream.CopyToAsync(fileStream);
            }
            
            cloudFileStream.Close();
            await cloudFileStream.DisposeAsync();
            //MainMenuUIHandler.Instance.cloudDownloadInfoText.text = "DATA " + fileName;
        }
        catch (CloudSaveException e) when (e.ErrorCode == 7007)
        {
            //Nothing to download
        }
        catch (CloudSaveException e) when (e.ErrorCode == 1)
        {
            //Do noting
            MainMenuUIHandler.Instance.Error("ERROR", "CHECK YOUR CONNECTION, SIGN OUT AND RETRY TO GET DATA");
        }
        catch (CloudSaveException e)
        {
            Debug.Log("Error Code"+e.ErrorCode);
            Debug.LogError($"Error loading data from Cloud Save: {e}");
        }
        catch (ArgumentException ex)
        {
            Debug.LogError($"JSON parse error: {ex.Message}");
        }
    }
    
    public async Task DeletePlayerFile(string fileName)
    {
        try
        {
            await CloudSaveService.Instance.Files.Player.DeleteAsync(fileName);
        }
        catch (CloudSaveException e) when (e.ErrorCode == 7007)
        {
            //Nothing to download
        }
        catch (CloudSaveException e) when (e.ErrorCode == 1)
        {
            //Do noting
            MainMenuUIHandler.Instance.Error("ERROR", "CHECK YOUR CONNECTION, SIGN OUT AND RETRY TO GET DATA");
        }
        catch (CloudSaveException e)
        {
            Debug.Log("Error Code"+e.ErrorCode);
            Debug.LogError($"Error loading data from Cloud Save: {e}");
        }
        catch (ArgumentException ex)
        {
            Debug.LogError($"JSON parse error: {ex.Message}");
        }
    }
    private static string GetPath (string filename) {
        return Application.persistentDataPath + "/" + filename;
    }
    
   
}
