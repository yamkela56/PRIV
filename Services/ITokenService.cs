using PRIV.Models;

namespace PRIV.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
