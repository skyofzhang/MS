using UnityEngine;

public static class CombatSystem
{
    // 暴击率和暴击伤害
    public const float CritRate = 0.1f;      // 10%暴击率
    public const float CritDamage = 1.5f;    // 暴击伤害150%
    
    /// <summary>
    /// 计算最终伤害 (考虑防御)
    /// 公式: damage = attack * (1 - defense/(defense+100))
    /// </summary>
    public static float CalculateDamage(float attack, float defense)
    {
        float reduction = defense / (defense + 100f);
        return attack * (1f - reduction);
    }
    
    /// <summary>
    /// 处理伤害流程 (含暴击判定)
    /// </summary>
    public static void DealDamage(GameObject attacker, GameObject target, float baseDamage)
    {
        // 暴击判定
        bool isCrit = Random.value < CritRate;
        float finalDamage = baseDamage;
        
        if (isCrit)
        {
            finalDamage *= CritDamage;
            Debug.Log($"[Combat] CRITICAL HIT! {attacker.name} -> {target.name}");
        }
        
        // 根据目标类型处理伤害
        var monster = target.GetComponent<MonsterController>();
        if (monster != null)
        {
            monster.TakeDamage(finalDamage);
            return;
        }
        
        var player = target.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(finalDamage);
            return;
        }
        
        Debug.LogWarning($"[Combat] Target {target.name} has no health component!");
    }
    
    /// <summary>
    /// 检查是否在攻击范围内
    /// </summary>
    public static bool IsInRange(Transform attacker, Transform target, float range)
    {
        return Vector3.Distance(attacker.position, target.position) <= range;
    }
    
    /// <summary>
    /// 检查是否在锥形范围内 (用于多重箭等技能)
    /// </summary>
    public static bool IsInCone(Transform attacker, Transform target, float range, float angle)
    {
        Vector3 toTarget = target.position - attacker.position;
        float distance = toTarget.magnitude;
        float angleToTarget = Vector3.Angle(attacker.forward, toTarget);
        
        return distance <= range && angleToTarget <= angle / 2f;
    }
    
    /// <summary>
    /// 检查是否在直线范围内 (用于穿透箭等技能)
    /// </summary>
    public static bool IsInLine(Transform attacker, Transform target, float range, float width)
    {
        Vector3 toTarget = target.position - attacker.position;
        float distance = toTarget.magnitude;
        
        Vector3 projected = Vector3.Project(toTarget, attacker.forward);
        float perpDistance = (toTarget - projected).magnitude;
        
        return distance <= range && perpDistance <= width && Vector3.Dot(toTarget, attacker.forward) > 0;
    }
}
