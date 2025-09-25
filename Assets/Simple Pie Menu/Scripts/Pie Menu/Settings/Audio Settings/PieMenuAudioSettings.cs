using System.Collections.Generic;
using UnityEngine;

namespace SimplePieMenu
{
    [ExecuteInEditMode]
    public class PieMenuAudioSettings : MonoBehaviour
    {
        [Range(0f, 1f)] [SerializeField] float volume = 1f;

        public float Volume
        {
            get { return volume; }
        }

        public int HoverClipsDropdownList { get; private set; }
        public int ClickClipsDropdownList { get; private set; }

        public List<string> MouseHoverClipNames { get; private set; } = new();
        public List<string> MouseClickClipNames { get; private set; } = new();

        public PieMenu PieMenu { get; private set; }
        public PieMenuAudioSettingsHandler AudioHandler { get; private set; }

        private void OnEnable()
        {
            volume = 1f;
            PieMenu = GetComponent<PieMenu>();
            PieMenu.OnComponentsInitialized += InitializeAudioSettings;
        }

        private void OnDisable()
        {
            PieMenu.OnComponentsInitialized -= InitializeAudioSettings;
        }

        public void ApplyVolume()
        {
            if (AudioHandler == null || PieMenu == null) return;
            if (AudioManager.Instance != null)
            {
                float globalVolume = AudioManager.Instance.GetSFXVolume();
                Debug.Log("value of sound " + globalVolume);
                SelectRightSoundsInTheLists(globalVolume);
            }
        }

        public void CreateHoverDropdownList(int list)
        {
            HoverClipsDropdownList = list;
        }

        public void CreateClickDropdownList(int list)
        {
            ClickClipsDropdownList = list;
        }

        private void InitializeAudioSettings()
        {
            AudioHandler = PieMenuShared.References.AudioSettingsHandler;

            InitializeSoundList(AudioHandler.MouseHoverClips, MouseHoverClipNames);
            InitializeSoundList(AudioHandler.MouseClickClips, MouseClickClipNames);

            SelectRightSoundsInTheLists();

            ApplyVolume();
        }

        private void InitializeSoundList(List<AudioClip> sourceList, List<string> targetList)
        {
            foreach (AudioClip audioClip in sourceList)
            {
                targetList.Add(audioClip.name);
            }
        }

        private void SelectRightSoundsInTheLists(float value = 0)
        {
            var template = PieMenu.GetTemplate();
            var templateSource = template != null ? template.GetComponent<AudioSource>() : null;
            if (templateSource != null)
            {
                AudioClip hoverClip = templateSource.clip;
                Debug.Log($"[PieMenuAudioSettings] TemplateSource Volume={templateSource.volume}," +
                $" Object={templateSource.gameObject.name}", templateSource.gameObject);

                PieMenu.PieMenuInfo.SetMouseHoverClip(hoverClip);
                HoverClipsDropdownList = AudioHandler.MouseHoverClips.IndexOf(hoverClip);
            }

            var clickSource = PieMenu.PieMenuElements != null ? PieMenu.PieMenuElements.MouseClickAudioSource : null;
            if (clickSource != null)
            {
                AudioClip clickClip = clickSource.clip;
                PieMenu.PieMenuInfo.SetMouseClickClip(clickClip);
                ClickClipsDropdownList = AudioHandler.MouseClickClips.IndexOf(clickClip);
            }
        }
    }
}
