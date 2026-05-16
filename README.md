# VotoTrack 🇧🇷

VotoTrack é uma plataforma de transparência pública projetada para aproximar o cidadão do Poder Legislativo. Através de uma interface moderna, o sistema consolida dados em tempo real da Câmara dos Deputados, permitindo o acompanhamento detalhado de gastos, projetos de lei, discursos e votações.

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
