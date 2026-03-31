using SQLite;

namespace MauiAppFarmaFacil.Models;

public class Medicamento
{

    [PrimaryKey, AutoIncrement]
    public int CodMedicamento { get; set; }
   
    public string Nome { get; set; }
   
    public string PrincipioAtivo { get; set; }
    
    public string Dosagem { get; set; }
 
    public string Fabricante { get; set; }
   
}