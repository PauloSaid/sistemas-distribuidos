
using Grpc.Core;
using Crypto.src.Infra.Entity;
using Crypto.src.Infra.Models;

namespace Crypto.Services
{
    public class WalletService : Wallet.WalletBase
    {
        private readonly Dictionary<Guid, WalletEntity> _wallets = new Dictionary<Guid, WalletEntity>();

        public WalletService()
        {
            var owner1 = new OwnerEntity(Guid.NewGuid(), "Alice");
            var crypto1 = new CryptoTypeEntity(Guid.NewGuid(), "Bitcoin", (float)50000.0);
            var crypto2 = new CryptoTypeEntity(Guid.NewGuid(), "Ethereum", (float)3000.0);
            _wallets.Add(Guid.NewGuid(), new WalletEntity(Guid.NewGuid(), owner1, 1.5, new List<CryptoTypeEntity> { crypto1, crypto2 }));

            var owner2 = new OwnerEntity(Guid.NewGuid(), "Bob");
            var crypto3 = new CryptoTypeEntity(Guid.NewGuid(), "Litecoin", (float)150.0);
            _wallets.Add(Guid.NewGuid(), new WalletEntity(Guid.NewGuid(), owner2, 100.0, new List<CryptoTypeEntity> { crypto3 }));
        }

        public override Task<GetWalletResponse> GetWallet(GetWalletRequest request, ServerCallContext context)
        {
            if (Guid.TryParse(request.Id, out var walletId) && _wallets.TryGetValue(walletId, out var walletEntity))
            {
                var walletResponse = new WalletResponse
                {
                    Id = walletEntity.Id.ToString(),
                    Balance = walletEntity.Balance,
                    Owner = new Owner { Id = walletEntity.Owner.Id.ToString(), Name = walletEntity.Owner.Name },
                    Cryptos = { walletEntity.Cryptos.Select(c => new CryptoType { Id = c.Id.ToString(), Name = c.Name, CurrencyValue = c.CurrencyValue }) }
                };
                return Task.FromResult(new GetWalletResponse { Wallet = walletResponse });
            }
            else
            {
                return Task.FromResult(new GetWalletResponse { });
            }
        }

        public override Task<CreateWalletResponse> CreateWallet(CreateWalletRequest request, ServerCallContext context)
        {
            var walletRequest = request.Wallet;
            if (walletRequest == null || !Guid.TryParse(walletRequest.Owner.Id, out var ownerId) || string.IsNullOrEmpty(walletRequest.Owner.Name))
            {
                return Task.FromResult(new CreateWalletResponse { });
            }

            var ownerEntity = new OwnerEntity(ownerId, walletRequest.Owner.Name);
            var cryptoEntities = walletRequest.Cryptos.Select(c =>
                new CryptoTypeEntity(Guid.Parse(c.Id), c.Name, float.Parse(c.CurrencyValue))).ToList();
            var newWalletEntity = new WalletEntity(Guid.NewGuid(), ownerEntity, walletRequest.Balance, cryptoEntities);

            _wallets.Add(newWalletEntity.Id, newWalletEntity);

            var walletResponse = new WalletResponse
            {
                Id = newWalletEntity.Id.ToString(),
                Balance = newWalletEntity.Balance,
                Owner = new Owner { Id = newWalletEntity.Owner.Id.ToString(), Name = newWalletEntity.Owner.Name },
                Cryptos = { newWalletEntity.Cryptos.Select(c => new CryptoType { Id = c.Id.ToString(), Name = c.Name, CurrencyValue = c.CurrencyValue }) } // Incluímos o CurrencyValue na resposta
            };

            return Task.FromResult(new CreateWalletResponse { Wallet = walletResponse });
        }
    }
}