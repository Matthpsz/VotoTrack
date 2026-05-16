# VotoTrack

VotoTrack é uma plataforma de transparência pública projetada para aproximar o cidadão do Poder Legislativo. Através de uma interface moderna, o sistema consolida dados em tempo real da Câmara dos Deputados, permitindo o acompanhamento detalhado de gastos, projetos de lei, discursos e votações.

<img width="1294" height="884" alt="image" src="https://github.com/user-attachments/assets/a57bafa9-7cde-4bf0-8234-1c45528b449f" />
<img width="1107" height="903" alt="image" src="https://github.com/user-attachments/assets/65cc73d6-9ac2-40ec-959e-aeaaf69b1b06" />
<img width="1078" height="905" alt="image" src="https://github.com/user-attachments/assets/fb7bde5f-9d2b-40b0-9462-492e54676fd0" />


## ✨ Diferenciais
- **Performance Otimizada**: Carregamento do dashboard através de integração direta e eficiente com a API de Dados Abertos.
- **Timeline de Atividades**: Acompanhe discursos e novos projetos de autoria dos parlamentares com links diretos para as fontes oficiais.
- **Gestão de Bancada**: Sistema de autenticação que permite salvar seus parlamentares favoritos para acompanhamento constante.
- **Transparência Financeira**: Visualização clara e categorizada das despesas parlamentares (CEAP).

## 🚀 Funcionalidades Principais
- [x] **Explorar Parlamentares**: Busca avançada por nome, partido ou estado.
- [x] **Dashboard em Tempo Real**: Feed de últimas votações no plenário e novos projetos de lei.
- [x] **Detalhes do Parlamentar**: Perfil completo com biografia, contatos, gastos mensais e histórico legislativo.
- [x] **Favoritos**: "Minha Bancada" personalizada para cada usuário logado.
- [x] **Segurança**: Autenticação robusta (Cadastro/Login/Recuperação de Senha) via Supabase.

## 🛠️ Stack Técnica
- **Backend**: ASP.NET Core 7.0 MVC
- **Frontend**: Razor Pages, Bootstrap 5 & Material Symbols
- **Auth & Database**: Supabase (PostgreSQL + GoTrue)
- **Fonte de Dados**: API de Dados Abertos da Câmara dos Deputados (v2)
