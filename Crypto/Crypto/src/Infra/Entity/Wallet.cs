using Crypto.src.Infra.Entity;

namespace Crypto.src.Infra.Models
{
    public class Wallet
    {
        public Guid Id { get; set; }
        public required Owner Owner { get; set; }
        public double Balance { get; set; }
        public required List<CryptoType> Cryptos { get; set; }
    }
}
