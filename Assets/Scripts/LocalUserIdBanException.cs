using System;

public class LocalUserIdBanException : Exception
{
    public LocalUserIdBanException(string localUserId) : base($"User ID {localUserId} is banned!!!")
    {
    }
}