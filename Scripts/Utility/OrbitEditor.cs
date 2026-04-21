using Godot;
using System;
using System.Collections.Generic;

public partial class OrbitEditor : Panel
{
    [Export] private Label modeLabel;
    [Export] private OptionButton objectDropdown;
    [Export] private OptionButton parentDropdown;
    [Export] private SpinBox semiMajorAxis;
    [Export] private SpinBox eccentricity;
    [Export] private SpinBox inclination;
    [Export] private SpinBox argumentOfPeriapsis;
    [Export] private SpinBox longitudeOfAscendingNode;
    [Export] private SpinBox trueAnomaly;
    [Export] private SpinBox meanAnomalyAtEpoch;
    
    [Export] private Mode mode;

    //private Craft selectedCraft;
    private CelestialBody selectedCBody;
    private CelestialBody selectedParent;

    private List<CelestialBody> cBodies;

    private enum Mode
    {
        Craft,
        CelestialBody
    }

    private void Update()
    {
        if (CelestialBodyManager.Instance != null && IsVisibleInTree())
        {
            cBodies = [];
            parentDropdown.Clear();
            foreach (CelestialBody cBody in CelestialBodyManager.Instance.CelestialBodies)
            {
                parentDropdown.AddItem(cBody.Name);
                cBodies.Add(cBody);
            }

            if (mode == Mode.Craft)
            {

            }else if (mode == Mode.CelestialBody){
                //foreach (CelestialBody cBody in PlanetSystem.Instance.celestialBodies)
                //{
                //    objectDropdown.AddItem(cBody.Name);
                //}
            }

            modeLabel.Text = $"Mode: {mode}";
        }
        
    }

    private void UpdateSelections()
    {
        selectedParent = cBodies[parentDropdown.Selected];
        Logger.Print($"Selected CBody {selectedParent} as parent!");
    }

    private KeplerianState.KeplerianElements CreateOrbit()
    {
        KeplerianState.KeplerianElements orbit = new()
        {
            semiMajorAxis = semiMajorAxis.Value,
            eccentricity = eccentricity.Value,
            inclination = inclination.Value,
            argumentOfPeriapsis = argumentOfPeriapsis.Value,
            longitudeOfAscendingNode = longitudeOfAscendingNode.Value,
            trueAnomaly = trueAnomaly.Value,
            meanAnomalyAtEpoch = meanAnomalyAtEpoch.Value,
        };

        return orbit;
    }

    private void ApplyOrbit()
    {
        UpdateSelections();

        KeplerianState.KeplerianElements orbit = CreateOrbit();

        if (mode == Mode.Craft)
        {
            //StateManager.Instance.CurrentFlightState.activeCraft.SetOrbitDriver(driver);
            StateManager.Instance.CurrentFlightState.activeCraft.SetTransformFromCartesian();
        }else if (mode == Mode.CelestialBody){
            //selectedCBody.SetOrbitDriver(driver);
        } 
    }

    private void OnSwitchPressed()
    {
        mode = mode.Next();
        Update();
    }
}
