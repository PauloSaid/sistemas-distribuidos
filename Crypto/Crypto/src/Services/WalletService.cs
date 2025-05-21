using Grpc.Core;
using Crypto.src.Infra.Entity;
using Crypto.src.Infra.Models;

namespace Crypto.Services
{
    public class WalletService : Wallet.WalletBase
    {
        private readonly Dictionary<Guid, WalletEntity> _wallets = new Dictionary<Guid, WalletEntity>();
        private readonly Dictionary<Guid, CryptoTypeEntity> _availableCryptoTypes = new Dictionary<Guid, CryptoTypeEntity>();

        public WalletService()
        {
            var bitcoin = new CryptoTypeEntity(Guid.Parse("e09a1843-d5c8-4f7d-9967-0abad3296149"), "Bitcoin", 50000.0f);
            var ethereum = new CryptoTypeEntity(Guid.Parse("938acb6b-05b5-475c-bb4d-81e12aa34b56"), "Ethereum", 3000.0f);

            _availableCryptoTypes.Add(bitcoin.Id, bitcoin);
            _availableCryptoTypes.Add(ethereum.Id, ethereum);

            var owner1 = new OwnerEntity(Guid.Parse("6f689a22-8860-4459-993f-ccd749db0a84"), "Alice");
            var cryptoValue1 = new CryptoAmountEntity(bitcoin, 0.0025f);
            var cryptoValue2 = new CryptoAmountEntity(ethereum, 0.1625f);
            _wallets.Add(Guid.Parse("c1649449-c560-4eb3-a08f-38fd443113e2"), new WalletEntity(Guid.Parse("13413013-ae00-4b42-b02b-e76b0f7f886f"), owner1, 1.5, new List<CryptoAmountEntity> { cryptoValue1, cryptoValue2 }));

            var owner2 = new OwnerEntity(Guid.Parse("63e33136-12a1-43ed-87dd-7d135cdc4ee0"), "Bob");
            var cryptoValue3 = new CryptoAmountEntity(bitcoin, 0.01197f);
            var cryptoValue4 = new CryptoAmountEntity(ethereum, 0.08255f);
            _wallets.Add(Guid.Parse("e4e9cbba-4775-4a5e-bc73-0a63bcda73c9"), new WalletEntity(Guid.Parse("045148b7-3742-48f9-a1ae-a0703c63a9df"), owner2, 100.0, new List<CryptoAmountEntity> { cryptoValue3, cryptoValue4 }));
        }

        public override Task<GetWalletResponse> GetWallet(GetWalletRequest request, ServerCallContext context)
        {
            // This verification it Is actually not working. After creating a new wallet, the new wallet Id it's not been added to the _wallet dictionary, so the program will never find the wallet.
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
            var cryptoAmountEntities = new List<CryptoAmountEntity>();

            foreach (var crypto in walletRequest.Cryptos)
            {
                if (!Guid.TryParse(crypto.Id, out var cryptoId) || !float.TryParse(crypto.Amount, out var amount))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid CryptoType data or Amount missing."));
                }

                if (_availableCryptoTypes.TryGetValue(cryptoId, out var existingCryptoType))
                {
                    cryptoAmountEntities.Add(new CryptoAmountEntity(existingCryptoType, amount));
                }
                else
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"CryptoType with ID {crypto.Id} not found."));
                }
            }

            var newWalletId = Guid.NewGuid();
            var newWalletEntity = new WalletEntity(newWalletId, ownerEntity, walletRequest.Balance, cryptoAmountEntities);

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

        public override async Task<ListAvailableCryptoTypesResponse> ListAvailableCryptoTypes(ListAvailableCryptoTypesRequest request, ServerCallContext context)
        {
            var response = new ListAvailableCryptoTypesResponse();
            response.CryptoTypes.AddRange(_availableCryptoTypes.Values.Select(ct => new AvailableCryptoType
            {
                Id = ct.Id.ToString(),
                Name = ct.Name,
                CurrencyValue = ct.CurrencyValue.ToString()
            }));
            return await Task.FromResult(response);
        }
    }
}