using SQLite;
using MauiAppFarmaFacil.Models;

namespace MauiAppFarmaFacil.Services;

/// <summary>
/// ═══════════════════════════════════════════════════════════════════
/// ARQUITETURA DO FARMA FÁCIL — CAMADA DE DADOS
/// ═══════════════════════════════════════════════════════════════════
///
/// O Farma Fácil é um sistema exclusivo de CONSULTA CIDADÃ.
/// Ele NÃO gerencia, NÃO cadastra e NÃO administra dados.
/// Sua função é ser o canal entre o cidadão e as informações
/// já existentes na rede pública de saúde do SUS.
///
/// ORIGEM REAL DOS DADOS (produção):
/// ┌─────────────────────────────────────────────────────────────┐
/// │  RENAME (Ministério da Saúde)                               │
/// │    → Catálogo nacional de medicamentos essenciais           │
/// │    → Fonte: https://www.gov.br/saude/rename                 │
/// │                                                             │
/// │  HÓRUS (Sistema Nacional de Gestão da AF)                   │
/// │    → Estoque atualizado pelas próprias unidades de saúde    │
/// │    → Cada UBS/UPA alimenta seu próprio sistema interno      │
/// │    → O Farma Fácil consulta via API REST municipal          │
/// │                                                             │
/// │  CNES (Cadastro Nacional de Estabelecimentos de Saúde)      │
/// │    → Dados cadastrais das unidades (endereço, telefone)     │
/// │                                                             │
/// │  API de Geolocalização (Google Maps / OpenStreetMap)        │
/// │    → Ordenação das unidades por proximidade do cidadão      │
/// └─────────────────────────────────────────────────────────────┘
///
/// PAPEL DESTA CLASSE EM CADA CONTEXTO:
///   Produção   → substituída por ApiService consumindo endpoints
///                REST do município/estado (HÓRUS + CNES + RENAME).
///   Acadêmico  → simula esses dados via SQLite local com seed,
///                representando fielmente o que viria das APIs.
///
/// O CIDADÃO É SEMPRE SOMENTE LEITOR.
/// NENHUM DADO É INSERIDO PELO CIDADÃO OU POR UM ADMINISTRADOR.
/// ═══════════════════════════════════════════════════════════════════
/// </summary>
public class DatabaseService
{
    private SQLiteAsyncConnection? _db;
    private static readonly SemaphoreSlim _initLock = new(1, 1);

    // ─── Inicialização ─────────────────────────────────────────────────────────

    private async Task Init()
    {
        if (_db is not null) return;

        await _initLock.WaitAsync();
        try
        {
            if (_db is not null) return;

            var path = Path.Combine(FileSystem.AppDataDirectory, "FarmaFacil.db3");
            _db = new SQLiteAsyncConnection(path);

            // Cria as tabelas locais que espelham os dados
            // que em produção viriam via API da rede SUS
            await _db.CreateTableAsync<Cidadao>();
            await _db.CreateTableAsync<UnidadeDeSaude>();
            await _db.CreateTableAsync<Medicamento>();
            await _db.CreateTableAsync<Estoque>();

            // Simula a carga inicial que viria da integração
            // com RENAME, HÓRUS e CNES
            await SimularIntegracaoSUS();
        }
        finally
        {
            _initLock.Release();
        }
    }

    // ─── Simulação da integração com a rede SUS ────────────────────────────────

