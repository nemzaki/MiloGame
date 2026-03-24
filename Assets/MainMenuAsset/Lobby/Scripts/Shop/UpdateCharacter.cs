using System;
using UnityEngine;

public class UpdateCharacter : MonoBehaviour
{
    
    //Only way I could get the update character visuals to work
    //was by doing this manually adding it to the object :)
    //I wrote this Feb 22, 2025 at 11:18 PM 
    private void OnEnable()
    {
        var parentTransform = transform.parent;
        if (parentTransform != null)
        {
            // Check if an UpdateCharacterVisuals component already exists
            var existingComponent = parentTransform.GetComponent<UpdateCharacterVisuals>();
            if (existingComponent != null)
            {
                Destroy(existingComponent); // Remove the existing component
            }

            // Add a new UpdateCharacterVisuals component
            var updateCharacterVisuals = parentTransform.gameObject.AddComponent<UpdateCharacterVisuals>();
            updateCharacterVisuals.inMenu = true;
            updateCharacterVisuals.character = this.transform;
        }
        else
        {
            Debug.LogWarning("Parent object is null. Cannot add UpdateCharacterVisuals.");
        }
    }
    
}
