using Godot;

public partial class PerformanceMenu : Control
{
    [Export] public RichTextLabel performanceLabel;
    [Export] public Button logDumpButton;

    public override void _Ready()
    {
        logDumpButton.Pressed += DumpToLog;
    }

    public override void _Process(double delta)
    {
        performanceLabel.Text = GetText();
    }

    public static void DumpToLog()
    {
        Logger.Print($"Performance stats dump: \n{GetText()}");
    }

    public static string GetText()
    {
        string compositeText = "";

        compositeText += $"FPS: {Engine.GetFramesPerSecond()} \n";
        compositeText += $"Frame time: {Performance.GetMonitor(Performance.Monitor.TimeProcess)} seconds(?)\n";
        compositeText += $"Phys. frame time: {Performance.GetMonitor(Performance.Monitor.TimePhysicsProcess)} seconds(?)\n";
        compositeText += $"Static mem. use: {Performance.GetMonitor(Performance.Monitor.MemoryStatic)} bytes\n";
        compositeText += $"Static mem. max: {Performance.GetMonitor(Performance.Monitor.MemoryStaticMax)} bytes\n";
        compositeText += $"Message buffer max: {Performance.GetMonitor(Performance.Monitor.MemoryMessageBufferMax)} bytes\n";
        compositeText += $"Object count: {Performance.GetMonitor(Performance.Monitor.ObjectCount)} \n";
        compositeText += $"Resource count: {Performance.GetMonitor(Performance.Monitor.ObjectResourceCount)} \n";
        compositeText += $"Node count: {Performance.GetMonitor(Performance.Monitor.ObjectNodeCount)} \n";
        compositeText += $"Orphan node count: {Performance.GetMonitor(Performance.Monitor.ObjectOrphanNodeCount)} \n";
        compositeText += $"Objects in frame: {Performance.GetMonitor(Performance.Monitor.RenderTotalObjectsInFrame)} \n";
        compositeText += $"Primitives in frame: {Performance.GetMonitor(Performance.Monitor.RenderTotalPrimitivesInFrame)} \n";
        compositeText += $"Draw calls in frame: {Performance.GetMonitor(Performance.Monitor.RenderTotalDrawCallsInFrame)} \n";
        compositeText += $"Video mem. use: {Performance.GetMonitor(Performance.Monitor.RenderVideoMemUsed)} bytes\n";
        compositeText += $"Texture mem. use: {Performance.GetMonitor(Performance.Monitor.RenderTextureMemUsed)} bytes\n";
        compositeText += $"Buffer mem. use: {Performance.GetMonitor(Performance.Monitor.RenderBufferMemUsed)} bytes\n";
        compositeText += $"Physics3D active objs: {Performance.GetMonitor(Performance.Monitor.Physics3DActiveObjects)} \n";
        compositeText += $"Physics3D coll. pairs: {Performance.GetMonitor(Performance.Monitor.Physics3DCollisionPairs)} \n";
        compositeText += $"Physics3D island count: {Performance.GetMonitor(Performance.Monitor.Physics3DIslandCount)} \n";

        return compositeText;
    }
}
