using System.Collections.Concurrent; // Using ConcurrentDictionary for thread-safety in a singleton
using Crypto.src.Infra.Entity;
using Crypto.src.Infra.Models;
using Grpc.Core;

namespace Crypto.Services
{
    public class WalletService : Wallet.WalletBase
    {
        // Use ConcurrentDictionary for thread-safety when the service is a singleton
        // This ensures that multiple requests won't cause issues when modifying the dictionaries
        private static readonly ConcurrentDictionary<Guid, WalletEntity> _wallets = new ConcurrentDictionary<Guid, WalletEntity>();
        private static readonly ConcurrentDictionary<Guid, CryptoTypeEntity> _availableCryptoTypes = new ConcurrentDictionary<Guid, CryptoTypeEntity>();

        // Static constructor to initialize dictionaries once when the class is first loaded
        static WalletService()
        {
            // Initialize available cryptos
            var bitcoin = new CryptoTypeEntity(Guid.Parse("e09a1843-d5c8-4f7d-9967-0abad3296149"), "Bitcoin", 50000.0f);
            var ethereum = new CryptoTypeEntity(Guid.Parse("938acb6b-05b5-475c-bb4d-81e12aa34b56"), "Ethereum", 3000.0f);

            _availableCryptoTypes.TryAdd(bitcoin.Id, bitcoin);
            _availableCryptoTypes.TryAdd(ethereum.Id, ethereum);

            // Initialize Alice's wallet
            var owner1 = new OwnerEntity(Guid.Parse("6f689a22-8860-4459-993f-ccd749db0a84"), "Alice");
            var wallet1 = new WalletEntity(
                Guid.Parse("13413013-ae00-4b42-b02b-e76b0f7f886f"),
                owner1,
                1.5,
                new List<CryptoAmountEntity>
                {
                    new CryptoAmountEntity(bitcoin, 0.0025f),
                    new CryptoAmountEntity(ethereum, 0.1625f)
                });
            _wallets.TryAdd(wallet1.Id, wallet1);


            // Initialize Bob's wallet
            var owner2 = new OwnerEntity(Guid.Parse("63e33136-12a1-43ed-87dd-7d135cdc4ee0"), "Bob");
            var wallet2 = new WalletEntity(
                Guid.Parse("045148b7-3742-48f9-a1ae-a0703c63a9df"),
                owner2,
                100.0,
                new List<CryptoAmountEntity>
                {
                    new CryptoAmountEntity(bitcoin, 0.01197f),
                    new CryptoAmountEntity(ethereum, 0.08255f)
                });
            _wallets.TryAdd(wallet2.Id, wallet2);
        }

        // Public constructor (can be empty if using static initializers for data)
        public WalletService()
        {
            // No need to initialize dictionaries here if using static constructor
        }

        public override Task<GetWalletResponse> GetWallet(GetWalletRequest request, ServerCallContext context)
        {
            if (Guid.TryParse(request.Id, out var walletId) && _wallets.TryGetValue(walletId, out var walletEntity))
            {
                return Task.FromResult(new GetWalletResponse
                {
                    Wallet = MapToWalletResponse(walletEntity)
                });
            }

            // Return a default response or throw an RpcException if wallet not found
            throw new RpcException(new Status(StatusCode.NotFound, "Carteira não encontrada."));
        }

        public override Task<CreateWalletResponse> CreateWallet(CreateWalletRequest request, ServerCallContext context)
        {
            var walletRequest = request.Wallet;

            if (walletRequest == null ||
                walletRequest.Owner == null ||
                !Guid.TryParse(walletRequest.Owner.Id, out var ownerId) ||
                string.IsNullOrWhiteSpace(walletRequest.Owner.Name) ||
                !Guid.TryParse(walletRequest.Id, out var walletId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Dados do proprietário ou ID da carteira inválidos."));
            }

            // Check if a wallet with this ID already exists
            if (_wallets.ContainsKey(walletId))
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, $"Carteira com ID {walletId} já existe."));
            }

            var ownerEntity = new OwnerEntity(ownerId, walletRequest.Owner.Name);
            var cryptoAmountEntities = new List<CryptoAmountEntity>();

            foreach (var crypto in walletRequest.Cryptos)
            {
                if (!Guid.TryParse(crypto.Id, out var cryptoId) ||
                    !float.TryParse(crypto.Amount, out var amount))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Cripto inválida ou valor ausente."));
                }

