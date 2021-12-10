public class LocalUserIdBanException : System.Exception {
    public LocalUserIdBanException(string localUserId) : base($"User ID {localUserId} is banned!!!") {
    }
}
