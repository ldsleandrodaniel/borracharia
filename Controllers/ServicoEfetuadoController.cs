using borracharia.Context;
using borracharia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace borracharia.Controllers
{
    [Authorize]
    public class ServicoEfetuadoController : Controller
    {
        private readonly AppDbContext _context;

        public ServicoEfetuadoController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List"); // Redireciona para a lista
        }

        // GET: ServicoEfetuado (Lista todos os serviços efetuados)
        public async Task<IActionResult> List()
        {
           

            var servicosEfetuados = await _context.ServicosEfetuados
                .Include(se => se.Servico)
                .ToListAsync();
            return View(servicosEfetuados);
        }

        public async Task<IActionResult> Relatorio()
        {


            var servicosEfetuados = await _context.ServicosEfetuados
                .Include(se => se.Servico)
                .ToListAsync();
            return View(servicosEfetuados);
        }



        // GET: ServicoEfetuado/Create (Exibe o formulário)
        public async Task<IActionResult> Create()
        {
            // Carrega os serviços para o dropdown
            ViewBag.Servicos = await _context.Servicos.ToListAsync();
            return View();
        }

        // POST: ServicoEfetuado/Create (Salva no banco)
        [HttpPost]
        public async Task<IActionResult> Create(ServicoEfetuado servicoEfetuado)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verifica se o ServicoId existe
                    if (!await _context.Servicos.AnyAsync(s => s.Id == servicoEfetuado.ServicoId))
                    {
                        ModelState.AddModelError("ServicoId", "Serviço inválido");
                    }
                    else
                    {
                        servicoEfetuado.Observacao ??= string.Empty;
                        _context.Add(servicoEfetuado);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(List));
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao salvar: {ex.Message}");
                    Console.WriteLine($"ERRO: {ex}");
                }
            }

            ViewBag.Servicos = await _context.Servicos.ToListAsync();
            return View(servicoEfetuado);
        }


        // GET: ServicoEfetuado/Edit/5 (Editar um serviço efetuado)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var servicoEfetuado = await _context.ServicosEfetuados.FindAsync(id);
            if (servicoEfetuado == null)
                return NotFound();

            ViewBag.Servicos = await _context.Servicos.ToListAsync();
            return View(servicoEfetuado);
        }

        // POST: ServicoEfetuado/Edit/5 (Atualiza no banco)
        [HttpPost]
        public async Task<IActionResult> Edit(int id, ServicoEfetuado servicoEfetuado)
        {
            if (id != servicoEfetuado.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(servicoEfetuado);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServicoEfetuadoExists(servicoEfetuado.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(List));
            }

            ViewBag.Servicos = await _context.Servicos.ToListAsync();
            return View(servicoEfetuado);
        }

        // GET: ServicoEfetuado/Delete/5 (Confirmação de exclusão)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var servicoEfetuado = await _context.ServicosEfetuados
                .Include(se => se.Servico)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (servicoEfetuado == null)
                return NotFound();

            return View(servicoEfetuado);
        }

        // POST: ServicoEfetuado/Delete/5 (Remove do banco)
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var servicoEfetuado = await _context.ServicosEfetuados.FindAsync(id);
            _context.ServicosEfetuados.Remove(servicoEfetuado);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }
   


        private bool ServicoEfetuadoExists(int id)
        {
            return _context.ServicosEfetuados.Any(e => e.Id == id);
        }
    }
}