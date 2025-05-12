namespace Crypto.src.Infra.Entity
{
    public class CryptoType
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public float Amount { get; set; }
        public float CurrencyValue { get; set; }
    }
}
