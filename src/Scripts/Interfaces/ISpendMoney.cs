/// <summary>
/// Interface for components require money/cache data to check if it can afford it
/// </summary>
public interface ISpendMoney
{
    void NotifyMoneyChange();
}
