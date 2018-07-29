using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonSound : MonoBehaviour, IPointerDownHandler {
    
    public void OnPointerDown(PointerEventData eventData)
    {
        AudioSource aduioSource = Camera.main.GetComponent<AudioSource>();
        if (aduioSource)
        {
            AudioClip clip = (AudioClip)Resources.Load(StringAssets.soundPath + "button", typeof(AudioClip));
            aduioSource.PlayOneShot(clip, 0.5f);
        }
    }
}
