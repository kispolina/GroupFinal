using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public bool IsRevoked { get; set; }
        public User User { get; set; } = null!;
        public int UserId { get; set; }
        public bool IsExpired => DateTime.Now >= Expires;
        public bool IsActive => !IsExpired && !IsRevoked;
    }
}
