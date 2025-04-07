using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    #region // <------- SINGLETON PATTERN -------> //
    private static VFXManager _instance;
    public static VFXManager Instance
    {
        get
        {
            // Create logic to create the instance
            if (_instance == null)
            {
                GameObject obj = new GameObject("VFX Manager");
                obj.AddComponent<VFXManager>();
            }

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region // <------- VARIABLE DEFINITIONS -------> //

    [Header("Player VFX")]
    [SerializeField] private ParticleSystem interactPS;
    [SerializeField] private ParticleSystem ghostsPS;
    [SerializeField] private ParticleSystem sparkPS;
    [SerializeField] private ParticleSystem magicCirclePS;

    [Header("NPC VFX")]
    [SerializeField] private ParticleSystem startledNPCPS;

    [Header("Furniture VFX")]
    [SerializeField] private ParticleSystem throwablesImpactPS;
    [SerializeField] private ParticleSystem togglablesSparkPS;
    #endregion


    // <----------------------------------------- PLAYER VFX ----------------------------------------- > //

    public void InstantiateInteractPS(Transform playerTransform)
    {
        Vector3 position = new Vector3(playerTransform.position.x, playerTransform.position.y - 0.8f, playerTransform.position.z);
        Instantiate(interactPS, position, Quaternion.identity);
    }

    public void PlayHauntPS()
    {
        ghostsPS.gameObject.SetActive(true);
        ghostsPS.Play(true);
        sparkPS.gameObject.SetActive(true);
        sparkPS.Play(true);
    }

    public void ToggleMagicCirclePS(bool isActivated)
    {
        if (isActivated)
        {
            magicCirclePS.gameObject.SetActive(true);
            magicCirclePS.Play(true);
        }
        else
        {
            magicCirclePS.Stop(true);
        }
    }


    // <----------------------------------------- NPC VFX ----------------------------------------- > //

    public void InstantiateStartedNPCPS(Transform npcTransform)
    {
        Vector3 position = new Vector3(npcTransform.position.x, npcTransform.position.y - 1.4f, npcTransform.position.z);
        Instantiate(startledNPCPS, position, Quaternion.identity);
    }

    // <----------------------------------------- FURNITURE VFX ----------------------------------------- > //

    public void InstantiateImpactPS(Vector3 impactPosition)
    {
        Vector3 position = new Vector3(impactPosition.x, impactPosition.y, impactPosition.z);
        Instantiate(throwablesImpactPS, position, Quaternion.identity);
    }

    public void InstantiateToggleSparkPS(Vector3 psPosition)
    {
        Vector3 position = new Vector3(psPosition.x, psPosition.y + 2f, psPosition.z);
        Instantiate(togglablesSparkPS, position, Quaternion.identity);
    }
}
