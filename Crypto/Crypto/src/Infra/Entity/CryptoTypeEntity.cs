namespace Crypto.src.Infra.Entity
{
    public class CryptoTypeEntity
    {
        public CryptoTypeEntity(Guid id, string name, float currencyValue)
        {
            Id = id;
            Name = name;
            CurrencyValue = currencyValue;
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public float CurrencyValue { get; set; }
    }
}
