using System.Threading.Tasks;

namespace CrashMissileCrash.IAP
{
    /// <summary>Always-succeeds local stand-in for the platform store, so the full purchase flow
    /// (initiate -> receipt -> server validation -> grant) is exercised without store setup.
    /// Replace with Unity IAP (or a platform-native plugin) wired to real product ids to go live.</summary>
    public class MockIAPProvider : IIAPProvider
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public bool IsProductAvailable(string productId) => !string.IsNullOrEmpty(productId);

        public Task<PurchaseResult> PurchaseAsync(string productId)
        {
            var result = new PurchaseResult
            {
                Success = true,
                ProductId = productId,
                ReceiptPayload = "{\"mock\":true,\"productId\":\"" + productId + "\"}"
            };
            return Task.FromResult(result);
        }

        public Task<bool> RestorePurchasesAsync() => Task.FromResult(true);
    }
}
