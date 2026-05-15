# VotoTrack 🇧🇷

VotoTrack é uma plataforma moderna e intuitiva para acompanhar o desempenho dos parlamentares brasileiros. O objetivo é facilitar o acesso aos dados públicos da Câmara dos Deputados, permitindo que cidadãos monitorem gastos, votações e atividades legislativas de forma simplificada.

![VotoTrack Preview](https://vototrack.onrender.com/favicon.ico) <!-- Substitua por um screenshot se tiver -->

## 🚀 Funcionalidades

- **Explorar Parlamentares**: Lista completa de deputados com filtros por estado e partido.
- **Painel em Destaque**: Acompanhamento das últimas votações e novos projetos de lei em tramitação.
- **Minha Bancada (Favoritos)**: Selecione seus deputados favoritos para ter um feed personalizado com notícias e atividades específicas.
- **Detalhes do Parlamentar**:
  - Dados biográficos e de contato.
  - Histórico de despesas detalhado.
  - Registro de atividades legislativas (discursos, presença em comissões e votos).
- **Autenticação Segura**: Sistema de login, cadastro e recuperação de senha integrado ao Supabase.

## 🛠️ Tecnologias Utilizadas

- **Backend**: [ASP.NET Core 7.0 MVC](https://dotnet.microsoft.com/en-us/apps/aspnet/mvc)
- **Frontend**: Razor Pages, HTML5, CSS3 (Bootstrap 5)
- **Banco de Dados & Auth**: [Supabase](https://supabase.com/) (PostgreSQL + GoTrue)
- **API de Dados**: [Portal de Dados Abertos da Câmara dos Deputados](https://dadosabertos.camara.leg.br/)
- **Hospedagem**: [Render](https://render.com/)

## ⚙️ Como rodar localmente

### Pré-requisitos
- [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
- Conta no [Supabase](https://supabase.com/)

### Passo a Passo

1. **Clone o repositório**
   ```bash
   git clone https://github.com/Matthpsz/VotoTrack.git
   cd VotoTrack
   ```

2. **Configuração do Supabase**
   - Crie um novo projeto no Supabase.
   - Execute o script SQL (disponível na pasta `/Data/setup.sql`) no SQL Editor do Supabase para criar as tabelas e políticas de segurança.
   - Habilite a autenticação por e-mail em Authentication > Providers.

3. **Variáveis de Ambiente**
   Crie um arquivo `.env` na raiz do projeto ou configure no seu sistema:
   ```env
   SUPABASE_URL=sua_url_do_supabase
   SUPABASE_KEY=sua_chave_anon_do_supabase
   ```

4. **Execute o projeto**
   ```bash
   dotnet run
   ```
   Acesse `https://localhost:5001` no seu navegador.

## 📦 Estrutura do Projeto

- `/Controllers`: Lógica de controle e integração com APIs.
- `/Models`: Definição das estruturas de dados e ViewModels.
- `/Views`: Interfaces do usuário (Razor).
- `/wwwroot`: Arquivos estáticos (CSS, JS, Imagens).
- `Program.cs`: Configurações de serviços e pipeline da aplicação.

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---
Desenvolvido por [Matheus](https://github.com/Matthpsz)
