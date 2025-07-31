using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace borracharia.Models
{
    public class Servico
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do serviço é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome do Serviço")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O valor é obrigatório")]
        [Column(TypeName = "decimal(10, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser positivo")]
        [Display(Name = "Valor (R$)")]
        public decimal Valor { get; set; }

 
    }

}
