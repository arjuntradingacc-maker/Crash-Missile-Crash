using System.Threading.Tasks;

namespace CrashMissileCrash.IAP
{
    public class PurchaseResult
    {
        public bool Success;
        public string ProductId;
        public string ReceiptPayload;
        public string ErrorMessage;
    }

    /// <summary>Abstraction over the platform store (Apple App Store / Google Play Billing).</summary>
    public interface IIAPProvider
    {
        Task InitializeAsync();
        bool IsProductAvailable(string productId);
        Task<PurchaseResult> PurchaseAsync(string productId);
        Task<bool> RestorePurchasesAsync();
    }
}
