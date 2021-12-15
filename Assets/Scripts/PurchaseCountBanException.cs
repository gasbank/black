using System;

public class PurchaseCountBanException : Exception
{
    public PurchaseCountBanException(int totalPurchaseCount) : base($"Total purchase count is {totalPurchaseCount}!!!")
    {
    }
}