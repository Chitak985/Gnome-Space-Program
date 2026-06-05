using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class CelestialBodyManager : Node3D
{
    public static readonly string classTag = "([color=green]CelestialBodyManager[color=white])";

    public static CelestialBodyManager Instance { get; private set; }

    // Could be a sun, could be a black hole in the center of a galaxy, or some virtual object that doesn't exist.
    public CelestialBody rootCBody;
    public List<CelestialBody> CelestialBodies { get; private set; } = [];

    public override void _EnterTree()
    {
        Instance = this;
    }

    // Recursive method to find a CBody from name
    public CelestialBody GetCBodyFromName(string name)
    {
        foreach (CelestialBody cBody in CelestialBodies)
        {
            if (cBody.Config.properties.name == name) return cBody;
        }

        return null;
    }

    // Jank as hell please never do this again sobbing emoji
    public void CreateCBodiesFromConfigs(List<string> chosenPacks)
    {
        // Get all relevant celestial body configs loaded in this save
        List<string> planetConfigs = [];
        foreach (string pack in chosenPacks)
        {
            string fullPath = $"{ConfigUtility.GameData}/{pack}";
            planetConfigs.AddRange(ConfigUtility.GetConfigs(fullPath, "cBody"));
            Logger.Print($"{classTag} Successfully indexed celestial pack '{fullPath}'");
        }

        foreach (string cfgStr in planetConfigs)
        {
            Dictionary cfg = ConfigUtility.ParseConfig(cfgStr);
            CreateCBodyFromDict(cfg);
        }
        InstantiateCBodies();

        Logger.Print($"{classTag} System created successfully!");
    }

    public CelestialBody CreateCBodyFromDict(Dictionary dict)
    {
        CelestialBody.Configuration config = ParseCBodyConfig(dict);

        CelestialBody cBody = new();
        CelestialBodies.Add(cBody);
        cBody.SetConfiguration(config);

        // Add root body if designated as one
        // You better PRAY that only one exists
        if (cBody.Config.isRootBody)
        {
            Logger.Print("fuck off");
            rootCBody = cBody;
        }

        return cBody;
    }

    // TODO: Find a better way than running a secondary loop
    public void InstantiateCBodies()
    {
        Logger.Print("WHAT THE FUCK");
        foreach (CelestialBody cBody in CelestialBodies)
        {
            cBody.Instantiate();
        }
    }

    public static CelestialBody.Configuration ParseCBodyConfig(Dictionary config)
    {
        CelestialBody.Configuration cBodyProperties = new();

        cBodyProperties.isRootBody = (bool)ConfigUtility.GetValue("rootBody", config, false);

        // Parse properties node
        Dictionary propertiesNode = (Dictionary)config["properties"];

        cBodyProperties.properties.name = (string)ConfigUtility.GetValue("name", propertiesNode);
        cBodyProperties.properties.focusOnLoad = (bool)ConfigUtility.GetValue("focusOnLoad", propertiesNode, false);
        cBodyProperties.properties.mass = (double)ConfigUtility.GetValue("mass", propertiesNode, -1);
        cBodyProperties.properties.geeASL = (double)ConfigUtility.GetValue("geeASL", propertiesNode, -1);
        cBodyProperties.properties.radius = (double)ConfigUtility.GetValue("radius", propertiesNode, 600000);
        cBodyProperties.properties.inverseRotAltitude = (double)ConfigUtility.GetValue("inverseRotAltitude", propertiesNode, 100000);

        // Parse rotation node
        Dictionary rotationNode = (Dictionary)config["rotation"];

        cBodyProperties.rotation.initial = (double)ConfigUtility.GetValue("initial", rotationNode, 0);
        cBodyProperties.rotation.period = (double)ConfigUtility.GetValue("period", rotationNode, 0);
        cBodyProperties.rotation.tilt = ConfigUtility.GetVector3("tilt", rotationNode, Vector3.Zero);

        // Parse orbit node (It may potentially not exist, so we encase it in a tryget)
        if (ConfigUtility.TryGetDictionary("orbit", config, out Dictionary orbitNode))
        {
            cBodyProperties.parentCBodyName = (string)ConfigUtility.GetValue("parent", orbitNode);
            cBodyProperties.orbitElements.semiMajorAxis = (double)ConfigUtility.GetValue("semiMajorAxis", orbitNode);
            cBodyProperties.orbitElements.inclination = (double)ConfigUtility.GetValue("inclination", orbitNode);
            cBodyProperties.orbitElements.eccentricity = (double)ConfigUtility.GetValue("eccentricity", orbitNode);
            cBodyProperties.orbitElements.argumentOfPeriapsis = (double)ConfigUtility.GetValue("argumentOfPeriapsis", orbitNode);
            cBodyProperties.orbitElements.longitudeOfAscendingNode = (double)ConfigUtility.GetValue("longitudeOfAscendingNode", orbitNode);
            cBodyProperties.orbitElements.meanAnomalyAtEpoch = (double)ConfigUtility.GetValue("meanAnomalyAtEpoch", orbitNode);
        }

        // PQS
        Dictionary pqsNode = (Dictionary)config["pqs"];

        cBodyProperties.terrainProperties.colour = ConfigUtility.GetVector3("colour", rotationNode, Vector3.One);
        cBodyProperties.terrainProperties.pqsModArr = (Array)pqsNode["pqsMods"];

        // UHHH LGIHTS TLIGHTS
        if (ConfigUtility.TryGetDictionary("light", config, out Dictionary light))
        {
            cBodyProperties.light.brightness = (float)ConfigUtility.GetValue("brightness", light, -1);
            if (ConfigUtility.TryGetArray("colour", light, out Array colour))
            {
                cBodyProperties.light.colour = new Color((float)colour[0], (float)colour[1], (float)colour[2], 1);
            }
        }

        return cBodyProperties;
    }
}
