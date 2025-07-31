using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace borracharia.Models
{
    public class ServicoEfetuado
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "O serviço é obrigatório")]
        [Display(Name = "Serviço")]
        public int ServicoId { get; set; }

        [ForeignKey("ServicoId")]
        public  virtual Servico? Servico { get; set; }


        [Required(ErrorMessage = "A data é obrigatória")]
        [Display(Name = "Data e Hora")]
        [Column(TypeName = "timestamp without time zone")]
        [DataType(DataType.Text)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime Data { get; set; } = DateTime.Now; // Valor padrão

        [Required(ErrorMessage = "O valor é obrigatório")]
        [Column(TypeName = "decimal(10, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser positivo")]
        [Display(Name = "Valor Cobrado (R$)")]
        public decimal Valor { get; set; }

        [StringLength(500, ErrorMessage = "A observação deve ter no máximo 500 caracteres")]
        [Display(Name = "Observações")]
        public string? Observacao { get; set; }
    }
}
