using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IJwtServices
    {
        public string GenerateAccessToken(int userId, string role);
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
