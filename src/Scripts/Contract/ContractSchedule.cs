using System;
[Serializable]
public class ContractSchedule
{
    public DateTime ContractCreatedTime
    {
        set; get;
    } = DateTime.UtcNow;

    public float ElapseTime
    {
        get; set;
    } = 0f;
}
