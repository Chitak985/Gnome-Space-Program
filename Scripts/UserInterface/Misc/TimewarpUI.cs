using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class TimewarpUI : Panel
{
    [Export] private PackedScene buttonPrefab;
    [Export] private bool addInReverse = true;
    [Export] private Container buttonContainer;
    [Export] private RichTextLabel indicator;
    private List<TimewarpButton> buttons;

    public override void _Ready()
    {
        Logger.Print(ActiveSave.Instance);
        Array<double> timeLevels = ActiveSave.Instance.timeSpeedLevels;
        if (addInReverse)
        {
            for (int i = timeLevels.Count-1; i >= 0; i--)
            {
                AddButton(i);
            }
        }else{
            for (int i = 0; i < timeLevels.Count; i++)
            {
                AddButton(i);
            }
        }
    }

    public override void _Process(double delta)
    {
        indicator.Text = $"x{Math.Round(ActiveSave.Instance.timeSpeed, 2).KiloFormat()}";
    }

    private void OnButtonPressed(int level)
    {
        ActiveSave.Instance.SetTimeSpeed(level);
    }

    private void AddButton(int level)
    {
        TimewarpButton button = (TimewarpButton)buttonPrefab.Instantiate();
        buttonContainer.AddChild(button);
        button.level = level;
        button.OnTimewarpClicked += OnButtonPressed;
    }
}
