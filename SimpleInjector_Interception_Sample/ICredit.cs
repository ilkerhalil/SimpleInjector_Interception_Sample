namespace SimpleInjector_Interception_Sample
{
    public interface ICredit
    {
        decimal CalculateCreditExpense(string requester, double requesterRate, decimal creditAmount);
    }
}