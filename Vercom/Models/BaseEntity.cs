using System;

namespace Vercom.Models;

public abstract class BaseEntity
{
    public virtual Guid Id { get; set; } = Guid.NewGuid();
    public virtual DateTimeOffset CreadoEn { get; set; } = DateTimeOffset.Now;
}

public interface IMultiTenantEntity
{
    Guid EntidadId { get; set; }
}

public abstract class MultiTenantEntity : BaseEntity, IMultiTenantEntity
{
    public virtual Guid EntidadId { get; set; }
}
