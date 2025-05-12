using Crypto.src.Infra.Models;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;

namespace Crypto.src.Services
{
    public class WalletService : Wallet.WalletBase
    {
        public override Task<CreateWalletResponse> CreateWallet(CreateWalletRequest request, ServerCallContext context)
        {

        }
    }
}
