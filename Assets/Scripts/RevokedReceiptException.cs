public class RevokedReceiptException : System.Exception {
    public RevokedReceiptException(string receipt) : base($"Receipt {receipt} is revoked!!!") {
    }
}
