namespace SimpleInjector_Interception_Sample
{
    public class Credit : ICredit
    {

        public decimal CalculateCreditExpense(string requester, double requesterRate, decimal creditAmount)
        {
            return 0;
        }
    }
}