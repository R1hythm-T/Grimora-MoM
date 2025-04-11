using Unity.VisualScripting;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("Major Stats")]
    public Stat strength; //increase both damage and crit damage
    public Stat agility; //increase both evasion and crit chance
    public Stat intelligence; //increase both magic damage and magic resistance
    public Stat vitality; //increase health

    [Header("Offensive Stats")]
    public Stat damage;
    public Stat critChance;
    public Stat critPower; //default 200%

    [Header("Defensive Stats")]
    public Stat maxHealth;
    public Stat armor;
    public Stat evasion;
    public Stat magicResistance;

    [Header("Magic Stats")]
    public Stat fireDamage;
    public Stat iceDamage;
    public Stat lightningDamage;

    public bool isIgnited; //dmg over time
    public bool isChilled; //weakens armor
    public bool isShocked; //increases evasion

    private float ignitedTimer;
    private float chilledTimer;
    private float shockedTimer;

    private float igniteDamageCooldown = .3f;
    private float igniteDamageTimer;
    private int igniteDamage;

    [SerializeField] private int currentHealth;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        critPower.SetDefaultValue(200);
        currentHealth = maxHealth.GetValue();
    }

    protected virtual void Update()
    {
        ignitedTimer -= Time.deltaTime;
        chilledTimer -= Time.deltaTime;
        shockedTimer -= Time.deltaTime;

        igniteDamageTimer -= Time.deltaTime;

        if (ignitedTimer < 0)
            isIgnited = false;

        if (chilledTimer < 0)
            isChilled = false;

        if (shockedTimer < 0)
            isShocked = false;

        if (igniteDamageTimer < 0 && isIgnited)
        {
            currentHealth -= igniteDamage;

            if (currentHealth < 0)
                Die();

            igniteDamageTimer = igniteDamageCooldown;
        }
    }

    public virtual void DoDamage(CharacterStats _targetStats)
    {
        if (TargetCanAvoidAttack(_targetStats))
            return;

        int totalDamage = damage.GetValue() + strength.GetValue();

        if (CanCrit())
            totalDamage = CalculateCriticalDamage(totalDamage);

        totalDamage = CheckTargetArmor(_targetStats, totalDamage);
        //_targetStats.TakeDamage(totalDamage);
        DoMagicalDamage(_targetStats);
    }
    public virtual void DoMagicalDamage(CharacterStats _targetStats)
    {
        int _fireDamage = fireDamage.GetValue();
        int _iceDamage = iceDamage.GetValue();
        int _lightningDamage = lightningDamage.GetValue();

        int totalMagicalDamage = _fireDamage + _iceDamage + _lightningDamage + intelligence.GetValue();

        totalMagicalDamage = CheckTargetResistance(_targetStats, totalMagicalDamage);
        _targetStats.TakeDamage(totalMagicalDamage);

        if (Mathf.Max(_fireDamage, _iceDamage, _lightningDamage) <= 0)
            return;

        bool canApplyIgnite = _fireDamage > _iceDamage && _fireDamage > _lightningDamage;
        bool canApplyChill = _iceDamage > _fireDamage && _iceDamage > _lightningDamage;
        bool canApplyShock = _lightningDamage > _fireDamage && _lightningDamage > _iceDamage;
        
        while (!canApplyIgnite && !canApplyChill && !canApplyShock)
        {
            if (Random.value < .5f && _fireDamage > 0)
            {
                canApplyIgnite = true;
                _targetStats.ApplyAilments(canApplyIgnite, canApplyChill, canApplyShock);
                return;
            }

            if (Random.value < .5f && _iceDamage > 0)
            {
                canApplyChill = true;
                _targetStats.ApplyAilments(canApplyIgnite, canApplyChill, canApplyShock);
                return;
            }

            if (Random.value < .5f && _lightningDamage > 0)
            {
                canApplyShock = true;
                _targetStats.ApplyAilments(canApplyIgnite, canApplyChill, canApplyShock);
                return;
            }

        }

        if (canApplyIgnite)
            _targetStats.SetupIgniteDamage(Mathf.RoundToInt(_fireDamage * .25f));

        _targetStats.ApplyAilments(canApplyIgnite, canApplyChill, canApplyShock);
    }
    private static int CheckTargetResistance(CharacterStats _targetStats, int totalMagicalDamage)
    {
        totalMagicalDamage -= _targetStats.magicResistance.GetValue() + (_targetStats.intelligence.GetValue() * 3);
        totalMagicalDamage = Mathf.Clamp(totalMagicalDamage, 0, int.MaxValue);
        return totalMagicalDamage;
    }

    public void ApplyAilments(bool _ignite, bool _chill, bool _shock)
    {
        if(isIgnited || isChilled || isShocked)
            return;

        if (_ignite)
        {
            isIgnited = _ignite;
            ignitedTimer = 4;
        }

        if (_chill)
        {
            isChilled = _chill;
            chilledTimer = 4;
        }

        if (_shock)
        {
            isShocked = _shock;
            shockedTimer = 4;
        }
    }

    public void SetupIgniteDamage(int _damage) => igniteDamage = _damage;

    public virtual void TakeDamage(int _damage)
    {
        currentHealth -= _damage;

        if (currentHealth < 0)
            Die();
    }

    protected virtual void Die()
    {
        //throw new NotImplementedException();
    }

    private int CheckTargetArmor(CharacterStats _targetStats, int totalDamage)
    {
        if (_targetStats.isChilled)
            totalDamage -= Mathf.RoundToInt(_targetStats.armor.GetValue() * .8f);
        else
            totalDamage -= _targetStats.armor.GetValue();

        totalDamage = Mathf.Clamp(totalDamage, 0, int.MaxValue);
        return totalDamage;
    }

    private bool TargetCanAvoidAttack(CharacterStats _targetStats)
    {
        int totalevasion = _targetStats.evasion.GetValue() + _targetStats.agility.GetValue();

        if (isShocked)
            totalevasion += 25;

        if (Random.Range(0, 100) < totalevasion)
        {
            return true;
        }

        return false;
    }

    private bool CanCrit()
    {
        int totalCriticalChance = critChance.GetValue() + agility.GetValue();

        if (Random.Range(0, 100) <= totalCriticalChance)
        {
            return true;
        }

        return false;
    }

    private int CalculateCriticalDamage(int _damage)
    {
        float totalCritPower = (critPower.GetValue() + strength.GetValue()) * .01f;
        float critDamage = totalCritPower * _damage;

        return Mathf.RoundToInt(critDamage);
    }
}
