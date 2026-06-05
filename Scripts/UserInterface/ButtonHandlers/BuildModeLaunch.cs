using Godot;
using Godot.Collections;

public partial class BuildModeLaunch : Button
{
    public LaunchSite launchsite;
    public override void _Ready()
    {
        Pressed += Launch;
    }

    public void Launch()
    {
        // Get the current list of parts in the editor (you can move this to the LaunchSite module if you want)
        Dictionary partData = BuildingManager.Instance.centralPart.GetData();
  
        Logger.Print(partData);

        launchsite.SpawnCraft(partData, true);
    }
}
