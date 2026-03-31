using SQLite;

namespace MauiAppFarmaFacil.Models;


    public class UnidadeDeSaude
    {
        [PrimaryKey, AutoIncrement]
        public int CodUnidade { get; set; } // Identificador único 

        public string Nome { get; set; } // Nome da unidade 

        public string Endereco { get; set; } // Endereço físico 

        public string Telefone { get; set; } // Telefone de contato 

        public string HorarioFuncionamento { get; set; } // Horário de atendimento 

        public string Coordenadas { get; set; } // Para integração com mapas 

        // Método: exibirDetalhes() definido no diagrama 
        public string ExibirDetalhes()
        {
            return $"{Nome}\nEndereço: {Endereco}\nAtendimento: {HorarioFuncionamento}";
        }
    }
