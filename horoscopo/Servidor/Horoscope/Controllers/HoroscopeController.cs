using Microsoft.AspNetCore.Mvc;
using HoroscopeServer.Models;
using System.Globalization;

namespace HoroscopeServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HoroscopeController : ControllerBase
    {
        private static readonly Dictionary<string, string[]> SignMessages = new()
        {
            ["Áries"] = new[]
            {
                "A energia está ao seu favor hoje.",
                "Evite confrontos desnecessários.",
                "Um novo começo pode surgir do inesperado.",
                "Hoje é um ótimo dia para tomar decisões importantes."
            },
            ["Touro"] = new[]
            {
                "Seu foco será recompensado.",
                "Evite teimosias para manter a harmonia.",
                "Um prazer simples trará grande felicidade.",
                "Valorize o conforto do seu lar hoje."
            },
            ["Gêmeos"] = new[]
            {
                "Converse com alguém que não vê há tempos.",
                "Sua criatividade estará em alta.",
                "Hoje pode ser um bom dia para mudanças.",
                "A flexibilidade será sua aliada."
            },
            ["Câncer"] = new[]
            {
                "Dê atenção às suas emoções hoje.",
                "Um gesto de carinho fará toda a diferença.",
                "O passado pode trazer boas lembranças.",
                "Confie na sua intuição para tomar decisões."
            },
            ["Leão"] = new[]
            {
                "Seu brilho pessoal será notado.",
                "Lidere com o coração, não com o ego.",
                "Seja generoso, mas sem se sobrecarregar.",
                "A autoconfiança abrirá novas portas hoje."
            },
            ["Virgem"] = new[]
            {
                "Organize suas ideias antes de agir.",
                "Atenção aos detalhes fará toda a diferença.",
                "Aproveite para colocar a vida em ordem.",
                "Hoje é um bom dia para cuidar da saúde."
            },
            ["Libra"] = new[]
            {
                "Busque equilíbrio nas suas relações.",
                "A beleza está nos pequenos gestos.",
                "Um bom diálogo resolverá conflitos.",
                "Momento ideal para harmonizar sua rotina."
            },
            ["Escorpião"] = new[]
            {
                "Intensidade emocional à flor da pele.",
                "Mantenha segredo sobre seus planos.",
                "Transformações trarão crescimento.",
                "Use sua intuição para guiar suas ações."
            },
            ["Sagitário"] = new[]
            {
                "Aventure-se em novas ideias.",
                "O otimismo será seu guia hoje.",
                "Evite prometer mais do que pode cumprir.",
                "Uma oportunidade de aprendizado surgirá."
            },
            ["Capricórnio"] = new[]
            {
                "Disciplina será sua aliada no dia.",
                "Foque no que realmente importa.",
                "Evite se cobrar demais.",
                "Planeje o futuro com os pés no presente."
            },
            ["Aquário"] = new[]
            {
                "Inove e surpreenda com suas ideias.",
                "Seja fiel aos seus princípios.",
                "O inesperado pode ser positivo.",
                "Conecte-se com pessoas diferentes hoje."
            },
            ["Peixes"] = new[]
            {
                "Deixe a intuição te guiar.",
                "Sonhos podem trazer mensagens importantes.",
                "Cuide da sua energia espiritual.",
                "Um momento de introspecção será benéfico."
            }
        };

        private static readonly Dictionary<string, string[]> FinanceTips = new()
        {
            ["Áries"] = new[]
            {
                "Evite gastos impulsivos hoje.",
                "Reavalie seus investimentos.",
                "Pequenas economias farão grande diferença.",
                "Um orçamento bem planejado evita surpresas."
            },
            ["Touro"] = new[]
            {
                "Invista no que traz segurança.",
                "Evite riscos financeiros desnecessários.",
                "Um bom momento para renegociar dívidas.",
                "Seja paciente com suas finanças hoje."
            },
            ["Gêmeos"] = new[]
            {
                "Diversifique suas fontes de renda.",
                "Não compartilhe planos financeiros ainda.",
                "Oportunidades surgem de onde menos espera.",
                "Controle pequenos gastos para evitar perdas."
            },
            ["Câncer"] = new[]
            {
                "Proteja seu patrimônio com cautela.",
                "Evite empréstimos desnecessários.",
                "Pense em investimentos de longo prazo.",
                "Guarde uma reserva para emergências."
            },
            ["Leão"] = new[]
            {
                "Seja generoso, mas com controle.",
                "Invista em algo que valorize sua imagem.",
                "Evite ostentar para não gastar demais.",
                "Reveja seus gastos com luxo."
            },
            ["Virgem"] = new[]
            {
                "Analise detalhadamente suas contas.",
                "Organize suas finanças para evitar surpresas.",
                "Planeje despesas futuras com cuidado.",
                "Controle rigoroso trará tranquilidade."
            },
            ["Libra"] = new[]
            {
                "Negocie para obter melhores condições.",
                "Evite decisões financeiras precipitadas.",
                "Compartilhe ideias financeiras com confiança.",
                "Busque equilíbrio entre gastar e poupar."
            },
            ["Escorpião"] = new[]
            {
                "Mantenha suas finanças em segredo hoje.",
                "Aposte na discrição para bons negócios.",
                "Evite exposições desnecessárias.",
                "Reveja contratos com atenção."
            },
            ["Sagitário"] = new[]
            {
                "Cuidado com gastos em viagens.",
                "Otimismo não deve comprometer seu bolso.",
                "Aproveite oportunidades, mas com cautela.",
                "Planeje seu orçamento para aventuras."
            },
            ["Capricórnio"] = new[]
            {
                "Disciplina financeira será sua maior aliada.",
                "Planeje investimentos com foco no futuro.",
                "Evite compras por impulso.",
                "Reavalie seus gastos mensais."
            },
            ["Aquário"] = new[]
            {
                "Inove em formas de aumentar sua renda.",
                "Seja cauteloso com propostas muito boas.",
                "Colabore em projetos financeiros com amigos.",
                "Fique atento a detalhes contratuais."
            },
            ["Peixes"] = new[]
            {
                "Não deixe as emoções guiarem seus gastos.",
                "Avalie bem propostas financeiras inesperadas.",
                "Reserve um tempo para organizar seu orçamento.",
                "Confie na sua intuição para investimentos."
            }
        };

        private static readonly Dictionary<string, string[]> LoveTips = new()
        {
            ["Áries"] = new[]
            {
                "Uma surpresa romântica pode mudar seu dia.",
                "Abra seu coração para novas possibilidades.",
                "Cuidado para não ser impulsivo com quem ama.",
                "Uma conversa sincera fortalecerá sua relação."
            },
            ["Touro"] = new[]
            {
                "Demonstre seu carinho com gestos simples.",
                "Hoje é dia de valorizar a estabilidade afetiva.",
                "Evite discussões por motivos banais.",
                "Surpreenda quem você ama com atenção especial."
            },
            ["Gêmeos"] = new[]
            {
                "A comunicação será fundamental no amor.",
                "Novas conexões podem surgir inesperadamente.",
                "Mantenha o diálogo aberto e leve.",
                "Um encontro divertido pode alegrar o dia."
            },
            ["Câncer"] = new[]
            {
                "Aproveite momentos de intimidade com quem ama.",
                "Demonstre mais suas emoções hoje.",
                "Cuidado com inseguranças desnecessárias.",
                "Gestos de carinho fortalecerão vínculos."
            },
            ["Leão"] = new[]
            {
                "Seja generoso no amor, sem esperar nada em troca.",
                "Sua presença será muito valorizada hoje.",
                "Evite ser dominante na relação.",
                "Um elogio sincero fará a diferença."
            },
            ["Virgem"] = new[]
            {
                "Detalhes importam mais do que palavras hoje.",
                "Demonstre seu amor com ações práticas.",
                "Evite críticas que possam magoar.",
                "Momento propício para resolver pendências."
            },
            ["Libra"] = new[]
            {
                "Busque equilíbrio entre dar e receber.",
                "Um convite especial pode surgir.",
                "Evite indecisões no relacionamento.",
                "Compartilhe seus sentimentos com honestidade."
            },
            ["Escorpião"] = new[]
            {
                "Intensidade e paixão marcarão o dia.",
                "Confie em seu parceiro(a) para fortalecer laços.",
                "Mantenha seus segredos, mas seja aberto no amor.",
                "Uma surpresa pode reacender a chama."
            },
            ["Sagitário"] = new[]
            {
                "Liberdade e aventura no amor são bem-vindas.",
                "Evite prometer mais do que pode cumprir.",
                "Novas paixões podem surgir em ambientes diferentes.",
                "Valorize momentos de diversão a dois."
            },
            ["Capricórnio"] = new[]
            {
                "Seja paciente e construa relações sólidas.",
                "Demonstre compromisso com gestos concretos.",
                "Evite cobranças excessivas.",
                "O amor verdadeiro exige dedicação constante."
            },
            ["Aquário"] = new[]
            {
                "Inove na forma de demonstrar afeto.",
                "Valorize amizades que podem virar algo mais.",
                "Seja aberto a novas experiências amorosas.",
                "Evite resistir a sentimentos profundos."
            },
            ["Peixes"] = new[]
            {
                "Deixe sua sensibilidade guiar o amor.",
                "Sonhos podem indicar caminhos no coração.",
                "Cuide para não se iludir com promessas vazias.",
                "Um momento de romantismo pode transformar seu dia."
            }
        };

        [HttpGet("{nickname}/{sign}/{plan}")]
        public IActionResult GetHoroscope(string nickname, string sign, string plan)
        {
            var message = GetDailyMessageForSign(sign);
            string financeTip = null;
            string loveTip = null;

            if (plan == "premium")
            {
                financeTip = GetDailyTipForSign(sign, FinanceTips);
                loveTip = GetDailyTipForSign(sign, LoveTips);
            }

            var result = new HoroscopeResult
            {
                Nickname = nickname,
                Sign = sign,
                Message = message,
                LuckyNumber = plan != "basico" ? new Random().Next(1, 100) : null,
                FinanceTip = financeTip,
                LoveTip = loveTip
            };

            return Ok(result);
        }

        private string GetDailyMessageForSign(string sign)
        {
            if (!SignMessages.TryGetValue(sign, out var messages))
            {
                return $"Hoje é um dia comum para quem é de {sign}.";
            }

            var daySeed = DateTime.Today.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            int hash = Math.Abs((sign + daySeed).GetHashCode());
            int index = hash % messages.Length;

            return messages[index];
        }

        private string GetDailyTipForSign(string sign, Dictionary<string, string[]> tipsDictionary)
        {
            if (!tipsDictionary.TryGetValue(sign, out var tips))
            {
                return null;
            }

            var daySeed = DateTime.Today.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            int hash = Math.Abs((sign + daySeed).GetHashCode());
            int index = hash % tips.Length;

            return tips[index];
        }
    }
}
