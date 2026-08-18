namespace Framework.Core.Data
{
    public interface IEntityDto
    {
    }

    public interface IEntityDto<TKey> : IEntityDto
    {
        TKey Id { get; set; }
    }
}