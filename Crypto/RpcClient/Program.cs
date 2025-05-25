using Crypto;
using Grpc.Net.Client;

class Program
{
    static async Task Main()
    {
        const string grpcServerUrl = "https://localhost:7247";
        var channel = GrpcChannel.ForAddress(grpcServerUrl);
        var client = new Wallet.WalletClient(channel);

        // [1] Listar criptomoedas disponíveis
        Console.WriteLine("🔍 Listando tipos de criptomoedas disponíveis...");
        var availableResponse = await client.ListAvailableCryptoTypesAsync(new ListAvailableCryptoTypesRequest());

        if (!availableResponse.CryptoTypes.Any())
        {
            Console.WriteLine("⚠️ Nenhuma criptomoeda disponível no sistema.");
            return;
        }

        foreach (var crypto in availableResponse.CryptoTypes)
        {
            Console.WriteLine($"✅ {crypto.Name} (ID: {crypto.Id}, Valor: {crypto.CurrencyValue})");
        }

        var bitcoin = availableResponse.CryptoTypes.FirstOrDefault(c => c.Name == "Bitcoin");
        var ethereum = availableResponse.CryptoTypes.FirstOrDefault(c => c.Name == "Ethereum");

        if (bitcoin == null || ethereum == null)
        {
            Console.WriteLine("❌ Criptomoedas 'Bitcoin' ou 'Ethereum' não encontradas.");
            return;
        }

        // [2] Criar carteira com Bitcoin
        Console.WriteLine("\n💼 Criando nova carteira com Bitcoin...");
        var walletId1 = Guid.NewGuid().ToString();
        var ownerId1 = Guid.NewGuid().ToString();

        var createWalletReply = await client.CreateWalletAsync(new CreateWalletRequest
        {
            Wallet = new WalletRequest
            {
                Id = walletId1,
                Owner = new Owner
                {
                    Id = ownerId1,
                    Name = "Zézinho do Foguete"
                },
                Balance = 500.25,
                Cryptos =
                {
                    new CryptoType
                    {
                        Id = bitcoin.Id,
                        Name = bitcoin.Name,
                        CurrencyValue = bitcoin.CurrencyValue,
                        Amount = "0.005"
                    }
                }
            }
        });

        Console.WriteLine("🆕 Carteira criada:");
        Console.WriteLine(createWalletReply.Wallet);

        // [3] Consultar saldo da carteira
        Console.WriteLine("\n💰 Consultando saldo da carteira...");
        var getWalletReply = await client.GetWalletAsync(new GetWalletRequest { Id = walletId1 });
        Console.WriteLine(getWalletReply.Wallet);

        // [4] Atualizar carteira: adicionar Ethereum, remover Bitcoin
        Console.WriteLine("\n🔄 Atualizando criptos da carteira (add ETH, remove BTC)...");
        var updateReply = await client.UpdateCryptoAsync(new UpdateCryptoRequest
        {
            WalletId = walletId1,
            CryptosToAdd =
            {
                new CryptoType
                {
                    Id = ethereum.Id,
                    Name = ethereum.Name,
                    CurrencyValue = ethereum.CurrencyValue,
                    Amount = "1.0"
                }
            },
            CryptoIdsToRemove = { bitcoin.Id }
        });

        Console.WriteLine("✅ Carteira atualizada:");
        Console.WriteLine(updateReply.Wallet);

        // [5] Criar segunda carteira
        Console.WriteLine("\n💼 Criando segunda carteira...");
        var walletId2 = Guid.NewGuid().ToString();
        var ownerId2 = Guid.NewGuid().ToString();

        var createSecondWalletReply = await client.CreateWalletAsync(new CreateWalletRequest
        {
            Wallet = new WalletRequest
            {
                Id = walletId2,
                Owner = new Owner
                {
                    Id = ownerId2,
                    Name = "Maria da Blockchain"
                },
                Balance = 100.00
            }
        });

        Console.WriteLine("🆕 Segunda carteira criada:");
        Console.WriteLine(createSecondWalletReply.Wallet);

        // [6] Transferir 0.5 ETH entre carteiras
        Console.WriteLine($"\n💸 Transferindo 0.5 ETH de {walletId1} para {walletId2}...");
        var transferReply = await client.TransferCryptoAsync(new TransferCryptoRequest
        {
            FromWalletId = walletId1,
            ToWalletId = walletId2,
            CryptoId = ethereum.Id,
            Amount = "0.5"
        });

        Console.WriteLine("📤 Carteira origem:");
        Console.WriteLine(transferReply.FromWallet);
        Console.WriteLine("📥 Carteira destino:");
        Console.WriteLine(transferReply.ToWallet);

        // [7] Excluir primeira carteira
        Console.WriteLine($"\n🗑️ Excluindo carteira {walletId1}...");
        var deleteReply = await client.DeleteWalletAsync(new DeleteWalletRequest { WalletId = walletId1 });
        Console.WriteLine(deleteReply.Success
            ? $"✅ Carteira {walletId1} excluída com sucesso."
            : $"❌ Falha ao excluir a carteira {walletId1}.");
    }
}
