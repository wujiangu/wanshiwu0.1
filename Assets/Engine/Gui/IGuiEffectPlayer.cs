using UnityEngine.Playables;
using UnityEngine.Events;

namespace cs
{
    public interface IGuiEffectPlayer
    {
        bool PlayFadeInEffect(DirectorUpdateMode a_eUpdateMode, UnityAction<string> a_onFinished = null);

        bool PlayFadeOutEffect(DirectorUpdateMode a_eUpdateMode, UnityAction<string> a_onFinished = null);

        void PlayEffect(string a_strEffectName, DirectorWrapMode a_eWrapMode, DirectorUpdateMode a_eUpdateMode, UnityAction<string> a_onFinished = null);

        void PauseEffect();

        void ResumeEffect();

        void StopEffect();

        bool IsPlaying();
    }
}

