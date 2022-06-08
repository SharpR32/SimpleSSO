using HeimdallMini.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeimdallMini.Domain.Entities
{
    public class Login: ReadOnlyEntity
    {
        public string IpAddress { get; init; } = string.Empty;
    }
}
