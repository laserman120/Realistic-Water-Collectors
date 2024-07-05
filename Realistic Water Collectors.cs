using Construction;
using Sons.Gui;
using SonsSdk;
using UnityEngine;
using SUI;
using RedLoader;
using Sons.Areas;
using HarmonyLib;
using Endnight.Utilities;
using TheForest.Utils;
using System.Reflection.Emit;
using System.Reflection;
using TheForest.Player.Actions;
using Endnight.Environment;
using System.Runtime.InteropServices;
using Sons.Gameplay;
using Sons.Gameplay.GPS;
using Sons.Ai;
using Sons.Animation.PlayerControl;
using Sons.Cutscenes;
using UnityEngine.Playables;
using static RedLoader.RLog;
using TheForest.Items.Inventory;
using TheForest.Items.Special;
using Sons.Settings;
using Sons.Atmosphere;
using Endnight.Extensions;
using Sons.Player;
using TheForest.World;
using System.Collections;
using Random = UnityEngine.Random;
using TheForest.Items.Craft;
using SonsSdk.Attributes;
using RedLoader.Utils;

namespace Realistic_Water_Collectors;
[RegisterTypeInIl2Cpp]
public class FireProximityTrigger : MonoBehaviour
{
    private HashSet<Collider> nearbyFireElements = new HashSet<Collider>(); // Use HashSet for fast lookups
    private float proximityRadius = 1.5f;

    private float checkInterval = 30f;
    private float timer = 0f;
    private RainCatcher _rainCatcher;

    private void Start()
    {
        timer = Random.Range(0f, checkInterval); // Randomize initial check time
        SphereCollider triggerCollider = gameObject.AddComponent<SphereCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = proximityRadius;
        Transform rainCatcherInteractionTransform = transform.Find("RainCatcherInteraction");
        if (rainCatcherInteractionTransform != null)
        {

            // Find the RainCatcher on that child
            _rainCatcher = rainCatcherInteractionTransform.GetComponent<RainCatcher>();
        }
        if(_rainCatcher == null)
        {
            RLog.Error("RainCatcher not found on RainCatcherInteraction child of FireProximityTrigger");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Transform fireElement = other.transform;
        
        while (fireElement != null && !fireElement.name.Contains("FireElement"))
        {
            fireElement = fireElement.parent;
        }

        if (fireElement != null)
        {
            nearbyFireElements.Add(fireElement.GetComponent<Collider>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Transform fireElement = other.transform;
        while (fireElement != null && !fireElement.name.Contains("FireElement"))
        {
            fireElement = fireElement.parent;
        }

        if (fireElement != null)
        {
            nearbyFireElements.Remove(fireElement.GetComponent<Collider>());
        }
    }

    public bool IsNearActiveFireElement()
    {
        foreach (Collider fireElementCollider in nearbyFireElements)
        {
            Transform temperatureVolume = fireElementCollider.transform.Find("TemperatureModifierVolume");
            if (temperatureVolume != null && temperatureVolume.GetComponent<TemperatureModifierVolume>()?.isActiveAndEnabled == true)
                return true; // At least one fire element is active
        }
        return false; // No active fire element found
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            timer = 0f; // Reset the timer

            if (IsNearActiveFireElement())
            {
                _rainCatcher._currentSeason = SeasonsManager.Season.Summer;
                _rainCatcher.SetFrozen(false);
            }
            else
            {
                _rainCatcher._currentSeason = SeasonsManager.Season.Winter;
                _rainCatcher.SetFrozen(true);
            }
            
        }
    }

    private int GetFireElementCount()
    {
        return nearbyFireElements.Count;
    }
}

public class Realistic_Water_Collectors : SonsMod, IOnGameActivatedReceiver
{
    public Realistic_Water_Collectors()
    {
    }

    protected override void OnInitializeMod()
    {
        Config.Init();
    }

    protected override void OnSdkInitialized()
    {
        Realistic_Water_CollectorsUi.Create();
    }

    protected override void OnGameStart()
    {
    }

    public void OnGameActivated()
    {
        ConstructionTools.GetRecipe(56)._builtPrefab.AddComponent<FireProximityTrigger>();
    }
}