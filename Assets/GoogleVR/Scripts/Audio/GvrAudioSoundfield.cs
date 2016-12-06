// Copyright 2016 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

// GVR soundfield component that allows playback of first-order ambisonic recordings. The
// audio sample should be in Ambix (ACN-SN3D) format.
[AddComponentMenu("GoogleVR/Audio/GvrAudioSoundfield")]
public class GvrAudioSoundfield : MonoBehaviour {

  /// Input gain in decibels.
  public float gainDb = 0.0f;

  /// Play source on awake.
  public bool playOnAwake = true;

  /// The default AudioClip to play.
  public AudioClip clip0102 {
    get { return soundfieldClip0102; }
    set {
      soundfieldClip0102 = value;
      if (audioSources != null && audioSources.Length > 0) {
        audioSources[0].clip = soundfieldClip0102;
      }
    }
  }
  [SerializeField]
  private AudioClip soundfieldClip0102 = null;

  public AudioClip clip0304 {
    get { return soundfieldClip0304; }
    set {
      soundfieldClip0304 = value;
      if (audioSources != null && audioSources.Length > 0) {
        audioSources[1].clip = soundfieldClip0304;
      }
    }
  }
  [SerializeField]
  private AudioClip soundfieldClip0304 = null;

  /// Is the clip playing right now (Read Only)?
  public bool isPlaying {
    get {
      if(audioSources != null && audioSources.Length > 0) {
        return audioSources[0].isPlaying;
      }
      return false;
    }
  }

  /// Is the audio clip looping?
  public bool loop {
    get { return soundfieldLoop; }
    set {
      soundfieldLoop = value;
      if(audioSources != null) {
        for (int channelSet = 0; channelSet < audioSources.Length; ++channelSet) {
          audioSources[channelSet].loop = soundfieldLoop;
        }
      }
    }
  }
  [SerializeField]
  private bool soundfieldLoop = false;

  /// Un- / Mutes the soundfield. Mute sets the volume=0, Un-Mute restore the original volume.
  public bool mute {
    get { return soundfieldMute; }
    set {
      soundfieldMute = value;
      if(audioSources != null) {
        for (int channelSet = 0; channelSet < audioSources.Length; ++channelSet) {
          audioSources[channelSet].mute = soundfieldMute;
        }
      }
    }
  }
  [SerializeField]
  private bool soundfieldMute = false;

  /// The pitch of the audio source.
  public float pitch {
    get { return soundfieldPitch; }
    set {
      soundfieldPitch = value;
      if(audioSources != null) {
        for (int channelSet = 0; channelSet < audioSources.Length; ++channelSet) {
          audioSources[channelSet].pitch = soundfieldPitch;
        }
      }
    }
  }
  [SerializeField]
  [Range(-3.0f, 3.0f)]
  private float soundfieldPitch = 1.0f;

  /// Sets the priority of the soundfield.
  public int priority {
    get { return soundfieldPriority; }
    set {
      soundfieldPriority = value;
      if(audioSources != null) {
        for (int channelSet = 0; channelSet < audioSources.Length; ++channelSet) {
          audioSources[channelSet].priority = soundfieldPriority;
        }
      }
    }
  }
  [SerializeField]
  [Range(0, 256)]
  private int soundfieldPriority = 32;

  /// Playback position in seconds.
  public float time {
    get {
      if(audioSources != null && audioSources.Length > 0) {
        return audioSources[0].time;
      }
      return 0.0f;
    }
    set {
      if(audioSources != null) {
        for (int channelSet = 0; channelSet < audioSources.Length; ++channelSet) {
          audioSources[channelSet].time = value;
        }
      }
    }
  }

  /// Playback position in PCM samples.
  public int timeSamples {
    get {
      if(audioSources != null && audioSources.Length > 0) {
        return audioSources[0].timeSamples;
      }
      return 0;
    }
    set {
      if(audioSources != null) {
        for (int channelSet = 0; channelSet < audioSources.Length; ++channelSet) {
          audioSources[channelSet].timeSamples = value;
        }
      }
    }
  }

  /// The volume of the audio source (0.0 to 1.0).
  public float volume {
    get { return soundfieldVolume; }
    set {
      soundfieldVolume = value;
      if(audioSources != null) {
        for (int channelSet = 0; channelSet < audioSources.Length; ++channelSet) {
          audioSources[channelSet].volume = soundfieldVolume;
        }
      }
    }
  }
  [SerializeField]
  [Range(0.0f, 1.0f)]
  private float soundfieldVolume = 1.0f;

  // Unique source id.
  private int id = -1;

  // Unity audio sources per each soundfield channel set.
  private AudioSource[] audioSources = null;

  // Denotes whether the source is currently paused or not.
  private bool isPaused = false;

