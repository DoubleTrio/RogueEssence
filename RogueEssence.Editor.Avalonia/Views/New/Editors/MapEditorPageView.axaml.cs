using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RogueEssence.Data;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Views;

public partial class MapEditorPageView : UserControl, IMapEditor
{
    public bool Active { get; private set; }
    public UndoStack Edits { get; }
    
    public MapEditorPageView()
    {
        InitializeComponent();
    }
    
    public void ProcessInput(InputManager input)
    {
        DevForm.ExecuteOrInvoke(() => ((MapEditorPageViewModel)DataContext).ProcessInput(input));
    }
        
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Active = true;
    }

    // public void Window_Loaded(object sender, EventArgs e)
    // {
    //     Active = true;
    // }

    private bool silentClose;
    public void SilentClose()
    {
        silentClose = true;
        // Close();
    }
    
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Active = false;
        // CloseChildren();
        if (!silentClose)
            GameManager.Instance.SceneOutcome = exitMapEdit();
    }


    // public void Window_Closed(object sender, EventArgs e)
    // {
    //     Active = false;
    //     CloseChildren();
    //     if (!silentClose)
    //         GameManager.Instance.SceneOutcome = exitMapEdit();
    // }


    private IEnumerator<YieldInstruction> exitMapEdit()
    {
        DevForm form = (DevForm)DiagManager.Instance.DevEditor;
        form.MapEditForm = null;

        //move to the previous scene or the title, if there was none
        if (DataManager.Instance.Save != null && DataManager.Instance.Save.NextDest.IsValid())
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.MoveToZone(DataManager.Instance.Save.NextDest, true, false));
        else
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.RestartToTitle());
    }
}