using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeimdallMini.Domain.Entities.Common
{
    public abstract class ReadOnlyEntity
    {
        public Guid Id { get; init; }
        public DateTime Created { get; init; } = DateTime.UtcNow;
    }
    public abstract class AuditableEntity: ReadOnlyEntity
    {
        public DateTime Updated { get; protected set; }

        public void SetUpdateTime()
            => Updated = DateTime.UtcNow;
    }
}
