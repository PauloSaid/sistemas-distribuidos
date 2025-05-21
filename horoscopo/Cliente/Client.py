import requests
import ipywidgets as widgets
from IPython.display import display, clear_output

SIGNOS = [
    "Áries", "Touro", "Gêmeos", "Câncer", "Leão", "Virgem",
    "Libra", "Escorpião", "Sagitário", "Capricórnio", "Aquário", "Peixes"
]


nickname_input = widgets.Text(
    description='Nickname:',
    placeholder='Digite seu apelido',
    style={'description_width': 'initial'},
    layout=widgets.Layout(width='50%')
)

signo_dropdown = widgets.Dropdown(
    options=SIGNOS,
    description='Signo:',
    style={'description_width': 'initial'},
    layout=widgets.Layout(width='50%')
)

plano_dropdown = widgets.Dropdown(
    options=[('Básico', 'basico'), ('Avançado', 'avancado'), ('Premium', 'premium')],
    description='Plano:',
    style={'description_width': 'initial'},
    layout=widgets.Layout(width='50%')
)

botao = widgets.Button(
    description='Obter Previsão',
    button_style='success',
    tooltip='Clique para obter sua previsão do horóscopo',
    icon='sun'  
)

saida = widgets.Output(layout=widgets.Layout(border='1px solid gray', padding='10px', width='80%', height='250px', overflow_y='auto'))


def ao_clicar(b):
    with saida:
        clear_output()
        nickname = nickname_input.value.strip()
        signo = signo_dropdown.value
        plano = plano_dropdown.value


        if not nickname:
            print("⚠️ Por favor, digite seu nickname.")
            return

        url = f"https://gz6wh12v-5001.brs.devtunnels.ms/api/Horoscope/{nickname}/{signo}/{plano}"
        print(f"Consultando previsão para {nickname} ({signo}) no plano {plano}...\n")
        try:
            response = requests.get(url)
            if response.status_code == 200:
                data = response.json()
                print(f"🌟 Olá, {data['nickname']}! Aqui está sua previsão para {data['sign']}:\n")
                print(f"📜 Mensagem: {data['message']}\n")
                if data.get('luckyNumber') is not None:
                    print(f"🍀 Número da sorte: {data['luckyNumber']}")
                if data.get('financeTip'):
                    print(f"💰 Dica financeira: {data['financeTip']}")
                if data.get('loveTip'):
                    print(f"❤️ Dica amorosa: {data['loveTip']}")
            else:
                print(f"❌ Erro {response.status_code}: Não foi possível se comunicar com o servidor.")
        except Exception as e:
            print(f"❌ Erro na requisição: {e}")

botao.on_click(ao_clicar)

form = widgets.VBox([
    nickname_input,
    signo_dropdown,
    plano_dropdown,
    botao,
    saida
],
layout=widgets.Layout(
    align_items='flex-start',
    width='100%',
    max_width='600px',
    padding='10px',
    border='2px solid #ddd',
    border_radius='10px',
    box_shadow='0 0 10px rgba(0,0,0,0.1)'
))

display(form)
