using AK.Wwise;
using BiangLibrary.Singleton;
using UnityEngine;
using Event = AK.Wwise.Event;

public class WwiseAudioManager : MonoSingleton<WwiseAudioManager>
{
    public Event BGM_Start;
    public Event BGM_Stop;
    public Event StopAllEvent;

    public void ShutDown()
    {
        StopAllEvent.Post(gameObject);
    }

    public RTPC Master_Volume;
    public RTPC BGM_Volume;
    public RTPC SFX_Volume;
    public RTPC TouchStepSurfacePitch;
    public RTPC TouchStepSurfaceVolume;

    void Awake()
    {
        if (PlayerPrefs.HasKey("Master_Volume"))
        {
            Master_Volume.SetGlobalValue(PlayerPrefs.GetFloat("Master_Volume"));
        }
        else
        {
            Master_Volume.SetGlobalValue(100f);
        }

        if (PlayerPrefs.HasKey("BGM_Volume"))
        {
            BGM_Volume.SetGlobalValue(PlayerPrefs.GetFloat("BGM_Volume"));
        }
        else
        {
            BGM_Volume.SetGlobalValue(100f);
        }

        if (PlayerPrefs.HasKey("SFX_Volume"))
        {
            SFX_Volume.SetGlobalValue(PlayerPrefs.GetFloat("SFX_Volume"));
        }
        else
        {
            SFX_Volume.SetGlobalValue(100f);
        }
    }

    public Event CheckPoint;
    public Event Dead;
    public Event DeadPong;
    public Event Freeze;
    public Event FreezeSkill;
    public Event Score;
    public Event TouchStepSurface;
}