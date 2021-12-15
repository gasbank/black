using System;

public class RevokedReceiptException : Exception
{
    public RevokedReceiptException(string receipt) : base($"Receipt {receipt} is revoked!!!")
    {
    }
}