using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class PlanetSystem : Node3D
{
    [Export] public PackedScene orbitRendererPrefab;
    [Export] public PackedScene planetIconPrefab;
    [Export] public Control planetIcons;

    [Export] public Camera3D localCamera;

    public static PlanetSystem Instance { get; set; }
    // Default config path
    public static readonly string ConfigPath = "res://GameData";

    public static readonly string classTag = "([color=green]PlanetSystem[color=white])";

    [Export] public Node3D localSpace;
    public Node3D localSpacePlanets;
    [Export] public ScaledSpace scaledSpace;
    [Export] public Control orbitRenderers;

    // Since patched conics require an SOI, then have a "root SOI" in the form of a "root body"
    public CelestialBody rootBody;
    // Also track the "focus on load" body to know which one to focus on when the save starts
    public CelestialBody focusOnLoadBody;

    public List<CelestialBody> celestialBodies = [];

    // DEBUG SHART
    [Export] public bool DEBUG_startWithGizmo;
    [Export] public PackedScene DEBUG_GizmoPrefab;
    public void ToggleGizmo(bool toggle)
    {
        foreach (CelestialBody celestialBody in celestialBodies)
        {
            Node3D gizmo;
            if (celestialBody.FindChild("gizmo") == null)
            {
                gizmo = (Node3D)DEBUG_GizmoPrefab.Instantiate();
                gizmo.Scale = Vector3.One * (float)celestialBody.radius * 0.03f;

                foreach (Node node in gizmo.GetChildren())
                {
                    if (node is MeshInstance3D mesh)
                    {
                        mesh.SetLayerMaskValue(1, true);
                        mesh.SetLayerMaskValue(2, true);
                    }
                }

                celestialBody.scaledObject.AddChild(gizmo);
            }else{
                gizmo = (Node3D)celestialBody.FindChild("gizmo");
            }

            gizmo.Visible = toggle;
        }
    }

    // Called when the node enters the scene tree for the first time.
    /*public override void _Ready()
    {
        Instance = this;
        localSpace = (Node3D)GetTree().GetFirstNodeInGroup("LocalSpace");
        localSpacePlanets = (Node3D)localSpace.FindChild("Planets");
        orbitRenderers = (Control)GetTree().GetFirstNodeInGroup("OrbitRenderers");
        CreateSystem(GetPlanetConfigs(ConfigPath));
    }*/

    // Start the Planet System for this particular save
    public void InitSystem(List<string> chosenPacks)
    {
        Instance = this;
        // Get all relevant celestial body configs loaded in this save
        List<string> planetConfigs = [];
        foreach (string pack in chosenPacks)
        {
            string fullPath = $"{ConfigUtility.GameData}/{pack}";
            planetConfigs.AddRange(GetPlanetConfigs(fullPath));
            Logger.Print($"{classTag} Successfully indexed celestial pack '{fullPath}'");
        }
        // Might aswell do this while we're at it
        //localSpace = (Node3D)GetTree().GetFirstNodeInGroup("LocalSpace");
        localSpacePlanets = (Node3D)localSpace.FindChild("Planets");
        //orbitRenderers = (Control)GetTree().GetFirstNodeInGroup("OrbitRenderers");
        CreateSystem(planetConfigs);
        Logger.Print($"{classTag} System created successfully!");

        // Debug pass
        if (DEBUG_startWithGizmo) ToggleGizmo(true);
    }

    public void CreateSystem(List<string> configs)
    {
        foreach (string str in configs)
        {
            //Logger.Print(str);
            CreateCBody(str);
        }
        PostProcessPlanets();
    }

    public CelestialBody FindCBodyByName(string name)
    {
        foreach (CelestialBody cBody in celestialBodies)
        {
            if (cBody.cBodyName == name) return cBody;
        }
        Logger.Print($"{classTag} Couldn't find CelestialBody with name '{name}'");
        return null;
    }

    // Recursive function to get all planet configs in a path
    // Outdated. Use ConfigUtility.GetConfigs() instead.
    public static List<string> GetPlanetConfigs(string path)
    {
        List<string> files = [];
        DirAccess dir = DirAccess.Open(path);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                string filePath = path + "/" + fileName;

                // Check if directory is a valid JSON and if it has the "cBody" configType
                FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
                if (file != null)
                {
                    string content = file.GetAsText();

                    Json jsonFile = new();
                    Error err = jsonFile.Parse(content);
                    Dictionary data = (Dictionary)jsonFile.Data;
                    
                    if (err == Error.Ok && (string)data["configType"] == "cBody")
                    {
                        Logger.Print($"{classTag} JSON found: {filePath}");
                        files.Add(filePath);
                    }else if (err == Error.ParseError)
                    {
                        Logger.Print($"Cannot parse JSON file: {filePath}");
                    }
                }
                
                List<string> subFiles = GetPlanetConfigs(filePath);
                if (subFiles.Count > 0) files.AddRange(subFiles);

                fileName = dir.GetNext();
            }
        }
        return files;
    }

    // Function to set the parent and child planets after all planets are created (because some might be in the wrong order)
    public void PostProcessPlanets()
    {
        foreach (CelestialBody cBody in celestialBodies)
        {
            localSpacePlanets.AddChild(cBody);
            cBody.Name = cBody.cBodyName;

            if (planetIconPrefab != null && planetIcons != null)
            {
                PlanetIcon icon = (PlanetIcon)planetIconPrefab.Instantiate();
                icon.planet = cBody;
                icon.camera = localCamera;
                planetIcons.AddChild(icon);
            }

            if (cBody.focusOnload) focusOnLoadBody = cBody;

            CelestialBody parent = FindCBodyByName(cBody.parentName);
            if (parent != null)
            {
                cBody.orbit.parent = parent;
                cBody.orbit.cBody = cBody;
                if (cBody.orbit.sphereOfInfluence <= 0) // 14959800320 * ((5.289772250524424*10^22) / (1.7565459*10^28))^(2/5)
                    cBody.orbit.sphereOfInfluence = cBody.orbit.semiMajorAxis * Math.Pow(5.289772250524424e22 / 1.7565459e28, 2f/5f);
                cBody.cartesianData.parent = parent;
                cBody.cartesianData.cBody = cBody;
                parent.childPlanets.Add(cBody);

                // Create orbit renderer (AAHHHH)
                OrbitRendererManager rendererManager = OrbitRendererManager.Instance;
                OrbitRenderer renderer = (OrbitRenderer)rendererManager.rendererPrefab.Instantiate();
                rendererManager.orbitRenderers.Add(renderer);
                renderer.orbit = cBody.orbit;
                cBody.orbit.parent.scaledObject.AddChild(renderer);
                renderer.enabled = true;
            }

            // Assign orbital process to RealityTangler
            RealityTangler.Instance.OrbitProcess += cBody.ProcessTransform;
            RealityTangler.Instance.OriginReset += cBody.ResetOrigin;
        }
    }

    public void CreateCBody(string configPath)
    {
        CelestialBody cBody = ParseConfig(configPath);
        // Add cBody to the great planetary list (and set as root body if it is)
        celestialBodies.Add(cBody);
        if (cBody.orbit != null)
        {
            //OrbitRenderer renderer = (OrbitRenderer)orbitRendererPrefab.Instantiate();
            //renderer.cBody = cBody;
            //renderer.camera = ((RemoteCam)GetTree().GetFirstNodeInGroup("Camera")).localCamera;
            //orbitRenderers.AddChild(renderer);
        }
        if (cBody.isRoot) rootBody = cBody;

        cBody.InitializeSelf();
    }

    public static CelestialBody ParseConfig(string path)
    {
        Dictionary data = ConfigUtility.ParseConfig(path);

        CelestialBody cBody = new();

        // Handle "properties" dictionary
        if (ConfigUtility.TryGetDictionary("properties", data, out Dictionary properties))
        {
            cBody.cBodyName = properties.TryGetValue("name", out var name) ? (string)name : MissingString(path, "name"); //GetValue("Properties", "name");
            Logger.Print($"{classTag} Parsing config for: {name}");
            cBody.focusOnload = properties.TryGetValue("focusOnload", out var fuck) && (bool)fuck;
            // only mass or geeASL is required, the unassigned one will be calculated based off one of the values.
            cBody.mass = properties.TryGetValue("mass", out var mass) ? (double)mass : -1;
            cBody.geeASL = properties.TryGetValue("geeASL", out var geeASL) ? (double)geeASL : -1;
            cBody.radius = properties.TryGetValue("radius", out var radius) ? (double)radius : MissingNum(path, "radius");

            if (cBody.mass < 0)
            {
                if (cBody.geeASL < 0) MissingNum(path, "geeASL");
                cBody.mass = cBody.geeASL * Conics.EarthGravity * Mathf.Pow(cBody.radius, 2f) / Conics.GravConstant;
            }
            if (cBody.geeASL < 0)
            {
                if (cBody.mass < 0) MissingNum(path, "mass");
                cBody.geeASL = cBody.mass * Conics.GravConstant / Mathf.Pow(cBody.radius, 2f) / Conics.EarthGravity;
            }
        }else{
            Logger.Print($"Properties dictionary does not exist in planet config {path}");
            return null;
        }

        // Handle rotation data
        if (ConfigUtility.TryGetDictionary("rotation", data, out Dictionary rotation))
        {
            cBody.initialRot = rotation.TryGetValue("initial", out var iRot) ? (double)iRot : MissingNum(path, "orbit/initial");
            cBody.rotPeriod = rotation.TryGetValue("period", out var rot) ? (double)rot : MissingNum(path, "orbit/period");
            if (ConfigUtility.TryGetArray("tilt", rotation, out Godot.Collections.Array tilt))
            {
                cBody.tilt = new Vector3((float)tilt[0],(float)tilt[1],(float)tilt[2]);
            }
        }else{
            Logger.Print($"{classTag} CBody {cBody.cBodyName} is missing its rotation!");
        }

        // The "parent" parameter is set in a different function because the parent might be parsed after this one
        if (ConfigUtility.TryGetDictionary("orbit", data, out Dictionary orbit))
        {
            cBody.parentName = orbit.TryGetValue("parent", out var pnm) ? (string)pnm : null;
            cBody.orbit = new Orbit{
                semiMajorAxis = orbit.TryGetValue("semiMajorAxis", out var sma) ? (double)sma : MissingNum(path, "orbit/semiMajorAxis"),
                inclination = orbit.TryGetValue("inclination", out var inc) ? (double)inc : MissingNum(path, "orbit/inclination"),
                eccentricity = orbit.TryGetValue("eccentricity", out var ecc) ? (double)ecc : MissingNum(path, "orbit/eccentricity"),
                argumentOfPeriapsis = orbit.TryGetValue("argumentOfPeriapsis", out var arp) ? (double)arp : MissingNum(path, "orbit/argumentOfPeriapsis"),
                longitudeOfAscendingNode = orbit.TryGetValue("longitudeOfAscendingNode", out var lon) ? (double)lon : MissingNum(path, "orbit/longitudeOfAscendingNode"),
                trueAnomaly = orbit.TryGetValue("trueAnomaly", out var tra) ? (double)tra : 0,
                trueAnomalyAtEpoch = orbit.TryGetValue("trueAnomalyAtEpoch", out var tre) ? (double)tre : MissingNum(path, "orbit/trueAnomalyAtEpoch"),
                sphereOfInfluence = orbit.TryGetValue("sphereOfInfluence", out var soi) ? (double)soi : -1,
            };
        }else{
            Logger.Print($"{classTag} CBody {cBody.cBodyName} is missing its orbit! If this is intended, then disregard this message.");
        }
        // Same here too
        cBody.cartesianData = new()
        {
            position = Vector3.Zero,
            velocity = Vector3.Zero
        };

        // Oceans
        if (ConfigUtility.TryGetDictionary("ocean", data, out Dictionary ocean))
        {
            
        }

        // PQS
        if (ConfigUtility.TryGetDictionary("pqs", data, out Dictionary pqs))
        {
            if (ConfigUtility.TryGetArray("pqsMods", pqs, out Godot.Collections.Array pqsMods))
            {
                // Initialize pqs mods for the cBody
                cBody.pqsMods = [];
                foreach (var pqMod in pqsMods)
                {
                    Dictionary pqsMod = (Dictionary)pqMod;
                    string pqsModType = pqsMod.TryGetValue("mod", out var mtp) ? (string)mtp : MissingString(path, "pqs/mods/mod");

                    switch (pqsModType)
                    {
                        case "fastNoise3D":
                            FastNoise3D mod = new();
                            mod.Initialize(pqsMod, path);
                            cBody.pqsMods.Add(mod);
                            break;
                        default:
                            Logger.Print($"Unknown PQS mod type in {path}");
                            break;
                    }
                }
            }
        }

        cBody.isRoot = data.TryGetValue("rootBody", out var rbd) && (bool)rbd;

        return cBody;
    }

    // Throw missing key errors when parsing configs
    public static string MissingString(string path, string key)
    {
        Logger.Print($"Parsing error in planet config {path}");
        Logger.Print($"Key {key} is missing!");

        return "Epic Fail :sob:";
    }
    public static double MissingNum(string path, string key)
    {
        Logger.Print($"Parsing error in planet config {path}");
        Logger.Print($"Key {key} is missing!");

        return double.NaN;
    }
}
