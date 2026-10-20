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
        public GenericPickupController Pickup;
        public ShopTerminalBehavior Shop;
        public TeleporterInteraction Teleporter;
        public NetworkIdentity Identity;

        // The teleporter's own state machines, which on a Primordial Teleporter include the one that aligns it with the
        // moon. Empty unless the aimed-at object is a teleporter.
        private EntityStateMachine[] teleporterMachines = System.Array.Empty<EntityStateMachine>();

        // The context token the game shows for the prompt. It is part of the prefab, so it matches on a client too.
        public string ContextToken =>
            Generic ? Generic.contextToken : Purchase ? Purchase.contextToken : null;

        // True when the aimed-at object is the teleporter itself, not a part of it such as the Primordial Teleporter's
        // prongs, which have their own prompt.
        public bool IsTeleporter => Teleporter && Root == Teleporter.gameObject;

        // True when this is a Primordial Teleporter aligned with the moon, or on its way there or back. Only its Idle
        // state sends the teleporter to the next stage (it sets SceneExitController.useRunNextStageScene, which only the
        // server sees), so any other state counts as aimed at the moon.
        public bool AimedAtMoon
        {
            get
            {
                foreach (EntityStateMachine machine in teleporterMachines)
                {
                    if (machine && machine.state is EntityStates.LunarTeleporter.LunarTeleporterBaseState
                        && !(machine.state is EntityStates.LunarTeleporter.Idle))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public void Refresh(GameObject aimed)
        {
            if (aimed == Root) return;

            Root = aimed;
            Generic = Find<GenericInteraction>(aimed);
            Purchase = Find<PurchaseInteraction>(aimed);
            Exit = Find<SceneExitController>(aimed);
            Descent = Find<ShrineCatastropheBehavior>(aimed);
            Halcyon = Find<HalcyoniteShrineInteractable>(aimed);
            Pickup = Find<GenericPickupController>(aimed);
            Shop = Find<ShopTerminalBehavior>(aimed);
            Teleporter = Find<TeleporterInteraction>(aimed);
            Identity = Find<NetworkIdentity>(aimed);
            teleporterMachines = IsTeleporter
                ? Teleporter.GetComponentsInChildren<EntityStateMachine>()
                : System.Array.Empty<EntityStateMachine>();
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
