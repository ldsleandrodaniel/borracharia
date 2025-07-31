using borracharia.Context;
using borracharia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace borracharia.Controllers
{
    [Authorize]
    public class ServicoController : Controller
    {
     

        private readonly AppDbContext _context;

        public ServicoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Servico (Lista todos os serviços)
        public async Task<IActionResult> List()
        {
            var servicos = await _context.Servicos.ToListAsync();
            return View(servicos);
        }

        // GET: Servico/Create (Exibe o formulário)
        public IActionResult Create()
        {
            return View();
        }

        // POST: Servico/Create (Salva no banco)
        [HttpPost]
       // [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Servico servico)
        {
            if (ModelState.IsValid)
            {
             
                _context.Servicos.Add(servico);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(List));
            }
            return View(servico);
        }

        // GET: Servico/Edit/5 (Editar um serviço)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var servico = await _context.Servicos.FindAsync(id);
            if (servico == null)
                return NotFound();

            return View(servico);
        }

        // POST: Servico/Edit/5 (Atualiza no banco)
        [HttpPost]
      //  [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Servico servico)
        {
            if (id != servico.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(servico);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(List));
            }
            return View(servico);
        }

        // GET: Servico/Delete/5 (Confirmação de exclusão)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var servico = await _context.Servicos.FindAsync(id);
            if (servico == null)
                return NotFound();

            return View(servico);
        }

        // POST: Servico/Delete/5 (Remove do banco)
        [HttpPost, ActionName("Delete")]
     //   [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var servico = await _context.Servicos.FindAsync(id);
            _context.Servicos.Remove(servico);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }
    }
}
