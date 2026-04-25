using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IJwtService
    {
       string GenerateToken(User user);
        string GenerateRefreshToken();
    }
}
