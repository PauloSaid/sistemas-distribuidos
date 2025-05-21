/*
 * Implementação do Web Services: Deve-se implementar as operações básicas de CRUD utilizando o
protocolo HTTP e o formato JSON para troca de dados. As operações podem incluir:
• 1 = Criação de uma nova carteira de criptomoedas;
• 2 = Consulta do saldo da carteira;
• 3 = Adição ou remoção de criptomoedas da carteira;
• 4 = Transferência de criptomoedas entre carteiras;
• 5 = Exclusão de uma carteira.
*/

using Crypto;
using Grpc.Net.Client;

const string localhost = "https://localhost:7247";

var channel = GrpcChannel.ForAddress(localhost);

var client = new Wallet.WalletClient(channel);

Console.WriteLine("Listando tipos de criptomoedas disponíveis...");
var availableCryptoTypesResponse = await client.ListAvailableCryptoTypesAsync(new ListAvailableCryptoTypesRequest());

if (availableCryptoTypesResponse.CryptoTypes.Any())
{
    Console.WriteLine("Criptomoedas disponíveis:");
    foreach (var cryptoType in availableCryptoTypesResponse.CryptoTypes)
    {
        Console.WriteLine($"- {cryptoType.Name} (ID: {cryptoType.Id}, Valor: {cryptoType.CurrencyValue})");
    }

    var bitcoinType = availableCryptoTypesResponse.CryptoTypes.FirstOrDefault(c => c.Name == "Bitcoin");
    var ethereumType = availableCryptoTypesResponse.CryptoTypes.FirstOrDefault(c => c.Name == "Ethereum");

    if (bitcoinType != null && ethereumType != null)
    {
        Console.WriteLine("\nCriando nova carteira com criptomoedas existentes...");
        var reply1 = await client.CreateWalletAsync(
            new CreateWalletRequest
            {
                Wallet = new WalletRequest
                {
                    Id = Guid.NewGuid().ToString(),
                    Owner = new Owner
                    {
                        Id = "c0c4c2c7-4453-4062-9efc-fd38f35ed6aa",
                        Name = "Zézinho do Foguete"
                    },
                    Balance = 500.25,
                    Cryptos =
                    {
                        new CryptoType
                        {
                            Id = bitcoinType.Id,
                            Name = bitcoinType.Name,
                            CurrencyValue = bitcoinType.CurrencyValue,
                            Amount = "0.005"
                        },
                        new CryptoType
                        {
                            Id = ethereumType.Id,
                            Name = ethereumType.Name,
                            CurrencyValue = ethereumType.CurrencyValue,
                            Amount = "0.2"
                        }
                    }
                },
            });

        Console.WriteLine("\nCarteira criada:");
        Console.WriteLine(reply1.Wallet.ToString());

        var createdWalletId = reply1.Wallet.Id;
        var getCreatedWalletReply = await client.GetWalletAsync(
            new GetWalletRequest
            {
                Id = createdWalletId
            });

        Console.WriteLine($"\nConsultando a carteira com ID: {createdWalletId}");
        Console.WriteLine(getCreatedWalletReply.ToString());
    }
    else
    {
        Console.WriteLine("Criptomoedas 'Bitcoin' ou 'Ethereum' não encontradas na lista de tipos disponíveis.");
    }
}
else
{
    Console.WriteLine("Nenhum tipo de criptomoeda disponível no serviço.");
}

var reply = await client.GetWalletAsync(
    new GetWalletRequest
    {
        Id = "c1649449-c560-4eb3-a08f-38fd443113e2"
    });

Console.WriteLine("\nCarteira existente (original):");
Console.WriteLine(reply.ToString());