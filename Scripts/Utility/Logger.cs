using Godot;
using System;

public partial class Logger : Node
{
    public static Logger Instance { get; private set; }

    // Storing logs in memory is stupid fucking idea so im ending this forever
    //public Dictionary<DateTime, string> LoggedMessages = [];

    // Events 
    //public delegate void CatchLog(DateTime time, string content);
    //public static event CatchLog OnLogged;

    // DateTime is converted to a binary integer to be reconstructed because Godot signals don't support DateTime objects
    [Signal] public delegate void OnLoggedEventHandler(long time, string content);

    // The first thing!!
    public override void _EnterTree()
    {
        Instance = this;

        Print("[color=45ffdb]Logger active.");
    }

    public static void Print(object content)
    {
        string text = "Null";
        if (content != null)
        {
            text = content.ToString();
        }

        DateTime time = DateTime.Now;
        //Instance.LoggedMessages.Add(time, text);
        GD.PrintRich($"[color=676767]{time:HH:mm:ss}[color=white]: {text}");
        //OnLogged?.Invoke(time, text);   
        Instance?.EmitSignal(SignalName.OnLogged, time.ToBinary(), text);
    }
}
