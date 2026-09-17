using CrashMissileCrash.Cards;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using CrashMissileCrash.Economy;
using CrashMissileCrash.IAP;
using UnityEngine;
using UnityEngine.UI;

namespace CrashMissileCrash.UI.Screens
{
    /// <summary>Shop: daily offers, bundles, card packs and premium products.</summary>
    public class ShopScreen : UIScreen
    {
        private RectTransform _content;

        protected override void Awake()
        {
            base.Awake();
            var root = UIBuilder.CreateFullScreenPanel(transform, "ShopScreen", UIBuilder.PanelColor);
            UIBuilder.CreateText(root, "Title", "Shop", 44, UIBuilder.TextColor).rectTransform.anchoredPosition = new Vector2(0f, 880f);

            _content = ScrollListFactory.Build(root, out var scroll);
            scroll.anchorMin = Vector2.zero; scroll.anchorMax = Vector2.one;
            scroll.offsetMin = new Vector2(20f, 20f); scroll.offsetMax = new Vector2(-20f, -160f);

            UIBuilder.CreateButton(root, "BackButton", "Back", new Vector2(180f, 80f), UIBuilder.PanelColorLight, () => UIManager.Instance.GoBack())
                .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 60f);
        }

        protected override void OnShown()
        {
            for (int i = _content.childCount - 1; i >= 0; i--) Destroy(_content.GetChild(i).gameObject);

            foreach (var pack in GameDatabase.Instance.CardPacks.Values) BuildPackRow(pack);
            foreach (var offer in GameDatabase.Instance.ShopOffers.Values) BuildOfferRow(offer);
        }

        private void BuildPackRow(CardPackData pack)
        {
            var row = UIBuilder.CreatePanel(_content, $"Pack_{pack.id}", new Vector2(0f, 120f), UIBuilder.PanelColorLight);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 120f;
            var label = UIBuilder.CreateText(row, "Label", $"{pack.displayName}\n<size=20>{pack.cardCount} cards</size>", 28, UIBuilder.TextColor, TextAnchor.MiddleLeft);
            UIBuilder.Anchor((RectTransform)label.transform, Vector2.zero, Vector2.one, new Vector2(30f, 0f), new Vector2(-250f, 0f));

            string costLabel = pack.gemCost > 0 ? $"{pack.gemCost} Gems" : $"{pack.coinCost} Coins";
            var buyButton = UIBuilder.CreateButton(row, "Buy", costLabel, new Vector2(200f, 80f), UIBuilder.AccentColorGreen, () => OpenPack(pack.id));
            UIBuilder.Anchor(buyButton.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-120f, 0f), new Vector2(200f, 80f));
        }

        private void OpenPack(string packId)
        {
            if (CardPackManager.Instance != null && CardPackManager.Instance.TryOpenPack(packId, out var rewards))
            {
                Debug.Log($"[Shop] Opened {packId}: {rewards.Count} cards granted");
            }
        }

        private void BuildOfferRow(ShopOfferData offer)
        {
            var row = UIBuilder.CreatePanel(_content, $"Offer_{offer.id}", new Vector2(0f, 120f), UIBuilder.PanelColorLight);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 120f;
            var label = UIBuilder.CreateText(row, "Label", offer.displayName, 28, UIBuilder.TextColor, TextAnchor.MiddleLeft);
            UIBuilder.Anchor((RectTransform)label.transform, Vector2.zero, Vector2.one, new Vector2(30f, 0f), new Vector2(-250f, 0f));

            string costLabel = !string.IsNullOrEmpty(offer.realMoneyProductId) ? "Buy" : offer.gemCost > 0 ? $"{offer.gemCost} Gems" : $"{offer.coinCost} Coins";
            var buyButton = UIBuilder.CreateButton(row, "Buy", costLabel, new Vector2(200f, 80f), UIBuilder.AccentColor, () => OnBuyOffer(offer));
            UIBuilder.Anchor(buyButton.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-120f, 0f), new Vector2(200f, 80f));
        }

        private async void OnBuyOffer(ShopOfferData offer)
        {
            if (!string.IsNullOrEmpty(offer.realMoneyProductId))
            {
                if (IAPManager.Instance != null) await IAPManager.Instance.PurchaseAsync(offer.realMoneyProductId);
                return;
            }

            if (!ServiceLocator.TryGet<EconomyManager>(out var economy)) return;
            if (offer.gemCost > 0 && !economy.TrySpend(CurrencyType.Gems, offer.gemCost)) return;
            if (offer.coinCost > 0 && !economy.TrySpend(CurrencyType.Coins, offer.coinCost)) return;

            economy.Add(CurrencyType.Coins, offer.grantCoins);
            economy.Add(CurrencyType.Gems, offer.grantGems);
            if (!string.IsNullOrEmpty(offer.grantCardId) && ServiceLocator.TryGet<CardManager>(out var cards))
            {
                cards.AddCards(offer.grantCardId, offer.grantCardCount);
            }
        }
    }
}
