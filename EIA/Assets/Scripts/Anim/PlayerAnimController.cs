using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "TimeDilationSettings", menuName = "Time Dilation Settings")]
public class TimeSlowParam : ScriptableObject
{
    public float slowMotionScale;
    public float duration;
}
public class PlayerAnimController : MonoBehaviour
{
    
    private Animator _animator;
    private Coroutine _slowMotionCoroutine;
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        if (_slowMotionCoroutine != null)
        {
            StopCoroutine(_slowMotionCoroutine);
            Time.timeScale = 1.0f;
        }
        //Hit();
    }

    public void Hit()
    {
        _animator.Play("hit", 0, 0);
    }

    public void Walk()
    {
        //animator.Play("walk", 0, 0);
        _animator.SetBool("IsWalking", true);
    }

    public void Idle(bool force = false)
    {
        if (force)
            _animator.Play("idle", 0, 0);
        else
            _animator.SetBool("IsWalking", false);
    }
    
    public void Parry()
    {
        _animator.Play("parry", 0, 0);
    }
    
    public void Dodge()
    {
        _animator.Play("dodge", 0, 0);
    }

    public void Attack()
    {
        _animator.Play("attack", 0,0);
    }

    public void TriggerEffect(string effectName)
    {
        var effectPrefab = Resources.Load<GameObject>($"Effect/{effectName}");
        Debug.Log($"Assets/Resources/Effect/{effectName}");
        var currentEffect = Instantiate(effectPrefab, transform.position, Quaternion.identity);
    
        // 方案1：延迟销毁（适用于有生命周期的特效）
        float effectDuration = currentEffect.GetComponent<ParticleSystem>()?.main.duration ?? 5f;
        Destroy(currentEffect, effectDuration);
    }

    public void CameraShake(int presetIndex)
    {
        ScreenShake.Instance.Shake(presetIndex);
    }

    public bool IsState(string stateName)
    {
        var state = _animator.GetCurrentAnimatorStateInfo(0);
        
        return state.IsName(stateName);
    }
    
    
    // 在动画事件中调用此方法
    public void ApplyTimeDilation(TimeSlowParam param)//(float slowMotionScale, float duration)
    {
        // 如果已有慢动作协程在运行，先停止它
        if (_slowMotionCoroutine != null)
            StopCoroutine(_slowMotionCoroutine);
            
        // 启动新的慢动作协程
        _slowMotionCoroutine = StartCoroutine(DoTimeDilation(param.slowMotionScale, param.duration));
    }
    
    // 执行时间膨胀的协程
    private IEnumerator DoTimeDilation(float slowMotionScale, float duration)
    {
        // 保存原始时间缩放值
        float originalTimeScale = Time.timeScale;
        float originalFixedDeltaTime = Time.fixedDeltaTime;
        
        // 应用时间膨胀
        Time.timeScale = slowMotionScale;
        // 调整FixedDeltaTime以保持物理稳定性
        Time.fixedDeltaTime = originalFixedDeltaTime * slowMotionScale;
        
        // 等待指定的持续时间
        yield return new WaitForSecondsRealtime(duration); // 使用Realtime避免受Time.timeScale影响
        
        // 恢复正常时间
        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime;
        
        _slowMotionCoroutine = null;
    }
    
}
