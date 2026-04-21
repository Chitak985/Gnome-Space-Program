using System;
using Godot;
using Godot.Collections;

// Major changes will need to be done here for the visual update
public partial class FastNoise3D : PQSMod
{
    public float deformity;
    public float offset;
    public FastNoiseLite noise;

    public override float SamplePoint(Vector3 position)
    {
        float height = (float)(noise.GetNoise3D(position.X*10, position.Y*10, position.Z*10) + 1.0f) * 0.5f;
        return height * deformity + offset;
    }

    public void Initialize(Dictionary dict, string path)
    {
        noise = new();

        string noiseType = (string)ConfigUtility.GetValue("type", dict);
        // dict.TryGetValue("type", out var ntp) ? (string)ntp : PlanetSystem.MissingString(path, "pqs/mods/fastNoise3D/type");

        // Select noise type
        noise.NoiseType = noiseType switch
        {
            "simplex" => FastNoiseLite.NoiseTypeEnum.Simplex,
            "simplexSmooth" => FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
            "cellular" => FastNoiseLite.NoiseTypeEnum.Cellular,
            "perlin" => FastNoiseLite.NoiseTypeEnum.Perlin,
            "valueCubic" => FastNoiseLite.NoiseTypeEnum.ValueCubic,
            "value" => FastNoiseLite.NoiseTypeEnum.Value,
            _ => FastNoiseLite.NoiseTypeEnum.Perlin,
        };

        noise.Seed = (int)ConfigUtility.GetValue("seed", dict, 42);
        deformity = (float)ConfigUtility.GetValue("deformity", dict);
        offset = (float)ConfigUtility.GetValue("offset", dict);
        noise.Offset = ConfigUtility.GetVector3("noiseOffset", dict, Vector3.Zero);
        noise.Frequency = (float)ConfigUtility.GetValue("frequency", dict) / 10000f;

        // Fractal noise
        if (ConfigUtility.TryGetDictionary("fractal", dict, out Dictionary fractal))
        {
            // Select fractal type
            string fractalType = (string)ConfigUtility.GetValue("type", fractal);
            noise.FractalType = fractalType switch
            {
                "fbm" => FastNoiseLite.FractalTypeEnum.Fbm,
                "ridged" => FastNoiseLite.FractalTypeEnum.Ridged,
                "pingPong" => FastNoiseLite.FractalTypeEnum.PingPong,
                _ => FastNoiseLite.FractalTypeEnum.None,
            };
            noise.FractalOctaves = (int)ConfigUtility.GetValue("octaves", fractal, 10);
            noise.FractalLacunarity = (double)ConfigUtility.GetValue("lacunarity", fractal, 2);
            noise.FractalGain = (double)ConfigUtility.GetValue("gain", fractal, 0.5);
            noise.FractalWeightedStrength = (double)ConfigUtility.GetValue("weightedStrength", fractal, 0);
            
            if (ConfigUtility.TryGetDictionary("pingPongParams", fractal, out Dictionary pingPong))
            {
                noise.FractalPingPongStrength = (double)ConfigUtility.GetValue("pingPongStrength", pingPong, 2);
            }
        }

        // Cellular shit
        if (ConfigUtility.TryGetDictionary("cellular", dict, out Dictionary cellular))
        {
            string distFunc = (string)ConfigUtility.GetValue("distanceFunction", cellular);
            noise.CellularDistanceFunction = distFunc switch
            {
                "euclidean" => FastNoiseLite.CellularDistanceFunctionEnum.Euclidean,
                "euclidianSquared" => FastNoiseLite.CellularDistanceFunctionEnum.EuclideanSquared,
                "manhattan" => FastNoiseLite.CellularDistanceFunctionEnum.Manhattan,
                "hybrid" => FastNoiseLite.CellularDistanceFunctionEnum.Hybrid,
                _ => FastNoiseLite.CellularDistanceFunctionEnum.Euclidean,
            };

            noise.CellularJitter = (double)ConfigUtility.GetValue("jitter", cellular);

            string returnType = (string)ConfigUtility.GetValue("returnType", cellular);
            noise.CellularReturnType = returnType switch
            {
                "cellValue" => FastNoiseLite.CellularReturnTypeEnum.CellValue,
                "distance" => FastNoiseLite.CellularReturnTypeEnum.Distance,
                "distance2" => FastNoiseLite.CellularReturnTypeEnum.Distance2,
                "distance2Add" => FastNoiseLite.CellularReturnTypeEnum.Distance2Add,
                "distance2Sub" => FastNoiseLite.CellularReturnTypeEnum.Distance2Sub,
                "distance2Mul" => FastNoiseLite.CellularReturnTypeEnum.Distance2Mul,
                "distance2Div" => FastNoiseLite.CellularReturnTypeEnum.Distance2Div,
                _ => FastNoiseLite.CellularReturnTypeEnum.Distance,
            };
        }
    }
}