    /// <summary>
    /// Em produção este método seria substituído por chamadas REST às APIs
    /// municipais (HÓRUS) e ao catálogo nacional (RENAME/CNES).
    ///
    /// Simula os três conjuntos de dados que essas integrações retornariam:
    ///   • Medicamentos → RENAME (catálogo nacional de essenciais)
    ///   • Unidades     → CNES  (cadastro nacional de estabelecimentos)
    ///   • Estoques     → HÓRUS (sistema interno de cada unidade de saúde)
    ///
    /// O cidadão não intervém neste processo.
    /// As próprias unidades de saúde alimentam seus sistemas internos,
    /// e o Farma Fácil apenas lê e apresenta essas informações.
    /// </summary>
    private async Task SimularIntegracaoSUS()
    {
        if (_db is null) return;

        // Executa apenas uma vez (banco vazio = primeira inicialização)
        var count = await _db.Table<Medicamento>().CountAsync();
        if (count > 0) return;

        // ── Catálogo de medicamentos — fonte: RENAME ───────────────────────────
        // A RENAME define quais medicamentos fazem parte da assistência
        // farmacêutica do SUS. Nenhum usuário ou administrador do Farma Fácil
        // cadastra ou altera este catálogo — ele é de responsabilidade do
        // Ministério da Saúde e atualizado periodicamente por portaria.
        var medicamentos = new List<Medicamento>
        {
            new() { Nome = "Paracetamol",  PrincipioAtivo = "Paracetamol",              Dosagem = "500mg",  Fabricante = "EMS"       },
            new() { Nome = "Ibuprofeno",   PrincipioAtivo = "Ibuprofeno",               Dosagem = "400mg",  Fabricante = "Medley"    },
            new() { Nome = "Amoxicilina",  PrincipioAtivo = "Amoxicilina",              Dosagem = "500mg",  Fabricante = "Eurofarma" },
            new() { Nome = "Losartana",    PrincipioAtivo = "Losartana Potássica",       Dosagem = "50mg",   Fabricante = "Germed"    },
            new() { Nome = "Metformina",   PrincipioAtivo = "Cloridrato de Metformina",  Dosagem = "850mg",  Fabricante = "EMS"       },
            new() { Nome = "Atenolol",     PrincipioAtivo = "Atenolol",                 Dosagem = "25mg",   Fabricante = "Medley"    },
            new() { Nome = "Omeprazol",    PrincipioAtivo = "Omeprazol",                Dosagem = "20mg",   Fabricante = "Eurofarma" },
            new() { Nome = "Dipirona",     PrincipioAtivo = "Dipirona Sódica",           Dosagem = "500mg",  Fabricante = "EMS"       },
            new() { Nome = "Azitromicina", PrincipioAtivo = "Azitromicina",              Dosagem = "500mg",  Fabricante = "Germed"    },
            new() { Nome = "Captopril",    PrincipioAtivo = "Captopril",                Dosagem = "25mg",   Fabricante = "Medley"    },
        };
        await _db.InsertAllAsync(medicamentos);

        // ── Unidades de saúde — fonte: CNES ───────────────────────────────────
        // O CNES (Cadastro Nacional de Estabelecimentos de Saúde) mantém
        // o registro oficial de todas as unidades públicas de saúde do Brasil.
        // Em produção, o Farma Fácil consultaria a API pública do CNES
        // para obter nome, endereço, telefone e coordenadas de cada unidade.
        var unidades = new List<UnidadeDeSaude>
        {
            new() { Nome = "UBS Central",         Endereco = "Rua das Flores, 100 - Centro",       Telefone = "(17) 3842-1000", HorarioFuncionamento = "Seg-Sex 07:00-17:00",                    Coordenadas = "-20.4297,-50.0832" },
            new() { Nome = "UPA 24h Norte",        Endereco = "Av. Brasil, 500 - Jd. América",     Telefone = "(17) 3842-2000", HorarioFuncionamento = "24 horas",                              Coordenadas = "-20.4210,-50.0750" },
            new() { Nome = "Farmácia Popular SUS", Endereco = "Rua XV de Novembro, 250 - Centro",  Telefone = "(17) 3842-3000", HorarioFuncionamento = "Seg-Sex 08:00-18:00, Sáb 08:00-12:00", Coordenadas = "-20.4320,-50.0870" },
            new() { Nome = "UBS Jardim Europa",    Endereco = "Rua Paraná, 80 - Jd. Europa",       Telefone = "(17) 3842-4000", HorarioFuncionamento = "Seg-Sex 07:00-17:00",                    Coordenadas = "-20.4400,-50.0900" },
            new() { Nome = "Hospital Municipal",   Endereco = "Av. Deputado Cunha, 1200 - Centro", Telefone = "(17) 3842-5000", HorarioFuncionamento = "24 horas",                              Coordenadas = "-20.4250,-50.0800" },
        };
        await _db.InsertAllAsync(unidades);

        // Recarrega com IDs gerados pelo banco
        var meds = await _db.Table<Medicamento>().ToListAsync();
        var units = await _db.Table<UnidadeDeSaude>().ToListAsync();

        // ── Estoques — fonte: HÓRUS / sistema interno de cada unidade ─────────
        // O HÓRUS é o sistema federal de gestão da Assistência Farmacêutica.
        // Cada UBS, UPA e farmácia popular registra no seu sistema interno
        // as entradas e saídas de medicamentos. O Farma Fácil consome
        // essas informações via API REST do município — nunca as escreve.
        // A quantidade disponível reflete o saldo atual de cada unidade.
        var estoques = new List<Estoque>
        {
            // UBS Central
            new() { CodUnidade = units[0].CodUnidade, CodMedicamento = meds[0].CodMedicamento, QuantidadeDisponivel = 150 },
            new() { CodUnidade = units[0].CodUnidade, CodMedicamento = meds[3].CodMedicamento, QuantidadeDisponivel = 80  },
            new() { CodUnidade = units[0].CodUnidade, CodMedicamento = meds[4].CodMedicamento, QuantidadeDisponivel = 60  },
            new() { CodUnidade = units[0].CodUnidade, CodMedicamento = meds[6].CodMedicamento, QuantidadeDisponivel = 200 },
            // UPA 24h Norte
            new() { CodUnidade = units[1].CodUnidade, CodMedicamento = meds[0].CodMedicamento, QuantidadeDisponivel = 300 },
            new() { CodUnidade = units[1].CodUnidade, CodMedicamento = meds[1].CodMedicamento, QuantidadeDisponivel = 120 },
            new() { CodUnidade = units[1].CodUnidade, CodMedicamento = meds[7].CodMedicamento, QuantidadeDisponivel = 500 },
            new() { CodUnidade = units[1].CodUnidade, CodMedicamento = meds[8].CodMedicamento, QuantidadeDisponivel = 45  },
            // Farmácia Popular SUS
            new() { CodUnidade = units[2].CodUnidade, CodMedicamento = meds[0].CodMedicamento, QuantidadeDisponivel = 250 },
            new() { CodUnidade = units[2].CodUnidade, CodMedicamento = meds[2].CodMedicamento, QuantidadeDisponivel = 90  },
            new() { CodUnidade = units[2].CodUnidade, CodMedicamento = meds[3].CodMedicamento, QuantidadeDisponivel = 110 },
            new() { CodUnidade = units[2].CodUnidade, CodMedicamento = meds[5].CodMedicamento, QuantidadeDisponivel = 70  },
            new() { CodUnidade = units[2].CodUnidade, CodMedicamento = meds[9].CodMedicamento, QuantidadeDisponivel = 55  },
            // UBS Jardim Europa
            new() { CodUnidade = units[3].CodUnidade, CodMedicamento = meds[4].CodMedicamento, QuantidadeDisponivel = 40  },
            new() { CodUnidade = units[3].CodUnidade, CodMedicamento = meds[6].CodMedicamento, QuantidadeDisponivel = 180 },
            new() { CodUnidade = units[3].CodUnidade, CodMedicamento = meds[7].CodMedicamento, QuantidadeDisponivel = 220 },
            // Hospital Municipal
            new() { CodUnidade = units[4].CodUnidade, CodMedicamento = meds[0].CodMedicamento, QuantidadeDisponivel = 1000 },
            new() { CodUnidade = units[4].CodUnidade, CodMedicamento = meds[1].CodMedicamento, QuantidadeDisponivel = 400  },
            new() { CodUnidade = units[4].CodUnidade, CodMedicamento = meds[2].CodMedicamento, QuantidadeDisponivel = 200  },
            new() { CodUnidade = units[4].CodUnidade, CodMedicamento = meds[8].CodMedicamento, QuantidadeDisponivel = 150  },
        };
        await _db.InsertAllAsync(estoques);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CASOS DE USO — somente leitura, acionados pelo Cidadão
    // ═══════════════════════════════════════════════════════════════════════════

    // ─── UC1: Buscar Medicamento por Nome ──────────────────────────────────────

    /// <summary>
    /// Permite ao cidadão pesquisar um medicamento pelo nome comercial
    /// ou pelo princípio ativo (ex: "Losartana" ou "Losartana Potássica").
    ///
    /// Em produção: GET /api/medicamentos?q={termo} → API RENAME/municipal.
    /// Aqui: consulta local na tabela Medicamento (espelho do catálogo RENAME).
    /// </summary>
    public async Task<List<Medicamento>> BuscarMedicamentos(string termo)
    {
        await Init();

        if (string.IsNullOrWhiteSpace(termo))
            return await _db!.Table<Medicamento>().ToListAsync();

        termo = termo.Trim();
        return await _db!.Table<Medicamento>()
            .Where(m => m.Nome.Contains(termo) || m.PrincipioAtivo.Contains(termo))
            .ToListAsync();
    }

    // ─── UC2: Consultar Unidades de Saúde Próximas ─────────────────────────────

    /// <summary>
    /// Retorna as unidades de saúde que possuem o medicamento em estoque,
    /// ordenadas pela distância em relação à localização atual do cidadão.
    ///
    /// Implementa a relação &lt;&lt;include&gt;&gt; UC1 → UC2 do diagrama de casos de uso:
    /// ao buscar um medicamento, o sistema identifica automaticamente quais
    /// unidades próximas o têm disponível — poupando o cidadão de se deslocar
    /// de UBS em UBS sem garantia de encontrar o medicamento.
    ///
    /// Em produção: GET /api/estoque?medicamento={id}&lat={lat}&lng={lng}
    ///              → API HÓRUS municipal, retorna unidades com estoque > 0.
    /// </summary>
    public async Task<List<UnidadeDeSaude>> ConsultarUnidadesComMedicamento(
        int codMedicamento,
        double latUsuario = 0,
        double lngUsuario = 0)
    {
        await Init();

        var estoques = await _db!.Table<Estoque>()
            .Where(e => e.CodMedicamento == codMedicamento && e.QuantidadeDisponivel > 0)
            .ToListAsync();

        if (estoques.Count == 0) return new List<UnidadeDeSaude>();

        var codUnidades = estoques.Select(e => e.CodUnidade).ToHashSet();
        var todasUnidades = await _db.Table<UnidadeDeSaude>().ToListAsync();

        var resultado = todasUnidades
            .Where(u => codUnidades.Contains(u.CodUnidade))
            .ToList();

        // Propaga a quantidade disponível de cada unidade para exibição
        foreach (var u in resultado)
        {
            var est = estoques.FirstOrDefault(e => e.CodUnidade == u.CodUnidade);
            u.QuantidadeDisponivel = est?.QuantidadeDisponivel ?? 0;
        }

        // Se o cidadão compartilhou localização: ordena da mais próxima à mais distante
        if (latUsuario != 0 || lngUsuario != 0)
        {
            foreach (var u in resultado)
                u.DistanciaKm = CalcularDistanciaKm(latUsuario, lngUsuario, u.Latitude, u.Longitude);

            return resultado.OrderBy(u => u.DistanciaKm).ToList();
        }

        return resultado;
    }

    // ─── UC3: Ver Detalhes da Unidade de Saúde ─────────────────────────────────

    /// <summary>
    /// Exibe ao cidadão as informações completas de uma unidade selecionada:
    /// endereço, telefone, horário de funcionamento e quantidade disponível.
    ///
    /// Relação &lt;&lt;extend&gt;&gt; em relação a UC1 e UC2: acionado opcionalmente
    /// pelo cidadão ao tocar em uma unidade da lista de resultados.
    ///
    /// Em produção: GET /api/unidades/{codUnidade} → API CNES.
    /// </summary>
    public async Task<UnidadeDeSaude?> ObterDetalhesUnidade(int codUnidade)
    {
        await Init();
        return await _db!.Table<UnidadeDeSaude>()
            .Where(u => u.CodUnidade == codUnidade)
            .FirstOrDefaultAsync();
    }

    // ─── Preferências locais do Cidadão ────────────────────────────────────────

    /// <summary>
    /// Armazena preferências do cidadão somente no dispositivo local
    /// (endereço favorito, aceite de notificações, etc.).
    /// Esses dados NUNCA são enviados à rede SUS nem alteram
    /// qualquer cadastro externo — são de uso exclusivo do app.
    /// </summary>
    public async Task<int> SalvarPreferenciasCidadao(Cidadao cidadao)
    {
        await Init();
        if (cidadao.Id == 0)
            return await _db!.InsertAsync(cidadao);
        return await _db!.UpdateAsync(cidadao);
    }

    public async Task<Cidadao?> BuscarPreferenciasCidadao(string email)
    {
        await Init();
        return await _db!.Table<Cidadao>()
            .Where(c => c.Email == email)
            .FirstOrDefaultAsync();
    }

    // ─── Utilitários ────────────────────────────────────────────────────────────

    /// <summary>
    /// Calcula a distância em quilômetros entre dois pontos geográficos
    /// usando a fórmula de Haversine. Utilizada para ordenar as unidades
    /// de saúde da mais próxima à mais distante do cidadão.
    /// </summary>
    private static double CalcularDistanciaKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Raio médio da Terra em km
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRad(double deg) => deg * Math.PI / 180;
}