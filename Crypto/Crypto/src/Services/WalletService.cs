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
            var bitcoin = new CryptoTypeEntity(Guid.NewGuid(), "Bitcoin", 50000.0f);
            var ethereum = new CryptoTypeEntity(Guid.NewGuid(), "Ethereum", 3000.0f);

            var owner1 = new OwnerEntity(Guid.NewGuid(), "Alice");
            var cryptoValue1 = new CryptoAmountEntity(bitcoin, 0.0025f);
            var cryptoValue2 = new CryptoAmountEntity(ethereum, 0.1625f);
            _wallets.Add(Guid.NewGuid(), new WalletEntity(Guid.NewGuid(), owner1, 1.5, new List<CryptoAmountEntity> { cryptoValue1, cryptoValue2 }));

            var owner2 = new OwnerEntity(Guid.NewGuid(), "Bob");
            var cryptoValue3 = new CryptoAmountEntity(bitcoin, 0.01197f);
            var cryptoValue4 = new CryptoAmountEntity(ethereum, 0.08255f);
            _wallets.Add(Guid.NewGuid(), new WalletEntity(Guid.NewGuid(), owner2, 100.0, new List<CryptoAmountEntity> { cryptoValue3, cryptoValue4 }));
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
                    Cryptos = { walletEntity.Cryptos.Select(cryptoAmount => new CryptoType
                    {
                        Id = cryptoAmount.CryptoType.Id.ToString(),
                        Name = cryptoAmount.CryptoType.Name,
                        CurrencyValue = cryptoAmount.CryptoType.CurrencyValue.ToString(),
                        Amount = cryptoAmount.Amount.ToString()
                    }) }
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
            var cryptoAmountEntities = walletRequest.Cryptos.Select(c =>
            {
                if (!Guid.TryParse(c.Id, out var cryptoId) || !float.TryParse(c.CurrencyValue, out var currencyValue) || !float.TryParse(c.Amount, out var amount))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid CryptoType data"));
                }
                var cryptoTypeEntity = new CryptoTypeEntity(cryptoId, c.Name, currencyValue);
                return new CryptoAmountEntity(cryptoTypeEntity, amount);
            }).ToList();

            var newWalletEntity = new WalletEntity(Guid.NewGuid(), ownerEntity, walletRequest.Balance, cryptoAmountEntities);

            _wallets.Add(newWalletEntity.Id, newWalletEntity);

            var walletResponse = new WalletResponse
            {
                Id = newWalletEntity.Id.ToString(),
                Balance = newWalletEntity.Balance,
                Owner = new Owner { Id = newWalletEntity.Owner.Id.ToString(), Name = newWalletEntity.Owner.Name },
                Cryptos = { newWalletEntity.Cryptos.Select(ca => new CryptoType
                {
                    Id = ca.CryptoType.Id.ToString(),
                    Name = ca.CryptoType.Name,
                    CurrencyValue = ca.CryptoType.CurrencyValue.ToString(),
                    Amount = ca.Amount.ToString()
                }) }
            };

            return Task.FromResult(new CreateWalletResponse { Wallet = walletResponse });
        }
    }
}