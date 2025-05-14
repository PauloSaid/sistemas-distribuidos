using Crypto.src.Infra.Entity;

namespace Crypto.src.Infra.Models
{
    public class WalletEntity
    {
        public WalletEntity(Guid id, OwnerEntity owner, double balance, List<CryptoTypeEntity> cryptos)
        {
            Id = id;
            Owner = owner;
            Balance = balance;
            Cryptos = cryptos;
        }

        public Guid Id { get; set; }
        public OwnerEntity Owner { get; set; }
        public double Balance { get; set; }
        public List<CryptoTypeEntity> Cryptos { get; set; }
    }
}
