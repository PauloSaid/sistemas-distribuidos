namespace Crypto.src.Infra.Entity
{
    public class CryptoAmountEntity
    {
        public CryptoAmountEntity(CryptoTypeEntity cryptoTypeEntity, float amount) 
        {
            CryptoType = cryptoTypeEntity;
            Amount = amount;
        }
        public CryptoTypeEntity CryptoType { get; set; }
        public float Amount { get; set; }
    }
}
