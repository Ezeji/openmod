﻿using HarmonyLib;
using OpenMod.API;
using OpenMod.API.Eventing;
using OpenMod.API.Users;
using OpenMod.UnityEngine.Extensions;
using OpenMod.Unturned.Events;
using OpenMod.Unturned.Players;
using SDG.Unturned;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace OpenMod.Unturned.Animals.Events
{
    internal class AnimalsEventsListener : UnturnedEventsListener
    {
        public AnimalsEventsListener(IOpenModHost openModHost,
            IEventBus eventBus,
            IUserManager userManager) : base(openModHost, eventBus, userManager)
        {
        }

        public override void Subscribe()
        {
            OnAnimalAdded += Events_OnAnimalAdded;
            OnAnimalRevived += Events_OnAnimalRevived;
            OnAnimalDamaging += Events_OnAnimalDamaging;
            OnAnimalDead += Events_OnAnimalDead;
            OnAnimalFleeing += Events_OnAnimalFleeing;
            OnAnimalAttackingPoint += Events_OnAnimalAttackingPoint;
            OnAnimalAttackingPlayer += Events_OnAnimalAttackingPlayer;
        }

        public override void Unsubscribe()
        {
            OnAnimalAdded -= Events_OnAnimalAdded;
            OnAnimalRevived -= Events_OnAnimalRevived;
            OnAnimalDamaging -= Events_OnAnimalDamaging;
            OnAnimalDead -= Events_OnAnimalDead;
            OnAnimalFleeing -= Events_OnAnimalFleeing;
            OnAnimalAttackingPoint -= Events_OnAnimalAttackingPoint;
            OnAnimalAttackingPlayer -= Events_OnAnimalAttackingPlayer;
        }

        private void Events_OnAnimalAdded(Animal nativeAnimal)
        {
            var animal = new UnturnedAnimal(nativeAnimal);

            var @event = new UnturnedAnimalAddedEvent(animal);

            Emit(@event);
        }

        private void Events_OnAnimalRevived(Animal nativeAnimal)
        {
            var animal = new UnturnedAnimal(nativeAnimal);

            var @event = new UnturnedAnimalRevivedEvent(animal);

            Emit(@event);
        }

        private void Events_OnAnimalDamaging(Animal nativeAnimal, ref ushort amount, ref Vector3 ragdoll,
            ref ERagdollEffect ragdollEffect, ref bool trackKill, ref bool dropLoot, ref bool cancel)
        {
            var animal = new UnturnedAnimal(nativeAnimal);

            var @event = amount >= animal.Health
                ? new UnturnedAnimalDyingEvent(animal, amount, ragdoll.ToSystemVector(), ragdollEffect, trackKill,
                    dropLoot)
                : new UnturnedAnimalDamagingEvent(animal, amount, ragdoll.ToSystemVector(), ragdollEffect, trackKill,
                    dropLoot);

            @event.IsCancelled = cancel;

            Emit(@event);

            amount = @event.DamageAmount;
            ragdoll = @event.Ragdoll.ToUnityVector();
            ragdollEffect = @event.RagdollEffect;
            trackKill = @event.TrackKill;
            dropLoot = @event.DropLoot;
            cancel = @event.IsCancelled;
        }

        private void Events_OnAnimalDead(Animal nativeAnimal, Vector3 ragdoll, ERagdollEffect ragdollEffect)
        {
            var animal = new UnturnedAnimal(nativeAnimal);

            var @event = new UnturnedAnimalDeadEvent(animal, ragdoll.ToSystemVector(), ragdollEffect);

            Emit(@event);
        }

        private void Events_OnAnimalFleeing(Animal nativeAnimal, ref Vector3 direction, ref bool sendToPack, ref bool cancel)
        {
            var animal = new UnturnedAnimal(nativeAnimal);

            var @event = new UnturnedAnimalFleeingEvent(animal, direction.ToSystemVector(), sendToPack)
            {
                IsCancelled = cancel
            };

            Emit(@event);

            direction = @event.Direction.ToUnityVector();
            sendToPack = @event.SendToPack;
            cancel = @event.IsCancelled;
        }

        private void Events_OnAnimalAttackingPoint(Animal nativeAnimal, ref Vector3 point, ref bool sendToPack, ref bool cancel)
        {
            var animal = new UnturnedAnimal(nativeAnimal);

            var @event = new UnturnedAnimalAttackingPointEvent(animal, point.ToSystemVector(), sendToPack)
            {
                IsCancelled = cancel
            };

            Emit(@event);

            point = @event.Point.ToUnityVector();
            sendToPack = @event.SendToPack;
            cancel = @event.IsCancelled;
        }

        private void Events_OnAnimalAttackingPlayer(Animal nativeAnimal, ref Player nativePlayer, ref bool sendToPack,
            ref bool cancel)
        {
            var animal = new UnturnedAnimal(nativeAnimal);

            var player = GetUnturnedPlayer(nativePlayer);

            var @event = new UnturnedAnimalAttackingPlayerEvent(animal, player, sendToPack)
            {
                IsCancelled = cancel
            };

            Emit(@event);

            nativePlayer = @event.Player?.Player;
            sendToPack = @event.SendToPack;
            cancel = @event.IsCancelled;
        }

        private delegate void AnimalSpawned(Animal nativeAnimal);
        private static event AnimalSpawned OnAnimalAdded;
        private static event AnimalSpawned OnAnimalRevived;

        private delegate void AnimalDamaging(Animal nativeAnimal, ref ushort amount, ref Vector3 ragdoll,
            ref ERagdollEffect ragdollEffect, ref bool trackKill, ref bool dropLoot, ref bool cancel);
        private static event AnimalDamaging OnAnimalDamaging;

        private delegate void AnimalDead(Animal nativeAnimal, Vector3 ragdoll, ERagdollEffect ragdollEffect);
        private static event AnimalDead OnAnimalDead;

        private delegate void AnimalFleeing(Animal nativeAnimal, ref Vector3 direction, ref bool sendToPack, ref bool cancel);
        private static event AnimalFleeing OnAnimalFleeing;

        private delegate void AnimalAttackingPoint(Animal nativeAnimal, ref Vector3 point, ref bool sendToPack, ref bool cancel);
        private static event AnimalAttackingPoint OnAnimalAttackingPoint;

        private delegate void AnimalAttackingPlayer(Animal nativeAnimal, ref Player player, ref bool sendToPack,
            ref bool cancel);
        private static event AnimalAttackingPlayer OnAnimalAttackingPlayer;

        [HarmonyPatch]
        private class Patches
        {
            [HarmonyPatch(typeof(AnimalManager), "addAnimal")]
            [HarmonyPostfix]
            private static void AddAnimal(Animal __result)
            {
                if (__result != null)
                {
                    OnAnimalAdded?.Invoke(__result);
                }
            }

            [HarmonyPatch(typeof(Animal), "tellAlive")]
            [HarmonyPostfix]
            private static void TellAlive(Animal __instance)
            {
                OnAnimalRevived?.Invoke(__instance);
            }

            [HarmonyPatch(typeof(Animal), "askDamage")]
            [HarmonyPrefix]
            private static bool AskDamage(Animal __instance, ref ushort amount, ref Vector3 newRagdoll,
                ref ERagdollEffect ragdollEffect, ref bool trackKill, ref bool dropLoot)
            {
                var cancel = false;

                if (amount == 0 || __instance.isDead) return false;

                OnAnimalDamaging?.Invoke(__instance, ref amount, ref newRagdoll, ref ragdollEffect, ref trackKill,
                    ref dropLoot, ref cancel);

                return !cancel;
            }

            [HarmonyPatch(typeof(Animal), "tellDead")]
            [HarmonyPostfix]
            private static void TellDead(Animal __instance, Vector3 newRagdoll, ERagdollEffect ragdollEffect)
            {
                OnAnimalDead?.Invoke(__instance, newRagdoll, ragdollEffect);
            }

            [HarmonyPatch(typeof(Animal), "alertDirection")]
            [HarmonyPrefix]
            private static bool AlertDirection(Animal __instance, ref Vector3 newDirection, ref bool sendToPack)
            {
                // Fleeing from given direction
                var cancel = false;

                OnAnimalFleeing?.Invoke(__instance, ref newDirection, ref sendToPack, ref cancel);

                return !cancel;
            }

            [HarmonyPatch(typeof(Animal), "alertGoToPoint")]
            [HarmonyPrefix]
            private static bool AlertGoToPoint(Animal __instance, ref Vector3 point, ref bool sendToPack)
            {
                // Attacking point
                var cancel = false;

                OnAnimalAttackingPoint?.Invoke(__instance, ref point, ref sendToPack, ref cancel);

                return !cancel;
            }

            [HarmonyPatch(typeof(Animal), "alertPlayer")]
            [HarmonyPrefix]
            private static bool AlertPlayer(Animal __instance, ref Player newPlayer, ref bool sendToPack)
            {
                // Attacking player
                var cancel = false;

                OnAnimalAttackingPlayer?.Invoke(__instance, ref newPlayer, ref sendToPack, out cancel);

                return !cancel;
            }
        }
    }
}
