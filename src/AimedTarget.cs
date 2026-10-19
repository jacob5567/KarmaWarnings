using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace KarmaWarnings
{
    // The object the local player is aiming at (InteractionDriver.currentInteractable) and the components on it that the
    // detectors care about. Looked up again only when the aimed-at object changes.
    internal sealed class AimedTarget
    {
        public GameObject Root;
        public GenericInteraction Generic;
        public PurchaseInteraction Purchase;
        public SceneExitController Exit;
        public ShrineCatastropheBehavior Descent;
        public HalcyoniteShrineInteractable Halcyon;
        public PortalStatueBehavior Statue;
        public GenericPickupController Pickup;
        public ShopTerminalBehavior Shop;
        public TeleporterInteraction Teleporter;
        public NetworkIdentity Identity;

        // The context token the game shows for the prompt. It is part of the prefab, so it matches on a client too.
        public string ContextToken =>
            Generic ? Generic.contextToken : Purchase ? Purchase.contextToken : null;

        public void Refresh(GameObject aimed)
        {
            if (aimed == Root) return;

            Root = aimed;
            Generic = Find<GenericInteraction>(aimed);
            Purchase = Find<PurchaseInteraction>(aimed);
            Exit = Find<SceneExitController>(aimed);
            Descent = Find<ShrineCatastropheBehavior>(aimed);
            Halcyon = Find<HalcyoniteShrineInteractable>(aimed);
            Statue = Find<PortalStatueBehavior>(aimed);
            Pickup = Find<GenericPickupController>(aimed);
            Shop = Find<ShopTerminalBehavior>(aimed);
            Teleporter = Find<TeleporterInteraction>(aimed);
            Identity = Find<NetworkIdentity>(aimed);
        }

        // The interactable is the entity's root, and the component can sit on the root, a child or a parent.
        private static T Find<T>(GameObject root) where T : Component
        {
            if (!root) return null;

            T found = root.GetComponent<T>();
            if (!found)
            {
                found = root.GetComponentInChildren<T>();
            }
            if (!found)
            {
                found = root.GetComponentInParent<T>();
            }
            return found;
        }
    }
}
