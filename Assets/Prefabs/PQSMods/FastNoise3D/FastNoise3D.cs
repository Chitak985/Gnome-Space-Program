using Godot;
using Godot.Collections;

// Major changes will need to be done here for the visual update
public partial class FastNoise3D : Node
{
    public float deformity;
    public float offset;
    public FastNoiseLite noise;

    public float SamplePoint(Vector3 position)
    {
        float height = noise.GetNoise3D(position.X, position.Y, position.Z);
        return height * deformity + offset;
    }

    public void Initialize(Dictionary dict, string path)
    {
        noise = new();
        
        string noiseType = dict.TryGetValue("type", out var ntp) ? (string)ntp : PlanetSystem.MissingString(path, "pqs/mods/fastNoise3D/type");
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
        noise.Seed = dict.TryGetValue("seed", out var sed) ? (int)sed : (int)PlanetSystem.MissingNum(path, "pqs/mods/fastNoise3D/type");
        deformity = dict.TryGetValue("deformity", out var def) ? (float)def : (float)PlanetSystem.MissingNum(path, "pqs/mods/fastNoise3D/deformity");
        offset = dict.TryGetValue("offset", out var off) ? (float)off : (float)PlanetSystem.MissingNum(path, "pqs/mods/fastNoise3D/offset");
        // convert array to vector3 offset
        if (ConfigUtility.TryGetArray("noiseOffset", dict, out Array offs))
        {
            noise.Offset = new Vector3((float)offs[0],(float)offs[1],(float)offs[2]);
        }else{
            noise.Offset = new Vector3(0,0,0);
        }
        noise.Frequency = dict.TryGetValue("frequency", out var frq) ? (float)frq : (float)PlanetSystem.MissingNum(path, "pqs/mods/fastNoise3D/frequency") / 10000f;

        // Fractal noise
        if (ConfigUtility.TryGetDictionary("fractal", dict, out Dictionary fractal))
        {
            // Select fractal type
            string fractalType = fractal.TryGetValue("type", out var ftp) ? (string)ftp : PlanetSystem.MissingString(path, "pqs/mods/fastNoise3D/fractal/type");
            noise.FractalType = fractalType switch
            {
                "fbm" => FastNoiseLite.FractalTypeEnum.Fbm,
                "ridged" => FastNoiseLite.FractalTypeEnum.Ridged,
                "pingPong" => FastNoiseLite.FractalTypeEnum.PingPong,
                _ => FastNoiseLite.FractalTypeEnum.None,
            };
            noise.FractalOctaves = fractal.TryGetValue("octaves", out var oct) ?
                (int)oct :
                (int)PlanetSystem.MissingNum(path, "pqs/mods/fastNoise3D/fractal/octaves");
            noise.FractalLacunarity = fractal.TryGetValue("lacunarity", out var lac) ?
                (int)lac :
                (int)PlanetSystem.MissingNum(path, "pqs/mods/fastNoise3D/fractal/lacunarity");
            noise.FractalGain = fractal.TryGetValue("lacunarity", out var gin) ?
                (float)gin :
                (float)PlanetSystem.MissingNum(path, "pqs/mods/fastNoise3D/fractal/gain");
            noise.FractalWeightedStrength = fractal.TryGetValue("weightedStrength", out var wsr) ?
                (float)wsr :
                (float)PlanetSystem.MissingNum(path, "pqs/mods/fastNoise3D/fractal/weightedStrength");
            
            if (ConfigUtility.TryGetDictionary("pingPongParams", fractal, out Dictionary pingPong))
            {
                noise.FractalPingPongStrength = pingPong.TryGetValue("pingPongStrength", out var pps) ?
                    (float)pps :
                    (float)PlanetSystem.MissingNum(path, "pqs/mods/fastNoise3D/fractal/pingPongParams/pingPongStrength");
            }
        }

        // Cellular shit
        if (ConfigUtility.TryGetDictionary("cellular", dict, out Dictionary cellular))
        {
            string distFunc = cellular.TryGetValue("distanceFunction", out var dtf) ? 
                (string)dtf :
                PlanetSystem.MissingString(path, "pqs/mods/fastNoise3D/cellular/distanceFunction");
            noise.CellularDistanceFunction = distFunc switch
            {
                "euclidean" => FastNoiseLite.CellularDistanceFunctionEnum.Euclidean,
                "euclidianSquared" => FastNoiseLite.CellularDistanceFunctionEnum.EuclideanSquared,
                "manhattan" => FastNoiseLite.CellularDistanceFunctionEnum.Manhattan,
                "hybrid" => FastNoiseLite.CellularDistanceFunctionEnum.Hybrid,
                _ => FastNoiseLite.CellularDistanceFunctionEnum.Euclidean,
            };

            noise.CellularJitter = cellular.TryGetValue("jitter", out var jtt) ? 
                (float)jtt :
                (float)PlanetSystem.MissingNum(path, "pqs/mods/fastNoise3D/cellular/jitter");

            string returnType = cellular.TryGetValue("returnType", out var rtp) ? 
                (string)rtp :
                PlanetSystem.MissingString(path, "pqs/mods/fastNoise3D/cellular/returnType");
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
