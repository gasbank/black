public class PurchaseCountBanException : System.Exception {
    public PurchaseCountBanException(int totalPurchaseCount) : base($"Total purchase count is {totalPurchaseCount}!!!") {
    }
}
