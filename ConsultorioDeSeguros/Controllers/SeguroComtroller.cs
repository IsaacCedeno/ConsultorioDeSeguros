using ConsultorioDeSeguros.Models;
using ConsultorioDeSeguros.Persistences.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConsultorioDeSeguros.Controllers
{
    public class SeguroController : Controller
    {
        private readonly IGenericRepository<Seguro>  _seguroRepository;

        public SeguroController( IGenericRepository<Seguro>  seguroRepository)
        {
            _seguroRepository = seguroRepository;
        }


        public async Task<IActionResult> Index(int? id)
        {
            IEnumerable<Seguro> seguros;

            if (id.HasValue) 
            {
                var seguro = await _seguroRepository.GetById(id.Value);
                seguros = seguro != null ? new List<Seguro> { seguro } : Enumerable.Empty<Seguro>();
            }
            else 
            {
                seguros = await _seguroRepository.GetAllAsync();
            }

            return View(seguros);
        }

  
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(Seguro seguro)
        {
            if (ModelState.IsValid)
            {
                await _seguroRepository.RegisterAsync(seguro);
                return RedirectToAction("Index");
            }
            return View(seguro);
        }


        public async Task<IActionResult> Edit(int id)
        {
            var seguro = await _seguroRepository.GetById(id);
            if (seguro == null)
            {
                return NotFound();
            }
            return View(seguro);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, Seguro seguro)
        {
            if (id != seguro.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                await _seguroRepository.EditAsync(seguro);
                return RedirectToAction("Index");
            }
            return View(seguro);
        }


        public async Task<IActionResult> Delete(int id)
        {
            var seguro = await _seguroRepository.GetById(id);
            if (seguro == null)
            {
                return NotFound();
            }
            return View(seguro);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _seguroRepository.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}