  void Awake () {
    // Route the source output to |GvrAudioMixer|.
    AudioMixer mixer = (Resources.Load("GvrAudioMixer") as AudioMixer);
    if(mixer == null) {
      Debug.LogError("GVRAudioMixer could not be found in Resources. Make sure that the GVR SDK" +
                     "Unity package is imported properly.");
      return;
    }
    audioSources = new AudioSource[GvrAudio.numFoaChannels / 2];
    for (int channelSet = 0; channelSet < audioSources.Length; ++channelSet) {
      GameObject channelSetObject = new GameObject("Channel Set " + channelSet);
      channelSetObject.transform.parent = gameObject.transform;
      channelSetObject.hideFlags = HideFlags.HideAndDontSave;
      audioSources[channelSet] = channelSetObject.AddComponent<AudioSource>();
      audioSources[channelSet].enabled = false;
      audioSources[channelSet].playOnAwake = false;
      audioSources[channelSet].bypassReverbZones = true;
      audioSources[channelSet].dopplerLevel = 0.0f;
      audioSources[channelSet].spatialBlend = 0.0f;
      audioSources[channelSet].outputAudioMixerGroup = mixer.FindMatchingGroups("Master")[0];
    }
    OnValidate();
  }

  void OnEnable () {
    for (int channelSet = 0; channelSet < audioSources.Length; ++channelSet) {
      audioSources[channelSet].enabled = true;
    }
    if (playOnAwake && !isPlaying && InitializeSoundfield()) {
      Play();
    }
  }

  void Start () {
    if (playOnAwake && !isPlaying) {
      Play();
    }
  }

  void OnDisable () {
    Stop();
    for (int channelSet = 0; channelSet < audioSources.Length; ++channelSet) {
      audioSources[channelSet].enabled = false;
    }
  }

  void OnDestroy () {
    for (int channelSet = 0; channelSet < audioSources.Length; ++channelSet) {
      Destroy(audioSources[channelSet].gameObject);
    }
  }

  void Update () {
    // Update soundfield.
    if (!isPlaying && !isPaused) {
      Stop();
    } else {
      GvrAudio.UpdateAudioSoundfield(id, transform, gainDb);
    }
  }

  void OnValidate () {
    clip0102 = soundfieldClip0102;
    clip0304 = soundfieldClip0304;
    loop = soundfieldLoop;
    mute = soundfieldMute;
    pitch = soundfieldPitch;
    priority = soundfieldPriority;
    volume = soundfieldVolume;
  }

  /// Pauses playing the clip.
  public void Pause () {
    if (audioSources != null) {
      isPaused = true;
      for (int channelSet = 0; channelSet < audioSources.Length; ++channelSet) {
        audioSources[channelSet].Pause();
      }
    }
  }

  /// Plays the clip.
  public void Play () {
    double dspTime = AudioSettings.dspTime;
    PlayScheduled(dspTime);
  }

  /// Plays the clip with a delay specified in seconds.
  public void PlayDelayed (float delay) {
    double delayedDspTime = AudioSettings.dspTime + (double)delay;
    PlayScheduled(delayedDspTime);
  }

  /// Plays the clip at a specific time on the absolute time-line that AudioSettings.dspTime reads
  /// from.
  public void PlayScheduled (double time) {
    if (audioSources != null && InitializeSoundfield()) {
      for (int channelSet = 0; channelSet < audioSources.Length; ++channelSet) {
        audioSources[channelSet].PlayScheduled(time);
      }
      isPaused = false;
    } else {
      Debug.LogWarning ("GVR Audio soundfield not initialized. Audio playback not supported " +
                        "until after Awake() and OnEnable(). Try calling from Start() instead.");
    }
  }

  /// Stops playing the clip.
  public void Stop () {
    if(audioSources != null) {
      for (int channelSet = 0; channelSet < audioSources.Length; ++channelSet) {
        audioSources[channelSet].Stop();
      }
      ShutdownSoundfield();
      isPaused = false;
    }
  }

  /// Unpauses the paused playback.
  public void UnPause () {
    if (audioSources != null) {
      for (int channelSet = 0; channelSet < audioSources.Length; ++channelSet) {
        audioSources[channelSet].UnPause();
      }
      isPaused = true;
    }
  }

  // Initializes the source.
  private bool InitializeSoundfield () {
    if (id < 0) {
      id = GvrAudio.CreateAudioSoundfield();
      if (id >= 0) {
        GvrAudio.UpdateAudioSoundfield(id, transform, gainDb);
        for (int channelSet = 0; channelSet < audioSources.Length; ++channelSet) {
          InitializeChannelSet(audioSources[channelSet], channelSet);
        }
      }
    }
    return id >= 0;
  }

  // Shuts down the source.
  private void ShutdownSoundfield () {
    if (id >= 0) {
      for (int channelSet = 0; channelSet < audioSources.Length; ++channelSet) {
        ShutdownChannelSet(audioSources[channelSet], channelSet);
      }
      GvrAudio.DestroyAudioSource(id);
      id = -1;
    }
  }

  // Initializes given channel set of the soundfield.
  private void InitializeChannelSet(AudioSource source, int channelSet) {
    source.spatialize = true;
    source.SetSpatializerFloat(0, (float)id);
    source.SetSpatializerFloat(1, (float)GvrAudio.SpatializerType.Soundfield);
    source.SetSpatializerFloat(2, (float)GvrAudio.numFoaChannels);
    source.SetSpatializerFloat(3, (float)channelSet);
  }

  // Shuts down given channel set of the soundfield.
  private void ShutdownChannelSet(AudioSource source, int channelSet) {
    source.SetSpatializerFloat(0, -1.0f);
    source.spatialize = false;
  }
}
