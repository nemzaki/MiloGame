using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class CheckInternetConnection : MonoBehaviour
{
    
    public bool hasInternetConnection;
    
    private async void Start()
    {
        try
        {
            hasInternetConnection = await IsConnectedToInternetAsync();
        }
        catch (Exception e)
        {
            throw; // TODO handle exception
        }
    }
    
    public async Task<bool> CheckHasConnection()
    {
        try
        {
            return await IsConnectedToInternetAsync();
        }
        catch (Exception e)
        {
            throw; // TODO handle exception
        }
    }
    
    private async Task<bool> IsConnectedToInternetAsync()
    {
        try
        {
            // Ping a reliable server (e.g., Google)
            using (UnityWebRequest request = new UnityWebRequest("http://www.google.com"))
            {
                request.timeout = 5; // Set a timeout (in seconds)
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || 
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    //Debug.LogError("Internet check failed: " + request.error);
                    return false;
                }

                return true;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Exception during internet check: " + ex.Message);
            return false;
        }
    }
}
