using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using EditorAttributes;

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

    [Header("Spawning and Removal VFX")]
    [SerializeField] private ParticleSystem spawningPS;
    [SerializeField] private ParticleSystem removalPS;

    [Header("Furniture VFX")]
    [SerializeField] private ParticleSystem throwablesImpactPS;
    [SerializeField] private ParticleSystem togglablesSparkPS;
    #endregion

    [Header("Lightning VFX")]
    [SerializeField] private Light mainDirectionalLight;
    [SerializeField] List<string> listOfThunderSounds = new List<string>();



    // <----------------------------------------- LIGHTNING VFX ----------------------------------------- > //

    private Sequence InitializeLightningSequence()
    {
        float minInterval = 0.1f;
        float maxInterval = 0.2f;
        float minIntensity = 3f;
        float maxIntensity = 4.5f;
        int minNumberOfFlashes = 2;
        int maxNumberOfFlashes = 4;

        Sequence lightningSequence = DOTween.Sequence();
        for (int i=0; i < Random.Range(minNumberOfFlashes, maxNumberOfFlashes); i++)
        {
            lightningSequence.Append(mainDirectionalLight.DOIntensity(Random.Range(minIntensity, maxIntensity), Random.Range(minInterval, maxInterval)));
            lightningSequence.Append(mainDirectionalLight.DOIntensity(1, Random.Range(minInterval, maxInterval)));
            lightningSequence.AppendInterval(Random.Range(0, 0.08f));
        }

        return lightningSequence;
    }

    [Button("Activate Lightning")]
    public void ActivateLightning()
    {
        InitializeLightningSequence().Play();
        AudioManager.instance.Play(listOfThunderSounds[Random.Range(0, listOfThunderSounds.Count)]);
    }


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

    // <----------------------------------------- SPAWNING AND REMOVAL VFX ----------------------------------------- > //
    public ParticleSystem InstantiateSpawningPS(Transform spawningTransform, float yOffset = -0.3f)
    {
        Vector3 position = new Vector3(spawningTransform.position.x, spawningTransform.position.y + yOffset, spawningTransform.position.z);
        ParticleSystem PS = Instantiate(spawningPS, position, Quaternion.identity);
        return PS;
    }

    public ParticleSystem InstantiateRemovalPS(Transform removalTransform, float yOffset = 0f)
    {
        Vector3 position = new Vector3(removalTransform.position.x, removalTransform.position.y + yOffset, removalTransform.position.z);
        ParticleSystem PS = Instantiate(removalPS, position, Quaternion.identity);
        return PS;
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
