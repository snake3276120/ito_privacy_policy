using System;
using UnityEngine;

[Serializable]
public class Contract
{
    public enum ContractStatus
    {
        NEW = 0,
        STARTED,
        EXPIRED
    }

    private DateTime m_StartTime;

    /*** Props ***/
    public string Requestor
    {
        get; set;
    } = "";

    public int EquivalentStartLevel
    {
        get; set;
    } = -1;

    #region Modifiers
    public float SoldierSpeedModifier
    {
        get; set;
    } = 1f;

    public float TurretBaseDamageModifier
    {
        get; set;
    } = 1f;

    public float SoldierValueModifier
    {
        get; set;
    } = 1f;

    public float TurretBaseFiringRateModifier
    {
        get; set;
    } = 1f;

    public bool GunTurretCanCrit
    {
        get; set;
    } = false;
    #endregion

    public string SessionIndicator
    {
        get; set;
    } = "";

    public int Difficulty
    {
        set; get;
    } = -1;

    public ContractStatus Status
    {
        get; set;
    } = ContractStatus.NEW;

    public int CurrentNode
    {
        get; set;
    } = -1;

    public float ElaspeTime
    {
        set; get;
    } = -999f;

    public DateTime StartTime
    {
        set
        {
            m_StartTime = value;
        }
    }

    public string TimeRemaining
    {
        get
        {
            if (ElaspeTime / 86400f - (float)(System.DateTime.UtcNow - m_StartTime).TotalDays >= 1)
                return Mathf.RoundToInt(ElaspeTime / 86400f - (float)(System.DateTime.UtcNow - m_StartTime).TotalDays).ToString() + " Days";
            if (ElaspeTime / 3600f - (float)(System.DateTime.UtcNow - m_StartTime).TotalHours >= 1)
                return Mathf.RoundToInt(ElaspeTime / 3600f - (float)(System.DateTime.UtcNow - m_StartTime).TotalHours).ToString() + " Hours";
            else if (ElaspeTime / 60f - (float)(System.DateTime.UtcNow - m_StartTime).TotalMinutes >= 1)
                return Mathf.RoundToInt(ElaspeTime / 60f - (float)(System.DateTime.UtcNow - m_StartTime).TotalMinutes).ToString() + " Minutes";
            else if (ElaspeTime - (float)(System.DateTime.UtcNow - m_StartTime).TotalSeconds > 0)
                return Mathf.RoundToInt(ElaspeTime - (float)(System.DateTime.UtcNow - m_StartTime).TotalSeconds).ToString() + " Seconds";
            else
            {
                Status = ContractStatus.EXPIRED;
                return "Expired";
            }
        }
    }

    /// <summary>
    /// Sole constructor
    /// </summary>
    public Contract()
    {
        m_StartTime = DateTime.UtcNow;
        ElaspeTime = -1f;
    }

    public void DebugSetExpireInSecs(float seconds)
    {
        m_StartTime = DateTime.UtcNow;
        m_StartTime = m_StartTime.AddSeconds(seconds);
        m_StartTime = m_StartTime.AddSeconds(-ElaspeTime);
    }
}
