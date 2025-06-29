using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioMgr : MonoBehaviour
{
    // 单例实现
    public static AudioMgr Instance { get; private set; }

    // 音频类型枚举
    public enum AudioType
    {
        BGM,        // 背景音乐
        SFX,        // 音效
        Voice,      // 语音
        Ambient     // 环境音
    }

    // 音频组设置
    [System.Serializable]
    public class AudioGroup
    {
        public AudioType type;
        public AudioSource source;
        [Range(0f, 1f)] public float volume = 1f;
        public bool mute = false;
        public bool loop = false;
    }

    [Header("音频组设置")]
    public List<AudioGroup> audioGroups = new List<AudioGroup>();

    [Header("音效池设置")]
    public int initialSfxPoolSize = 10;
    private Queue<AudioSource> sfxSourcePool = new Queue<AudioSource>();

    [Header("背景音乐淡入淡出")]
    public float bgmFadeTime = 1f;
    public AudioClip currentBGM;
    private Coroutine bgmFadeCoroutine;

    private void Awake()
    {
        // 确保单例唯一性
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioGroups();
            InitializeSfxPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayBGM((currentBGM));
    }

    // 初始化音频组
    private void InitializeAudioGroups()
    {
        // 确保每种类型的音频组都存在
        foreach (AudioType type in System.Enum.GetValues(typeof(AudioType)))
        {
            if (!audioGroups.Exists(g => g.type == type))
            {
                GameObject groupObj = new GameObject(type.ToString() + "Group");
                groupObj.transform.SetParent(transform);
                
                AudioSource source = groupObj.AddComponent<AudioSource>();
                source.playOnAwake = false;
                
                audioGroups.Add(new AudioGroup
                {
                    type = type,
                    source = source,
                    volume = 1f,
                    mute = false,
                    loop = type == AudioType.BGM || type == AudioType.Ambient
                });
            }
        }

        // 应用设置
        foreach (var group in audioGroups)
        {
            group.source.volume = group.volume;
            group.source.mute = group.mute;
            group.source.loop = group.loop;
        }
    }

    // 初始化音效池
    private void InitializeSfxPool()
    {
        GameObject poolContainer = new GameObject("SFXPool");
        poolContainer.transform.SetParent(transform);

        for (int i = 0; i < initialSfxPoolSize; i++)
        {
            CreateSfxSource(poolContainer.transform);
        }
    }

    // 创建音效源
    private AudioSource CreateSfxSource(Transform parent)
    {
        GameObject sfxObj = new GameObject("SFXSource");
        sfxObj.transform.SetParent(parent);
        
        AudioSource source = sfxObj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        sfxSourcePool.Enqueue(source);
        
        return source;
    }

    // 从池中获取音效源
    private AudioSource GetSfxSource()
    {
        if (sfxSourcePool.Count > 0)
        {
            return sfxSourcePool.Dequeue();
        }
        
        return CreateSfxSource(transform);
    }

    // 归还音效源到池中
    private void ReturnSfxSource(AudioSource source)
    {
        source.Stop();
        source.clip = null;
        source.transform.SetParent(transform.Find("SFXPool"));
        sfxSourcePool.Enqueue(source);
    }

    // 设置音频组音量
    public void SetGroupVolume(AudioType type, float volume)
    {
        AudioGroup group = GetAudioGroup(type);
        if (group != null)
        {
            group.volume = Mathf.Clamp01(volume);
            group.source.volume = group.volume;
        }
    }

    // 设置音频组静音
    public void SetGroupMute(AudioType type, bool mute)
    {
        AudioGroup group = GetAudioGroup(type);
        if (group != null)
        {
            group.mute = mute;
            group.source.mute = mute;
        }
    }

    // 播放背景音乐
    public void PlayBGM(AudioClip clip, bool forceRestart = false, float fadeTime = -1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("Cannot play null BGM clip");
            return;
        }

        AudioGroup bgmGroup = GetAudioGroup(AudioType.BGM);
        if (bgmGroup == null) return;

        // 如果相同的BGM已经在播放，并且不需要强制重启，则直接返回
        if (currentBGM == clip && bgmGroup.source.isPlaying && !forceRestart)
        {
            return;
        }

        currentBGM = clip;

        // 如果有正在进行的淡入淡出，先停止它
        if (bgmFadeCoroutine != null)
        {
            StopCoroutine(bgmFadeCoroutine);
        }

        // 如果需要淡入淡出
        if (fadeTime > 0f || (fadeTime < 0f && bgmFadeTime > 0f))
        {
            float actualFadeTime = fadeTime > 0f ? fadeTime : bgmFadeTime;
            bgmFadeCoroutine = StartCoroutine(FadeBGM(bgmGroup, clip, actualFadeTime));
        }
        else
        {
            // 直接播放
            bgmGroup.source.clip = clip;
            bgmGroup.source.Play();
        }
    }

    // 背景音乐淡入淡出
    private IEnumerator FadeBGM(AudioGroup group, AudioClip newClip, float fadeTime)
    {
        float originalVolume = group.volume;
        float targetVolume = group.source.isPlaying ? 0f : originalVolume;
        float elapsedTime = 0f;

        // 如果当前有音乐在播放，先淡出
        if (group.source.isPlaying && group.source.clip != null)
        {
            while (elapsedTime < fadeTime)
            {
                group.source.volume = Mathf.Lerp(originalVolume, 0f, elapsedTime / fadeTime);
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            group.source.Stop();
        }

        // 切换到新音乐并淡入
        group.source.clip = newClip;
        group.source.volume = 0f;
        group.source.Play();

        elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            group.source.volume = Mathf.Lerp(0f, originalVolume, elapsedTime / fadeTime);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        group.source.volume = originalVolume;
        bgmFadeCoroutine = null;
    }

    // 停止背景音乐
    public void StopBGM(float fadeTime = -1f)
    {
        AudioGroup bgmGroup = GetAudioGroup(AudioType.BGM);
        if (bgmGroup == null || !bgmGroup.source.isPlaying) return;

        // 如果有正在进行的淡入淡出，先停止它
        if (bgmFadeCoroutine != null)
        {
            StopCoroutine(bgmFadeCoroutine);
        }

        // 如果需要淡入淡出
        if (fadeTime > 0f || (fadeTime < 0f && bgmFadeTime > 0f))
        {
            float actualFadeTime = fadeTime > 0f ? fadeTime : bgmFadeTime;
            bgmFadeCoroutine = StartCoroutine(StopBGMWithFade(bgmGroup, actualFadeTime));
        }
        else
        {
            // 直接停止
            bgmGroup.source.Stop();
        }
    }

    // 带淡出效果的停止背景音乐
    private IEnumerator StopBGMWithFade(AudioGroup group, float fadeTime)
    {
        float originalVolume = group.volume;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            group.source.volume = Mathf.Lerp(originalVolume, 0f, elapsedTime / fadeTime);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        group.source.Stop();
        group.source.volume = originalVolume;
        bgmFadeCoroutine = null;
    }

    // 播放音效
    public AudioSource PlaySFX(AudioClip clip, Vector3 position = default, float volume = 1f, float pitch = 1f, bool is3D = false)
    {
        if (clip == null)
        {
            Debug.LogWarning("Cannot play null SFX clip");
            return null;
        }

        AudioGroup sfxGroup = GetAudioGroup(AudioType.SFX);
        if (sfxGroup == null || sfxGroup.mute) return null;

        AudioSource source = GetSfxSource();
        source.transform.position = position;
        source.clip = clip;
        source.volume = volume * sfxGroup.volume;
        source.pitch = pitch;
        source.spatialBlend = is3D ? 1f : 0f;
        source.Play();

        // 播放完毕后自动归还到池中
        StartCoroutine(WaitAndReturnSfxSource(source, clip.length / pitch));

        return source;
    }

    // 播放语音
    public void PlayVoice(AudioClip clip, bool interrupt = true)
    {
        if (clip == null)
        {
            Debug.LogWarning("Cannot play null Voice clip");
            return;
        }

        AudioGroup voiceGroup = GetAudioGroup(AudioType.Voice);
        if (voiceGroup == null || voiceGroup.mute) return;

        if (interrupt)
        {
            voiceGroup.source.Stop();
        }

        voiceGroup.source.clip = clip;
        voiceGroup.source.Play();
    }

    private Dictionary<string, AudioClip> audioClips = new();
    public void PlayVoice(string name, bool interrupt = true)
    {
        if (!audioClips.ContainsKey(name))
        {
            var clip = Resources.Load<AudioClip>($"Sound/{name}");
            audioClips.Add(name, clip);
        }
        
        PlayVoice(audioClips[name], interrupt);
    }

    // 播放环境音
    public void PlayAmbient(AudioClip clip, bool loop = true)
    {
        if (clip == null)
        {
            Debug.LogWarning("Cannot play null Ambient clip");
            return;
        }

        AudioGroup ambientGroup = GetAudioGroup(AudioType.Ambient);
        if (ambientGroup == null || ambientGroup.mute) return;

        ambientGroup.source.clip = clip;
        ambientGroup.source.loop = loop;
        ambientGroup.source.Play();
    }

    // 等待音效播放完毕并归还到池中
    private IEnumerator WaitAndReturnSfxSource(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnSfxSource(source);
    }

    // 获取音频组
    private AudioGroup GetAudioGroup(AudioType type)
    {
        return audioGroups.Find(g => g.type == type);
    }

    // 停止特定类型的所有音频
    public void StopAllAudioOfType(AudioType type)
    {
        AudioGroup group = GetAudioGroup(type);
        if (group != null)
        {
            group.source.Stop();
        }

        // 停止所有音效源
        if (type == AudioType.SFX)
        {
            foreach (var source in sfxSourcePool)
            {
                source.Stop();
            }
        }
    }

    // 暂停特定类型的所有音频
    public void PauseAllAudioOfType(AudioType type)
    {
        AudioGroup group = GetAudioGroup(type);
        if (group != null)
        {
            group.source.Pause();
        }

        // 暂停所有音效源
        if (type == AudioType.SFX)
        {
            foreach (var source in sfxSourcePool)
            {
                if (source.isPlaying)
                {
                    source.Pause();
                }
            }
        }
    }

    // 恢复特定类型的所有音频
    public void ResumeAllAudioOfType(AudioType type)
    {
        AudioGroup group = GetAudioGroup(type);
        if (group != null)
        {
            group.source.UnPause();
        }

        // 恢复所有音效源
        if (type == AudioType.SFX)
        {
            foreach (var source in sfxSourcePool)
            {
                if (source.isActiveAndEnabled && !source.isPlaying && source.clip != null)
                {
                    source.UnPause();
                }
            }
        }
    }
}