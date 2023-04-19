using MBS.Lightfall;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.Items;
using Opsive.UltimateCharacterController.Items.Actions;
using Opsive.UltimateCharacterController.Items.Actions.Modules.Shootable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightfallMunitionsBox : MonoBehaviour
{
    //TODO
    //Ammo amount builds up over time, to a max.
    //Option to disable ammo buildup.
    //grenade and ammo buildup are seperate counters.
    //audio for picking up ammo 

    [Range(0f, 100f)]
    public float MaxPercentClipsToGive;
    public float StartingClipsPercent;
    public float TimeTillNextClipPercent;
    public int MaxGrenadesToGive;
    public int StartingGrenades;
    public float TimeTillGrenadeUp;

    protected float m_currentClipsPercent;
    protected int m_currentGrenades;

    private List<BlacklistItem> canInteractBlacklist;


    // Start is called before the first frame update
    void Start()
    {
        canInteractBlacklist = new List<BlacklistItem>();
        m_currentClipsPercent = MaxPercentClipsToGive;
        m_currentGrenades = StartingGrenades;

        Debug.Log("currently working in this script...");
    }

    // Update is called once per frame
    void Update()
    {
        List<BlacklistItem> toRemoveFromBlacklist = new List<BlacklistItem>();
        foreach (var item in canInteractBlacklist)
        {
            item.Timer -= Time.deltaTime;
            if (item.Timer <= 0)
                toRemoveFromBlacklist.Add(item);
        }

        foreach (var item in toRemoveFromBlacklist)
        {
            canInteractBlacklist.Remove(item);
        }
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
        canInteractBlacklist.Add(new BlacklistItem(interactor.gameObject, 5f));

        Inventory inventory = interactor.gameObject.GetComponentInParent<Inventory>();
        inventory.GetAllCharacterItems();

        float maxOverflow = 0;

        foreach (var item in inventory.GetAllCharacterItems().ToArray())
        {
            ShootableAction shootableAction = item.gameObject.GetComponent<ShootableAction>();
            if (shootableAction != null)
            {
                LightfallAmmo ammoModule = shootableAction.GetFirstActiveModule<LightfallAmmo>();
                if (ammoModule != null)
                {
                    float overflow = ammoModule.AdjustAmmoAmountByPercentOfClips(m_currentClipsPercent / 100);
                    if (overflow > maxOverflow && !interactor.DoNotReduceBoxAmmoValue)
                        maxOverflow = overflow;
                }

            }
            else
            {
                Debug.Log("No shootable action on character. Maybe it has a throwable, which is unhandled?");
            }
            Debug.Log("Applied ammo to " + item.name);
        }

        m_currentClipsPercent = 0 + maxOverflow;//I think there is a better way...
        //TODO: It gives percent of max clips, then gets returned percent of clips used (AdjustAmmoAmountByPercentOfClips() should only take percent equal to the amount of whole clips useable)
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
