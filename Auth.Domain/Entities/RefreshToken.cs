using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
    }
}
