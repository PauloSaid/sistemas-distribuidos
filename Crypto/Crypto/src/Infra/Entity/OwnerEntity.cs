namespace Crypto.src.Infra.Entity
{
    public class OwnerEntity
    {
        public OwnerEntity(Guid id, string name) 
        {
            Id = id;
            Name = name;
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
