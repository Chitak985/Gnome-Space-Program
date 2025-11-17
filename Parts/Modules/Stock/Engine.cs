using Godot;
using Godot.Collections;
using System;

/* Engine characteristics to use for example
-- Rocketdyne F-1 (main Saturn V engine, 5 were used, characteristics shown for 1) --
isp = 304
propellant1 = LiquidFuel
propellant2 = Oxidizer
thrust = 7770
chamberPressure = 70
fuelCOnsumption1 = 788
fuelCOnsumption2 = 1001
mass = 8400
*/

/*
    This part module is BUILT IN to GSP.
    As such, this module should remain internal and not be compiled as a mod.
*/
public partial class Engine : PartModule
{
    public float isp = 0.0f;               // The engine's specific impulse in vacuum
    public string propellant1 = null;      // Propellant type 1 used for the engine
    public string propellant2 = null;      // Propellant type 2 used for the engine
    public float thrust = 0.0f;            // Engine's thrust in vacuum
    public float chamberPressure = 0.0f;   // Internal chamber pressure (used for calculations) (bar)
    public float fuelConsumption1 = 0.0f;  // Fuel consumption per second for Propellant 1
    public float fuelConsumption2 = 0.0f;  // Fuel consumption per second for Propellant 2
    public float mass = 0.0f;              // Mass of the engine (kg)

    public Dictionary<string,Dictionary<string,float>> engineList = new Dictionary<string,Dictionary<string,float>>{
      {
      "rocketdyneF1", new Dictionary<string,float>
        {
          {"isp", 304},
          {"propellant1", "LiquidFuel"},
          {"propellant2", "Oxidizer"},
          {"thrust", 7770.0f},
          {"chamberPressure", 70.0f},
          {"fuelConsumption1", 788.0f},
          {"fuelConsumption2", 1001.0f},
          {"mass", 8400.0f}
        }
      }
    };

    public override void PartInit() 
    {
        
    }

    // Loads an engine from an internal list
    public bool LoadEnginePart(string engineType)
    {
        if (engineList.ContainsKey(engineType))
        {
            // Load the parameters (not checking since the parameters are hard coded anyway, maybe make it in a json and add them in from other mods?)
            isp = engineList[engineType]["isp"];
            propellant1 = engineList[engineType]["propellant1"];
            propellant2 = engineList[engineType]["propellant2"];
            thrust = engineList[engineType]["thrust"];
            chamberPressure = engineList[engineType]["chamberPressure"];
            fuelConsumption1 = engineList[engineType]["fuelConsumption1"];
            fuelConsumption2 = engineList[engineType]["fuelConsumption2"];
            mass = engineList[engineType]["mass"];
        }
        else
        {
          GD.Print("PartModule[Engine]: Loading failed! (engineType is "+engineType+", which is invalid)");
          return false;
        }
    }

    // Loads an engine from provided values (2 added at the end of each variable)
    public bool LoadEnginePart(float isp2, string propellant12, string propellant22, float thrust2, float chamberPressure2, float fuelConsumption12, float fuelConsumption22, float mass2)
    {
        isp = isp2;
        propellant1 = propellant12;
        propellant2 = propellant22;
        thrust = thrust2;
        chamberPressure = chamberPressure2;
        fuelConsumption1 = fuelConsumption12;
        fuelConsumption2 = fuelConsumption22;
        mass = mass2;
    }
}
