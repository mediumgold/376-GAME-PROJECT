using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;

public class OptionSelectorUI : MonoBehaviour
{
    private TMP_Text textBox;
    private List<string> options;
    private int selectedIndex;
    private Action<int> callback;
    private bool active;

    public void Initialize(TMP_Text sharedTextBox)
    {
        textBox = sharedTextBox;
    }

    public void ShowOptions(string[] newOptions, Action<int> onConfirm)
    {
        options = new List<string>(newOptions);
        callback = onConfirm;
        selectedIndex = 0;
        active = true;

        Redraw();
    }

    public void MoveSelection(int direction)
    {
        if (!active) return;

        selectedIndex += direction;

        if (selectedIndex < 0) selectedIndex = options.Count - 1;
        if (selectedIndex >= options.Count) selectedIndex = 0;

        Redraw();
    }

    public void Confirm()
    {
        if (!active) return;

        Debug.Log("CONFIRMING OPTION: " + options[selectedIndex]);

        active = false;

        callback?.Invoke(selectedIndex);
    }

    private void Redraw()
    {
        string txt = "";

        for (int i = 0; i < options.Count; i++)
        {
            if (i == selectedIndex)
                txt += $"> {options[i]}\n";
            else
                txt += $"  {options[i]}\n";
        }

        textBox.text = txt;
    }
}
