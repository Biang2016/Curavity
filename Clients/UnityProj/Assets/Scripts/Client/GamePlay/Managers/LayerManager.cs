using BiangLibrary.Singleton;
using UnityEngine;

public class LayerManager : TSingletonBaseManager<LayerManager>
{
    public int LayerMask_UI;
    public int LayerMask_Step;
    public int LayerMask_Spike;
    public int LayerMask_Player;
    public int LayerMask_CheckPoint;
    public int LayerMask_DeadZone;
    public int LayerMask_Star;

    public int Layer_UI;
    public int Layer_Step;
    public int Layer_Spike;
    public int Layer_Player;
    public int Layer_CheckPoint;
    public int Layer_DeadZone;
    public int Layer_Star;

    public override void Awake()
    {
        LayerMask_UI = LayerMask.GetMask("UI");
        LayerMask_Step = LayerMask.GetMask("Step");
        LayerMask_Spike = LayerMask.GetMask("Spike");
        LayerMask_Player = LayerMask.GetMask("Player");
        LayerMask_CheckPoint = LayerMask.GetMask("CheckPoint");
        LayerMask_DeadZone = LayerMask.GetMask("DeadZone");
        LayerMask_Star = LayerMask.GetMask("Star");

        Layer_UI = LayerMask.NameToLayer("UI");
        Layer_Step = LayerMask.NameToLayer("Step");
        Layer_Spike = LayerMask.NameToLayer("Spike");
        Layer_Player = LayerMask.NameToLayer("Player");
        Layer_CheckPoint = LayerMask.NameToLayer("CheckPoint");
        Layer_DeadZone = LayerMask.NameToLayer("DeadZone");
        Layer_Star = LayerMask.NameToLayer("Star");
    }
}