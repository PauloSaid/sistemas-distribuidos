import requests # type: ignore

def obter_previsao(nickname, signo, plano):
    url = f"http://localhost:5184/api/horoscope/{nickname}/{signo}/{plano}"
    try:
        response = requests.get(url)
        if response.status_code == 200:
            data = response.json()
            print(f"\n Ola {data['nickname']}! Aqui esta sua previsao astrologica para {data['sign']}:")
            print(f" Mensagem: {data['message']}")
            if data.get('luckyNumber'):
                print(f" Numero da sorte: {data['luckyNumber']}")
            if data.get('financeTip'):
                print(f" Dica financeira: {data['financeTip']}")
            if data.get('loveTip'):
                print(f" Dica amorosa: {data['loveTip']}")
        else:
            print(" Erro ao se comunicar com o servidor.")
    except Exception as e:
        print(f"Erro: {e}")

if __name__ == "__main__":
    print("=== Cliente Astrologico ===")
    nickname = input("Digite seu nickname: ")
    signo = input("Digite seu signo: ").lower()
    plano = input("Escolha seu plano (basico, avancado, premium): ").lower()

    obter_previsao(nickname, signo, plano)
