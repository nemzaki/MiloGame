using Unity.Services.CloudCode;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;

public class VersionChecker : MonoBehaviour
{

    /// <summary>
    /// todo check gameversioncheck.js to update game version
    /// </summary>
    public GameObject updateGameScreen;
    public TextMeshProUGUI latestVersionText;
    public TextMeshProUGUI errorText;
    public string gameUrl;
    public string gameUrlAndroid;
    
    public async Task CheckGameVersion()
    {
        try
        {
            string currentVersion = Application.version; // Get Unity’s game version
            
            var arguments = new Dictionary<string, object>
            {
                { "version", currentVersion }
            };

            var result = await CloudCodeService.Instance.CallEndpointAsync<Dictionary<string, object>>(
                "GameVersionCheck",
                arguments
            );

            if (result.TryGetValue("error", out object errorMessage))
            {
                string error = errorMessage.ToString();
                if (error == "MISSING_VERSION")
                {
                    Debug.Log("Missing game version");
                    errorText.text = "Missing game version";
                    updateGameScreen.SetActive(true);
                }
                else if (error == "OUTDATED_VERSION")
                {
                    Debug.Log("GAME IS OUTDATED DUMMY");
                    errorText.text = "GAME IS OUTDATED DUMMY";
                    updateGameScreen.SetActive(true);
                }
                else
                {
                    Debug.Log("Unknown error: " + error);
                    errorText.text = "Unknown error: " + error;
                }
            }
            else
            {
                Debug.Log("Game version is up to date!");
            }
            
            latestVersionText.text = currentVersion;
        }
        catch (CloudCodeException e)
        {
            //Debug.LogError($"Cloud Code Error: {e.Message}");
        }
    }

    public void OpenGameUrl()
    {
        
    }
}