using Godot;
using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

public partial class Console : RichTextLabel
{
    bool ctrlHeld = false;
    public override void _EnterTree()
    {
        // Test code to output a helpful error if building with single precision
        // If this fails, you are likely using a single-precision build of Godot.
        // Refer to BuildingGodotEditor.md.
        Vector2 testVector = new ((double)1, (double)2);
        
        GuiInput += OnInput;
        VisibilityChanged += () => {if(IsVisibleInTree())Logger.Print("[color=cyan]Welcome to the console! ctrl+scroll changes the font size.[color=white]"); };
        Logger.Instance.OnLogged += AddLog;
        AppDomain.CurrentDomain.FirstChanceException += CatchException;

        Text = "";
        Logger.Print("Console ready");
        Logger.Print("[color=ff8080] _______    _______   _______");
        Logger.Print("[color=ffa77f]/          /         /       \\");
        Logger.Print("[color=ffe17f]|   ____   \\______   |_______/");
        Logger.Print("[color=7fff7f]|       \\         \\  |");
        Logger.Print("[color=8080ff]\\_______/  _______/  |");
        Logger.Print("[color=c880ff]Gnome Space Program v0.2");
        Logger.Print("");
    }

    public void AddLog(long time, string content)
    {
        DateTime date = DateTime.FromBinary(time);
        Text += $"[color=676767]{date:HH:mm:ss}[color=white]: {content}\n";
    }

    public void CatchException(object sender, FirstChanceExceptionEventArgs args)
    {
        // God forbid an exception happens in here
        Logger.Print("[color=red]Exception was logged:");
        if (args.Exception is Exception ex)
            AddLog(DateTime.Now.ToBinary(), $"[color=red]{ex}");
    }

    public override void _UnhandledInput(InputEvent inpEvent)
    {
        if (inpEvent is InputEventKey key)
        {
            if (key.Pressed && key.Keycode == Key.Ctrl)
            {
                ctrlHeld = true;
            }else if (!key.Pressed && key.Keycode == Key.Ctrl)
            {
                ctrlHeld = false;
            }
        }
    }

    public void OnInput(InputEvent inpEvent)
    {
        if (inpEvent is InputEventMouseButton mouseMotion && ctrlHeld)
        {
            if (mouseMotion.ButtonIndex == MouseButton.WheelUp)
            {
                AddThemeFontSizeOverride("normal_font_size", GetThemeFontSize("normal_font_size") + 1);
            }
            if (mouseMotion.ButtonIndex == MouseButton.WheelDown)
            {
                AddThemeFontSizeOverride("normal_font_size", GetThemeFontSize("normal_font_size") - 1);
            }
        }
    }
}
