using System.Linq;
using System.Threading.Tasks;
using CrashMissileCrash.Backend;
using CrashMissileCrash.Cards;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using CrashMissileCrash.Economy;
using UnityEngine;

namespace CrashMissileCrash.IAP
{
    public readonly struct PurchaseStartedEvent : IGameEvent { public readonly string ProductId; public PurchaseStartedEvent(string productId) { ProductId = productId; } }
    public readonly struct PurchaseCompletedEvent : IGameEvent { public readonly string ProductId; public readonly bool Success; public PurchaseCompletedEvent(string productId, bool success) { ProductId = productId; Success = success; } }

    /// <summary>
    /// Drives real-money purchases: platform store -> receipt -> server-side validation via
    /// IBackendService -> grant. A purchase is never granted on the strength of the client's own
    /// "success" flag alone in production - ValidatePurchaseAsync must confirm the receipt with
    /// the store server first. The offer being purchased is resolved from ShopOfferData by its
    /// realMoneyProductId, so new IAP products are pure content additions to shop.json.
    /// </summary>
    public class IAPManager : MonoBehaviour
    {
        public static IAPManager Instance { get; private set; }

        private IIAPProvider _provider;
        private IBackendService _backend;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
            _provider = new MockIAPProvider(); // swap for Unity IAP / platform SDK to go live
        }

        private async void Start()
        {
            await _provider.InitializeAsync();
            ServiceLocator.TryGet(out _backend);
        }

        public async Task<bool> PurchaseAsync(string productId)
        {
            EventBus.Publish(new PurchaseStartedEvent(productId));

            var result = await _provider.PurchaseAsync(productId);
            bool validated = result.Success && (_backend == null || await _backend.ValidatePurchaseAsync(productId, result.ReceiptPayload));

            if (validated) GrantProduct(productId);

            EventBus.Publish(new PurchaseCompletedEvent(productId, validated));
            return validated;
        }

        private void GrantProduct(string productId)
        {
            var offer = GameDatabase.Instance.ShopOffers.Values.FirstOrDefault(o => o.realMoneyProductId == productId);
            if (offer == null)
            {
                Debug.LogWarning($"[IAPManager] No shop offer maps to product id {productId}");
                return;
            }

            if (ServiceLocator.TryGet<EconomyManager>(out var economy))
            {
                economy.Add(CurrencyType.Coins, offer.grantCoins);
                economy.Add(CurrencyType.Gems, offer.grantGems);
            }
            if (!string.IsNullOrEmpty(offer.grantCardId) && ServiceLocator.TryGet<CardManager>(out var cards))
            {
                cards.AddCards(offer.grantCardId, offer.grantCardCount);
            }
        }
    }
}