                if (_availableCryptoTypes.TryGetValue(cryptoId, out var existingCryptoType))
                {
                    cryptoAmountEntities.Add(new CryptoAmountEntity(existingCryptoType, amount));
                }
                else
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Tipo de cripto com ID {crypto.Id} não encontrado."));
                }
            }

            var newWalletEntity = new WalletEntity(walletId, ownerEntity, walletRequest.Balance, cryptoAmountEntities);

            // Correctly add the wallet to the dictionary using TryAdd for thread-safety
            if (!_wallets.TryAdd(walletId, newWalletEntity))
            {
                // This case should ideally be caught by the ContainsKey check above,
                // but TryAdd provides an extra layer of safety.
                throw new RpcException(new Status(StatusCode.AlreadyExists, $"Falha ao criar carteira com ID {walletId}. Pode já existir."));
            }

            return Task.FromResult(new CreateWalletResponse
            {
                Wallet = MapToWalletResponse(newWalletEntity)
            });
        }

        public override Task<ListAvailableCryptoTypesResponse> ListAvailableCryptoTypes(ListAvailableCryptoTypesRequest request, ServerCallContext context)
        {
            var response = new ListAvailableCryptoTypesResponse();
            response.CryptoTypes.AddRange(
                _availableCryptoTypes.Values.Select(ct => new AvailableCryptoType
                {
                    Id = ct.Id.ToString(),
                    Name = ct.Name,
                    CurrencyValue = ct.CurrencyValue.ToString()
                }));

            return Task.FromResult(response);
        }

        public override Task<UpdateCryptoResponse> UpdateCrypto(UpdateCryptoRequest request, ServerCallContext context)
        {
            if (!Guid.TryParse(request.WalletId, out var walletId) || walletId == Guid.Empty || !_wallets.TryGetValue(walletId, out var wallet))
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Carteira não encontrada."));
            }

            // Adicionar criptos
            foreach (var crypto in request.CryptosToAdd)
            {
                if (!Guid.TryParse(crypto.Id, out var cryptoId) || !float.TryParse(crypto.Amount, out var amount))
                {
                    // Log or handle invalid crypto add request, but don't stop the whole process
                    continue;
                }

                if (_availableCryptoTypes.TryGetValue(cryptoId, out var cryptoType))
                {
                    // This part needs careful handling with ConcurrentDictionary.
                    // For modifications to nested lists/objects within a ConcurrentDictionary value,
                    // you typically need to ensure thread safety of those nested objects themselves,
                    // or use locks if their modification is not atomic.
                    // For simplicity, we'll assume CryptoAmountEntity and its list operations are handled.
                    // In a highly concurrent scenario, you might need to lock the wallet object
                    // or re-fetch/update the wallet in the dictionary.

                    var existing = wallet.Cryptos.FirstOrDefault(c => c.CryptoType.Id == cryptoId);
                    if (existing != null)
                    {
                        existing.Amount += amount;
                    }
                    else
                    {
                        wallet.Cryptos.Add(new CryptoAmountEntity(cryptoType, amount));
                    }
                }
                else
                {
                    // Log or handle the case where cryptoType is not available
                    Console.WriteLine($"Tentativa de adicionar cripto com ID desconhecido: {cryptoId}");
                }
            }

            // Remover criptos
            foreach (var cryptoIdStr in request.CryptoIdsToRemove)
            {
                if (Guid.TryParse(cryptoIdStr, out var cryptoId))
                {
                    // Again, direct modification of the list within the stored object.
                    // Consider thread-safety if 'wallet.Cryptos' itself is not thread-safe and
                    // accessed by multiple threads simultaneously.
                    wallet.Cryptos.RemoveAll(c => c.CryptoType.Id == cryptoId);
                }
            }

            // After modifications, you might want to "update" the wallet in the dictionary
            // to ensure other threads see the latest version, especially if you were to replace the entire wallet object.
            // For simple modifications to existing nested properties, direct modification is often sufficient
            // if the WalletEntity itself is not replaced entirely.
            // If you replaced the wallet entity entirely, you would do something like:
            // _wallets.TryUpdate(walletId, newWalletEntity, oldWalletEntity);

            return Task.FromResult(new UpdateCryptoResponse
            {
                Wallet = MapToWalletResponse(wallet)
            });
        }


        public override Task<TransferCryptoResponse> TransferCrypto(TransferCryptoRequest request, ServerCallContext context)
        {
            if (!Guid.TryParse(request.FromWalletId, out var fromId) ||
                !Guid.TryParse(request.ToWalletId, out var toId) ||
                !Guid.TryParse(request.CryptoId, out var cryptoId) ||
                !float.TryParse(request.Amount, out var amount))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Parâmetros inválidos."));
            }

            // Retrieve wallets using TryGetValue
            if (!_wallets.TryGetValue(fromId, out var fromWallet))
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Carteira de origem não encontrada."));
            }
            if (!_wallets.TryGetValue(toId, out var toWallet))
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Carteira de destino não encontrada."));
            }

            // Basic check to prevent self-transfer that might complicate logic, unless intended
            if (fromId == toId)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Não é possível transferir para a mesma carteira."));
            }

            // Important: When modifying shared objects like wallets in a concurrent environment,
            // you should often use locks to prevent race conditions.
            // A common strategy is to lock on the wallets involved in the transaction.
            // However, this can lead to deadlocks if not carefully ordered (e.g., always lock on the smaller ID first).
            // For this in-memory example, we'll demonstrate a basic locking, but in a real-world scenario,
            // database transactions handle this much more robustly.

            // Example of locking both wallets to ensure atomic transfer
            // This is a simplified approach and might not be suitable for all high-concurrency scenarios.
            // A more sophisticated approach might involve a transaction manager or retry logic.
            lock (fromWallet) // Lock the source wallet
            {
                lock (toWallet) // Lock the destination wallet
                {
                    var fromCrypto = fromWallet.Cryptos.FirstOrDefault(c => c.CryptoType.Id == cryptoId);
                    if (fromCrypto == null || fromCrypto.Amount < amount)
                    {
                        throw new RpcException(new Status(StatusCode.FailedPrecondition, "Saldo insuficiente na carteira de origem."));
                    }

                    fromCrypto.Amount -= amount;
                    if (fromCrypto.Amount <= 0)
                    {
                        fromWallet.Cryptos.Remove(fromCrypto);
                    }

                    var toCrypto = toWallet.Cryptos.FirstOrDefault(c => c.CryptoType.Id == cryptoId);
                    if (toCrypto != null)
                    {
                        toCrypto.Amount += amount;
                    }
                    else if (_availableCryptoTypes.TryGetValue(cryptoId, out var cryptoType))
                    {
                        toWallet.Cryptos.Add(new CryptoAmountEntity(cryptoType, amount));
                    }
                    else
                    {
                        // This case means the crypto type exists in fromWallet but not in available types, which is inconsistent.
                        throw new RpcException(new Status(StatusCode.NotFound, $"Tipo de cripto com ID {cryptoId} não encontrado nos tipos disponíveis."));
                    }
                }
            }


            return Task.FromResult(new TransferCryptoResponse
            {
                FromWallet = MapToWalletResponse(fromWallet),
                ToWallet = MapToWalletResponse(toWallet)
            });
        }

        public override Task<DeleteWalletResponse> DeleteWallet(DeleteWalletRequest request, ServerCallContext context)
        {
            if (!Guid.TryParse(request.WalletId, out var walletId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "ID da carteira inválido."));
            }

            // Use TryRemove for thread-safe removal from ConcurrentDictionary
            var success = _wallets.TryRemove(walletId, out _); // The 'out _' discards the removed value

            if (!success)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Carteira não encontrada para exclusão."));
            }

            return Task.FromResult(new DeleteWalletResponse { Success = success });
        }

        private WalletResponse MapToWalletResponse(WalletEntity wallet)
        {
            // Ensure wallet is not null before mapping
            if (wallet == null)
            {
                return null; // Or throw an exception, depending on desired behavior
            }

            return new WalletResponse
            {
                Id = wallet.Id.ToString(),
                Balance = wallet.Balance,
                Owner = new Owner
                {
                    Id = wallet.Owner.Id.ToString(),
                    Name = wallet.Owner.Name
                },
                Cryptos =
                {
                    // Convert the list of CryptoAmountEntity to the gRPC CryptoType messages
                    wallet.Cryptos.Select(ca => new CryptoType
                    {
                        Id = ca.CryptoType.Id.ToString(),
                        Name = ca.CryptoType.Name,
                        CurrencyValue = ca.CryptoType.CurrencyValue.ToString(),
                        Amount = ca.Amount.ToString()
                    })
                }
            };
        }
    }
}