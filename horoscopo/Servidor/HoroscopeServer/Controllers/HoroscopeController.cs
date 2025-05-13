using Microsoft.AspNetCore.Mvc;
using HoroscopeServer.Models;

namespace HoroscopeServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HoroscopeController : ControllerBase
    {
        [HttpGet("{nickname}/{sign}/{plan}")]
        public IActionResult GetHoroscope(string nickname, string sign, string plan)
        {
            var result = new HoroscopeResult
            {
                Nickname = nickname,
                Sign = sign,
                Message = $"Hoje é um ótimo dia para quem é de {sign}!",
                LuckyNumber = plan != "basico" ? new Random().Next(1, 100) : null,
                FinanceTip = plan == "premium" ? "Evite grandes compras hoje." : null,
                LoveTip = plan == "premium" ? "Uma surpresa romântica pode mudar seu dia." : null
            };

            return Ok(result);
        }
    }
}