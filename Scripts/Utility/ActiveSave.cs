using Godot;
using System;
using System.Collections.Generic;

// All major save data is stored here (crafts, celestials, etc)

public partial class ActiveSave : Node3D
{
	public static readonly string classTag = "([color=orange]ActiveSave[color=white])";
	public static ActiveSave Instance { get; private set; }
	[Export] public CelestialBodyManager celestialBodyManager;
	[Export] public PartManager partManager;
	[Export] public ColonyManager colonyManager;
	[Export] public FlightCamera flightCam;
	[Export] public StateManager stateManager;

    // Spaces
    [Export] public LocalSpace localSpace;
	[Export] public ScaledSpace scaledSpace;
	[Export] public MapView mapSpace;

	// Please NEVER include negative numbers
    [Export] public Godot.Collections.Array<double> timeSpeedLevels;
    [Export] public int maxPhysicsSpeedLevel; // Maximum level where physics still applies
    [Export] private double timeSpeedChangeDuration = 2.0;
	// This should always be 1.0 upon loading!
	[Export] public double timeSpeed = 1;
    public int TimeSpeedLevel { get; private set; }
    public bool TimeSpeedAtSafeLevel { get; private set; } = true;
    public bool TimePaused { get; private set; }

    // Inputs
    [Export] private StringName accelTimeEvent;
    [Export] private StringName deccelTimeEvent;

    // The great dictionary
    public Dictionary<string, Variant> saveParams;

    // In seconds
    public double SaveTime { get; private set; }
	public double UnscaledDelta { get; private set; }

    // Signals
    [Signal] public delegate void GameInitCompleteEventHandler();
    [Signal] public delegate void TimeLevelChangedEventHandler(int level);
    [Signal] public delegate void TimeLevelSafeStateEventHandler(); // Emits when the time level is safe enough to return to normal physics sim

    private double previousEngineTime;
	

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
	{
        Logger.Print($"{classTag} Active save starting...");

		foreach (KeyValuePair<string, Variant> param in saveParams)
		{
			Logger.Print(param);
		}
		Logger.Print($"{classTag} Active save ready for init!");
	}

	public override void _Process(double delta)
	{
        // Custom delta ohh yeahh
        double engineTime = Time.GetTicksUsec();
        UnscaledDelta = (engineTime - previousEngineTime) / 1000000.0;
        previousEngineTime = Time.GetTicksUsec();

        double trueTimeSpeed = timeSpeed;
        if (TimePaused) trueTimeSpeed = 0;

        // Increment time since save creation (for orbital calculations mostly)
        SaveTime += UnscaledDelta * 1000 * trueTimeSpeed / 1000;

        // Set physics speed to match time speed
        Engine.TimeScale = trueTimeSpeed;

        // For craft easing
        if (!TimeSpeedAtSafeLevel && timeSpeed <= timeSpeedLevels[maxPhysicsSpeedLevel-1])
        {
            TimeSpeedAtSafeLevel = true;
            EmitSignal(SignalName.TimeLevelSafeState);
        }else if (timeSpeed > timeSpeedLevels[maxPhysicsSpeedLevel-1]){
            TimeSpeedAtSafeLevel = false;
        }
    }

	// Start up all vital systems such as the planet system and whatnot
	public void InitSave()
	{
        // Initialize game state
        Logger.Print($"{classTag} Starting StateManager");
        stateManager.Initialize();

		// Initialize the planets
		Logger.Print($"{classTag} Starting PlanetSystem");
		Dictionary<string, PlanetPack> planetPacks = SaveManager.GetPlanetPacks();
		string chosenRootSystem = (string)saveParams["Celestial Bodies/Root System"];

		// !!! ADD EXTRA SYSTEMS IMPLEMENTATION WHEN RELEVANT !!!
		List<string> planetPackPaths = [];
		planetPackPaths.Add(planetPacks[chosenRootSystem].path);
		celestialBodyManager.CreateCBodiesFromConfigs(planetPackPaths);

		// Handle part packs
		Dictionary<string, PartPack> partPacks = SaveManager.GetPartPacks();
        List<PartPack> pPacksToLoad = [];

        // Yes hello welcome to hell.
        foreach (KeyValuePair<string, PartPack> partPack in partPacks)
		{
			// Ideally we don't want to use display names for this
			if (((Godot.Collections.Array<string>)saveParams["Parts/Selected Part Packs"]).Contains(partPack.Value.displayName))
			{
                //Logger.Print($"{classTag} Loading part pack '{partPack.Value.displayName}'...");
                pPacksToLoad.Add(partPack.Value);
            }
		}

		Logger.Print($"{classTag} Starting PartManager");
        partManager.LoadPartModules();
        // Start it
        partManager.LoadPartPacks(pPacksToLoad);

		Logger.Print($"{classTag} Starting ColonyManager");
        colonyManager.Initialize(planetPacks);

        // Loop over all the sweet new colonies we just got
        foreach (Colony colony in colonyManager.colonies)
		{
            Logger.Print($"{colony.name}, {colony.initialBase}");
            // Spawn at the colony marked as "initial"
            if (colony.initialBase)
			{
                Logger.Print($"{classTag} Loading into default colony '{colony.name}'");
                colony.Enter();
                break;
            }
		}

		// Activate local input after the game starts because it doesn't take effect if enabled by default in the editor for some reason
        localSpace.Viewport.HandleInputLocally = false;
        localSpace.Viewport.HandleInputLocally = true;

        EmitSignal(SignalName.GameInitComplete);
    }

	// Level must be below the length of timeSpeedLevels
	public void SetTimeSpeed(int level, double duration)
	{
        TimeSpeedLevel = level;
        double targetTimeSpeed = timeSpeedLevels[level];
        EmitSignal(SignalName.TimeLevelChanged, targetTimeSpeed);

        Tween tween = CreateTween();
        tween.SetIgnoreTimeScale(true);
        tween.SetTrans(Tween.TransitionType.Linear);
        tween.TweenProperty(this, "timeSpeed", targetTimeSpeed, duration);
    }

    public void SetTimeSpeed(int level)
    {
        SetTimeSpeed(level, timeSpeedChangeDuration);
    }

    public void Pause(bool toggle)
    {
        TimePaused = toggle;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // Time accel keys
        if (@event.IsActionPressed(accelTimeEvent))
        {
            if (TimeSpeedLevel < timeSpeedLevels.Count)
            {
                SetTimeSpeed(TimeSpeedLevel + 1, 0.01);
            }
        }
        if (@event.IsActionPressed(deccelTimeEvent))
        {
            if (TimeSpeedLevel > 0)
            {
                SetTimeSpeed(TimeSpeedLevel - 1, 0.01);
            }
        }
    }

    // Important function
    public static void SendThoughtsAndPrayers()
    {
        Logger.Print($"{classTag} Thoughts and prayers sent.");
    }
}
