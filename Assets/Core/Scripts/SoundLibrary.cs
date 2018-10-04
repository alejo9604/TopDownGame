using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundLibrary : MonoBehaviour {

    public SoundGroup[] soundGroups;

    Dictionary<string, AudioClip[]> groupDictionary = new Dictionary<string, AudioClip[]>();


    private void Awake()
    {
        foreach (SoundGroup group in soundGroups)
        {
            groupDictionary.Add(group.groupID, group.group);
        }
    }

    public AudioClip GetClipFromName(string soundName)
    {
        if (groupDictionary.ContainsKey(soundName))
        {
            AudioClip[] sounds = groupDictionary[soundName];
            return sounds[Random.Range(0, sounds.Length)];
        }

        Debug.Log(soundName + " - Null");
        return null;
    }


    [System.Serializable]
    public class SoundGroup
    {
        public string groupID;
        public AudioClip[] group;
    }

}
