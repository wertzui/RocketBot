﻿using GalaSoft.MvvmLight.Command;
using POGOProtos.Data;
using POGOProtos.Networking.Responses;
using PokemonGo.Bot.Messages;
using PokemonGo.Bot.MVVMLightUtils;
using PokemonGo.Bot.Utils;
using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PokemonGo.Bot.ViewModels
{
    public class CaughtPokemonViewModel : PokemonDataViewModel, IUpdateable<CaughtPokemonViewModel>
    {
        readonly SessionViewModel session;
        readonly InventoryViewModel inventory;

        #region Transfer

        AsyncRelayCommand transfer;

        public AsyncRelayCommand Transfer
        {
            get
            {
                if (transfer == null)
                    transfer = new AsyncRelayCommand(ExecuteTransfer, CanExecuteTransfer);

                return transfer;
            }
        }

        async Task ExecuteTransfer()
        {
            var response = await session.TransferPokemon(Id);
            if (response.Result == ReleasePokemonResponse.Types.Result.Success)
            {
                inventory.Pokemon.Remove(this);
                inventory.AddCandyForFamily(response.CandyAwarded, FamilyId);
                MessengerInstance.Send(new Message(Colors.Green, $"Transferred {Name} ({CombatPoints} CP, {PerfectPercentage:P2} IV). Got {response.CandyAwarded} Candy."));
            }
            else
            {
                MessengerInstance.Send(new Message(Colors.Red, $"Error transferring {Name}: {Enum.GetName(typeof(ReleasePokemonResponse.Types.Result), response.Result)}"));
            }
        }

        bool CanExecuteTransfer() => true;

        #endregion Transfer

        #region ToggleFavorite

        AsyncRelayCommand toggleFavorite;

        public AsyncRelayCommand ToggleFavorite
        {
            get
            {
                if (toggleFavorite == null)
                    toggleFavorite = new AsyncRelayCommand(ExecuteToggleFavorite, CanExecuteToggleFavorite);

                return toggleFavorite;
            }
        }

        async Task ExecuteToggleFavorite()
        {
            var targetValue = !IsFavorite;
            var response = await session.SetFavoritePokemon(Id, targetValue);
            if (response.Result == SetFavoritePokemonResponse.Types.Result.Success)
                IsFavorite = targetValue;
        }

        bool CanExecuteToggleFavorite() => true;

        #endregion ToggleFavorite

        #region Evolve

        AsyncRelayCommand evolve;

        public AsyncRelayCommand Evolve
        {
            get
            {
                if (evolve == null)
                    evolve = new AsyncRelayCommand(ExecuteEvolve, CanExecuteEvolve);

                return evolve;
            }
        }

        async Task ExecuteEvolve()
        {
            var response = await session.EvolvePokemon(Id);
        }

        bool CanExecuteEvolve() => CandyToEvolve > 0 && CandyToEvolve <= Candy;

        #endregion Evolve

        #region Upgrade

        AsyncRelayCommand upgrade;

        public AsyncRelayCommand Upgrade
        {
            get
            {
                if (upgrade == null)
                    upgrade = new AsyncRelayCommand(ExecuteUpgrade, CanExecuteUpgrade);

                return upgrade;
            }
        }

        async Task ExecuteUpgrade()
        {
            var response = await session.UpgradePokemon(Id);
        }

        bool CanExecuteUpgrade() => CandyToUpgrade <= Candy;

        #endregion Upgrade

        int candy;

        public int Candy
        {
            get { return candy; }
            set { if (Candy != value) { candy = value; RaisePropertyChanged(); Evolve.RaiseCanExecuteChanged(); Upgrade.RaiseCanExecuteChanged(); } }
        }

        int level;

        public int Level
        {
            get { return level; }
            set { if (Level != value) { level = value; RaisePropertyChanged(); } }
        }

        int candyToUpgrade;
        public int CandyToUpgrade
        {
            get { return candyToUpgrade; }
            set { if (CandyToUpgrade != value) { candyToUpgrade = value; RaisePropertyChanged(); Upgrade.RaiseCanExecuteChanged(); } }
        }

        int stardustToupgrade;
        public int StardustToUpgrade
        {
            get { return stardustToupgrade; }
            set { if (StardustToUpgrade != value) { stardustToupgrade = value; RaisePropertyChanged(); Upgrade.RaiseCanExecuteChanged(); } }
        }

        public CaughtPokemonViewModel(PokemonData pokemon, SessionViewModel session, InventoryViewModel inventory, Settings settings) : base(pokemon, settings)
        {
            if (Id == 0)
                throw new ArgumentException("This is not a caught pokemon.", nameof(pokemon));

            this.session = session;
            this.inventory = inventory;

            Candy = inventory.GetCandyForFamily(FamilyId);
            CalculateLevel();
        }

        void CalculateLevel()
        {
            var pokemonSettings = settings.GetPokemonSettings(PokemonId);
            Level = settings.PlayerLevel.GetLevelFromCpMultiplier(CpMultiplier);
            CandyToUpgrade = settings.PokemonUpgrade.GetCandyCostForUpgradeFromLevel(Level);
            StardustToUpgrade = settings.PokemonUpgrade.GetStardustCostForUpgradeFromLevel(Level);
        }

        public void UpdateWith(CaughtPokemonViewModel other)
        {
            if (!Equals(other))
                throw new ArgumentException($"Expected a {Name} with Id {Id} but got a {other?.Name} with Id {other?.Id}", nameof(other));

            base.UpdateWith(other);

            Candy = other.Candy;
            CalculateLevel();
        }
    }
}