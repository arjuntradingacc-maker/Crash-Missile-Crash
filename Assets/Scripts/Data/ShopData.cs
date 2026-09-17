using System;

namespace CrashMissileCrash.Data
{
    public enum ShopOfferType
    {
        CoinsBundle,
        GemsBundle,
        CardPack,
        ChampionOffer,
        CannonOffer,
        SeasonPass,
        NoAds,
        StarterBundle,
        EventItem
    }

    [Serializable]
    public class ShopOfferData
    {
        public string id;
        public ShopOfferType type = ShopOfferType.CoinsBundle;
        public string displayName;
        public int coinCost;      // pay with soft currency (rare)
        public int gemCost;       // pay with premium currency
        public string realMoneyProductId; // maps to platform IAP product id, empty = not a real-money offer
        public int grantCoins;
        public int grantGems;
        public string grantCardId;
        public int grantCardCount;
        public bool isOneTime;
        public string availabilityWindowUtcIso; // optional ISO date range "start|end" for daily/rotating offers
    }
}
