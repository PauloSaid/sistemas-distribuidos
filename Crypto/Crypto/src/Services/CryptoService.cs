namespace Crypto.src.Services
{
    public class CryptoService : Greeter.GreeterBase
    {
        private readonly ILogger<CryptoService> _logger;
        public CryptoService(ILogger<CryptoService> logger)
        {
            _logger = logger;
        }
    }
}
