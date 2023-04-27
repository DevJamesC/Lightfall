using MBS.Lightfall;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.Items;
using Opsive.UltimateCharacterController.Items.Actions;
using Opsive.UltimateCharacterController.Items.Actions.Modules.Shootable;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightfallMunitionsBox : MonoBehaviour
{
    //TODO: can actually pick up grenades and have them applied to abilites

    [Range(0f, 100f)]
    public float MaxPercentClipsToGive;
    public float StartingClipsPercent;
    public float TimeTillAddAmmo;
    public float AmmoPercentToAdd;
    public bool AmmoBuildupEnabled;
    public GameObject ClipGraphic;
    public GameObject ClipPlaceholderGraphic;
    public Vector3 ClipPlacementOffset;
    public Vector2 ClipPlacementSpacing;
    public GameObject GrenadeGraphic;
    public Vector3 GrenadePlacementOffset;
    public Vector2 GrenadePlacementSpacing;

    public int MaxGrenadesToGive;
    public int StartingGrenades;
    public float TimeTillNextGrenade;
    public bool GrenadeBuildupEnabled;

    //Change to private, add a readonly UI field when internet is available
    [SerializeField, Range(0, 100), ReadOnly] protected float m_currentClipsPercent;
    [SerializeField, ReadOnly] protected int m_currentGrenades;
    private List<BlacklistItem> canInteractBlacklist;
    private float timeTillNextClipTick;
    private float timeTillNextGrenade;
    private AudioSource ammoPickupAudioSource;
    private GameObject[] clipGraphicObjects;
    private GameObject[] grenadeGraphicObjects;

    // Start is called before the first frame update
    void Awake()
    {
        canInteractBlacklist = new List<BlacklistItem>();
        m_currentClipsPercent = StartingClipsPercent;
        m_currentGrenades = StartingGrenades;
        timeTillNextClipTick = TimeTillAddAmmo;
        timeTillNextGrenade = TimeTillNextGrenade;
        ammoPickupAudioSource = GetComponent<AudioSource>();
        clipGraphicObjects = new GameObject[10];
        grenadeGraphicObjects = new GameObject[MaxGrenadesToGive];
    }

    private void Start()
    {
        //instanciate clip graphics
        for (int i = 0; i < clipGraphicObjects.Length; i++)
        {
            Vector3 pos = ClipPlacementOffset;
            pos.z += (ClipPlacementSpacing.x * i);
            pos.x += (ClipPlacementSpacing.y * i);
            float percentClipsToShow = Mathf.Ceil(MaxPercentClipsToGive / 10) * 10;
            GameObject prefabToInstanciate = percentClipsToShow > i * 10 ? ClipGraphic : ClipPlaceholderGraphic;
            clipGraphicObjects[i] = Instantiate(prefabToInstanciate, transform);
            clipGraphicObjects[i].transform.localPosition = pos;

        }
        //instanciate grenade graphics
        for (int i = 0; i < grenadeGraphicObjects.Length; i++)
        {
            Vector3 pos = GrenadePlacementOffset;
            pos.z += (GrenadePlacementSpacing.x * i);
            pos.x += (GrenadePlacementSpacing.y * i);
            grenadeGraphicObjects[i] = Instantiate(GrenadeGraphic, transform);
            grenadeGraphicObjects[i].transform.localPosition = pos;
        }

        CalculateClipAndGrenadeGraphicsDisplay();

    }

    private void CalculateClipAndGrenadeGraphicsDisplay()
    {
        for (int i = 0; i < clipGraphicObjects.Length; i++)
        {
            float percentClipsToShow = Mathf.Ceil(MaxPercentClipsToGive / 10) * 10;

            //if we are operating on a non-placeholder...
            if (percentClipsToShow > i * 10)
            {
                //and if our current clip percent is lower than the percent this graphic is meant to show, then set it inactive

                clipGraphicObjects[i].SetActive(m_currentClipsPercent > i * 10);


            }
        }

        for (int i = 0; i < grenadeGraphicObjects.Length; i++)
        {
            grenadeGraphicObjects[i].SetActive(m_currentGrenades > i);

        }
    }

    // Update is called once per frame
    void Update()
    {
        //Blacklist managment
        List<BlacklistItem> toRemoveFromBlacklist = new List<BlacklistItem>();
        foreach (var item in canInteractBlacklist)
        {
            item.Timer -= Time.deltaTime;
            if (item.Timer <= 0)
                toRemoveFromBlacklist.Add(item);
        }
        //remove items from blacklist
        foreach (var item in toRemoveFromBlacklist)
        {
            canInteractBlacklist.Remove(item);
        }

        //Incrimenting Ammo Percent
        if (m_currentClipsPercent < MaxPercentClipsToGive && AmmoBuildupEnabled)
        {
            if (timeTillNextClipTick > 0)
                timeTillNextClipTick -= Time.deltaTime;
            else
            {
                timeTillNextClipTick = TimeTillAddAmmo;
                m_currentClipsPercent = Mathf.Clamp(m_currentClipsPercent + AmmoPercentToAdd, 0, MaxPercentClipsToGive);
                CalculateClipAndGrenadeGraphicsDisplay();
            }
        }

        //Incrimenting Grenade Count
        if (m_currentGrenades < MaxGrenadesToGive && GrenadeBuildupEnabled)
        {
            if (timeTillNextGrenade > 0)
                timeTillNextGrenade -= Time.deltaTime;
            else
            {
                timeTillNextGrenade = TimeTillNextGrenade;
                m_currentGrenades += 1;
                CalculateClipAndGrenadeGraphicsDisplay();
            }
        }

    }

    private bool AddAmmoClips(LightfallMunitionBoxInteractor interactor)
    {
        bool updateDisplay = false;
        Inventory inventory = interactor.gameObject.GetComponentInParent<Inventory>();
        if (inventory == null)
            return updateDisplay;

        inventory.GetAllCharacterItems();
        float lowestAmountOfClipsRemaining = m_currentClipsPercent;

        //pickup ammo
        foreach (var item in inventory.GetAllCharacterItems().ToArray())
        {
            ShootableAction shootableAction = item.gameObject.GetComponent<ShootableAction>();
            if (shootableAction != null)//if we have a shootable action...
            {
                LightfallAmmo ammoModule = shootableAction.GetFirstActiveModule<LightfallAmmo>();
                if (ammoModule != null)//and we have an ammo module that supports interafacing with the munition box...
                {
                    //then pickup ammo clips.
                    float percentUsed = Mathf.Clamp01(ammoModule.AdjustAmmoAmountByClipIncriment(m_currentClipsPercent / 100));
                    if (!interactor.DoNotReduceBoxAmmoValue)
                    {
                        float previousPercent = m_currentClipsPercent;
                        float newPercent = Mathf.Floor((m_currentClipsPercent * (1 - percentUsed)));
                        if (newPercent < lowestAmountOfClipsRemaining)
                            lowestAmountOfClipsRemaining = newPercent;
                    }
                }

            }
            else
            {
                Debug.Log("No shootable action on character. Maybe it has a throwable, which is unhandled?");
            }
        }

        if (m_currentClipsPercent > lowestAmountOfClipsRemaining)
        {
            updateDisplay = true;
            ammoPickupAudioSource.Play();
        }

        m_currentClipsPercent = lowestAmountOfClipsRemaining;

        return updateDisplay;
    }

    private bool AddGrenades(LightfallMunitionBoxInteractor interactor)
    {
        bool updateDisplay = false;

        UltimateCharacterLocomotion locomotion = interactor.GetComponent<UltimateCharacterLocomotion>();
        if (locomotion == null)
            return updateDisplay;

        LightfallAbilityBase ability = locomotion.GetAbility<LightfallAbilityOne>();
        if (ability == null)
            return updateDisplay;

        int chargesUsed = ability.AddCharges(m_currentGrenades);
        int previousGrenades = m_currentGrenades;
        m_currentGrenades -= chargesUsed;

        if (previousGrenades != m_currentGrenades)
            updateDisplay = true;

        return updateDisplay;
    }

    private void OnTriggerEnter(Collider other)
    {
        LightfallMunitionBoxInteractor interactor = other.gameObject.GetComponentInParent<LightfallMunitionBoxInteractor>();
        if (interactor == null)
            return;

        foreach (var item in canInteractBlacklist)
        {
            if (item.GameObject == interactor.gameObject)
                return;
        }
        canInteractBlacklist.Add(new BlacklistItem(interactor.gameObject, 1f));

        bool updateDisplay = AddAmmoClips(interactor);

        //pickup grenades
        if (!updateDisplay)
            updateDisplay = AddGrenades(interactor);
        else
            AddGrenades(interactor);

        //update graphics
        if (updateDisplay)
            CalculateClipAndGrenadeGraphicsDisplay();
    }

    private class BlacklistItem
    {
        public GameObject GameObject;
        public float Timer;

        public BlacklistItem(GameObject gameObject, float timer)
        {
            GameObject = gameObject;
            Timer = timer;
        }
    }

}
